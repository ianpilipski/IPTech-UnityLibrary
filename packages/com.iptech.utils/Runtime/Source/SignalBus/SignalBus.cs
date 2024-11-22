using System;
using System.Collections.Generic;
using UnityEngine;

namespace IPTech.Utils {
    public class SignalBus : ISignalBus {
        readonly Dictionary<Type, List<Delegate>> listeners = new();
        readonly Dictionary<Type, ISignal> stickySignals = new();

        public event Action<Type, Delegate> ListenerRegistered;

        public void Listen<T>(Action<T> listener) where T : ISignal {
            listeners.TryAdd(typeof(T), new List<Delegate>());
            listeners[typeof(T)].Add(listener);
            ListenerRegistered?.Invoke(typeof(T), listener);

            if(stickySignals.TryGetValue(typeof(T), out var sig)) {
                listener.DynamicInvoke(sig);
            }
        }

        public void Unlisten<T>(Action<T> listener) where T : ISignal {
            if(listeners.TryGetValue(typeof(T), out var delegates)) {
                delegates.Remove(listener);
            }
        }

        public void Send<T>(T signal) where T : ISignal {
            if(signal.Sticky) stickySignals[typeof(T)] = signal;

            if(listeners.TryGetValue(typeof(T), out var delegates)) {
                var copy = new List<Delegate>(delegates);
                foreach(var del in copy) {
                    try {
                        del.DynamicInvoke(signal);
                    } catch(Exception e) {
                        Debug.LogException(e);
                    }
                }
            }
        }
    }
}
