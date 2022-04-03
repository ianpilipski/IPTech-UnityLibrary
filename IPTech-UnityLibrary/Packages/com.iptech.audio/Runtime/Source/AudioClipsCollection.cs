using UnityEngine;
using UnityEngine.Audio;
using System.Collections;

namespace IPTech.Audio {
	[CreateAssetMenu(fileName = "AudioClipsCollection", menuName = "IPTech/Audio/Create ClipsCollection")]
	public class AudioClipsCollection : ScriptableObject {
		public AudioMixerGroup AudioMixer;
		public AudioClip[] AudioClips;

		public AudioClip GetRandom() {
			int index = Random.Range(0, AudioClips.Length - 1);
			return AudioClips[index];
		}

		private void OnValidate() {
			if(AudioClips == null || AudioClips.Length==0) {
				Debug.LogError("AudioClips can not be null or empty.");
			}
		}
	}
}
