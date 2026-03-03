using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace YL2.Audio
{
	public abstract class AudioProcessor : MonoBehaviour
	{
		public abstract void Process(ref Vector3 cameraPosition);
		public abstract void OnExit(AudioTrigger trigger);
	}
}