using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Rendering;

namespace YL2.Audio
{
	public class AudioAffectAudioMixerParameter : AudioAffect
	{
		public AudioMixer audioMixer;
		public string paramName;

		public float multiplier;

		public override void Affect(float influence, bool remap = true)
		{
			influence = Remap(influence, remap);

			audioMixer.SetFloat(paramName, influence * multiplier);
		}
	}
}