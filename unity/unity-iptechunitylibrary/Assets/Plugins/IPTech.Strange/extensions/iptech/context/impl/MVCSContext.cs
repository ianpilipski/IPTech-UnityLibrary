using strange.extensions.implicitBind.api;
using strange.extensions.implicitBind.impl;
using UnityEngine;
using strange.extensions.command.api;
using strange.extensions.command.impl;
using strange.extensions.context.api;
using strange.extensions.dispatcher.api;
using strange.extensions.dispatcher.eventdispatcher.api;
using strange.extensions.dispatcher.eventdispatcher.impl;
using strange.extensions.injector.api;
using strange.extensions.mediation.api;
using strange.extensions.mediation.impl;
using strange.extensions.sequencer.api;
using strange.extensions.sequencer.impl;
using strange.framework.api;
using strange.framework.impl;
using strange.extensions.context.impl;
using strange.extensions.iptech.context.api;

namespace strange.extensions.iptech.context.impl
{
	class MVCSContext : MVCSContextBase
	{
		public MVCSContext() : base() { }

		/// The recommended Constructor
		/// Just pass in the instance of your ContextView. Everything will begin automatically.
		/// Other constructors offer the option of interrupting startup at useful moments.
		public MVCSContext(object view) : base(view) {
		}

		public MVCSContext(object view, ContextStartupFlags flags) : base(view, flags) {
		}

		public MVCSContext(object view, bool autoMapping) : base(view, autoMapping) {
		}

		protected override void addCoreComponents() {
			base.addCoreComponents();
			injectionBinder.Unbind<IContextViewAdapter>();
			injectionBinder.Bind<IContextViewAdapter>().To<UnityContextViewAdapter>().ToSingleton();
		}
	}
}
