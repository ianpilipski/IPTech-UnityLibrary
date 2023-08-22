using strange.extensions.command.api;
using strange.extensions.command.impl;
using strange.extensions.context.api;
using strange.extensions.context.impl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace IPTech.DialogManager.Strange
{
	public class SignalContext : MVCSContext
	{
		public SignalContext(MonoBehaviour contextView) : base(contextView, ContextStartupFlags.MANUAL_MAPPING) {
		}

		protected override void addCoreComponents() {
			base.addCoreComponents();
			injectionBinder.Unbind<ICommandBinder>();
			injectionBinder.Bind<ICommandBinder>().To<SignalCommandBinder>().ToSingleton();
		}

		public override void Launch() {
			base.Launch();
			StartSignal startSignal = (StartSignal)injectionBinder.GetInstance<StartSignal>();
			startSignal.Dispatch();
		}

		protected override void mapBindings() {
			base.mapBindings();
		}
	}
}
