using System;
using UnityEngine;

namespace IPTech.Audio {
	[Serializable]
	public struct AudioHandle {
		static int counter = 0;

		readonly AudioEngine engine;
		public readonly int ID;

		public AudioHandle(AudioEngine engine) {
			ID = counter++;
			this.engine = engine;
		}

		public bool isPlaying => engine != null ? engine.IsPlaying(this) : false;
	}

	interface IAudioEngine {
		AudioHandle Play(AudioClipsCollection audioClipsCollection, bool looping = false);
		void Stop(AudioHandle handle);
		void StopAll();
		bool IsPlaying(AudioHandle handle);
	}
}
