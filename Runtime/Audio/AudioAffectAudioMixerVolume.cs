using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Rendering;

namespace YL2.Audio
{
	public class AudioAffectAudioMixerVolume : AudioAffect
	{
		public AudioMixer audioMixer;
		public string paramName;

		public override void Affect(float influence, bool remap = true)
		{
			float dB = ToVolume(Remap(influence, remap));

			audioMixer.SetFloat(paramName, dB);
		}
	}
}