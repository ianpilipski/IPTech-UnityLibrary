using IPTech.Strange.Api;
using strange.extensions.context.api;
using strange.extensions.injector.api;
using System.Collections;
using UnityEngine;

namespace IPTech.Strange
{
	//An implicit binding. We map this binding as Cross-Context by default.
	[Implements(typeof(ICoroutineRunner))]
	public class CoroutineRunner : ICoroutineRunner
	{
		private CoroutineRunnerBehaviour mb;

		[PostConstruct]
		public void PostConstruct() {
			GameObject go = new GameObject("CoroutineRunner");
			mb = go.AddComponent<CoroutineRunnerBehaviour>();
		}

		public Coroutine StartCoroutine(IEnumerator routine) {
			return mb.StartCoroutine(routine);
		}

		public void StopCoroutine(Coroutine coroutine) {
			mb.StopCoroutine(coroutine);
		}
	}

	public class CoroutineRunnerBehaviour : MonoBehaviour
	{
	}
}
