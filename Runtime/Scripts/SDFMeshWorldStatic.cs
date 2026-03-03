using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.VFX.SDF;
using System.Threading.Tasks;
#if UNITY_EDITOR
using Unity.EditorCoroutines.Editor;
using UnityEditor;
#endif

namespace YL2
{
	public class SDFMeshWorldStatic : MonoBehaviour
	{
#if UNITY_EDITOR
		[Range(16, 64)]
		public int resolution = 32;

		//[Tooltip("This slider doesn't actually change the SDF or anything, only how it is displayed.\n\nIf there are a lot of holes at higher values, that's an indicator you may want to thicken things up a bit.")]
		//[Range(-1f, 2f)]
		//public float drawAdjust = .9f;

		[Range(0f, .02f)]
		public float offset = 0f;

		public bool drawSDF = true;
		[Tooltip("Just for disaply")]
		public string RAMUsage;

		private int resolutionMemory = 0;
		private float offsetMemory;
		private bool drawSDFMemory;
#endif

		[SerializeField] [HideInInspector]
		private float4[] data;

		public float4[] Data => data;

		[SerializeField] [HideInInspector]
		private uint3 dimensions;

		public uint3 Dimensions => dimensions;

		[SerializeField] [HideInInspector]
		private Bounds localExtendedBounds;

		public Bounds LocalExtendedBounds => localExtendedBounds;

		[SerializeField] [HideInInspector]
		private float maxExtents;

#if UNITY_EDITOR
		private RenderTexture sdfTexture;
		private GameObject boxGO;
		private EditorCoroutine coroutine;

		private List<GameObject> boxGOs = new List<GameObject>();

		public static Mesh MakeReadableMeshCopy(Mesh nonReadableMesh)
		{
			Mesh meshCopy = new Mesh();
			meshCopy.indexFormat = nonReadableMesh.indexFormat;

			// Handle vertices
			GraphicsBuffer verticesBuffer = nonReadableMesh.GetVertexBuffer(0);
			int totalSize = verticesBuffer.stride * verticesBuffer.count;
			byte[] data = new byte[totalSize];
			verticesBuffer.GetData(data);
			meshCopy.SetVertexBufferParams(nonReadableMesh.vertexCount, nonReadableMesh.GetVertexAttributes());
			meshCopy.SetVertexBufferData(data, 0, 0, totalSize);
			verticesBuffer.Release();

			// Handle triangles
			meshCopy.subMeshCount = nonReadableMesh.subMeshCount;
			GraphicsBuffer indexesBuffer = nonReadableMesh.GetIndexBuffer();
			int tot = indexesBuffer.stride * indexesBuffer.count;
			byte[] indexesData = new byte[tot];
			indexesBuffer.GetData(indexesData);
			meshCopy.SetIndexBufferParams(indexesBuffer.count, nonReadableMesh.indexFormat);
			meshCopy.SetIndexBufferData(indexesData, 0, 0, tot);
			indexesBuffer.Release();

			// Restore submesh structure
			uint currentIndexOffset = 0;
			for (int i = 0; i < meshCopy.subMeshCount; i++)
			{
				uint subMeshIndexCount = nonReadableMesh.GetIndexCount(i);
				meshCopy.SetSubMesh(i, new UnityEngine.Rendering.SubMeshDescriptor((int)currentIndexOffset, (int)subMeshIndexCount));
				currentIndexOffset += subMeshIndexCount;
			}

			// Recalculate normals and bounds
			meshCopy.RecalculateNormals();
			meshCopy.RecalculateBounds();

			return meshCopy;
		}

		private static Dictionary<Mesh, Mesh> toReadable = new Dictionary<Mesh, Mesh>();

		private void ComputeSDF()
		{
			var mesh = GetComponent<MeshFilter>()?.sharedMesh;

			UnityEngine.Assertions.Assert.IsNotNull(mesh);

			if (!mesh.isReadable)
			{
				if (!toReadable.TryGetValue(mesh, out var _newMesh))
				{
					_newMesh = MakeReadableMeshCopy(mesh);
					toReadable.Add(mesh, _newMesh);
					mesh = _newMesh;
				}
			}

			var newMesh = new Mesh();

			var verts = mesh.vertices;
			var normals = mesh.normals;

			UnityEngine.Assertions.Assert.IsTrue(verts.Length > 0);

			var matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, transform.localScale);

			Parallel.For(0, mesh.vertexCount, (i) =>
			{
				verts[i] = matrix.MultiplyPoint3x4(verts[i]);
				normals[i] = matrix.MultiplyVector(normals[i]);
			});

			newMesh.vertices = verts;
			newMesh.normals = normals;
			newMesh.triangles = mesh.triangles;

			UnityEngine.Assertions.Assert.IsTrue(newMesh.triangles.Length > 0);

			newMesh.RecalculateBounds();

			var bounds = newMesh.bounds;
			maxExtents = Mathf.Max(bounds.size.x, Mathf.Max(bounds.size.y, bounds.size.z));

