using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace IPTech.Zenject {
    public class SignalBusProxyInstaller : Installer<SignalBusProxyInstaller> {
        public override void InstallBindings() {
            SignalBusInstaller.Install(Container);
            Container.Bind<ISignalBus>().To<SignalBusProxy>().AsSingle().CopyIntoAllSubContainers();
        }
    }
}
