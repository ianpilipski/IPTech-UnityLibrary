
using System.Collections.Generic;

namespace IPTech.Utils {
    public static class Locator {
        static IServiceLocator inst;
        static readonly Dictionary<string, IServiceContainer> namedContexts = new();

        public static IServiceLocator Inst {
            get {
                if(inst==null) {
                    inst = GetContext();
                }
                return inst;
            }

            #if UNITY_EDITOR
            // for unit testing be sure to set to null when test is completed
            set {
                inst = value;
            }
            #endif
        }

        public static IServiceContainer GetContext(string name = null) {
            lock(namedContexts) {
                if(!namedContexts.ContainsKey(name)) namedContexts[name] = new ServiceContainer();
            }
            return namedContexts[name];
        }
    }
}