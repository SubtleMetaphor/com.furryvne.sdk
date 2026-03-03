using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace YL2.Audio
{
	public class AudioTrigger : AudioHandler
	{
		public bool Active { private set; get; }

		public int order = 0;

		public AudioAffect[] onExitAudioAffects = new AudioAffect[0];

		private void OnTriggerEnter(Collider other)
		{
			if (other.CompareTag("MainCamera"))
			{
				Active = true;

				AudioTriggerHandler.Instance.Add(this);
			}
		}

		private void OnTriggerExit(Collider other)
		{
			if (other.CompareTag("MainCamera"))
			{
				Active = false;
				AudioTriggerHandler.Instance.Remove(this);

				foreach(var affect in onExitAudioAffects)
				{
					DisableAffect(affect);
				}

				foreach(var affect in continuousAffectors.Where(x => x.set0InfluenceOnExit))
				{
					DisableAffect(affect);
				}

				foreach(var processor in processors)
				{
					processor.OnExit(this);
				}
			}
		}

		public void DisableAffect(AudioAffect affect)
		{
			StartCoroutine(DisableAffectCoroutine(affect));
		}

		private IEnumerator DisableAffectCoroutine(AudioAffect affect)
		{
			yield return new WaitForEndOfFrame();

			affect.Affect(0f, false);
		}

		private void OnDisable()
		{
			if (Active)
			{
				AudioTriggerHandler.Instance.Remove(this);
				Active = false;
			}
		}
	}
}