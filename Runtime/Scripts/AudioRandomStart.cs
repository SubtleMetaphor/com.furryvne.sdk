using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AudioRandomStart : MonoBehaviour
{
	public bool specificTime = false;
	public float startTime = 0f;

	void Start()
	{
		var audio = GetComponent<AudioSource>();
		var clip = audio.clip;
		float randomStartTime = Random.Range(0f, clip.length);

		audio.Stop();
		audio.time = specificTime ? startTime * clip.length : randomStartTime;
		audio.Play();
	}
}
