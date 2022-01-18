/*
 * Copyright 2013 ThirdMotion, Inc.
 *
 *	Licensed under the Apache License, Version 2.0 (the "License");
 *	you may not use this file except in compliance with the License.
 *	You may obtain a copy of the License at
 *
 *		http://www.apache.org/licenses/LICENSE-2.0
 *
 *		Unless required by applicable law or agreed to in writing, software
 *		distributed under the License is distributed on an "AS IS" BASIS,
 *		WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 *		See the License for the specific language governing permissions and
 *		limitations under the License.
 */

/**
 * @class strange.extensions.mediation.impl.MediationBinder
 * 
 * Binds Views to Mediators.
 * 
 * Please read strange.extensions.mediation.api.IMediationBinder
 * where I've extensively explained the purpose of View mediation
 */

using System;
using System.Collections;
using UnityEngine;
using strange.extensions.injector.api;
using strange.extensions.mediation.api;
using strange.framework.api;
using strange.framework.impl;

namespace strange.extensions.mediation.impl
{
	public class MediationBinder : Binder, IMediationBinder
	{

		[Inject]
		public IInjectionBinder injectionBinder{ get; set;}

		public MediationBinder ()
		{
		}


		public override IBinding GetRawBinding ()
		{
			return new MediationBinding (resolver) as IBinding;
		}

		public void Trigger(MediationEvent evt, IView view)
		{
			Type viewType = GetTranslatedViewType(view);
			IMediationBinding binding = GetBinding (viewType) as IMediationBinding;
			if (binding != null)
			{
				switch(evt)
				{
					case MediationEvent.AWAKE:
						injectViewAndChildren(view);
						mapView (view, binding);
						break;
					case MediationEvent.DESTROYED:
						unmapView (view, binding);
						break;
					default:
						break;
				}
			}
			else if (evt == MediationEvent.AWAKE)
			{
				//Even if not mapped, Views (and their children) have potential to be injected
				injectViewAndChildren(view);
			}
		}
		
		/// Initialize all IViews within this view
		virtual protected void injectViewAndChildren(IView view)
		{
			IView[] views = view.GetChildViews();
			
			int aa = views.Length;
			for (int a = aa - 1; a > -1; a--)
			{
				IView iView = views[a];
				if (iView != null && iView.autoRegisterWithContext)
				{
					if (iView.registeredWithContext)
					{
						continue;
					}
					iView.registeredWithContext = true;
					if (iView.Equals(view) == false)
						Trigger (MediationEvent.AWAKE, iView);
				}
			}
			injectionBinder.injector.Inject (view, false);
		}

		new public IMediationBinding Bind<T> ()
		{
			return base.Bind<T> () as IMediationBinding;
		}

		public IMediationBinding BindView<T>() where T : MonoBehaviour
		{
			return base.Bind<T> () as IMediationBinding;
		}

		virtual protected Type GetTranslatedViewType(IView key) {
			Type type = key.GetType();
			if(TypeHasMediateAsAttribute(type)) {
				return GetValideMediateAsAttributeTypeFrom(type);
			}
			return type;
		}

		private bool TypeHasMediateAsAttribute(Type type) {
			Type mediateAsTypeAttribute = typeof(MediateAs);
			object[] mediatAsAtributes = type.GetCustomAttributes(mediateAsTypeAttribute, false);
			return (mediatAsAtributes.Length > 0);
		}

		private Type GetValideMediateAsAttributeTypeFrom(Type type) {
			MediateAs mediateAsAttribute = GetMediateAsAttributeFrom(type);
			Type mediateAsType = mediateAsAttribute.MediateAsType;
			if (!IsSameOrSubclass(mediateAsType, type)) {
				throw new MediationException("The " + mediateAsAttribute.GetType().Name + " specifies a type that is not implemented by " + type.Name, MediationExceptionType.BINDAS_TYPE_NOT_IMPLEMENTED);
			}

			return mediateAsType;
		}

		private MediateAs GetMediateAsAttributeFrom(Type type) {
			Type mediateAsTypeAttribute = typeof(MediateAs);
			object[] mediatAsAtributes = type.GetCustomAttributes(mediateAsTypeAttribute, false);
			if (mediatAsAtributes.Length > 1) {
				throw new MediationException("The class " + type.Name + " has multiple " + mediateAsTypeAttribute.Name + ". Make sure only one is specified to allow binding.", MediationExceptionType.MULTIPLE_BINDAS_ATTRIBUTE_DEFINITIONS);
			}
			return (MediateAs)mediatAsAtributes[0];
		}

		private bool IsSameOrSubclass(Type potentialBase, Type potentialDescendant) {
			return potentialDescendant.IsSubclassOf(potentialBase)
				   || potentialDescendant == potentialBase
				   || (potentialBase.IsInterface &&
						potentialDescendant.GetInterface(potentialBase.Name) != null);
		}

		/// Creates and registers one or more Mediators for a specific View instance.
		/// Takes a specific View instance and a binding and, if a binding is found for that type, creates and registers a Mediator.
		virtual protected void mapView(IView view, IMediationBinding binding)
		{
			Type viewType = GetTranslatedViewType(view);

			if (bindings.ContainsKey(viewType))
			{
				object[] values = binding.value as object[];
				int aa = values.Length;
				for (int a = 0; a < aa; a++)
				{
					Type mediatorType = values [a] as Type;
					if (mediatorType == viewType)
					{
						throw new MediationException(viewType + "mapped to itself. The result would be a stack overflow.", MediationExceptionType.MEDIATOR_VIEW_STACK_OVERFLOW);
					}
					object mediator = view.AddMediatorComponent(mediatorType);
					if (mediator == null)
						throw new MediationException ("The view: " + viewType.ToString() + " is mapped to mediator: " + mediatorType.ToString() + ". AddComponent resulted in null, which probably means " + mediatorType.ToString().Substring(mediatorType.ToString().LastIndexOf(".")+1) + " is not a MonoBehaviour.", MediationExceptionType.NULL_MEDIATOR);
					if (mediator is IMediator)
						((IMediator)mediator).PreRegister ();

					Type typeToInject = (binding.abstraction == null || binding.abstraction.Equals(BindingConst.NULLOID)) ? viewType : binding.abstraction as Type;
					injectionBinder.Bind (typeToInject).ToValue (view).ToInject(false);
					injectionBinder.injector.Inject (mediator);
					injectionBinder.Unbind(typeToInject);
					if (mediator is IMediator)
						((IMediator)mediator).OnRegister ();
				}
			}
		}

		/// Removes a mediator when its view is destroyed
		virtual protected void unmapView(IView view, IMediationBinding binding)
		{
			Type viewType = GetTranslatedViewType(view);

			if (bindings.ContainsKey(viewType))
			{
				object[] values = binding.value as object[];
				int aa = values.Length;
				for (int a = 0; a < aa; a++)
				{
					Type mediatorType = values[a] as Type;
					IMediator mediator = view.GetMediatorComponent(mediatorType) as IMediator;
					if (mediator != null)
					{
						mediator.OnRemove();
					}
				}
			}
		}

		private void enableView(IView view)
		{
		}

		private void disableView(IView view)
		{
		}
	}
}

