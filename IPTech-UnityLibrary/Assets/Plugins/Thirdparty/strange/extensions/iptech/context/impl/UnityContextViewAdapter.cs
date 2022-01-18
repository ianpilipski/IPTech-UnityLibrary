using UnityEngine;
using strange.extensions.context.api;
using strange.extensions.context.impl;
using strange.extensions.injector.api;
using strange.extensions.iptech.context.api;
using strange.extensions.mediation.api;
using System;

namespace strange.extensions.iptech.context.impl
{
	public class UnityContextViewAdapter : IContextViewAdapter
	{
		private MonoBehaviour GetMonoBehaviourFromContextView(object contextView) {
			MonoBehaviour mono = contextView as MonoBehaviour;
			if (mono == null) {
				throw new ContextException("This MVCSContext requires a ContextView of type MonoBehaviour", ContextExceptionType.NO_CONTEXT_VIEW);
			}
			return mono;
		}
		public void BindContextViewAdapter(object contextView, IInjectionBinder injectionBinder) {
			MonoBehaviour mono = GetMonoBehaviourFromContextView(contextView);
			injectionBinder.Bind<GameObject>().ToValue(mono.gameObject).ToName(ContextKeys.CONTEXT_VIEW);
		}

		public IView GetIViewFromContextView(object contextView) {
			return GetMonoBehaviourFromContextView(contextView).GetComponent<IView>();
		}
	}
}
