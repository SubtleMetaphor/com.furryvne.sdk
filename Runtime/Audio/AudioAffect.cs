using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace YL2.Audio
{
    public abstract class AudioAffect : MonoBehaviour
    {
		[Range(0f, 1f)]
		public float min = 0f;

		[Range(0f, 1f)]
		public float max = 1f;

		public bool set0InfluenceOnExit = false;

		public string affectName;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected float Remap(float influence, bool remap)
		{
			return remap ? Mathf.Lerp(min, max, influence) : influence;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected static float ToVolume(float influence)
		{
			return Mathf.Log10(Mathf.Max(influence, 1e-4f)) * 20f; // Convert linear to dB.
		}

		public abstract void Affect(float influence, bool remap = true);

		public override string ToString()
		{
			return string.IsNullOrEmpty(affectName) ? base.ToString() : affectName;
		}
	}
}