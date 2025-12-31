using System;
using System.Collections.Generic;
using System.Net.Http;
using _ProjectEvE.Scripts.Utilities;
using _ProjectEvE.Scripts.UX;
using Cysharp.Threading.Tasks;
using Evo.UI;
using Sirenix.OdinInspector;
using UnityEngine;
using Constants = _EvEMap.Scripts.Core.Constants;

namespace _ProjectEvE.Scripts.Data {
    [CreateAssetMenu(menuName = "EvE Map/Data", fileName = "Map Data")]
    public class MapData : SerializedScriptableObject {
        public List<long> ConstellationIDs {
            get {
                if (constellationIDs != null && constellationIDs.Count == Constants.ConstellationCount) {
                    return constellationIDs;
                }
                else {
                    if (ES3.FileExists(Constants.SaveFileName) && ES3.KeyExists(Constants.ConstellationIDsKey)) {
                        Debug.Log($"Loading Constellation IDs from file");
                        constellationIDs = ES3.Load<List<long>>(Constants.ConstellationIDsKey, Constants.SaveFileName);
                    }

                    if (constellationIDs == null) {
                        Debug.Log($"Failed to load ConstellationIDs from disk. Unable to rebuild here. ");
                    }

                    return new List<long>();
                }
            }
            set { constellationIDs = value; }
        }
        [SerializeField] private List<long> constellationIDs;
        public List<long> RegionIDs;
        public List<long> SystemIDs;
        [HideInInspector] public Dictionary<long, ConstellationInfo> ConstellationInfos = new();
        [HideInInspector] public Dictionary<long, RegionInfo> RegionInfos = new();
        [HideInInspector] public Dictionary<long, SystemInfo> SystemInfos = new();
        [HideInInspector] public Dictionary<long, List<StargateInfo>> StargateInfos = new();

        public async UniTask InitializeMapData(Progress<(float value, string message)> progress, HttpClient client = null) {
            client ??= new HttpClient();

            // Constellation IDs
            if (constellationIDs.Count < Constants.ConstellationCount) {
                if (ES3.FileExists(Constants.SaveFileName) && ES3.KeyExists(Constants.ConstellationIDsKey)) {
                    Debug.Log($"Loading Constellation IDs from file");
                    constellationIDs = ES3.Load<List<long>>(Constants.ConstellationIDsKey);
                }
                else {
                    Debug.Log($"Pulling Constellation IDs from EvE");
                    constellationIDs = await GetConstellationIDs(client);
                    ES3.Save(Constants.ConstellationIDsKey, ConstellationIDs);
                }
            }

            // ConstellationInfos
            if (ConstellationInfos.Count < Constants.ConstellationCount) {
                if (ES3.FileExists(Constants.SaveFileName) && ES3.KeyExists(Constants.ConstellationInfosKey)) {
                    Debug.Log($"Loading Constellation Infos from file");
                    ConstellationInfos = ES3.Load<Dictionary<long, ConstellationInfo>>(Constants.ConstellationInfosKey, Constants.SaveFileName);
                }
                else {
                    Debug.Log($"Pulling Constellation Infos from EvE");
                    ConstellationInfos = await GetConstellationInfos(SystemIDs, client);
                    ES3.Save(Constants.ConstellationInfosKey, ConstellationInfos, Constants.SaveFileName);
                }
            }


            // Region IDS
            if (RegionIDs.Count < Constants.RegionCount) {
                if (ES3.FileExists(Constants.SaveFileName) && ES3.KeyExists(Constants.RegionIDsKey)) {
                    Debug.Log($"Loading Region IDs from file");
                    RegionIDs = ES3.Load<List<long>>(Constants.RegionIDsKey);
                }
                else {
                    Debug.Log($"Pulling Region IDs from EvE");
                    RegionIDs = await GetRegionIDs(client);
                    ES3.Save(Constants.RegionIDsKey, RegionIDs, Constants.SaveFileName);
                }
            }

            // Region Infos
            if (RegionInfos.Count < Constants.RegionCount) {
                if (ES3.FileExists(Constants.SaveFileName) && ES3.KeyExists(Constants.RegionInfosKey)) {
                    Debug.Log($"Loading Region Infos from file");
                    RegionInfos = ES3.Load<Dictionary<long, RegionInfo>>(Constants.RegionInfosKey, Constants.SaveFileName);
                }
                else {
                    Debug.Log($"Pulling Region Infos from EvE");
                    RegionInfos = await GetRegionInfos(RegionIDs, client);
                    ES3.Save(Constants.RegionInfosKey, RegionInfos, Constants.SaveFileName);
                }
            }

            // SystemIDs
            if (SystemIDs.Count < Constants.SystemCount) {
                if (ES3.FileExists(Constants.SaveFileName) && ES3.KeyExists(Constants.SystemIDsKey)) {
                    Debug.Log($"Loading System IDs from file");
                    SystemIDs = ES3.Load<List<long>>(Constants.SystemIDsKey, Constants.SaveFileName);
                }
                else {
                    Debug.Log($"Pulling System IDs from EvE");
                    SystemIDs = await GetSystemIDs(client);
                    ES3.Save(Constants.SystemIDsKey, SystemIDs, Constants.SaveFileName);
                }
            }

            // SystemInfos
            if (SystemInfos.Count < Constants.SystemCount) {
                if (ES3.FileExists(Constants.SaveFileName) && ES3.KeyExists(Constants.SystemInfosKey)) {
                    Debug.Log($"Loading System Infos from file");
                    SystemInfos = ES3.Load<Dictionary<long, SystemInfo>>(Constants.SystemInfosKey, Constants.SaveFileName);
                }
                else {
                    Debug.Log($"Pulling System Infos from EvE");
                    SystemInfos = await GetKspaceInfos(SystemIDs, client);
                    ES3.Save(Constants.SystemInfosKey, SystemInfos, Constants.SaveFileName);
                }
            }
            
            // Stargate Infos
            if (StargateInfos == null ||StargateInfos.Count < Constants.StargatesCount) {
                if (ES3.FileExists(Constants.SaveFileName) && ES3.KeyExists(Constants.StargateInfosKey)) {
                    Debug.Log($"Loading Stargate Infos from file");
                    StargateInfos = ES3.Load<Dictionary<long, List<StargateInfo>>>(Constants.StargateInfosKey, Constants.SaveFileName);
                }
                else {
                    Debug.Log($"Pulling Stargate Infos from EvE");
                    StargateInfos = await GetStargateInfosAsync(progress, SystemIDs, client);
                    ES3.Save(Constants.StargateInfosKey, StargateInfos, Constants.SaveFileName);
                }
            }
        }


