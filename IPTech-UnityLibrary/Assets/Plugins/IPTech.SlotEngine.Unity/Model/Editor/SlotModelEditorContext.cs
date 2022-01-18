using IPTech.SlotEngine.Api;
using IPTech.SlotEngine.Model.Api;
using IPTech.SlotEngine.Unity.Model.Editor.Api;
using strange.extensions.context.api;
using strange.extensions.context.impl;
using strange.extensions.injector.api;
using strange.extensions.injector.impl;
using strange.framework.api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IPTech.SlotEngine.Unity.Model.Editor
{
	class SlotModelEditorContext : Context
	{
		private ISlotModelEditor slotModelEditor { get; set; }

		private IInjectionBinder _injectionBinder;

		private IInjectionBinder injectionBinder {
			get {
				if(this._injectionBinder==null) {
					this._injectionBinder = new InjectionBinder();
				}
				return this._injectionBinder;
			}
			set {
				this._injectionBinder = value;
			}
		}

		public SlotModelEditorContext() {
			addCoreComponents();
			this.autoStartup = true;
			Start();
		}

		protected override void addCoreComponents() {
			base.addCoreComponents();
			injectionBinder.Bind<IInstanceProvider>().Bind<IInjectionBinder>().ToValue(injectionBinder);
			injectionBinder.Bind<IContext>().ToValue(this).ToName(ContextKeys.CONTEXT);
			//injectionBinder.Bind<ICommandBinder>().To<EventCommandBinder>().ToSingleton();
			//This binding is for local dispatchers
			//injectionBinder.Bind<IEventDispatcher>().To<EventDispatcher>();
			//This binding is for the common system bus
			//injectionBinder.Bind<IEventDispatcher>().To<EventDispatcher>().ToSingleton().ToName(ContextKeys.CONTEXT_DISPATCHER);
			//injectionBinder.Bind<IMediationBinder>().To<MediationBinder>().ToSingleton();
			//injectionBinder.Bind<ISequencer>().To<EventSequencer>().ToSingleton();
			//injectionBinder.Bind<IImplicitBinder>().To<ImplicitBinder>().ToSingleton();

			injectionBinder.Bind<ISlotModelEditor>().To<SlotModelEditor>();
			injectionBinder.Bind<IReelSetEditor>().To<ReelSetEditor>();
			injectionBinder.Bind<IReelEditor>().To<ReelEditor>();
			injectionBinder.Bind<ISymbolEditor>().To<SymbolEditor>();
			injectionBinder.Bind<IPaylineEditor>().To<PaylineEditor>();
			injectionBinder.Bind<IPaylineEntryEditor>().To<PaylineEntryEditor>();
			injectionBinder.Bind<IPaylineSetEditor>().To<PaylineSetEditor>();
			injectionBinder.Bind<IPayoutTableEditor>().To<PayoutTableEditor>();
			injectionBinder.Bind<IPayoutTableEntryEditor>().To<PayoutTableEntryEditor>();
			injectionBinder.Bind<IWildSymbolSetEditor>().To<WildSymbolSetEditor>();
			injectionBinder.Bind<IWildSymbolEditor>().To<WildSymbolEditor>();
			
			injectionBinder.Bind<ObjectToEditorMap<IReel, IReelEditor>>().To<ObjectToEditorMap<IReel, IReelEditor>>();
			injectionBinder.Bind<ObjectToEditorMap<IPayline, IPaylineEditor>>().To<ObjectToEditorMap<IPayline, IPaylineEditor>>();
			injectionBinder.Bind<ObjectToEditorMap<IPayoutTableEntry, IPayoutTableEntryEditor>>().To<ObjectToEditorMap<IPayoutTableEntry, IPayoutTableEntryEditor>>();
			injectionBinder.Bind<ObjectToEditorMap<IWildSymbol, IWildSymbolEditor>>().To<ObjectToEditorMap<IWildSymbol, IWildSymbolEditor>>();

			injectionBinder.Bind<ISlotEngine>().To<SlotEngineBasic>().ToSingleton();
			injectionBinder.Bind<ISlotEngineSimulator>().To<SlotEngineSimulator>().ToSingleton();
		}

		protected override void instantiateCoreComponents() {
			base.instantiateCoreComponents();

			this.slotModelEditor = injectionBinder.GetInstance<ISlotModelEditor>();
		}

		public ISlotModelEditor GetSlotModelEditor() {
			return this.slotModelEditor;
		}
	}
}
