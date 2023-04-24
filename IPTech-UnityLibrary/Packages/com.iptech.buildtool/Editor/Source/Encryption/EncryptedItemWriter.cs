
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace IPTech.BuildTool {
    public class EncryptedItemWriter {
        static string EmptyString = "";

        readonly IDictionary<string,string> dict;

        public EncryptedItemWriter(IDictionary<string,string> dict) {
            this.dict = dict;
        }

        public void WriteBytes(string key, byte[] value) {
            if(value==null || value.Length==0) {
                dict[key] = EmptyString;
            } else {
                dict[key] = Convert.ToBase64String(value);
            }
        }

        public void WriteString(string key, string value) {
            if(!string.IsNullOrEmpty(value)) {
                dict[key] = Convert.ToBase64String(Encoding.UTF8.GetBytes(value));
            } else {
                dict[key] = EmptyString;
            }
        }
    }
}