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

        /*
        public MapData(MapData data) {
            RegionIDs = data.RegionIDs != null ? new List<long>(data.RegionIDs) : new ();
            SystemIDs = data.SystemIDs != null ? new List<long>(data.SystemIDs) : new ();
            SystemInfos = data.SystemInfos != null ? new Dictionary<long, SystemInfo>(data.SystemInfos) : new ();
        }
        

        public MapData() {
            RegionIDs = new();
            SystemIDs = new();
            SystemInfos = new();
        }
        */

        public bool IsEmpty() {
            if (SystemIDs == null || SystemIDs.Count == 0) return true;
            if (SystemInfos == null || SystemInfos.Count == 0) return true;
            if (RegionIDs == null || RegionIDs.Count == 0) return true;
            
            return false;
        }
    }
}