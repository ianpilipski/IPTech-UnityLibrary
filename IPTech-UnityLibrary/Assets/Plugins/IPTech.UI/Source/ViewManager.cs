using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IPTech.UI {
	public interface IViewController {
		void RegisterView(IView view);
	}

	public static class IViewExtensionMethods {
		public static T FindViewController<T>(this IView view) where T : IViewController {
			MonoBehaviour mb = view as MonoBehaviour;
			if(mb!=null) {
				return ViewManagerLocator.Locate(mb).FindViewController<T>(view);
			}
			throw new NotSupportedException("Can not use this extension method for non-monobehaviour views");
		}
	}

	public interface IViewManager {
		void RegisterView(IView view);
		T FindViewController<T>(IView view) where T : IViewController;
	}

	public class ViewManagerLocator : IDisposable {
		static ViewManagerLocator _inst;
		bool _alreadyDisposed;
		Func<MonoBehaviour, IViewManager> _locator;

  		public static IViewManager Locate(MonoBehaviour monoBehaviour) {
			if(_inst==null) {
				throw new Exception("You must construct a ViewManager class, or supply a Locator function in ViewManagerLocator to return one.");
			}
			return _inst._locator(monoBehaviour);
		}

		public static bool HasLocator { get { return _inst != null; } }

		public static ViewManagerLocator SetLocator(Func<MonoBehaviour, IViewManager> locator) {
			_inst = new ViewManagerLocator() { _locator = locator };
			return _inst;
		}

		public void Dispose() {
			if(!_alreadyDisposed) {
				_alreadyDisposed = true;
				if(_inst == this) {
					_inst = null;
				}
			}
		}
	}

	public class ViewManager : IViewManager, IDisposable {
		readonly IDictionary<IView, RegistrationContext> _viewControllers;
		readonly IList<ViewMapping> _viewMappings;
		readonly IViewManager _parentViewManager;
		ViewManagerLocator _locator;

		public class ViewMapping {
			public string ViewId;
			public Func<IViewController> CreateController;
		}

		public ViewManager(IList<ViewMapping> viewMappings, IViewManager parentViewManager) {
			_viewControllers = new Dictionary<IView, RegistrationContext>();
			_viewMappings = viewMappings;
			_parentViewManager = parentViewManager;

			if(!ViewManagerLocator.HasLocator) {
				_locator = ViewManagerLocator.SetLocator((mb) => this);
			}
		}

		public void RegisterView(IView view) {
			if(HasViewMapping(view)) {
				var regContext = GetRegistrationContext(view);
				regContext.RegisterView(view);
			} else {
				if(_parentViewManager!=null) {
					_parentViewManager.RegisterView(view);
				} else {
					view.AwakeView();
				}
			}
		}

		public T FindViewController<T>(IView view) where T : IViewController {
			if(HasViewMapping(view) || _parentViewManager == null) {
				var registrationContext = GetRegistrationContext(view);
				registrationContext.PreRegisterView(view);
				return (T)registrationContext.Controller;
			}

			return _parentViewManager.FindViewController<T>(view);
		}

		RegistrationContext GetRegistrationContext(IView view) {
			if(!_viewControllers.TryGetValue(view, out RegistrationContext registrationContext)) {
				registrationContext = CreateRegistrationContext(view);
			}
			return registrationContext;
		}

		RegistrationContext CreateRegistrationContext(IView view) {
			TryCreateControllerForView(out IViewController controller);
			RegistrationContext registrationContext = new RegistrationContext(controller);
			view.ViewDestroyed += HandleViewDestroyed;
			_viewControllers.Add(view, registrationContext);
			return registrationContext;

			bool TryCreateControllerForView(out IViewController createdController) {
				if(TryGetViewMapping(view, out ViewMapping vm)) { 
					createdController = vm.CreateController();
					return true;
				}
				createdController = null;
				return false;
			}
		}

		bool HasViewMapping(IView view) {
			return TryGetViewMapping(view, out ViewMapping _);
		}

		bool TryGetViewMapping(IView view, out ViewMapping viewMapping) {
			foreach(var vm in _viewMappings) {
				if(vm.ViewId == view.ViewId) {
					viewMapping = vm;
					return true;
				}
			}
			viewMapping = null;
			return false;
		}

		void HandleViewDestroyed(IView view) {
			view.ViewDestroyed -= HandleViewDestroyed;
			_viewControllers.Remove(view);
		}

		public void Dispose() {
			if(_locator != null) {
				_locator.Dispose();
				_locator = null;
			}
		}

		class RegistrationContext {
			public readonly IViewController Controller;
			public bool IsRegistered;
			bool IsPreRegistered;

			public RegistrationContext(IViewController controller) {
				Controller = controller;
			}

			public void RegisterView(IView view) {
				if(IsRegistered) {
					throw new InvalidOperationException("View has already been regsitered.");
				}
				IsRegistered = true;
				PreRegisterView(view);
			}

			public void PreRegisterView(IView view) {
				if(!IsPreRegistered) {
					IsPreRegistered = true;
					if(Controller != null) {
						view.AwakeView();
						Controller.RegisterView(view);
					}
				}
			}
		}
	}
}
