using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using System;

namespace IPTech.Audio
{
    [CreateAssetMenu(menuName ="IPTech/Audio/Engine Config")]
    public class AudioEngineConfig : ScriptableObject
    {
        public List<AudioClipsCollection> AudioClips;

		public T Instantiate<T>(AudioEngine engine) {
			Type t = Type.GetType(MakeSafeCodeName(name));
			return (T)Activator.CreateInstance(t, engine, this);
		}

		public static string MakeSafeCodeName(string name) {
			return name.Replace(" ", "");
		}
	}
}
