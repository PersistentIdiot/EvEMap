using System;

namespace _ProjectEvE.Scripts.Data {
    [Serializable]
    public class PlanetInfo {
        public string name;
        public long planet_id;
        public Position position;
        public long system_id;
        public long type_id;
    }
}