
using System;
using System.Collections.Generic;
using System.Text;

namespace IPTech.BuildTool.Encryption {
    public class EncryptedItemReader {
        static byte[] EmptyByteArray = new byte[0];
        static string EmptyString = "";

        readonly IDictionary<string,string> data;

        public EncryptedItemReader(IDictionary<string,string> data) {
            this.data = data;
        }

        public byte[] ReadBytes(string key) {
            var val = data[key];
            if(val==null || val.Length==0) {
                return EmptyByteArray;
            }
            return Convert.FromBase64String(val);
        }

        public string ReadString(string key) {
            var val = data[key];
            if(val==null || val.Length==0) {
                return EmptyString;
            }
            return Encoding.UTF8.GetString(Convert.FromBase64String(data[key]));
        }
    }
}