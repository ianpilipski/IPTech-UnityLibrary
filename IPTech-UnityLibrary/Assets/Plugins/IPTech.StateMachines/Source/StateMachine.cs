using System;
using System.Collections.Generic;
using System.Collections;
using IPTech.Coroutines;

namespace IPTech.StateMachines
{
	public class StateMachine
	{
		IDictionary<string, IState> _states;
		string _targetState;
		StateInfo _currentState;

		public StateMachine() {
			_states = new Dictionary<string, IState>();
		}

		public StateMachine(IDictionary<string, IState> states) {
			_states = new Dictionary<string, IState>(states);
		}

		public void Add(string name, IState state) {
			_states.Add(name, state);
		}

		public void TransitionToState(string name) {
			_targetState = name;
		}

		IEnumerator Update() {
			for(;;) {
				if(_currentState != null) {
					bool calledExit = false;
					while(_currentState.MoveNext()) {
						yield return _currentState.Current;
						if(!calledExit && _currentState.Name != _targetState) {
							_currentState.Exit();
							calledExit = true;
						}
					}
				}

				if(!string.IsNullOrEmpty(_targetState)) {
					_currentState = new StateInfo(_targetState, _states[_targetState]);
					_currentState.Reset();
					_targetState = null;
				}
			}
		}

		class StateInfo : IEnumerator {
			IState _state;
			public Phase CurrentPhase { get; private set; }

			ICFunc _wrapper;

			public string Name { get; private set; }

			public enum Phase {
				Idle,
				Entering,
				Running,
				Exiting,
			}

			public StateInfo(string name, IState state) {
				_state = state;
				Name = name;
				Reset();
			}

			IEnumerator RunState() {
				CurrentPhase = Phase.Entering;
				IEnumerator iter = _state.BeginState();
				yield return iter;

				if(CurrentPhase == Phase.Entering) {
					CurrentPhase = Phase.Running;
					CFunc cw = new CFunc(_state.StateTick());

					while(CurrentPhase == Phase.Running && cw.MoveNext()) {
						yield return null;
					}
				}

				CurrentPhase = Phase.Exiting;
				iter = _state.EndState();
				yield return iter;
			}

			public void Exit() {
				if(CurrentPhase != Phase.Idle) {
					CurrentPhase = Phase.Exiting;
				}
			}

			#region IEnumerator implementation

			public bool MoveNext() {
				if(CurrentPhase == Phase.Idle) return false;
				return _wrapper.MoveNext();
			}

			public void Reset() {
				CurrentPhase = Phase.Idle;
				_wrapper = new CFunc(RunState());
			}

			public object Current {
				get {
					return _wrapper.Current;
				}
			}

			#endregion


		}
	}
}

