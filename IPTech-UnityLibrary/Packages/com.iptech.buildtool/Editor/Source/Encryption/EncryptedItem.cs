using System;
using UnityEngine;

namespace IPTech.BuildTool
{
    public abstract class EncryptedItem : ScriptableObject, IEncryptedItemSerialization {
        string IEncryptedItemSerialization.Name{ get => this.name; set => this.name=value; }
        void IEncryptedItemSerialization.Serialize(IPTech.BuildTool.EncryptedItemWriter writer) => InternalSerialize(writer);
        void IEncryptedItemSerialization.Deserialize(IPTech.BuildTool.EncryptedItemReader reader) => InternalDeserialize(reader);

        protected abstract void InternalSerialize(EncryptedItemWriter writer);
        protected abstract void InternalDeserialize(EncryptedItemReader reader);

        public virtual string NameHintPropertyPath => "";
    }


    public abstract class EncryptedItem<T> : EncryptedItem where T : EncryptedItem<T> {
        [Serializable]
        public class Reference : EncryptedItemInfo, ISerializationCallbackReceiver {
            public Reference() {
                FullTypeName = typeof(T).FullName;
            }

            void ISerializationCallbackReceiver.OnAfterDeserialize()
            {
                FullTypeName = typeof(T).FullName;
            }

            void ISerializationCallbackReceiver.OnBeforeSerialize()
            {
                FullTypeName = typeof(T).FullName;
            }
        }
    }
}