			float voxelSize = maxExtents / resolution;
			int paddingCells = 2;
			var padding = voxelSize * paddingCells;
			int _resolution = resolution;
			_resolution += paddingCells * 2;
			bounds.extents += new Vector3(padding, padding, padding);
			maxExtents = Mathf.Max(bounds.size.x, Mathf.Max(bounds.size.y, bounds.size.z));

			localExtendedBounds = bounds;

			var baker = new MeshToSDFBaker(bounds.size, bounds.center, _resolution, newMesh, 1, .5f, offset);
			baker.BakeSDF();
			var _sdfTexture = baker.SdfTexture;

			try
			{
				sdfTexture?.Release();
			}
			catch (System.Exception) { }

			sdfTexture = new RenderTexture(_sdfTexture.descriptor);
			Graphics.CopyTexture(_sdfTexture, sdfTexture);

			RAMUsage = BytesToHumanReadable(sdfTexture.width * sdfTexture.height * sdfTexture.volumeDepth * sizeof(float) * 4 * 4);

			//try
			//{
			//	EditorCoroutineUtility.StopCoroutine(coroutine);
			//}
			//catch(System.Exception) { }

			coroutine = EditorCoroutineUtility.StartCoroutine(RefreshVectorField(), this);

			StartCoroutine(DisposeBaker(baker));

			RefreshBoxMesh();

			StartCoroutine(RefreshBoxCollider(bounds));
		}

		public static string BytesToHumanReadable(long bytes)
		{
			string[] sizes = { "B", "KB", "MB", "GB", "TB", "PB" };
			double len = bytes;
			int order = 0;
			while (len >= 1024 && order < sizes.Length - 1)
			{
				++order;
				len = len / 1024;
			}

			// Adjust the format as needed
			return String.Format("{0:0.##} {1}", len, sizes[order]);
		}

		private IEnumerator RefreshVectorField()
		{
			yield return new EditorWaitForSeconds(0f);

			(data, dimensions) = SDFToVectorFieldSampler(sdfTexture, maxExtents);
		}

		private IEnumerator DisposeBaker(MeshToSDFBaker baker)
		{
			yield return new WaitForSeconds(0f);

			GameObject.DestroyImmediate(baker.SdfTexture);
		}

		private void OnValidate()
		{
			if (Application.isPlaying || !gameObject.activeInHierarchy)
			{
				return;
			}

#if UNITY_EDITOR
			if (BuildPipeline.isBuildingPlayer)
			{
				return;
			}
#endif

			if (data == null || drawSDF != drawSDFMemory || offset != offsetMemory || resolution != resolutionMemory)
			{
				RefreshLayer();
				ComputeSDF();
				RefreshBoxMesh();
				CheckClear();

				drawSDFMemory = drawSDF;
				offsetMemory = offset;
				resolutionMemory = resolution;
			}
		}

		private void RefreshLayer()
		{
			int layer = LayerMask.NameToLayer("WorldStaticSDF");

			if (layer > -1)
			{
				gameObject.layer = layer;
			}
		}

		private void RefreshBoxMesh()
		{
			EditorCoroutineUtility.StartCoroutine(ClearOld(), this);

			var mat = GameObject.Instantiate<Material>(Resources.Load<Material>("FurryVNE SDK/YL2_SDF_Matcap"));

			var bounds = localExtendedBounds;

			mat.SetVector("_BoundsMin", bounds.min);
			mat.SetVector("_BoundsMax", bounds.max);
			mat.SetVector("_Dimensions", new Vector3(sdfTexture.width, sdfTexture.height, sdfTexture.volumeDepth));
			mat.SetFloat("_MaxExtents", maxExtents);
			mat.SetTexture("_SDF", sdfTexture);
			mat.SetInt("_SDFComponentIndex", 0);
			mat.SetInt("_Steps", 100);

			var boxMesh = CreateCubeMeshFromBounds(localExtendedBounds);

			boxGO = GameObject.CreatePrimitive(PrimitiveType.Cube);
			boxGO.name = "SDF_" + gameObject.GetInstanceID();
			boxGO.hideFlags = HideFlags.HideAndDontSave;
			boxGO.GetComponent<MeshFilter>().sharedMesh = boxMesh;
			boxGO.GetComponent<MeshRenderer>().sharedMaterial = mat;
			boxGO.SetActive(drawSDF);
			boxGO.transform.SetPositionAndRotation(transform.position, transform.rotation);
			boxGOs.Add(boxGO);
		}

		private IEnumerator DestroyGO(GameObject go)
		{
			yield return null;

			boxGOs.Remove(go);
			GameObject.DestroyImmediate(go);
		}

