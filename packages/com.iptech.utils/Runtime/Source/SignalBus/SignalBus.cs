using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace IPTech.Utils {
    public class SignalBus : ISignalBus {
        readonly Dictionary<Type, List<Delegate>> listeners = new();
        readonly Dictionary<Type, ISignal> stickySignals = new();

        public event Action<Type, Delegate> ListenerRegistered;

        public void Listen<T>(Action<T> listener) where T : ISignal {
            listeners.TryAdd(typeof(T), new List<Delegate>());
            listeners[typeof(T)].Add(listener);

            try 
            {
                ListenerRegistered?.Invoke(typeof(T), listener);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }

            if(stickySignals.Count > 0)
            {
                stickySignals.Where(kv => CanListenerReceiveSignal(typeof(T), kv.Value)).Select(kv => kv.Value)
                    .ToList()
                    .ForEach(sig => SafeInvoke(listener, sig));
            }
        }

        public void Unlisten<T>(Action<T> listener) where T : ISignal {
            if(listeners.TryGetValue(typeof(T), out var delegates)) {
                delegates.Remove(listener);
            }
        }

        public void Send<T>(T signal) where T : ISignal {
            if(signal.Sticky) stickySignals[typeof(T)] = signal;
            
            listeners.Where(kv => CanListenerReceiveSignal(kv.Key, signal)).SelectMany(kv => kv.Value)
                .ToList()
                .ForEach(del => SafeInvoke(del, signal));
        }

        private bool CanListenerReceiveSignal(Type listenerType, ISignal signal)
        {
            return listenerType.IsAssignableFrom(signal.GetType());
        }

        private void SafeInvoke(Delegate del, ISignal signal)
        {
            try
            {
                del.DynamicInvoke(signal);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
    }
}