        public async UniTask<List<long>> GetConstellationIDs(HttpClient client = null) {
            client ??= new HttpClient();
            List<long> returnValue = new();

            var request = new HttpRequestMessage {
                Method = HttpMethod.Get,
                RequestUri = new Uri("https://esi.evetech.net/universe/constellations"),
                Headers = {
                    {
                        "Accept-Language", "en"
                    }, {
                        "X-Compatibility-Date", "2025-12-16"
                    }, {
                        "X-Tenant", ""
                    }, {
                        "Accept", "application/json"
                    },
                },
            };

            using (var response = await client.SendAsync(request)) {
                response.EnsureSuccessStatusCode();
                var body = await response.Content.ReadAsStringAsync();
                var constellationIDStrings = body.Split(',');

                for (int index = 0; index < constellationIDStrings.Length; index++) {
                    string constellationIDString = constellationIDStrings[index];

                    UIManager.Instance.ProgressBarLabel.text = $"Getting constellation names ({constellationIDString})";
                    UIManager.Instance.ProgressBar.Value = (float)index / constellationIDStrings.Length;

                    if (long.TryParse(constellationIDString, out long constellationID)) {
                        returnValue.Add(constellationID);
                    }
                    else {
                        Debug.Log($"Failed to parse ConstellationID: {constellationIDString}");
                    }
                }
            }

            UIManager.Instance.SetProgressBarVisibility(false);
            return returnValue;
        }

        public async UniTask<Dictionary<long, ConstellationInfo>> GetConstellationInfos(List<long> systemIDs, HttpClient client = null) {
            UIManager.Instance.SetProgressBarVisibility(true);
            client ??= new HttpClient();
            Dictionary<long, ConstellationInfo> constellationInfos = new();

            for (int index = 0; index < systemIDs.Count; index++) {
                var systemID = systemIDs[index];
                UIManager.Instance.ProgressBar.Value = (float)index / systemIDs.Count;
                var constellationInfo = await GetConstellationInfo(systemID, client);
                constellationInfos.Add(systemID, constellationInfo);
            }

            UIManager.Instance.SetProgressBarVisibility(false);
            return constellationInfos;
        }

        public async UniTask<ConstellationInfo> GetConstellationInfo(long systemID, HttpClient client = null) {
            client ??= new HttpClient();
            var request = new HttpRequestMessage {
                Method = HttpMethod.Get,
                RequestUri = new Uri($"https://esi.evetech.net/universe/constellations/{systemID}"),
                Headers = {
                    {
                        "Accept-Language", "en"
                    }, {
                        "X-Compatibility-Date", "2025-12-16"
                    }, {
                        "X-Tenant", ""
                    }, {
                        "Accept", "application/json"
                    },
                },
            };

            using (var response = await client.SendAsync(request)) {
                response.EnsureSuccessStatusCode();
                var body = await response.Content.ReadAsStringAsync();
                var info = JsonUtility.FromJson<ConstellationInfo>(body);
                return info;
            }
        }

