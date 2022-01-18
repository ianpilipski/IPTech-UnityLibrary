using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

namespace IPTech.Timeline {
	public class DialogueTriggerMixerBehaviour : PlayableBehaviour {
		Playable _playable;

		IDialogueHandle _currentDialogueHandle;
		DialogueTriggerDriver _dialogueDriver;

		DialogueTriggerBehaviour _previousBehaviour;

		public static bool DebugDismissDialog;
		public static double LastRelativeTime;

		public override void OnPlayableCreate(Playable playable) {
			base.OnPlayableCreate(playable);
			_playable = playable;
		}

		public override void ProcessFrame(Playable playable, FrameData info, object playerData) {
			DialogueTriggerDriver driver = playerData as DialogueTriggerDriver;

			if(driver == null) return;

			if(!playable.GetGraph().IsPlaying()) {
				//Scrubbing so clear all
				ClearCurrentDialogue();
			}

			int inputCount = playable.GetInputCount();

			for(int i = 0; i < inputCount; i++) {
				float inputWeight = playable.GetInputWeight(i);
				if(inputWeight > 0) {
					ScriptPlayable<DialogueTriggerBehaviour> inputPlayable = (ScriptPlayable<DialogueTriggerBehaviour>)playable.GetInput(i);
					DialogueTriggerBehaviour b = inputPlayable.GetBehaviour();
					if(_previousBehaviour != null && _previousBehaviour!=b) {
						ProcessBehaviour(playerData);
						if(_previousBehaviour != null) return;
					}
					_previousBehaviour = b;
					break;
				}
			}
			if(_previousBehaviour != null) {
				ProcessBehaviour(playerData);
			}
		}

		void ProcessBehaviour(object playerData) {
			Playable rootPlayable = _playable.GetGraph().GetRootPlayable(0);

			double time = rootPlayable.GetTime();

			if(_previousBehaviour.IsInClipTimeRange(time)) {
				if(!HasAlreadyShownDialogue()) {
					StartDialogue(playerData);
				}
				if(IsCurrentDialogueCompleted()) {
					if(_previousBehaviour.Behaviour == DialogueTriggerBehaviour.TriggerType.Loop) {
						JumpToEndofPlayable();
					} else {
						UnPausePlayable();
					}
				} else if(_previousBehaviour.Behaviour == DialogueTriggerBehaviour.TriggerType.PauseAtStart) {
					PausePlayable();
				}
			} else {
				if(!IsCurrentDialogueCompleted() && !_previousBehaviour.IsPriorToClipEnd(time)) {
					if(_previousBehaviour.Behaviour == DialogueTriggerBehaviour.TriggerType.Loop) {
						JumpToLoopStartofPlayable();
					} else {
						JumpToEndofPlayable();
						PausePlayable();
					}
				} else {
					ClearCurrentDialogue();
					UnPausePlayable();
					_previousBehaviour = null;
				}
			}
		}

		void SetDialogueDriverFromPlayerData(object playerData) {
			_dialogueDriver = playerData as DialogueTriggerDriver;
		}

		void StartDialogue(object playerData) {
			SetDialogueDriverFromPlayerData(playerData);
			if(HasDialogueDriver()) {
				_currentDialogueHandle = _dialogueDriver.BeginNewDialogue(_previousBehaviour.DialogId);
			}
		}

		bool HasDialogueDriver() {
			return _dialogueDriver != null;
		}

		bool HasAlreadyShownDialogue() {
			return _currentDialogueHandle != null;
		}

		bool IsCurrentDialogueCompleted() {
			if(DebugDismissDialog) {
				return true;
			}

			return _currentDialogueHandle == null || _currentDialogueHandle.IsComplete;
		}

		void ClearCurrentDialogue() {
			if(_currentDialogueHandle != null) {
				_currentDialogueHandle.Dismiss();
			}
			_currentDialogueHandle = null;
			DebugDismissDialog = false;
		}

		void PausePlayable() {
			_playable.GetGraph().GetRootPlayable(0).SetSpeed(0);
		}

		void UnPausePlayable() {
			_playable.GetGraph().GetRootPlayable(0).SetSpeed(1);
		}

		void JumpToEndofPlayable() {
			_playable.GetGraph().GetRootPlayable(0).SetTime(_previousBehaviour.ClipEnd);
		}

		void JumpToLoopStartofPlayable() {
			if(Application.isPlaying) {
				PlayableDirector pd = _playable.GetGraph().GetResolver() as PlayableDirector;
				pd.time = _previousBehaviour.ClipStart + _previousBehaviour.LoopOffset;
			} else {
				_playable.GetGraph().GetRootPlayable(0).SetTime(_previousBehaviour.ClipStart + _previousBehaviour.LoopOffset);
			}
		}
	}
}
