using System;

namespace IPTech.Utils {
    public interface ISignalBus {
        void Listen<T>(Action<T> listener) where T : ISignal;
        void Unlisten<T>(Action<T> listener) where T : ISignal;
        void Send<T>(T signal) where T : ISignal;
        event Action<Type, Delegate> ListenerRegistered;
    }
}
