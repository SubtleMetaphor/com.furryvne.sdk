using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YL2.Audio
{
	public class AudioProcessorPlaceAudioSourceAtClosestPoint : AudioProcessor
	{
		public AudioSource audioSource;
		public Collider closestPoint;

		public override void Process(ref Vector3 cameraPosition)
		{
			var point = closestPoint.ClosestPoint(cameraPosition);

			audioSource.transform.position = point;
		}

		public override void OnExit(AudioTrigger trigger) { }
	}
}