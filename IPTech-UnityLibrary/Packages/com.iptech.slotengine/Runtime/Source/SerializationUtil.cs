using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
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

            byte[] bytes = Convert.FromBase64String(base64String);
			using(var stream = new MemoryStream(bytes)) {
			    return serializer.Deserialize(stream);
            }
        }
    }
}
