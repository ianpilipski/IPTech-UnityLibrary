using System;
using strange.extensions.injector.api;
using strange.extensions.iptech.context.api;
using strange.extensions.context.api;
using strange.extensions.mediation.api;

namespace strange.extensions.iptech.context.impl
{
	class ContextViewAdapter : IContextViewAdapter
	{
		private IContextView GetIContextView(object contextView) {
			IContextView contView = contextView as IContextView;
			if (contView == null) {
				throw new Exception("contextView must be IContextView");
			}
			return contView;
		}

		public void BindContextViewAdapter(object contextView, IInjectionBinder injectionBinder) {
			IContextView iContextView = GetIContextView(contextView);
			injectionBinder.Bind<IContextView>().ToValue(iContextView).ToName(ContextKeys.CONTEXT_VIEW);
		}

		public IView GetIViewFromContextView(object contextView) {
			return GetIContextView(contextView) as IView;
		}
	}
}
