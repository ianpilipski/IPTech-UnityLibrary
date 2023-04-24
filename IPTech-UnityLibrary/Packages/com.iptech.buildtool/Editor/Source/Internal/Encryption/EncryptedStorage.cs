using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using IPTech.BuildTool.Internal;
using System.Linq;

namespace IPTech.BuildTool.Internal {
    using Encryption;

    public class EncryptedStorage<T> : IEncryptedStorage<T>  where T : class, IEncryptedItemSerialization {
        static readonly ICollection<byte[]> EmptyCollectionByteArray = new List<byte[]>();
        static readonly ICollection<string> EmptyCollectionStrings = new List<string>();
        static readonly string[] EmptyStringArray = new string[0];

        readonly EncryptionUtil encryption;
        readonly string dirPath;
        string password;

        Dictionary<string, EncryptedItemInfo> items;
        DateTime lastModifiedCheck;

        public EncryptedStorage(string dirPath) {
            if(!Path.GetFullPath(dirPath).StartsWith(Path.GetFullPath(Path.Combine(Application.dataPath, "..")))) {
                throw new Exception("the dirPath must be relative to the unity project");
            }
            this.dirPath = dirPath;
            this.encryption = new EncryptionUtil();
            this.LastModified = DateTime.UtcNow;
            items = new Dictionary<string, EncryptedItemInfo>(StringComparer.OrdinalIgnoreCase);
            HasPassword = DoesVerificationFileExists();
        }

        public bool HasPassword { get; private set; }
        public bool IsUnlocked => !string.IsNullOrEmpty(password);
        public DateTime LastModified { get; private set; }

        public void Unlock(string password) {
            VerifyPassword(password);
            this.password = password;
            HasPassword = true;
        }

        public void Lock() {
            this.password = null;
        }

        public int Count {
            get {
                ConditionallyUpdateFiles();
                return items.Count;
            }
        }

        public void Add(string key, T value) {
            AssertKeyArgumentIsValid(key);
            AssertKeyDoesNotExist(key);
            AssertNotNull(value, nameof(value));
            AssertIsUnlocked();

            var data = SerializeItem(value);
            var info = new EncryptedItemInfo() {
                Name = value.Name,
                FullTypeName = value.GetType().FullName
            };
            WriteAllBytesEncrypted(key, data, info);
            LastModified = DateTime.UtcNow;
        }

        

        public void DeleteAllStorageAndPassword() {
            Lock();
            if(Directory.Exists(dirPath)) {
                Directory.Delete(dirPath, true);
                LastModified = DateTime.UtcNow;
                HasPassword = false;
            }
        }

        public bool ContainsKey(string key) {
            AssertKeyArgumentIsValid(key);

            ConditionallyUpdateFiles();
            return items.ContainsKey(key);
        }

        public IEnumerator<EncryptedItemInfo> GetEnumerator() {
            ConditionallyUpdateFiles();
            return items.Values.GetEnumerator();
        }

        public bool Remove(string key) {
            AssertKeyArgumentIsValid(key);
            AssertIsUnlocked();

            if(ContainsKey(key)) {
                File.Delete(Path.Combine(dirPath,KeyToFileName(key)));
                File.Delete(Path.Combine(dirPath, KeyToInfoFileName(key)));
                LastModified = DateTime.UtcNow;
                return true;
            }
            return false;
        }

        public bool TryGetInfo(string key, out EncryptedItemInfo info) {
            if(ContainsKey(key)) {
                info = items[key];
                return true;
            }

            info = default;
            return false;
        }

        public bool TryGetEncryptedValue(string key, out string encryptedValue) {
            if(ContainsKey(key)) {
                encryptedValue = File.ReadAllText(Path.Combine(dirPath, KeyToFileName(key)));
                return true;
            }

            encryptedValue = null;
            return false;
        }

