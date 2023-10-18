

using System;
using System.Collections.Generic;


namespace IPTech.Utils {
    public delegate object ServiceCreatorCallback(IServiceLocator locator, Type serviceType);

    public interface IServiceLocator {
        bool HasService<T>();
        bool HasService(Type type);
        bool HasServiceInstance<T>();
        bool HasServiceInstance(Type type);
        bool TryGetService<T>(out T service);
        bool TryGetService(Type type, out object service);
        T GetService<T>();
        object GetService(Type type);
    }

    public interface IServiceContext : IServiceLocator {
        void AddService<T>(T service);
        void AddService(Type type, object service);
        void AddService<T>(ServiceCreatorCallback factory);
        void AddService(Type type, ServiceCreatorCallback factory);
        void RemoveService<T>();
        void RemoveService(Type type);
    }


    public class ServiceContext : IServiceContext {
        Dictionary<Type, RegisteredService> services = new();

        public T GetService<T>() => (T)GetService(typeof(T));
        public object GetService(Type type) {
            if(TryGetService(type, out var service)) return service;
            throw new KeyNotFoundException($"service {type} not found in container");
        }

        public bool HasService<T>() => HasService(typeof(T));
        public bool HasService(Type type) {
            return services.ContainsKey(type);
        }

        public bool HasServiceInstance<T>() => HasServiceInstance(typeof(T));
        public bool HasServiceInstance(Type type) {
            if(services.TryGetValue(type, out var regServ)) {
                return regServ.HasInstance;
            }
            return false;
        }

        public bool TryGetService<T>(out T service) {
            if(TryGetService(typeof(T), out var serv)) {
                service = (T)serv;
                return true;
            }
            service = default;
            return false;
        } 

        public bool TryGetService(Type type, out object service) {
            if(services.TryGetValue(type, out var regService)) {
                service = regService.Instance;
                return true;
            }
            service = null;
            return false;
        }

        public void AddService<T>(T service) => AddService(typeof(T), service);
        public void AddService(Type type, object service) {
            services.Add(type, new RegisteredService(type, service, this));
        }

        public void AddService<T>(ServiceCreatorCallback factory) => AddService(typeof(T), factory);
        public void AddService(Type type, ServiceCreatorCallback factory) {
            services.Add(type, new RegisteredService(type, factory, this));
        }

        public void RemoveService<T>() => RemoveService(typeof(T));
        public void RemoveService(Type type) {
            services.Remove(type);
        }

        class RegisteredService {
            readonly Type type;
            readonly IServiceLocator provider;
            readonly ServiceCreatorCallback serviceCreatorCallback;
            object service;


            public RegisteredService(Type type, object service, IServiceLocator provider) {
                this.type = type;
                this.service = service;
                this.provider = provider;
            }

            public RegisteredService(Type type, ServiceCreatorCallback callback, IServiceLocator provider) {
                this.type = type;
                this.provider = provider;
                this.serviceCreatorCallback = callback;
            }

            public bool HasInstance => service!=null;

            public object Instance {
                get {
                    if(service==null && serviceCreatorCallback!=null) {
                        service = serviceCreatorCallback(provider, type);
                        if(service==null) {
                            throw new Exception($"failed to create service {type}");
                        }
                    }
                    return service;
                }
            }
        }

    }
}