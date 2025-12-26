using System;

namespace _ProjectEvE.Scripts.Data {
    [Serializable]
    public class SystemInfo {
        public long constellation_id;
        public string name;
        public Planet[] planets;
        public Position position;
        public string security_class;
        public float security_status;
        public long star_id;
        public long[] stargates;
        public long[] stations;
        public long system_id;
    }
    
    [Serializable]
    public class Planet
    {
        public long[] asteroid_belts;
        public long[] moons;
        public long planet_id;
    }
}