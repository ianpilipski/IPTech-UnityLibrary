using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System;

namespace IPTech.Audio.EditorExtensions {
	[CustomEditor(typeof(AudioEngineConfig))]	
    public class AudioEngineConfigEditor : Editor {
		static AudioEngine audioEngine;
		Vector2 scrollPos;

		Dictionary<string, AudioHandle> playingAudio = new Dictionary<string, AudioHandle>();

        private void OnEnable() {
            if(audioEngine == null) {
				var allAudioEngines = Resources.FindObjectsOfTypeAll<AudioEngineEditorPlayer>();
				audioEngine = allAudioEngines != null && allAudioEngines.Length > 0 ? allAudioEngines[0] : null;
				if(audioEngine == null) {
					audioEngine = AudioEngine.Create<AudioEngineEditorPlayer>();
					audioEngine.gameObject.name = "AudioEngine (Editor)";
					var go = audioEngine.gameObject;
					go.hideFlags = HideFlags.HideAndDontSave;
				}
            }
        }

     
        public override void OnInspectorGUI() {
			base.OnInspectorGUI();

			if(GUILayout.Button("Generate API For Config")) {
				GenerateApi((AudioEngineConfig)serializedObject.targetObject);
			}
			
			OnGUIForTesting();
		}

        private void OnGUIForTesting() {
			if(GUILayout.Button("Stop All Sounds")) {
				audioEngine.StopAll();
            }
			AudioEngineConfig config = this.target as AudioEngineConfig;
			if(config!=null) {
				if(config.AudioClips != null) {
					using(var scroll = new EditorGUILayout.ScrollViewScope(scrollPos)) {
						scrollPos = scroll.scrollPosition;
						foreach(var acc in config.AudioClips) {
							if(acc != null) {
								var mixerName = acc.AudioMixer != null ? acc.AudioMixer.name : "(no audio mixer assigned)";
								EditorGUILayout.LabelField(acc.name + " : " + mixerName);
								using(new EditorGUI.IndentLevelScope()) {
									if(acc.AudioClips != null && acc.AudioClips.Length > 0) {
										foreach(var ac in acc.AudioClips) {
											var key = $"{acc.name}:${ac.name}";

											using(new EditorGUILayout.HorizontalScope()) {
												if(playingAudio.TryGetValue(key, out AudioHandle handle)) {
													if(handle.isPlaying) {
														if(GUILayout.Button("Stop")) {
															audioEngine.Stop(handle);
															playingAudio.Remove(key);
														}
														EditorApplication.delayCall += Repaint;
													} else {
														playingAudio.Remove(key);
														DrawPlayButton(key, acc, ac);
													}
												} else {
													DrawPlayButton(key, acc, ac);
												}
												GUILayout.Label(ac.name);
											}
										}
									} else {
										GUILayout.Label("(empty)");
									}
								}
							}
						}
					}
				}
            }

			void DrawPlayButton(string key, AudioClipsCollection acc, AudioClip ac) {
				if(GUILayout.Button("Play")) {
					var h = audioEngine.Play(ac, false, acc.AudioMixer);
					playingAudio.Add(key, h);
				}
				if(GUILayout.Button("Loop")) {
					var h = audioEngine.Play(ac, true, acc.AudioMixer);
					playingAudio.Add(key, h);
                }
			}
        }

        public static void GenerateApi(AudioEngineConfig config) {
			var acg = new AudioConfigGenerator(config);
			string newClass = acg.TransformText();

			string assetPath = AssetDatabase.GetAssetPath(config);
			string generatedPath = Path.Combine(
				Path.GetDirectoryName(assetPath),
				Path.GetFileNameWithoutExtension(assetPath) + ".Generated.cs"
			);
			File.WriteAllText(generatedPath, newClass);

			AssetDatabase.ImportAsset(generatedPath);
		}
	}
}
