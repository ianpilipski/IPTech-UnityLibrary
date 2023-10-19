using UnityEngine.Timeline;
using UnityEngine;
using UnityEngine.Playables;
using System.Reflection;

namespace IPTech.Timeline {

	[TrackColor(0.9716981f, 0.4145757f, 0.3529281f)]
	[TrackClipType(typeof(DialogueTriggerClip))]
	[TrackBindingType(typeof(DialogueTriggerDriver))]
	public class DialogueTriggerTrack : TrackAsset {
		public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount) {
			return ScriptPlayable<DialogueTriggerMixerBehaviour>.Create(graph, inputCount);
		}

		protected override void OnCreateClip(TimelineClip clip) {
			base.OnCreateClip(clip);
			foreach(var f in clip.GetType().GetFields(System.Reflection.BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance)) {
				Debug.Log(f.Name);
			}
			FieldInfo fi = clip.GetType().GetField("m_PostExtrapolationMode", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
			if(fi!=null) {
				fi.SetValue(clip, TimelineClip.ClipExtrapolation.Loop);
			} else {
				Debug.LogError("Could not find property private ClipExtrapolation m_PostExtrapolationMode");
			}
		}

		protected override Playable CreatePlayable(PlayableGraph graph, GameObject gameObject, TimelineClip clip) {
			var p = base.CreatePlayable(graph, gameObject, clip);
			if(typeof(DialogueTriggerBehaviour).IsAssignableFrom(p.GetPlayableType())) {
				var sp = (ScriptPlayable<DialogueTriggerBehaviour>)p;
				sp.GetBehaviour().SetClipInfo(clip);
			}
			return p;
		}
	}
}
