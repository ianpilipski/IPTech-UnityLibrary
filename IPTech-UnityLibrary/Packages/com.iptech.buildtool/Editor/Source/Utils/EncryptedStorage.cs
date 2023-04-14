using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using System.Linq;

namespace IPTech.BuildTool {
    public class EncryptedStorage : IEnumerable<string> {
        static readonly ICollection<byte[]> EmptyCollectionByteArray = new List<byte[]>();
        static readonly ICollection<string> EmptyCollectionStrings = new List<string>();
        static readonly string[] EmptyStringArray = new string[0];

        readonly Encryption encryption;
        readonly string dirPath;
        string password;

        public EncryptedStorage(string dirPath) {
            if(!Path.GetFullPath(dirPath).StartsWith(Path.GetFullPath(Path.Combine(Application.dataPath, "..")))) {
                throw new Exception("the dirPath must be relative to the unity project");
            }
            this.dirPath = dirPath;
            this.encryption = new Encryption();
        }

        public bool IsUnlocked => !string.IsNullOrEmpty(password);

        public void Unlock(string password) {
            VerifyPassword(password);
            this.password = password;
        }

        public void Lock() {
            this.password = null;
        }

        public int Count {
            get {
                if(TryGetFiles(out string[] files)) {
                    return files.Length;
                }
                return 0;
            }
        }

        public void Add(string key, byte[] value) {
            AssertKeyArgumentIsValid(key);
            
            if(ContainsKey(key)) {
                throw new ArgumentException("the key already exists in the dictionary");
            }

            if(value==null || value.Length==0) {
                throw new Exception("the byte array is null or empty");
            }
            AssertIsUnlocked();

            WriteAllBytesEncrypted(KeyToFileName(key), value);
        }

        

        public void DeleteAllStorageAndPassword() {
            Lock();
            if(Directory.Exists(dirPath)) {
                Directory.Delete(dirPath, true);
            }
        }

        public bool ContainsKey(string key) {
            AssertKeyArgumentIsValid(key);

            if(TryGetFiles(out string[] files)) {
                var keyFileName = KeyToFileName(key);
                return files.Any(f => f.Equals(keyFileName, StringComparison.OrdinalIgnoreCase));
            }
            return false;
        }

        public IEnumerator<string> GetEnumerator() {
            if(TryGetFiles(out string[] files)) {
                foreach(var f in files) {
                    yield return FileNameToKey(f);
                }
            }
        }

        public bool Remove(string key) {
            AssertKeyArgumentIsValid(key);
            AssertIsUnlocked();

            if(ContainsKey(key)) {
                File.Delete(Path.Combine(dirPath,KeyToFileName(key)));
                return true;
            }
            return false;
        }

        
        public bool TryGetValue(string key, out byte[] value) {
            AssertIsUnlocked();
            AssertKeyArgumentIsValid(key);

            if(ContainsKey(key)) {
                value = ReadAllBytesDecrypted(KeyToFileName(key));
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

        bool TryGetFiles(out string[] files) {
            if(Directory.Exists(dirPath)) {
                files = Directory.GetFiles(dirPath, "*.encrypted");
                if(files.Length>0) {
                    files = files.Select(f => Path.GetFileName(f)).ToArray();
                    return true;
                }
            }
            files = EmptyStringArray;
            return false;
        }

        string FileNameToKey(string filename) {
            return filename.Substring(0, filename.Length - ".encrypted".Length);
        }

        string KeyToFileName(string key) {
            return key.ToLower() + ".encrypted";
        }

        byte[] ReadAllBytesDecrypted(string filename) {
            return Decrypt(File.ReadAllText(Path.Combine(dirPath, filename)));
        }

        void WriteAllBytesEncrypted(string filename, byte[] bytes) {
            File.WriteAllText(Path.Combine(dirPath, filename), Encrypt(bytes));
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
            var verificationFile = Path.Combine(dirPath, "verification");

            if(File.Exists(verificationFile)) {
                string hash = File.ReadAllText(verificationFile);
                if(pwHash != hash) {
                    throw new Exception("password is invalid");
                }
            } else {
                File.WriteAllText(verificationFile, pwHash);
            }
        }

        void EnsureDirectoryCreated() {
            if(!Directory.Exists(dirPath)) {
                Directory.CreateDirectory(dirPath);
                if(!Directory.Exists(dirPath)) {
                    throw new Exception("unable to create the encrypted directory");
                }
            }
        }

       
    }
}
