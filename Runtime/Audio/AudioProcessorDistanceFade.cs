using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace YL2.Audio
{
    public class AudioProcessorDistanceFade : AudioProcessor
    {
		[Range(1f, 30f)]
		public float fadeDistance = 10f;

		public AnimationCurve fadeFalloff = new AnimationCurve(new Keyframe(0f, 1f, -1f, -1f), new Keyframe(1f, 0f, -1f, -1f));
		public AudioAffect[] affects = new AudioAffect[0];

		public override void Process(ref Vector3 cameraPosition)
		{
			float distance = Vector3.Distance(cameraPosition, transform.position);
			float ratio = Mathf.Clamp01(distance / fadeDistance);
			ratio = fadeFalloff.Evaluate(ratio);

			foreach(var affect in affects)
			{
				affect.Affect(ratio);
			}
		}

		public override void OnExit(AudioTrigger trigger)
		{
			foreach (var affect in affects.Where(x => x.set0InfluenceOnExit))
			{
				affect.Affect(0f, false);
			}
		}
	}
}