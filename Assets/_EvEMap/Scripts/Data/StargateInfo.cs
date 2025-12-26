using System;

namespace _ProjectEvE.Scripts.Data {
    [Serializable]
    public class StargateInfo {
        public Destination destination;
        public string name;
        public Position position;
        public long stargate_id;
        public long system_id;
        public long type_id;
    }

    [Serializable]
    public class Destination {
        public long stargate_id;
        public long system_id;
    }
}