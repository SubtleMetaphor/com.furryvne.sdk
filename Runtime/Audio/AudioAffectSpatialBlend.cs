using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Rendering;

namespace YL2.Audio
{
	public class AudioAffectSpatialBlend : AudioAffect
	{
		public AudioSource audioSource;

		public override void Affect(float influence, bool remap = true)
		{
			audioSource.spatialBlend = Remap(influence, remap);
		}
	}
}