        public async UniTask<Dictionary<long, RegionInfo>> GetRegionInfos(List<long> regionIDs, HttpClient client = null) {
            client ??= new HttpClient();
            Dictionary<long, RegionInfo> returnValue = new Dictionary<long, RegionInfo>();

            foreach (var regionID in regionIDs) {
                var regionInfo = await GetRegionInfo(regionID, client);
                returnValue.Add(regionID, regionInfo);
            }

            return returnValue;
        }

        public async UniTask<RegionInfo> GetRegionInfo(long regionID, HttpClient client = null) {
            if (RegionInfos.TryGetValue(regionID, out RegionInfo regionInfo)) {
                return regionInfo;
            }


            client ??= new HttpClient();
            var request = new HttpRequestMessage {
                Method = HttpMethod.Get,
                RequestUri = new Uri($"https://esi.evetech.net/universe/regions/{regionID}"),
                Headers = {
                    {
                        "Accept-Language", "en"
                    }, {
                        "X-Compatibility-Date", "2025-12-16"
                    }, {
                        "X-Tenant", ""
                    }, {
                        "Accept", "application/json"
                    },
                },
            };

            using (var response = await client.SendAsync(request)) {
                response.EnsureSuccessStatusCode();
                var body = await response.Content.ReadAsStringAsync();
                return JsonUtility.FromJson<RegionInfo>(body);
            }
        }

        public async UniTask<List<long>> GetSystemIDs(HttpClient client = null) {
            UIManager.Instance.SetProgressBarVisibility(true);
            List<long> returnValue = new();
            client ??= new HttpClient();

            var request = new HttpRequestMessage {
                Method = HttpMethod.Get,
                RequestUri = new Uri("https://esi.evetech.net/universe/systems"),
                Headers = {
                    {
                        "Accept-Language", "en"
                    }, {
                        "X-Compatibility-Date", "2025-12-16"
                    }, {
                        "X-Tenant", ""
                    }, {
                        "Accept", "application/json"
                    },
                },
            };


            using (var response = await client.SendAsync(request)) {
                response.EnsureSuccessStatusCode();
                var body = await response.Content.ReadAsStringAsync();
                string[] systemIDNames = body.Split(',');

                UIManager.Instance.ProgressBarLabel.text = $"Getting system IDs...";

                // Parse system name strings to ints
                for (int index = 0; index < systemIDNames.Length; index++) {
                    string systemIDName = systemIDNames[index];
                    UIManager.Instance.ProgressBar.Value = (float)index / systemIDNames.Length;
                    UIManager.Instance.ProgressBarLabel.text = $"Getting system names ({systemIDName})";

                    if (long.TryParse(systemIDName, out long systemID)) {
                        returnValue.Add(systemID);
                    }
                    else {
                        Debug.Log($"Failed to parse SystemID: {systemIDName}");
                    }
                }
            }

            UIManager.Instance.SetProgressBarVisibility(false);
            return returnValue;
        }

        public async UniTask<Dictionary<long, List<StargateInfo>>> GetStargateInfosAsync(IProgress<(float progress, string message)> progress,
                                                                                         List<long> systemIDs,
                                                                                         HttpClient client = null) {
            Dictionary<long, List<StargateInfo>> returnValue = new();
            client ??= new HttpClient();

            await UniTask.WaitForSeconds(1f);

            for (int index = 0; index < systemIDs.Count; index++) {
                long systemID = systemIDs[index];
                
                if (SystemInfos.TryGetValue(systemID, out SystemInfo systemInfo)) {
                    var stargateInfos = await GetStargateInfosForSystem(systemInfo, client);
                    progress.Report(((float)index / systemIDs.Count, $"Getting Stargate info for {systemInfo.name}"));
                    returnValue.Add(systemID, stargateInfos);
                }
                else {
                    progress.Report(((float)index / systemIDs.Count, $"Skipping SystemID ({systemID}) as it is not present in dictionary. ToDo: Populate it here."));
                    continue;
                }
            }

            return returnValue;
        }

