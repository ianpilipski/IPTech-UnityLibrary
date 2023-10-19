using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace IPTech.Timeline
{
	// A behaviour that is attached to a playable
	[Serializable]
	public class DialogueTriggerBehaviour : PlayableBehaviour
	{
		TimelineClip _clip;
		Playable _playable;

		public enum TriggerType {
			Loop,
			PauseAtStart,
			PauseAtEnd
		}

		public string DialogId;
		public TriggerType Behaviour = TriggerType.PauseAtEnd;
		public double LoopOffset;

		public double ClipStart {
			get { return _clip.start; }
		}

		public double ClipEnd {
			get { return _clip.end; }
		}

		public void SetClipInfo(TimelineClip clip) {
			_clip = clip;
			_clip.displayName = DialogId;
		}

		public override void OnPlayableCreate(Playable playable)
		{
			_playable = playable;
		}

		public bool IsPriorToClipEnd(double time) {
			return time < _clip.end;
		}

		public bool IsInClipTimeRange(double time) {
			return time >= _clip.start && time < _clip.end;
		}
	}
}