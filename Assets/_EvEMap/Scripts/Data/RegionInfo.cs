using System;

namespace _ProjectEvE.Scripts.Data {
    [Serializable]
    public class RegionInfo {
        public long[] constellations;
        public string description;
        public string name;
        public long region_id;
    }
}