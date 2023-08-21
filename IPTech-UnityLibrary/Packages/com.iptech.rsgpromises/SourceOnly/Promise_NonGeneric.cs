using RSG.Promises;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RSG
{
	public interface IPromise
	{
		int Id { get; }

		IPromise WithName(string name);

		IPromise<ConvertedT> Then<ConvertedT>(Func<IPromise<ConvertedT>> onResolved);
		IPromise Then(Func<IPromise> onResolved);
		IPromise Then(Action onResolved);
		IPromise<ConvertedT> Then<ConvertedT>(Func<IPromise<ConvertedT>> onResolved, Action<Exception> onRejected);
		IPromise Then(Func<IPromise> onResolved, Action<Exception> onRejected);
		IPromise Then(Action onResolved, Action<Exception> onRejected);

		IPromise Catch(Action<Exception> onRejected);

		void Done(Action onResolved, Action<Exception> onRejected);
		void Done(Action onResolved);
		void Done();

		IPromise Finally(Action onComplete);
		IPromise Finally(Func<IPromise> onResolved);
		IPromise<ConvertedT> Finally<ConvertedT>(Func<IPromise<ConvertedT>> onComplete);

		IPromise ThenAll(Func<IEnumerable<IPromise>> chain);

		IPromise<IEnumerable<ConvertedT>> ThenAll<ConvertedT>(Func<IEnumerable<IPromise<ConvertedT>>> chain);
		IPromise ThenSequence(Func<IEnumerable<Func<IPromise>>> chain);
		IPromise ThenRace(Func<IEnumerable<IPromise>> chain);
		IPromise<ConvertedT> ThenRace<ConvertedT>(Func<IEnumerable<IPromise<ConvertedT>>> chain);
	}

	public interface IPendingPromise : IRejectable {
		int Id { get; }
		void Resolve();
	}

	public interface IPromiseInfo {
		int Id { get; }
		string Name { get; }
	}

	public class ExceptionEventArgs : EventArgs {
		public ExceptionEventArgs(Exception exception) {
//            Argument.NotNull(() => exception);
			this.Exception = exception;
		}

		public Exception Exception {
			get;
			private set;
		}
	}

	public struct RejectHandler {
		public Action<Exception> callback;
		public IRejectable rejectable;
	}

	public class PromiseBase {
		protected static HashSet<IPromiseInfo> pendingPromises = new HashSet<IPromiseInfo>();
	}

	public class Promise : PromiseBase, IPromise, IPendingPromise, IPromiseInfo {
		public static bool EnablePromiseTracking = false;

		public static event EventHandler<ExceptionEventArgs> UnhandledException {
			add { unhandlerException += value; }
			remove { unhandlerException -= value; }
		}

		private static EventHandler<ExceptionEventArgs> unhandlerException;

		private static int nextPromiseId = 0;

		public static IEnumerable<IPromiseInfo> GetPendingPromises() {
			return pendingPromises;
		}

		private Exception rejectionException;

		private List<RejectHandler> rejectHandlers;

		public struct ResolveHandler
		{
			public Action callback;
			public IRejectable rejectable;
		}

		private List<ResolveHandler> resolveHandlers;

		public int Id { get; private set; }

		public string Name { get; private set; }

		public PromiseState CurState { get; private set; }

		public Promise() {
			this.CurState = PromiseState.Pending;
			this.Id = NextId();
			if(EnablePromiseTracking) {
				pendingPromises.Add(this);
			}
		}

		public Promise(Action<Action, Action<Exception>> resolver) {
			this.CurState = PromiseState.Pending;
			this.Id = NextId();
			if(EnablePromiseTracking) {
				pendingPromises.Add(this);
			}

			try {
				resolver(
                    // Resolve
					() => Resolve(),

                    // Reject
					ex => Reject(ex)
				);
			} catch(Exception ex) {
				Reject(ex);
			}
		}

		public static int NextId() {
			return ++nextPromiseId;
		}

		private void AddRejectHandler(Action<Exception> onRejected, IRejectable rejectable) {
			if(rejectHandlers == null) {
				rejectHandlers = new List<RejectHandler>();
			}

			rejectHandlers.Add(new RejectHandler() {
				callback = onRejected,
				rejectable = rejectable
			});
		}

		private void AddResolveHandler(Action onResolved, IRejectable rejectable) {
			if(resolveHandlers == null) {
				resolveHandlers = new List<ResolveHandler>();
			}

			resolveHandlers.Add(new ResolveHandler() {
				callback = onResolved,
				rejectable = rejectable
			});
		}

		private void InvokeRejectHandler(Action<Exception> callback, IRejectable rejectable, Exception value) {
//            Argument.NotNull(() => callback);
//            Argument.NotNull(() => rejectable);

			try {
				callback(value);
			} catch(Exception ex) {
				rejectable.Reject(ex);
			}
		}

		private void InvokeResolveHandler(Action callback, IRejectable rejectable) {
//            Argument.NotNull(() => callback);
//            Argument.NotNull(() => rejectable);

			try {
				callback();
			} catch(Exception ex) {
				rejectable.Reject(ex);
			}
		}

		private void ClearHandlers() {
			rejectHandlers = null;
			resolveHandlers = null;
		}

		private void InvokeRejectHandlers(Exception ex) {
//            Argument.NotNull(() => ex);

			if(rejectHandlers != null) {
				rejectHandlers.Each(handler => InvokeRejectHandler(handler.callback, handler.rejectable, ex));
			}

			ClearHandlers();
		}

		private void InvokeResolveHandlers() {
			if(resolveHandlers != null) {
				resolveHandlers.Each(handler => InvokeResolveHandler(handler.callback, handler.rejectable));
			}

			ClearHandlers();
		}

		public void Reject(Exception ex) {
//            Argument.NotNull(() => ex);

			if(CurState != PromiseState.Pending) {
				throw new ApplicationException("Attempt to reject a promise that is already in state: " + CurState + ", a promise can only be rejected when it is still in state: " + PromiseState.Pending);
			}

			rejectionException = ex;
			CurState = PromiseState.Rejected;

			if(EnablePromiseTracking) {
				pendingPromises.Remove(this);
			}

			InvokeRejectHandlers(ex);            
		}

		public void Resolve() {
			if(CurState != PromiseState.Pending) {
				throw new ApplicationException("Attempt to resolve a promise that is already in state: " + CurState + ", a promise can only be resolved when it is still in state: " + PromiseState.Pending);
			}

			CurState = PromiseState.Resolved;

			if(EnablePromiseTracking) {
				pendingPromises.Remove(this);
			}

			InvokeResolveHandlers();
		}

		public void Done(Action onResolved, Action<Exception> onRejected) {
			Then(onResolved, onRejected)
                .Catch(ex =>
                    Promise.PropagateUnhandledException(this, ex)
			);
		}

		public void Done(Action onResolved) {
			Then(onResolved)
                .Catch(ex => 
                    Promise.PropagateUnhandledException(this, ex)
			);
		}

		public void Done() {
			Catch(ex => PropagateUnhandledException(this, ex));
		}

		public IPromise WithName(string name) {
			this.Name = name;
			return this;
		}

		public IPromise Catch(Action<Exception> onRejected) {
//            Argument.NotNull(() => onRejected);

			var resultPromise = new Promise();
			resultPromise.WithName(Name);

			Action resolveHandler = () => resultPromise.Resolve();

			Action<Exception> rejectHandler = ex => {
				try {
					onRejected(ex);
					resultPromise.Resolve();
				} catch(Exception callbackException) {
					resultPromise.Reject(callbackException);
				}
			};

			ActionHandlers(resultPromise, resolveHandler, rejectHandler);

			return resultPromise;
		}

		public IPromise<ConvertedT> Then<ConvertedT>(Func<IPromise<ConvertedT>> onResolved) {
			return Then(onResolved, null);
		}

		public IPromise Then(Func<IPromise> onResolved) {
			return Then(onResolved, null);
		}

		public IPromise Then(Action onResolved) {
			return Then(onResolved, null);
		}

		public IPromise<ConvertedT> Then<ConvertedT>(Func<IPromise<ConvertedT>> onResolved, Action<Exception> onRejected) {
			// This version of the function must supply an onResolved.
			// Otherwise there is now way to get the converted value to pass to the resulting promise.
//            Argument.NotNull(() => onResolved);

			var resultPromise = new Promise<ConvertedT>();
			resultPromise.WithName(Name);

			Action resolveHandler = () => {
				onResolved()
                    .Then(
                        // Should not be necessary to specify the arg type on the next line, but Unity (mono) has an internal compiler error otherwise.
					(ConvertedT chainedValue) => resultPromise.Resolve(chainedValue),
					ex => resultPromise.Reject(ex)
				);
			};

			Action<Exception> rejectHandler = ex => {
				if(onRejected != null) {
					onRejected(ex);
				}

				resultPromise.Reject(ex);
			};

			ActionHandlers(resultPromise, resolveHandler, rejectHandler);

			return resultPromise;
		}

		public IPromise Then(Func<IPromise> onResolved, Action<Exception> onRejected) {
			var resultPromise = new Promise();
			resultPromise.WithName(Name);

			Action resolveHandler = () => {
				if(onResolved != null) {
					onResolved()
                        .Then(
						() => resultPromise.Resolve(),
						ex => resultPromise.Reject(ex)
					);
				} else {
					resultPromise.Resolve();
				}
			};

			Action<Exception> rejectHandler = ex => {
				if(onRejected != null) {
					onRejected(ex);
				}

				resultPromise.Reject(ex);
			};

			ActionHandlers(resultPromise, resolveHandler, rejectHandler);

			return resultPromise;
		}

		public IPromise Then(Action onResolved, Action<Exception> onRejected) {
			var resultPromise = new Promise();
			resultPromise.WithName(Name);

			Action resolveHandler = () => {
				if(onResolved != null) {
					onResolved();
				}

				resultPromise.Resolve();
			};

			Action<Exception> rejectHandler = ex => {
				if(onRejected != null) {
					onRejected(ex);
				}

				resultPromise.Reject(ex);
			};

			ActionHandlers(resultPromise, resolveHandler, rejectHandler);

			return resultPromise;
		}

		private void ActionHandlers(IRejectable resultPromise, Action resolveHandler, Action<Exception> rejectHandler) {
			if(CurState == PromiseState.Resolved) {
				InvokeResolveHandler(resolveHandler, resultPromise);
			} else if(CurState == PromiseState.Rejected) {
				InvokeRejectHandler(rejectHandler, resultPromise, rejectionException);
			} else {
				AddResolveHandler(resolveHandler, resultPromise);
				AddRejectHandler(rejectHandler, resultPromise);
			}
		}
			
		public static IPromise Resolved() {
			var promise = new Promise();
			promise.Resolve();
			return promise;
		}

		public static IPromise Rejected(Exception ex) {
//            Argument.NotNull(() => ex);

			var promise = new Promise();
			promise.Reject(ex);
			return promise;
		}

		public IPromise Finally(Action onComplete) {
			Promise promise = new Promise();
			promise.WithName(Name);

			this.Then(() => {
				promise.Resolve();
			});
			this.Catch((e) => {
				promise.Resolve();
			});

			return promise.Then(onComplete);
		}

		public IPromise Finally(Func<IPromise> onComplete) {
			Promise promise = new Promise();
			promise.WithName(Name);

			this.Then(() => {
				promise.Resolve();
			});
			this.Catch((e) => {
				promise.Resolve();
			});

			return promise.Then(onComplete);
		}

		public IPromise<ConvertedT> Finally<ConvertedT>(Func<IPromise<ConvertedT>> onComplete) {
			Promise promise = new Promise();
			promise.WithName(Name);

			this.Then(() => {
				promise.Resolve();
			});
			this.Catch((e) => {
				promise.Resolve();
			});

			return promise.Then(() => {
				return onComplete();
			});
		}

		public static void PropagateUnhandledException(object sender, Exception ex) {
			if(unhandlerException != null) {
				unhandlerException(sender, new ExceptionEventArgs(ex));
			}
		}

		public IPromise ThenAll(Func<IEnumerable<IPromise>> chain)
        {
            return Then(() => Promise.All(chain()));
        }

		public IPromise<IEnumerable<ConvertedT>> ThenAll<ConvertedT>(Func<IEnumerable<IPromise<ConvertedT>>> chain)
        {
            return Then(() => Promise<ConvertedT>.All(chain()));
        }

		public static IPromise All(params IPromise[] promises)
        {
            return All((IEnumerable<IPromise>)promises); // Cast is required to force use of the other All function.
        }

		public static IPromise All(IEnumerable<IPromise> promises)
        {
            var promisesArray = promises.ToArray();
            if (promisesArray.Length == 0)
            {
                return Promise.Resolved();
            }

            var remainingCount = promisesArray.Length;
            var resultPromise = new Promise();
            resultPromise.WithName("All");

            promisesArray.Each((promise, index) =>
            {
                promise
                    .Catch(ex =>
                    {
                        if (resultPromise.CurState == PromiseState.Pending)
                        {
                            // If a promise errorred and the result promise is still pending, reject it.
                            resultPromise.Reject(ex);
                        }
                    })
                    .Then(() =>
                    {
                        --remainingCount;
                        if (remainingCount <= 0)
                        {
                            // This will never happen if any of the promises errorred.
                            resultPromise.Resolve();
                        }
                    })
                    .Done();
            });

            return resultPromise;
        }

		public IPromise ThenSequence(Func<IEnumerable<Func<IPromise>>> chain)
        {
            return Then(() => Sequence(chain()));
        }

		public static IPromise Sequence(params Func<IPromise>[] fns)
        {
            return Sequence((IEnumerable<Func<IPromise>>)fns);
        }

		public static IPromise Sequence(IEnumerable<Func<IPromise>> fns)
        {
            return fns.Aggregate(
                Resolved(),
                (prevPromise, fn) =>
                {
                    return prevPromise.Then(() => fn());
                }
            );
        }

		public IPromise ThenRace(Func<IEnumerable<IPromise>> chain)
        {
            return Then(() => Race(chain()));
        }

		public IPromise<ConvertedT> ThenRace<ConvertedT>(Func<IEnumerable<IPromise<ConvertedT>>> chain)
        {
            return Then(() => Promise<ConvertedT>.Race(chain()));
        }

		public static IPromise Race(params IPromise[] promises)
        {
            return Race((IEnumerable<IPromise>)promises); // Cast is required to force use of the other function.
        }

		public static IPromise Race(IEnumerable<IPromise> promises)
        {
            var promisesArray = promises.ToArray();
            if (promisesArray.Length == 0)
            {
                throw new ApplicationException("At least 1 input promise must be provided for Race");
            }

            var resultPromise = new Promise();
            resultPromise.WithName("Race");

            promisesArray.Each((promise, index) =>
            {
                promise
                    .Catch(ex =>
                    {
                        if (resultPromise.CurState == PromiseState.Pending)
                        {
                            // If a promise errorred and the result promise is still pending, reject it.
                            resultPromise.Reject(ex);
                        }
                    })
                    .Then(() =>
                    {
                        if (resultPromise.CurState == PromiseState.Pending)
                        {
                            resultPromise.Resolve();
                        }
                    })
                    .Done();
            });

            return resultPromise;
        }
		
	}
}