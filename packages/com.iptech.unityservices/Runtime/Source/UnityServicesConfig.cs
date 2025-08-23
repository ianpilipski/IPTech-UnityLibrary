using System;
using System.Collections.Generic;
using IPTech.Platform;

namespace IPTech.UnityServices
{
    public class UnityServicesConfig
    {
        private IIPTechPlatformConfig _platformTechConfig;

        internal List<Action> configVisitors = new();

        internal void RegisterFactory<TService>(Func<IUnityServicesManager, TService> factory)
            where TService : class
        {
            if (_platformTechConfig != null)
            {
                Visit();
                return;
            }

            configVisitors.Add(Visit);

            void Visit()
            {
                _platformTechConfig.RegisterFactory<TService>(new IPTechServiceFactory<TService>((p) =>
                {
                    var unityServicesManager = p.Services.GetService<IUnityServicesManager>();
                    return factory(unityServicesManager);
                }));
            }
        }

        public void Visit(IIPTechPlatformConfig config)
        {
            _platformTechConfig = config;
            if (configVisitors != null)
            {
                foreach (var visitor in configVisitors)
                {
                    visitor();
                }
            }
        }
    }  
}
