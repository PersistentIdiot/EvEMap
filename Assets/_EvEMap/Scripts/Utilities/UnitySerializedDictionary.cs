using System;
using System.Collections.Generic;
using _ProjectEvE.Scripts.Data;
using _ProjectEvE.Scripts.UX;
using UnityEngine;
using SystemInfo = _ProjectEvE.Scripts.Data.SystemInfo;

namespace _ProjectEvE.Scripts.Utilities {
    [Serializable] public class BuffsDictionary : UnitySerializedDictionary<string, int> {}

    [Serializable] public class ConstellationInfosDictionary : UnitySerializedDictionary<long, ConstellationInfo> {}
    
    [Serializable] public class RegionInfosDictionary : UnitySerializedDictionary<long, RegionInfo> {}
    [Serializable] public class SystemInfosDictionary : UnitySerializedDictionary<long, SystemInfo> {}

    [Serializable] public class UISystemDictionary : UnitySerializedDictionary<long, UISystem> {}

    public abstract class UnitySerializedDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver {
        [SerializeField, HideInInspector]
        private List<TKey> keyData = new List<TKey>();
	
        [SerializeField, HideInInspector]
        private List<TValue> valueData = new List<TValue>();

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            this.Clear();
            for (int i = 0; i < this.keyData.Count && i < this.valueData.Count; i++)
            {
                this[this.keyData[i]] = this.valueData[i];
            }
        }

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            this.keyData.Clear();
            this.valueData.Clear();

            foreach (var item in this)
            {
                this.keyData.Add(item.Key);
                this.valueData.Add(item.Value);
            }
        }
    }
}