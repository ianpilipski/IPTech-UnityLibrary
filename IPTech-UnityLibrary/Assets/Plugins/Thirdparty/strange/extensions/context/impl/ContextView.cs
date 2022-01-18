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
 * @class strange.extensions.context.impl.ContextView
 * 
 * The Root View of a Context hierarchy.
 * 
 * Extend this class to create your AppRoot, then attach
 * that MonoBehaviour to the GameObject at the very top of
 * your display hierarchy.
 * 
 * The startup sequence looks like this:

		void Awake()
		{
			context = new MyContext(this, true);
			context.Start ();
		}

 */

using System;
using UnityEngine;
using strange.extensions.context.api;
using strange.extensions.mediation.api;
using System.Collections.Generic;
using System.Linq;

namespace strange.extensions.context.impl
{
	public class ContextView : MonoBehaviour, IContextView
	{
		public IContext context{get;set;}
		
		public ContextView ()
		{
		}

		/// <summary>
		/// When a ContextView is Destroyed, automatically removes the associated Context.
		/// </summary>
		protected virtual void OnDestroy()
		{
			if (context != null && Context.firstContext != null)
				Context.firstContext.RemoveContext(context);
		}

		#region IView implementation

		public bool requiresContext {get;set;}

		public bool registeredWithContext {get;set;}

		public bool autoRegisterWithContext{ get; set; }

		public IView[] GetChildViews() {
			List<IView> viewList = new List<IView>();
			Stack<GameObject> gameObjectStack = new Stack<GameObject>();
			gameObjectStack.Push(this.gameObject);
			while(gameObjectStack.Count>0) {
				GameObject go = gameObjectStack.Pop();
				foreach(Transform transform in go.transform) {
					IView[] views = transform.GetComponents<IView>();
					if(views!=null) {
						if (!views.Any(v => (v as IContextView)!=null)) {
							gameObjectStack.Push(transform.gameObject);
							foreach (IView view in views) {
								viewList.Add(view);
							}
						}
					}
				}
			}
			return viewList.ToArray();
		}

		public object AddMediatorComponent(Type mediatorType) {
			return gameObject.AddComponent(mediatorType);
		}

		public object GetMediatorComponent(Type mediatorType) {
			return gameObject.GetComponent(mediatorType);
		}


		#endregion
	}
}