        public async UniTask<List<StargateInfo>> GetStargateInfosForSystem(SystemInfo systemInfo, HttpClient client = null) {
            Debug.Assert(systemInfo != null);
            List<StargateInfo> returnValue = new();
            client ??= new HttpClient();

            if (systemInfo.stargates == null) {
                systemInfo.stargates = Array.Empty<long>();
            }

            // Send stargate info requests
            for (int index = 0; index < systemInfo.stargates.Length; index++) {
                long ID = systemInfo.stargates[index];
                var request = new HttpRequestMessage {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri($"https://esi.evetech.net/universe/stargates/{ID}"),
                    Headers = {
                        {
                            "Accept-Language", "en"
                        }, {
                            "X-Compatibility-Date", "2025-12-16"
                        }, {
                            "X-Tenant", ""
                        }, {
                            "Accept", "application/json"
                        },
                    },
                };


                using (var response = await client.SendAsync(request)) {
                    response.EnsureSuccessStatusCode();
                    var body = await response.Content.ReadAsStringAsync();
                    //SystemInfo systemInfo = JsonUtility.FromJson<SystemInfo>(body);
                    StargateInfo stargateInfo = JsonUtility.FromJson<StargateInfo>(body);

                    if (stargateInfo != null) {
                        returnValue.Add(stargateInfo);
                    }
                    else {
                        Debug.Log($"Failed to parse");
                    }
                }
            }

            UIManager.Instance.SetProgressBarVisibility(false);
            return returnValue;
        }

        /// <summary>
        /// Gets a list of System Infos from a list of System IDs
        /// </summary>
        /// <param name="systemIDs"></param>
        /// <param name="httpClient"></param>
        /// <returns></returns>
        public async UniTask<SystemInfosDictionary> GetKspaceInfos(List<long> systemIDs, HttpClient client = null, ProgressBar progressBar = null) {
            UIManager.Instance.SetProgressBarVisibility(true);
            SystemInfosDictionary returnValue = new();
            client ??= new HttpClient();

            // Send system info requests
            for (int index = 0; index < systemIDs.Count; index++) {
                if (systemIDs[index] >= 30999999) continue;
                UIManager.Instance.SetProgressBarVisibility(true);
                UIManager.Instance.ProgressBar.Value = (float)index / systemIDs.Count;
                long ID = systemIDs[index];

                var systemInfo = await GetSystemInfo(ID, client);
                returnValue.Add(ID, systemInfo);
            }

            UIManager.Instance.SetProgressBarVisibility(false);
            return returnValue;
        }

        public async UniTask<SystemInfo> GetSystemInfo(long systemID, HttpClient client = null) {
            client ??= new HttpClient();
            var request = new HttpRequestMessage {
                Method = HttpMethod.Get,
                RequestUri = new Uri($"https://esi.evetech.net/universe/systems/{systemID}"),
                Headers = {
                    {
                        "Accept-Language", "en"
                    }, {
                        "X-Compatibility-Date", "2025-12-16"
                    }, {
                        "X-Tenant", ""
                    }, {
                        "Accept", "application/json"
                    },
                },
            };


            using (var response = await client.SendAsync(request)) {
                response.EnsureSuccessStatusCode();
                var body = await response.Content.ReadAsStringAsync();
                SystemInfo systemInfo = JsonUtility.FromJson<SystemInfo>(body);

                if (systemInfo != null) {
                    UIManager.Instance.ProgressBarLabel.text = $"Getting system information ({systemInfo.name})";
                }
                else {
                    Debug.Log($"Failed to parse ID: {systemID}");
                }

                return systemInfo;
            }

        }

        public async UniTask<List<long>> GetRegionIDs(HttpClient client = null) {
            UIManager.Instance.SetProgressBarVisibility(true);
            List<long> returnValue = new();
            client ??= new HttpClient();
            var request = new HttpRequestMessage {
                Method = HttpMethod.Get,
                RequestUri = new Uri("https://esi.evetech.net/universe/regions"),
                Headers = {
                    {
                        "Accept-Language", "en"
                    }, {
                        "X-Compatibility-Date", "2025-12-16"
                    }, {
                        "X-Tenant", ""
                    }, {
                        "Accept", "application/json"
                    },
                },
            };

            using (var response = await client.SendAsync(request)) {
                response.EnsureSuccessStatusCode();
                var body = await response.Content.ReadAsStringAsync();
                string[] regionIDNames = body.Split(',');

                UIManager.Instance.ProgressBarLabel.text = $"Getting region IDs...";

                // Parse system name strings to ints
                for (int index = 0; index < regionIDNames.Length; index++) {
                    string regionIDName = regionIDNames[index];
                    UIManager.Instance.ProgressBar.Value = (float)index / regionIDNames.Length;

                    if (long.TryParse(regionIDName, out long systemID)) {
                        UIManager.Instance.ProgressBarLabel.text = $"Getting region names ({systemID})";
                        returnValue.Add(systemID);
                    }
                    else {
                        Debug.Log($"Failed to parse region ID: {regionIDName}");
                    }
                }
            }

            UIManager.Instance.SetProgressBarVisibility(false);
            return returnValue;
        }
    }
}