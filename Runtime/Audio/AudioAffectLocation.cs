using UnityEngine;

namespace YL2.Audio
{
	public class AudioAffectLocation : AudioAffect
	{
		public AudioSource affectedAudio;
		public Transform targetDestination;

		private Vector3 startLocation;

		private void Start()
		{
			startLocation = affectedAudio.transform.position;
		}

		public override void Affect(float influence, bool remap = true)
		{
			affectedAudio.transform.position = Vector3.Lerp(targetDestination.position, startLocation, influence);
		}
	}
}