using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using IPTech.SlotEngine.Model.Api;
using UnityEngine;

namespace IPTech.SlotEngine {
    public static class SerializationUtil {
        public static string SerializeToBase64String(object obj) {
            var serializer = new BinaryFormatter();

            using (MemoryStream stream = new MemoryStream()) {
				serializer.Serialize(stream, obj);
				stream.Flush();
				return Convert.ToBase64String(stream.ToArray());
			}
        }

        public static object DeserializeFromBase64String(string base64String) {
            var serializer = new BinaryFormatter();

            serializer.Binder = new CustomSerializationBinder();
            
            byte[] bytes = Convert.FromBase64String(base64String);
			using(var stream = new MemoryStream(bytes)) {
			    return serializer.Deserialize(stream);
            }
        }

        class CustomSerializationBinder : SerializationBinder
        {
            static Dictionary<string, Type> knownTypes = new Dictionary<string, Type>() {
                { "System.Collections.Generic.List`1[[IPTech.SlotEngine.Model.Api.IPayline, Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null]]",
                    typeof(List<IPayline>) },
                { "System.Collections.Generic.List`1[[IPTech.SlotEngine.Model.Api.IPayoutTableEntry, Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null]]",
                    typeof(List<IPayoutTableEntry>) },
                { "System.Collections.Generic.List`1[[IPTech.SlotEngine.Model.Api.IReel, Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null]]",
                    typeof(List<IReel>)},
                { "System.Collections.Generic.List`1[[IPTech.SlotEngine.Model.Api.IWildSymbol, Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null]]",
                    typeof(List<IWildSymbol>)},
                { "System.Collections.Generic.List`1[[IPTech.SlotEngine.Model.Api.IPaylineEntry, Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null]]",
                    typeof(List<IPaylineEntry>)},
                { "System.Collections.Generic.List`1[[IPTech.SlotEngine.Model.Api.ISymbol, Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null]]",
                    typeof(List<ISymbol>)}
            };

            public override Type BindToType(string assemblyName, string typeName)
            {
                //Debug.Log($"BindToType: {assemblyName} : {typeName}");
                
                if(assemblyName.Equals("Assembly-CSharp")) {
                    return Type.GetType(typeName);
                }

                if(knownTypes.TryGetValue(typeName, out Type t)) {
                    return t;
                }

                return null;
            }
        }
    }
}