		private IEnumerator RefreshBoxCollider(Bounds bounds)
		{
			yield return new WaitForSeconds(0f);

			for (int i = 0; i < 100; i++)
			{
				var c = GetComponent<Collider>();

				if (c != null)
				{
					GameObject.DestroyImmediate(c);
				}
				else
				{
					break;
				}
			}

			var bc = gameObject.GetComponent<BoxCollider>();

			if (bc == null)
			{
				bc = gameObject.AddComponent<BoxCollider>();
			}

			var boxLocalToWorld = boxGO.GetComponent<Renderer>().localToWorldMatrix;
			var thisWorldToLocal = GetComponent<Renderer>().worldToLocalMatrix;

			var matrix = thisWorldToLocal * boxLocalToWorld;
			var min = matrix.MultiplyPoint3x4(bounds.min);
			var max = matrix.MultiplyPoint3x4(bounds.max);

			var newMin = Vector3.Min(min, max);
			var newMax = Vector3.Max(min, max);

			bounds.min = newMin;
			bounds.max = newMax;
			bc.center = Vector3.Lerp(newMin, newMax, 0.5f);
			bc.size = newMax - newMin;
		}

		public static ComputeBuffer CreateBuffer<T>(int length, ComputeBufferType type = ComputeBufferType.Default)
		{
			int size = System.Runtime.InteropServices.Marshal.SizeOf(typeof(T));

			UnityEngine.Assertions.Assert.IsTrue(length > 0);

			var cb = new ComputeBuffer(length, size, type);

			return cb;
		}

		public static int GetComputeShaderThreads(int count, int threads = 64)
		{
			return (count + threads - 1) / threads;
		}

		public static (float4[], uint3) SDFToVectorFieldSampler(RenderTexture sdfRT, float maxExtents)
		{
			var buffer = CreateBuffer<float4>(sdfRT.width * sdfRT.height * sdfRT.volumeDepth);

			var shader = Resources.Load<ComputeShader>("FurryVNE SDK/SDFToTexture3D");

			UnityEngine.Assertions.Assert.IsNotNull(shader);

			shader.SetFloat("MaxExtents", maxExtents);
			shader.SetTexture(0, "SDF", sdfRT);
			shader.SetVector("Dimensions", new Vector4(sdfRT.width, sdfRT.height, sdfRT.volumeDepth, 0));
			shader.SetBuffer(0, "Result", buffer);
			shader.Dispatch(0, GetComputeShaderThreads(sdfRT.width, 4), GetComputeShaderThreads(sdfRT.height, 4), GetComputeShaderThreads(sdfRT.volumeDepth, 4));

			var pixels = GetData<float4>(buffer);

			buffer.Release();

			var dimensions = new uint3((uint)sdfRT.width, (uint)sdfRT.height, (uint)sdfRT.volumeDepth);

			return (pixels, dimensions);
		}

		public static T[] GetData<T>(ComputeBuffer buffer)
		{
			var arr = new T[buffer.count];
			buffer.GetData(arr);

			return arr;
		}

		public static Mesh CreateCubeMeshFromBounds(Bounds bounds)
		{
			Vector3 center = bounds.center;
			float x = bounds.extents.x;
			float y = bounds.extents.y;
			float z = bounds.extents.z;

			Vector3[] vertices = {
				center + new Vector3(-x, -y, -z),
				center + new Vector3(x, -y, -z),
				center + new Vector3(x, -y, z),
				center + new Vector3(-x, -y, z),
				center + new Vector3(-x, y, -z),
				center + new Vector3(x, y, -z),
				center + new Vector3(x, y, z),
				center + new Vector3(-x, y, z),
			};

			int[] triangles = {
				// Bottom
				0, 2, 1, 0, 3, 2,
				// Left
				0, 4, 7, 0, 7, 3,
				// Front
				0, 1, 5, 0, 5, 4,
				// Back
				3, 7, 6, 3, 6, 2,
				// Right
				1, 2, 6, 1, 6, 5,
				// Top
				4, 5, 6, 4, 6, 7
			};

			triangles = triangles.Reverse().ToArray();

			Vector3[] normals = new Vector3[8];

			for (int i = 0; i < normals.Length; i++)
			{
				normals[i] = vertices[i] - center;
				normals[i].Normalize();
			}

			Mesh mesh = new Mesh();
			mesh.vertices = vertices;
			mesh.triangles = triangles;
			mesh.normals = normals;

			return mesh;
		}

		private void CheckClear()
		{
			EditorCoroutineUtility.StartCoroutine(_CheckClear(), this);
		}

		private IEnumerator _CheckClear()
		{
			yield return null;

			if (this != null && !isActiveAndEnabled && boxGO != null)
			{
				ClearOld();
			}
		}

		private IEnumerator ClearOld()
		{
			foreach (var go in boxGOs)
			{
				if (go != boxGO)
				{
					go.SetActive(false);
				}
			}

			yield return null;

			foreach(var go in boxGOs.ToArray())
			{
				if (go != boxGO)
				{
					boxGOs.Remove(go);
					DestroyImmediate(go);
				}
			}
		}
#endif
	}
}