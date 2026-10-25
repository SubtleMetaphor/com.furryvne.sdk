using UnityEngine;

#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
#endif

namespace YL2.Interaction
{
	[DisallowMultipleComponent]
	public class Prop : MonoBehaviour
	{
		public bool mirrorAttachedNodesInPropSpace = false;
		
		[SerializeField] [HideInInspector]
		private string id;

		public string ID => id;

#if UNITY_EDITOR
		private void Reset()
		{
			EnsureId();
			PropIdEditor.ScheduleRefresh();
		}

		private void OnValidate()
		{
			EnsureId();
			PropIdEditor.ScheduleRefresh();
		}

		private void EnsureId()
		{
			if (string.IsNullOrEmpty(id))
			{
				id = System.Guid.NewGuid().ToString();
			}
		}
#endif
	}

#if UNITY_EDITOR
	[InitializeOnLoad]
	internal static class PropIdEditor
	{
		private static Dictionary<string, Prop> owners = new Dictionary<string, Prop>();

		static PropIdEditor()
		{
			EditorApplication.hierarchyChanged += ScheduleRefresh;
			EditorSceneManager.sceneSaving += (scene, path) => RefreshLoadedScenes();
			ScheduleRefresh();
		}

		internal static void ScheduleRefresh()
		{
			EditorApplication.delayCall -= RefreshLoadedScenes;
			EditorApplication.delayCall += RefreshLoadedScenes;
		}

		private static void RefreshLoadedScenes()
		{
			if (EditorApplication.isPlayingOrWillChangePlaymode) return;

			var props = new List<Prop>();
			for (int i = 0; i < SceneManager.sceneCount; i++)
			{
				var scene = SceneManager.GetSceneAt(i);
				if (!scene.IsValid() || !scene.isLoaded || EditorSceneManager.IsPreviewScene(scene)
					|| (!string.IsNullOrEmpty(scene.path)
						&& !scene.path.EndsWith(".unity", System.StringComparison.OrdinalIgnoreCase)))
				{
					continue;
				}

				foreach (var root in scene.GetRootGameObjects())
				{
					props.AddRange(root.GetComponentsInChildren<Prop>(true));
				}
			}

			ValidateIds(props);
		}

		private static void ValidateIds(List<Prop> props)
		{
			// Reserve the originals' IDs before scanning, regardless of hierarchy order.
			// Rebuilding also drops registry entries for removed props.
			var loaded = new HashSet<Prop>(props);
			var currentOwners = new Dictionary<string, Prop>();
			foreach (var pair in owners)
			{
				if (pair.Value != null && loaded.Contains(pair.Value) && pair.Value.ID == pair.Key)
				{
					currentOwners.Add(pair.Key, pair.Value);
				}
			}
			owners = currentOwners;

			foreach (var prop in props)
			{
				string value = prop.ID;
				bool duplicate = !string.IsNullOrEmpty(value)
					&& owners.TryGetValue(value, out var owner) && owner != prop;

				if (string.IsNullOrEmpty(value) || duplicate)
				{
					do { value = System.Guid.NewGuid().ToString(); }
					while (owners.ContainsKey(value));

					var serialized = new SerializedObject(prop);
					serialized.FindProperty("id").stringValue = value;
					serialized.ApplyModifiedPropertiesWithoutUndo();
					EditorSceneManager.MarkSceneDirty(prop.gameObject.scene);
				}

				owners[value] = prop;
			}
		}
	}
#endif
}