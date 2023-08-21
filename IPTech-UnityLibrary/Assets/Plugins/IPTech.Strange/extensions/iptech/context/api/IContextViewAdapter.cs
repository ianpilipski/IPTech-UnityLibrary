
using strange.extensions.injector.api;
using strange.extensions.mediation.api;

namespace strange.extensions.iptech.context.api
{
	public interface IContextViewAdapter
	{
		void BindContextViewAdapter(object contextView, IInjectionBinder injectionBinder);
		IView GetIViewFromContextView(object contextView);
	}
}
