using System;

namespace IPTech.Platform {
    public interface IIPTechServiceFactory {
        Type CreatesType { get; }
        object Create(IIPTechPlatform platform);
    }

    public interface IIPTechServiceFactory<T> : IIPTechServiceFactory {

    }
}
