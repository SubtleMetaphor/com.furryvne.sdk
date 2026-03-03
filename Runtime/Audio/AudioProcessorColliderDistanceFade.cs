using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YL2.Audio;

namespace YL2.Audio
{
    public class AudioProcessorColliderDistanceFade : AudioProcessor
    {
		public Collider[] distanceTo;

		[Range(1f, 30f)]
		public float fadeDistance = 10f;
		public bool raycast = false;
		public AnimationCurve fadeFalloff = new AnimationCurve(new Keyframe(0f, 1f, -1f, -1f), new Keyframe(1f, 0f, -1f, -1f));

        public AudioAffect[] audioAffects;

		private static int layerMask;

		private void Awake()
		{
			layerMask = LayerMask.GetMask("World");
		}

		public override void Process(ref Vector3 cameraPosition)
		{
			float smallestSqDist = float.MaxValue;

			foreach(var collider in distanceTo)
			{
				var point = collider.ClosestPoint(cameraPosition);

				float sqDist = Vector3.SqrMagnitude(point - cameraPosition);

				if (raycast)
				{
					var dir = collider.transform.position - cameraPosition;

					if (Physics.Raycast(cameraPosition, dir.normalized, dir.magnitude, layerMask))
					{
						sqDist = float.MaxValue;
					}

					if (Physics.Raycast(collider.transform.position, -dir.normalized, dir.magnitude, layerMask))
					{
						sqDist = float.MaxValue;
					}
				}

				smallestSqDist = Mathf.Min(smallestSqDist, sqDist);
			}

			float distance = Mathf.Sqrt(smallestSqDist);

			float influence = Mathf.Clamp01(distance / fadeDistance);
			influence = fadeFalloff.Evaluate(influence);

			foreach (var effect in audioAffects)
			{
				effect.Affect(influence);
			}
		}

		public override void OnExit(AudioTrigger trigger)
		{
			foreach(var affector in audioAffects)
			{
				trigger.DisableAffect(affector);
			}
		}
	}
}