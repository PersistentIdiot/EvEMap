using System.Collections.Generic;
using System.Net.Http.Headers;

namespace _EvEMap.Scripts.Core {
    public class Constants {
        public const string SaveFilePath = "";
        public const string SaveFileName = "MapData.es3";

        public const string RegionIDsKey = "RegionIDs";
        public const string SystemIDsKey = "SystemIDs";
        public const string SystemInfosKey = "SystemInfos";
        public const string ConstellationIDsKey = "ConstellationIDs";
        public const string ConstellationInfosKey = "ConstellationInfos";

        public readonly Dictionary<string, string> Headers = new() {
            {
                "Accept-Language", "en"
            }, {
                "X-Compatibility-Date", "2025-12-16"
            }, {
                "X-Tenant", ""
            }, {
                "Accept", "application/json"
            },
        };
    }
}