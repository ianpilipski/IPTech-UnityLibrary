
using System;
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
        }

        public static IServiceContainer GetContext(string name = "default") {
            if(string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Context name cannot be null or whitespace", nameof(name));
            lock(namedContexts) {
                if(!namedContexts.ContainsKey(name)) namedContexts[name] = new ServiceContainer();
            }
            return namedContexts[name];
        }

        #if UNITY_EDITOR
        public static class Spy
        {
            public static void SetInst(IServiceLocator value) 
            {
                lock(namedContexts) {
                    inst = value;
                }
            }

            public static void ClearAll()
            {
                lock(namedContexts) {
                    inst = null;
                    namedContexts.Clear();
                }
            }
        }
        #endif
    }
}