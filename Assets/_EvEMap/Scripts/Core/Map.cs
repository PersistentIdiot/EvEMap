using System;
using System.Collections.Generic;
using System.Net.Http;
using _EvEMap.Scripts.Core;
using _ProjectEvE.Scripts.Data;
using _ProjectEvE.Scripts.Utilities;
using _ProjectEvE.Scripts.UX;
using Cysharp.Threading.Tasks;
using UnityEngine;
using SystemInfo = _ProjectEvE.Scripts.Data.SystemInfo;

public class Map : Singleton<Map> {
    [SerializeField] private UISystem SystemPrefab;
    [SerializeField] private MapData Data = new();
    public float SystemDistanceScaling = 0.01f;
    public float SystemObjectScaling = 0.125f;

    private List<UISystem> systems = new();

    // Start is called before the first frame update
    void Start() {
        InitializeMapData().Forget();
    }
    
    public void SelectSystem(UISystem system) {
        for (int i = 0; i < systems.Count; i++) {
            systems[i].SetSelected(false);
        }

        system.SetSelected(true);
        UIManager.Instance.HideSystemInfo();
        UIManager.Instance.ShowSystemInfo(system.SystemInfo, system.transform.position).Forget();
    }

    private async UniTask InitializeMapData() {
        var client = new HttpClient();
        
        // Constellation IDs
        if (ES3.FileExists(Constants.SaveFileName) && ES3.KeyExists(Constants.ConstellationIDsKey)) {
            Debug.Log($"Loading Constellation IDs from file");
            Data.ConstellationIDs = ES3.Load<List<long>>(Constants.ConstellationIDsKey);
        }
        else {
            Debug.Log($"Pulling Constellation IDs from EvE");
            Data.ConstellationIDs = await GetConstellationIDs(client);
            ES3.Save(Constants.ConstellationIDsKey, Data.ConstellationIDs);
        }
        
        // ConstellationInfos
        if (ES3.FileExists(Constants.SaveFileName) && ES3.KeyExists(Constants.ConstellationInfosKey)) {
            Debug.Log($"Loading Constellation Infos from file");
            Data.ConstellationInfos = ES3.Load<Dictionary<long, ConstellationInfo>>(Constants.ConstellationInfosKey, Constants.SaveFileName);
        }
        else {
            Debug.Log($"Pulling Constellation Infos from EvE");
            Data.ConstellationInfos = await GetConstellationInfos(Data.SystemIDs, client);
            ES3.Save(Constants.ConstellationInfosKey, Data.ConstellationInfos, Constants.SaveFileName);
        }
        

        // Region IDS
        if (ES3.FileExists(Constants.SaveFileName) && ES3.KeyExists(Constants.RegionIDsKey)) {
            Debug.Log($"Loading Region IDs from file");
            Data.RegionIDs = ES3.Load<List<long>>(Constants.RegionIDsKey);
        }
        else {
            Debug.Log($"Pulling Region IDs from EvE");
            Data.RegionIDs = await GetRegionIDs();
            ES3.Save(Constants.RegionIDsKey, Data.RegionIDs, Constants.SaveFileName);
        }

        // SystemIDs
        if (ES3.FileExists(Constants.SaveFileName) && ES3.KeyExists(Constants.SystemIDsKey)) {
            Debug.Log($"Loading System IDs from file");
            Data.SystemIDs = ES3.Load<List<long>>(Constants.SystemIDsKey, Constants.SaveFileName);
        }
        else {
            Debug.Log($"Pulling System IDs from EvE");
            Data.SystemIDs = await GetSystemIDs(client);
            ES3.Save(Constants.SystemIDsKey, Data.SystemIDs, Constants.SaveFileName);
        }

        // SystemInfos
        if (ES3.FileExists(Constants.SaveFileName) && ES3.KeyExists(Constants.SystemInfosKey)) {
            Debug.Log($"Loading System Infos from file");
            Data.SystemInfos = ES3.Load<Dictionary<long, SystemInfo>>(Constants.SystemInfosKey, Constants.SaveFileName);
        }
        else {
            Debug.Log($"Pulling System Infos from EvE");
            Data.SystemInfos = await GetSystemInfos(Data.SystemIDs);
            ES3.Save(Constants.SystemInfosKey, Data.SystemInfos, Constants.SaveFileName);
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

    private async UniTask<Dictionary<long, ConstellationInfo>> GetConstellationInfos(List<long> systemIDs, HttpClient client = null) {
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
                    //Data.SystemInfos.Add(systemInfo);
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
    /// <returns></returns>
    private async UniTask<Dictionary<long, SystemInfo>> GetSystemInfos(List<long> systemIDs) {
        UIManager.Instance.SetProgressBarVisibility(true);
        Dictionary<long, SystemInfo> returnValue = new();
        var client = new HttpClient();

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

    private async UniTask<List<long>> GetRegionIDs() {
        UIManager.Instance.SetProgressBarVisibility(true);
        List<long> returnValue = new();
        var client = new HttpClient();
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
            Console.WriteLine(body);

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

    private async UniTask DisplaySystems() {
        /*
        foreach (SystemInfo systemInfo in Data.SystemInfos) {
            var system = Instantiate(SystemPrefab, transform);
            systems.Add(system);
            system.Init(systemInfo);
            system.transform.position = new Vector3((float)systemInfo.position.x, (float)systemInfo.position.y, (float)systemInfo.position.z) * SystemDistanceScaling;
            system.transform.localScale = Vector3.one * SystemObjectScaling;
        }
        
        */
    }
}