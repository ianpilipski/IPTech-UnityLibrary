using UnityEditor;
using NUnit.Framework;
using strange.extensions.iptech.context.impl;
using strange.extensions.mediation.api;
using System;
using UnityEngine;
using strange.extensions.signal.impl;
using strange.extensions.context.api;

namespace strange.extensions.iptech.context.tests
{
	[TestFixture]
	public class TestMVCSContextBase
	{

		public class MockView : IView
		{
			public bool isInited;
			public bool functionCalled;

			private object mediatorComponent;

			public bool autoRegisterWithContext { get; set; }
			public bool registeredWithContext { get; set; }
			public bool requiresContext { get; set; }
			public object AddMediatorComponent(Type mediatorType) {
				mediatorComponent = new MockMediator();
				return mediatorComponent;
			}

			public IView[] GetChildViews() {
				return new IView[0];
			}

			public object GetMediatorComponent(Type mediatorType) {
				return mediatorComponent;
			}

			public void init() {
				isInited = true;
			}

			public void someFunctionWasCalled() {
				functionCalled = true;
			}
		}

		public class MockSignal : Signal
		{
			public MockSignal() { }
		}

		public class MockMediator : IMediator
		{
			public GameObject contextView { get; set; }

			public void OnRemove() {
				
			}

			[Inject]
			public MockView view { get; set; }

			[Inject]
			public MockSignal exampleSignal { get; set; }

			public void PreRegister() { }

			public void OnRegister() {
				exampleSignal.AddListener(onSomeFunctionCalled);
				view.init();
			}

			void onSomeFunctionCalled() {
				view.someFunctionWasCalled();
			}
		}

		public class MockContextView : IContextView
		{
			public MockContextView() { }

			public bool autoRegisterWithContext { get; set; }
			public IContext context { get; set; }
			public bool registeredWithContext { get; set; }
			public bool requiresContext { get; set; }
			public object AddMediatorComponent(Type mediatorType) {
				throw new NotImplementedException();
			}

			public IView[] GetChildViews() {
				return new IView[0];
			}

			public object GetMediatorComponent(Type mediatorType) {
				throw new NotImplementedException();
			}
		}


		[SetUp]
		public void Setup() {

		}

		[Test]
		public void Test() {
			MVCSContextBase.firstContext = null;
			MVCSContextBase context = new MVCSContextBase(new MockContextView(), true);
			context.Start();
			context.mediationBinder.Bind<MockView>().To<MockMediator>();
			context.injectionBinder.Bind<MockSignal>().ToSingleton();

			MockView view = new MockView();
			context.AddView(view);
			Assert.IsTrue(view.isInited);

			MockSignal signal = context.injectionBinder.GetInstance<MockSignal>() as MockSignal;
			signal.Dispatch();
			Assert.IsTrue(view.functionCalled);
		}
	}
}
