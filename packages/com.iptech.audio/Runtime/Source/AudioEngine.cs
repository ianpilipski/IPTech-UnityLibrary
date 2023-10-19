using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using Object = UnityEngine.Object;

namespace IPTech.Audio {
	public class AudioEngine : MonoBehaviour, IAudioEngine, IDisposable {
		const int FREEID = -1;

		[SerializeField]
		AudioToIdDict sourcepool = new AudioToIdDict();
		[SerializeField]
		IdToAudioDict idToAudioMap = new IdToAudioDict();

		[Serializable] class AudioToIdDict : SerializedDict<AudioSource, int> { }
		[Serializable] class IdToAudioDict : SerializedDict<int, AudioSource> { }

        [Serializable]
        class SerializedDict<TKey,TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver {
			[SerializeField, HideInInspector]
			List<TKey> keys = new List<TKey>();
			[SerializeField, HideInInspector]
			List<TValue> values = new List<TValue>();

            public void OnAfterDeserialize() {
				this.Clear();
				if(keys.Count!=values.Count) {
					throw new Exception("bad data");
                }

				for(int i=0;i<keys.Count;i++) {
					this.Add(keys[i], values[i]);
                }
            }

            public void OnBeforeSerialize() {
				keys.Clear();
				values.Clear();
				foreach(var kvp in this) {
					keys.Add(kvp.Key);
					values.Add(kvp.Value);
                }
            }
        }


        bool alreadyDisposed;

		public static T Create<T>() where T : AudioEngine {
			var go = new GameObject("AudioEngine");
			var ae = go.AddComponent<T>();
			if(Application.isPlaying) {
				Object.DontDestroyOnLoad(go);
            }
			return ae;
        }

		public AudioHandle Play(AudioClipsCollection audioClipsCollection, bool looping = false) {
			return Play(audioClipsCollection.GetRandom(), looping, audioClipsCollection.AudioMixer);
		}

		public AudioHandle Play(AudioClip audioClip, bool looping = false, AudioMixerGroup mixerGroup = null) {
			return CreateSource(audioClip, looping, mixerGroup);
        }

		public void Stop(AudioHandle handle) {
			if(idToAudioMap.TryGetValue(handle.ID, out AudioSource source)) {
				source.Stop();
				sourcepool[source] = FREEID;
				idToAudioMap.Remove(handle.ID);
            }
		}

		public void StopAll() {
			var keys = new List<AudioSource>(sourcepool.Keys);
			foreach(var key in keys) {
				if(key.isPlaying) {
					key.Stop();
                }
				sourcepool[key] = FREEID;
            }
			idToAudioMap.Clear();
        }

		public bool IsPlaying(AudioHandle handle) {
			if(idToAudioMap.TryGetValue(handle.ID, out AudioSource source)) {
				return source.isPlaying;
            }
			return false;
        }

		AudioHandle CreateSource(AudioClip clip, bool looping, AudioMixerGroup mixergroup = null) {
			AudioSource source = null;
			
			foreach (AudioSource s in sourcepool.Keys) {
				if (!s.isPlaying || sourcepool[s] == FREEID) {
					source = s;
					idToAudioMap.Remove(sourcepool[source]);
					break;
				}
			}
			
			if (source == null) {
				source = gameObject.AddComponent<AudioSource>();
			}
			
			source.volume = 1.0f;
			source.clip = clip;
			source.loop = looping;
			source.outputAudioMixerGroup = mixergroup;
			source.Play();
			return CreateAudioHandle();

			AudioHandle CreateAudioHandle() {
				var audioHandle = new AudioHandle(this);
				sourcepool[source] = audioHandle.ID;
				idToAudioMap[audioHandle.ID] = source;
				return audioHandle;
			}
		}

		void IDisposable.Dispose() {
			if(!alreadyDisposed) {
				alreadyDisposed = true;
				if(gameObject!=null) {
					if(Application.isPlaying) {
						Destroy(gameObject);
					} else {
						DestroyImmediate(gameObject);
                    }
				}
			}
		}
	}
}