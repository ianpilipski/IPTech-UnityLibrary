using UnityEngine;

namespace IPTech.Audio {
	public struct AudioHandle {
		static int counter = 0;

		public readonly AudioSource source;
		public readonly int ID;

		public AudioHandle(AudioSource source) {
			ID = counter++;
			this.source = source;
		}
	}

	interface IAudioEngine {
		AudioHandle Play(AudioClipsCollection audioClipsCollection, bool looping = false);
		void Stop(AudioHandle source);
	}
}
