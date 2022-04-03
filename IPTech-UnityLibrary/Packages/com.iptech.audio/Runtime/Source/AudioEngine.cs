using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using Object = UnityEngine.Object;

namespace IPTech.Audio {
	public class AudioEngine : IAudioEngine, IDisposable {
		IDictionary<AudioSource, int> sourcepool;
		Stack<AudioSource> freeSources;

		GameObject gameObject;
		bool alreadyDisposed;

		public AudioEngine() {
			sourcepool = new Dictionary<AudioSource, int>();
			freeSources = new Stack<AudioSource>();
			gameObject = new GameObject("AudioEngine");
			Object.DontDestroyOnLoad(gameObject);
			
		}

		public AudioHandle Play(AudioClipsCollection audioClipsCollection, bool looping = false) {
			return CreateSource(audioClipsCollection, looping);
		}

		AudioHandle CreateSource(AudioClipsCollection audioclipscollection, bool looping) {
			return CreateSource(audioclipscollection.GetRandom(), looping, audioclipscollection.AudioMixer);
		}

		public void Stop(AudioHandle handle) {
			if(sourcepool.TryGetValue(handle.source, out int id)) {
				if(id == handle.ID) {
					handle.source.Stop();
					freeSources.Push(handle.source);
				}
			}
		}

		AudioHandle CreateSource(AudioClip clip, bool looping, AudioMixerGroup mixergroup = null) {
			AudioSource source = null;
			AudioHandle audioHandle;
			if (freeSources.Count > 0) {
				source = freeSources.Pop();
			} else {
				foreach (AudioSource s in sourcepool.Keys) {
					if (!s.isPlaying) {
						source = s;
						break;
					}
				}
			}

			if (source == null) {
				source = gameObject.AddComponent<AudioSource>();
				audioHandle = new AudioHandle(source);
				sourcepool.Add(source, audioHandle.ID);
			} else {
				audioHandle = new AudioHandle(source);
				sourcepool[source] = audioHandle.ID;
			}

			source.volume = 1.0f;
			source.clip = clip;
			source.loop = looping;
			source.outputAudioMixerGroup = mixergroup;
			source.Play();
			return audioHandle;
		}

		void IDisposable.Dispose() {
			if(!alreadyDisposed) {
				alreadyDisposed = true;
				if(gameObject!=null) {
					Object.Destroy(gameObject);
					gameObject = null;
				}
			}
		}
	}
}