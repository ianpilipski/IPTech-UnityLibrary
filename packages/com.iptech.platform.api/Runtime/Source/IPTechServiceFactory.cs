using System;

namespace IPTech.Platform {
    public class IPTechServiceFactory : IIPTechServiceFactory {
        private readonly Func<IIPTechPlatform, object> createFunc;

        public IPTechServiceFactory(Type type, Func<IIPTechPlatform, object> createFunc) {
            this.CreatesType = type;
            this.createFunc = createFunc;
        }

        public Type CreatesType { get; }
        public object Create(IIPTechPlatform platform) => createFunc(platform);
    }

    public class IPTechServiceFactory<T> : IPTechServiceFactory, IIPTechServiceFactory<T> {
        public IPTechServiceFactory(Func<IIPTechPlatform, T> createFunc) : base(typeof(T), (p) => createFunc(p)) {
        }
    }
}
