using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YL2.Audio
{
    [DefaultExecutionOrder(10)] // Run after AudioGlobal.
    public class AudioTriggerHandler : MonoBehaviour
    {
        public static AudioTriggerHandler Instance { get; private set; }

        private List<AudioTrigger> triggers = new List<AudioTrigger>();

		private void Awake()
		{
			if (Instance != null)
			{
                gameObject.SetActive(false);
                return;
			}

			Instance = this;

            GameObject.DontDestroyOnLoad(gameObject);
		}

        public void Add(AudioTrigger trigger)
        {
			if (!triggers.Contains(trigger))
			{
			    triggers.Add(trigger);
                triggers.Sort((a, b) => a.order.CompareTo(b.order));
			}
		}

        public void Remove(AudioTrigger trigger)
        {
            triggers.Remove(trigger);
        }

		private void Update()
        {
            foreach (AudioTrigger trigger in triggers)
            {
                trigger.ProcessAudio();
            }
        }
    }
}