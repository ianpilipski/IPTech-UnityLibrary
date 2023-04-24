using System.Collections.Generic;
using System;

namespace IPTech.BuildTool
{

    public interface IEncryptedItemSerialization {
        string Name { get; set; }
        void Serialize(EncryptedItemWriter writer);
        void Deserialize(EncryptedItemReader reader);
    }

    public interface IEncryptedStorage<T> : IEnumerable<EncryptedItemInfo> {
        void Lock();
        void Unlock(string password);
        bool IsUnlocked { get; }
        int Count { get; }
        void Add(string key, T value);
        void DeleteAllStorageAndPassword();
        bool ContainsKey(string key);
        bool Remove(string key);
        DateTime LastModified { get; }
        bool TryGetInfo(string key, out EncryptedItemInfo info);
        bool TryGetDecryptedValue(string key, out T value);
        bool TryGetEncryptedValue(string key, out string encryptedValue);
    }
}
