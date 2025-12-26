using System;
using System.Collections.Generic;
using System.Net.Http;
using _EvEMap.Scripts.Core;
using _ProjectEvE.Scripts.Utilities;
using _ProjectEvE.Scripts.UX;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace _ProjectEvE.Scripts.Data {
    [CreateAssetMenu(menuName = "EvE Map/Data", fileName = "Map Data")]
    public class MapData : ScriptableObject {
        public List<Int64> ConstellationIDs = new();
        public List<Int64> RegionIDs = new();
        public List<Int64> SystemIDs = new();
        public SystemInfosDictionary SystemInfos = new();
        public ConstellationInfosDictionary ConstellationInfos = new();


        public async UniTask InitializeMapData(HttpClient client = null) {
            client ??= new HttpClient();

            // Constellation IDs
            if (ConstellationIDs.Count < Constants.ConstellationCount) {
                if (ES3.FileExists(Constants.SaveFileName) && ES3.KeyExists(Constants.ConstellationIDsKey)) {
                    Debug.Log($"Loading Constellation IDs from file");
                    ConstellationIDs = ES3.Load<List<long>>(Constants.ConstellationIDsKey);
                }
                else {
                    Debug.Log($"Pulling Constellation IDs from EvE");
                    ConstellationIDs = await GetConstellationIDs(client);
                    ES3.Save(Constants.ConstellationIDsKey, ConstellationIDs);
                }
            }

            // ConstellationInfos
            if (ConstellationInfos.Count < Constants.ConstellationCount) {
                if (ES3.FileExists(Constants.SaveFileName) && ES3.KeyExists(Constants.ConstellationInfosKey)) {
                    Debug.Log($"Loading Constellation Infos from file");
                    ConstellationInfos = ES3.Load<ConstellationInfosDictionary>(Constants.ConstellationInfosKey, Constants.SaveFileName);
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
            if (SystemIDs.Count < Constants.SystemCount) {
                if (ES3.FileExists(Constants.SaveFileName) && ES3.KeyExists(Constants.SystemInfosKey)) {
                    Debug.Log($"Loading System Infos from file");
                    SystemInfos = ES3.Load<SystemInfosDictionary>(Constants.SystemInfosKey, Constants.SaveFileName);
                }
                else {
                    Debug.Log($"Pulling System Infos from EvE");
                    SystemInfos = await GetSystemInfos(SystemIDs, client);
                    ES3.Save(Constants.SystemInfosKey, SystemInfos, Constants.SaveFileName);
                }
            }
        }


        private async UniTask<List<long>> GetConstellationIDs(HttpClient client = null) {
            UIManager.Instance.SetProgressBarVisibility(true);
            client ??= new HttpClient();
            List<long> constellationIDs = new();

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
                        constellationIDs.Add(constellationID);
                    }
                    else {
                        Debug.Log($"Failed to parse ConstellationID: {constellationIDString}");
                    }
                }
            }

            UIManager.Instance.SetProgressBarVisibility(false);
            return constellationIDs;
        }

        private async UniTask<ConstellationInfosDictionary> GetConstellationInfos(List<long> systemIDs, HttpClient client = null) {
            UIManager.Instance.SetProgressBarVisibility(true);
            client ??= new HttpClient();
            ConstellationInfosDictionary constellationInfos = new();

            for (int index = 0; index < systemIDs.Count; index++) {
                var systemID = systemIDs[index];
                UIManager.Instance.ProgressBar.Value = (float)index / systemIDs.Count;
                var constellationInfo = await GetConstellationInfo(systemID, client);
                constellationInfos.Add(systemID, constellationInfo);
            }

            UIManager.Instance.SetProgressBarVisibility(false);
            return constellationInfos;
        }

        private async UniTask<ConstellationInfo> GetConstellationInfo(long systemID, HttpClient client = null) {
            client ??= new HttpClient();
            var request = new HttpRequestMessage {
                Method = HttpMethod.Get,
                RequestUri = new Uri($"https://esi.evetech.net/universe/constellations/{systemID}"),
                Headers = {
                    {
                        "Accept-Language", ""
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

        private async UniTask<List<long>> GetSystemIDs(HttpClient client = null) {
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

        public async UniTask<List<StargateInfo>> GetStargateInfos(SystemInfo systemInfo) {

            UIManager.Instance.SetProgressBarVisibility(true);
            List<StargateInfo> returnValue = new();
            var client = new HttpClient();

            // Send system info requests
            for (int index = 0; index < systemInfo.stargates.Length; index++) {
                UIManager.Instance.ProgressBar.Value = (float)index / systemInfo.stargates.Length;
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
                        UIManager.Instance.ProgressBarLabel.text = $"Getting stargate info for ({systemInfo.name})";
                        returnValue.Add(stargateInfo);
                        //SystemInfos.Add(systemInfo);
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
        private async UniTask<SystemInfosDictionary> GetSystemInfos(List<long> systemIDs, HttpClient client) {
            UIManager.Instance.SetProgressBarVisibility(true);
            SystemInfosDictionary returnValue = new();
            client ??= new HttpClient();

            // Send system info requests
            for (int index = 0; index < systemIDs.Count; index++) {
                UIManager.Instance.ProgressBar.Value = (float)index / systemIDs.Count;
                long ID = systemIDs[index];

                var systemInfo = await GetSystemInfo(ID, client);
                returnValue.Add(ID, systemInfo);
            }

            UIManager.Instance.SetProgressBarVisibility(false);
            return returnValue;
        }

        private async UniTask<SystemInfo> GetSystemInfo(long systemID, HttpClient client = null) {
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

        private async UniTask<List<long>> GetRegionIDs(HttpClient client = null) {
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