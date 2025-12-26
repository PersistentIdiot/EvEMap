using System;
using System.Collections.Generic;

namespace _ProjectEvE.Scripts.Data {
    [Serializable]
    public class MapData {
        public List<long> ConstellationIDs = new();
        public List<long> RegionIDs = new();
        public List<long> SystemIDs = new();
        public Dictionary<long, SystemInfo> SystemInfos = new();
        public Dictionary<long, ConstellationInfo> ConstellationInfos = new();
        

        public bool IsEmpty() {
            if (SystemIDs == null || SystemIDs.Count == 0) return true;
            if (SystemInfos == null || SystemInfos.Count == 0) return true;
            if (RegionIDs == null || RegionIDs.Count == 0) return true;
            
            return false;
        }
    }
}