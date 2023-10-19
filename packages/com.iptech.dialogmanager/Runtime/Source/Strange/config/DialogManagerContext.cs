using IPTech.DialogManager.Api;
using IPTech.DialogManager.Strange.Api;
using strange.extensions.context.impl;
using UnityEngine;

namespace IPTech.DialogManager.Strange
{
	public class DialogManagerContext : SignalContext
	{
		public IDialogWindowView templateDialogWindowBasicView { get; set; }

		public DialogManagerContext(MonoBehaviour contextView, IDialogWindowView templateDialogWindowBasicView) : base(contextView) {
			this.templateDialogWindowBasicView = templateDialogWindowBasicView;
			Start();
		}

		protected override void mapBindings() {
			base.mapBindings();

			if (Context.firstContext == this) {
				
			}

			if (!IsBound<ShowDialogSignal>()) {
				injectionBinder.Bind<ShowDialogSignal>().ToSingleton().CrossContext();
			}
			
			injectionBinder.Bind<IDialogManager>().To<DialogManagerBasic>().ToSingleton();
			injectionBinder.Bind<IDialogFactory>().To<DialogFactory>().ToSingleton();
			injectionBinder.Bind<StartSignal>().ToSingleton();
			Debug.Assert(this.templateDialogWindowBasicView != null);
			injectionBinder.Bind<IDialogWindowView>().SetValue(this.templateDialogWindowBasicView).ToName("BASICDIALOG");

			mediationBinder.Bind<IDialogWindowView>().To<DialogWindowMediator>();

			commandBinder.Bind<ShowDialogSignal>().To<ShowDialogWindowCommand>();
		}

		private bool IsBound<T>() {
			return injectionBinder.GetBinding<T>() != null;
		}
		protected override void postBindings() {
			base.postBindings();
		}
	}
}
