using IPTech.DialogManager.Strange.Api;
using strange.extensions.context.impl;
using UnityEngine;

namespace IPTech.DialogManager.Strange
{
	class DialogManagerBootstrap : ContextView
	{
		public GameObject templateDialogWindowBasicView;

		public void Start() {
			this.context = new DialogManagerContext(this, GetTemplateDialogWindowBasicView());
		}

		private IDialogWindowView GetTemplateDialogWindowBasicView() {
			if (this.templateDialogWindowBasicView != null) {
				return this.templateDialogWindowBasicView.GetComponent<IDialogWindowView>();
			}
			return null;
		}

#if UNITY_EDITOR
		protected void OnValidate() {
			if (GetTemplateDialogWindowBasicView() == null) {
				this.templateDialogWindowBasicView = null;
			}
		}
#endif
	}
}
