using System;
using UnityEngine;
using System.Collections;
using IPTech.Coroutines;

namespace RSG.Promises
{
	public class PromiseCoroutine
	{
		static CoroutineRunner _promiseRunner;
		static ICoroutineRunner PromiseRunner {
			get {
				if (_promiseRunner == null) {
					_promiseRunner = new CoroutineRunner ();
				}
				return _promiseRunner;
			}
		}

		public static Promise Create (IEnumerator routine)
		{
			Promise promise = new Promise ();
			PromiseRunner.Start(
				new CFunc(routine)
					.Catch( e => promise.Reject(e) )
					.Finally( () => {
						if(promise.CurState == PromiseState.Pending) {
							promise.Resolve();
						}
					})
			);
			return promise;
		}

		public static Promise<T> Create<T> (Func<Action<T>, IEnumerator> routine)
		{
			Promise<T> promise = new Promise<T> ();
			PromiseRunner.Start(
				new CFunc(routine (x => promise.Resolve (x)))
					.Catch(e => promise.Reject(e))
					.Finally(() => {
						if(promise.CurState == PromiseState.Pending) {
							promise.Reject(new PromiseCoroutineFinishedWithoutValue());
						}
					})
			);
			return promise;
		}

		public class PromiseCoroutineFinishedWithoutValue : Exception
		{
			public PromiseCoroutineFinishedWithoutValue () : base () { }
		}
	}
}

