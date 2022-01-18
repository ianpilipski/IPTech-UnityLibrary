using UnityEngine;

namespace IPTech.Timeline {
	public interface IDialogueHandle {
		bool IsComplete { get; }
		void Dismiss();
	}

	public abstract class DialogueTriggerDriver : MonoBehaviour {

		public abstract IDialogueHandle BeginNewDialogue(string dialogId);

		protected class DummyDialogueHandle : IDialogueHandle {
			public bool IsComplete {
				get;
				private set;
			}

			public void Dismiss() {
				IsComplete = true;
			}
		}
	}
}
