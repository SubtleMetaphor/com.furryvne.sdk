using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace YL2.Audio
{
	public abstract class AudioHandler : MonoBehaviour
	{
		public AudioProcessor[] processors = new AudioProcessor[0];
		public AudioAffect[] continuousAffectors = new AudioAffect[0];

		public void ProcessAudio()
		{
			var cameraPos = Camera.main.transform.position;

			foreach (var affector in continuousAffectors)
			{
				affector.Affect(1f);
			}

			foreach (var processor in processors)
			{
				processor.Process(ref cameraPos);
			}
		}

		public bool Has(AudioProcessor processor)
		{
			return processors.Contains(processor);
		}
	}
}