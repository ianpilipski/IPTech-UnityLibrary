using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace IPTech.Zenject
{
    public interface ISignalBus {
        ISignalBus ParentBus { get; }
		int NumSubscribers { get; }
        void Fire<TSignal>(object identifier = null);
        void Fire(object signal, object identifier = null);
        void TryFire<TSignal>(object identifier = null);
        void TryFire(object signal, object identifier = null);
        void Subscribe<TSignal>(Action callback, object identifier = null);
        void Subscribe<TSignal>(Action<TSignal> callback, object identifier = null);
        void Subscribe(Type signalType, Action<object> callback, object identifier = null);
        void Unsubscribe<TSignal>(Action callback, object identifier = null);
        void Unsubscribe(Type signalType, Action callback, object identifier = null);
        void Unsubscribe(Type signalType, Action<object> callback, object identifier = null);
        void Unsubscribe<TSignal>(Action<TSignal> callback, object identifier = null);
        void TryUnsubscribe<TSignal>(Action callback, object identifier = null);
        void TryUnsubscribe(Type signalType, Action callback, object identifier = null);
        void TryUnsubscribe(Type signalType, Action<object> callback, object identifier = null);
        void TryUnsubscribe<TSignal>(Action<TSignal> callback, object identifier = null);
#if ZEN_SIGNALS_ADD_UNIRX
        public IObservable<TSignal> GetStream<TSignal>(object identifier = null);
        public IObservable<object> GetStream(Type signalType, object identifier = null);
#endif
    }

    public class SignalBusProxy : ISignalBus {
        readonly SignalBus _signalBus;

        public SignalBusProxy(SignalBus signalBus) {
            _signalBus = signalBus;
		}

		public ISignalBus ParentBus => _signalBus.ParentBus==null ? null : new SignalBusProxy(_signalBus.ParentBus);

		public int NumSubscribers => _signalBus.NumSubscribers;

		public void Fire<TSignal>(object identifier = null) {
			_signalBus.Fire<TSignal>(identifier);
		}

		public void Fire(object signal, object identifier = null) {
			_signalBus.Fire(signal, identifier);
		}

		public void Subscribe<TSignal>(Action callback, object identifier = null) {
			_signalBus.Subscribe<TSignal>(callback, identifier);
		}

		public void Subscribe<TSignal>(Action<TSignal> callback, object identifier = null) {
			_signalBus.Subscribe(callback, identifier);
		}

		public void Subscribe(Type signalType, Action<object> callback, object identifier = null) {
			_signalBus.Subscribe(signalType, callback, identifier);
		}

		public void TryFire<TSignal>(object identifier = null) {
			_signalBus.TryFire<TSignal>(identifier);
		}

		public void TryFire(object signal, object identifier = null) {
			_signalBus.TryFire(signal, identifier);
		}

		public void TryUnsubscribe<TSignal>(Action callback, object identifier = null) {
			_signalBus.TryUnsubscribe<TSignal>(callback, identifier);
		}

		public void TryUnsubscribe(Type signalType, Action callback, object identifier = null) {
			_signalBus.TryUnsubscribe(signalType, callback, identifier);
		}

		public void TryUnsubscribe(Type signalType, Action<object> callback, object identifier = null) {
			_signalBus.TryUnsubscribe(signalType, callback, identifier);
		}

		public void TryUnsubscribe<TSignal>(Action<TSignal> callback, object identifier = null) {
			_signalBus.TryUnsubscribe(callback, identifier);
		}

		public void Unsubscribe<TSignal>(Action callback, object identifier = null) {
			_signalBus.Unsubscribe<TSignal>(callback, identifier);
		}

		public void Unsubscribe(Type signalType, Action callback, object identifier = null) {
			_signalBus.Unsubscribe(signalType, callback, identifier);
		}

		public void Unsubscribe(Type signalType, Action<object> callback, object identifier = null) {
			_signalBus.Unsubscribe(signalType, callback, identifier);
		}

		public void Unsubscribe<TSignal>(Action<TSignal> callback, object identifier = null) {
			_signalBus.Unsubscribe(callback, identifier);
		}

#if ZEN_SIGNALS_ADD_UNIRX
		public IObservable<TSignal> GetStream<TSignal>(object identifier = null) {
			return _signalBus.GetStream<TSignal>(identifier);
		}

		public IObservable<object> GetStream(Type signalType, object identifier = null) {
			return _signalBus.GetStream(signalType, identifier);
		}
#endif

	}
}