        public bool TryGetDecryptedValue(string key, out T value) {
            AssertIsUnlocked();
            if(TryGetEncryptedValue(key, out string encryptedValue)) {
                var bytes = Decrypt(encryptedValue);
                value = DeserializeItem(items[key], bytes);
                return true;
            }
                
            value = null;
            return false;
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        void AssertIsUnlocked() {
            if(!IsUnlocked) {
                throw new InvalidOperationException("the encrypted storage is locked");
            }
        }

        void AssertKeyArgumentIsValid(string key) {
            if(string.IsNullOrWhiteSpace(key)) {
                throw new ArgumentNullException("the key is null or whitespace");
            }
        }

        void AssertKeyDoesNotExist(string key) {
            if(ContainsKey(key)) {
                throw new Exception("the key already exists in the collection");
            }
        }

        void AssertNotNull(object obj, string argName) {
            if(obj==null) {
                throw new ArgumentNullException(argName);
            }
        }

        void ConditionallyUpdateFiles() {
            if(lastModifiedCheck!=LastModified) {
                lastModifiedCheck = LastModified;
                items.Clear();
                if(Directory.Exists(dirPath)) {
                    var ff = Directory.GetFiles(dirPath, "*.encrypted");
                    if(ff.Length > 0) {
                        foreach(var f in ff) {
                            var key = Path.GetFileNameWithoutExtension(f);
                            var infoFile = Path.Combine(dirPath, KeyToInfoFileName(key));
                            EncryptedItemInfo info = JsonUtility.FromJson<EncryptedItemInfo>(File.ReadAllText(infoFile));
                            items.Add(key, info);
                        }
                    }
                }
            }
        }

        string KeyToFileName(string key) {
            return key.ToLower() + ".encrypted";
        }
        
        string KeyToInfoFileName(string key) {
            return key.ToLower() + ".info";
        }

        void WriteAllBytesEncrypted(string key, byte[] bytes, EncryptedItemInfo info) {
            File.WriteAllText(Path.Combine(dirPath, KeyToFileName(key)), Encrypt(bytes));
            File.WriteAllText(Path.Combine(dirPath, KeyToInfoFileName(key)), JsonUtility.ToJson(info));
        }

        byte[] Decrypt(string encryptedString) {
            return encryption.OpenSSLDecrypt(encryptedString, password);
        }

        string Encrypt(byte[] bytes) {
            return encryption.OpenSSLEncrypt(bytes, password);
        }

        void VerifyPassword(string password) {
            EnsureDirectoryCreated();

            var pwHash = Hash128.Compute(password).ToString();
            var verificationFile = GetVerificationFilePath();

            if(File.Exists(verificationFile)) {
                string hash = File.ReadAllText(verificationFile);
                if(pwHash != hash) {
                    throw new Exception("password is invalid");
                }
            } else {
                File.WriteAllText(verificationFile, pwHash);
            }
        }

        bool DoesVerificationFileExists() {
            return File.Exists(GetVerificationFilePath());
        }

        string GetVerificationFilePath() {
            return Path.Combine(dirPath, "verification");
        }

        void EnsureDirectoryCreated() {
            if(!Directory.Exists(dirPath)) {
                Directory.CreateDirectory(dirPath);
                if(!Directory.Exists(dirPath)) {
                    throw new Exception("unable to create the encrypted directory");
                }
            }
        }

        byte[] SerializeItem(T item) {
            var d = new Dictionary<string,string>();
            item.Serialize(new EncryptedItemWriter(d));
            return DictionaryToBytes(d);
        }

        T DeserializeItem(EncryptedItemInfo info, byte[] bytes) {
            if(TryGetTypeFromFullName(info.FullTypeName, out Type t)) {
                T item = default;
                if(t.IsSubclassOf(typeof(ScriptableObject))) {
                    item = ScriptableObject.CreateInstance(t) as T;
                } else {
                    item = (T)Activator.CreateInstance(t);
                }
                item.Name = info.Name;
                item.Deserialize(new EncryptedItemReader(BytesToDictionary(bytes)));
                return item;
            } else {
                throw new Exception($"could not decrypt type {info.FullTypeName}, not found");
            }
        }

        bool TryGetTypeFromFullName(string fullTypeName, out Type type) {
            var typeList = Context.ListGenerator.GetListImmediate(typeof(T));
            type = typeList.FirstOrDefault(o => o.FullName == fullTypeName);
            return type!=null;
        }

        byte[] DictionaryToBytes(IDictionary<string,string> d) {
            using(var ms = new MemoryStream()) {
                using(var sw = new StreamWriter(ms)) {
                    foreach(var kvp in d) {
                        sw.WriteLine($"{kvp.Key}:{kvp.Value}");
                    }
                }
                return ms.ToArray();
            }
        }

        IDictionary<string,string> BytesToDictionary(byte[] bytes) {
            Dictionary<string,string> retVal = new Dictionary<string, string>();
            using(var ms = new MemoryStream(bytes)) {
                using(var r = new StreamReader(ms)) {
                    string line;
                    while((line = r.ReadLine())!=null) {
                        int i = line.IndexOf(":");
                        var key = line.Substring(0, i);
                        var val = line.Substring(i+1);
                        retVal.Add(key, val);
                    }
                }
            }
            return retVal;
        }
       
    }
}
