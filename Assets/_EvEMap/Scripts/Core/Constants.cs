using System.Collections.Generic;
using System.Net.Http.Headers;

namespace _EvEMap.Scripts.Core {
    #region GameNotes
    // System < Constellation < Region
    #endregion
    public class Constants {
        public const string GameName = "EvE Map";
        public const string SaveFilePath = "";
        public const string SaveFileName = "MapData.es3";

        public const string RegionIDsKey = "RegionIDs";
        public const string ConstellationIDsKey = "ConstellationIDs";
        public const string SystemIDsKey = "SystemIDs";
        
        
        public const string RegionInfosKey = "RegionInfos";
        public const string ConstellationInfosKey = "ConstellationInfos";
        public const string SystemInfosKey = "SystemInfos";
        public const string StargateInfosKey = "StargateInfos";

        public const int ConstellationCount = 1173;
        public const int RegionCount = 111;
        public const int SystemCount = 8435;
        public const int StargatesCount = 10;
    }
}