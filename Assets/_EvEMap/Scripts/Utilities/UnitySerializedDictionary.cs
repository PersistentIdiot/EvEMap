using System;
using System.Collections.Generic;
using UnityEngine;
using SystemInfo = _ProjectEvE.Scripts.Data.SystemInfo;

namespace _ProjectEvE.Scripts.Utilities {
    [Serializable] public class BuffsDictionary : UnitySerializedDictionary<string, int> {}

    [Serializable] public class SystemInfosDictionary : UnitySerializedDictionary<long, SystemInfo> {}
    [Serializable] public class ConstellationInfosDictionary : UnitySerializedDictionary<long, ConstellationInfo> {}
    public abstract class UnitySerializedDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver {
        [SerializeField] protected List<KeyValueData> keyValueData = new();

        public void OnBeforeSerialize() {
            keyValueData.Clear();

            foreach (var kvp in this) {
                keyValueData.Add(
                    new KeyValueData() {
                        key = kvp.Key,
                        value = kvp.Value
                    });
            }
        }

        public void OnAfterDeserialize() {
            Clear();

            foreach (var item in keyValueData) {
                this[item.key] = item.value;
            }
        }

        [Serializable]
        protected struct KeyValueData {
            public TKey key;
            public TValue value;
        }
    }
}