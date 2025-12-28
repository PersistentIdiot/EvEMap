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
    public static MapData Data { get => Instance.data; }
    [SerializeField] private MapData data;
    public double SystemDistanceScaling = 1000000;
    public float SystemObjectScaling = 0.125f;

    [SerializeField] private UISystemDictionary systems = new();

    // Start is called before the first frame update
    void Start() {
        InitializeDisplay().Forget();
    }

    private async UniTask InitializeDisplay() {
        await Data.InitializeMapData(new HttpClient());
        await DisplaySystems(Data.SystemInfos);
    }

    public async UniTask SelectSystem(UISystem system) {
        foreach (var kvp in systems) {
            kvp.Value.SetSelected(false);
        }

        system.SetSelected(true);
        UIManager.Instance.HideSystemInfo();
        await UIManager.Instance.ShowSystemInfo(system.SystemInfo, system.transform.position);
    }

    private async UniTask DisplaySystems(Dictionary<long, SystemInfo> systemInfos) {
        Debug.Log($"Displaying systems");
        int systemsThisBatch = 0;
        foreach (var kvp in systemInfos) {
            var systemInfo = kvp.Value;

            if (systemInfo == null) {
                Debug.Log($"System info for key {kvp.Key} was null");
                continue;
            }

            if (systemInfo.system_id >= 30999999) continue;

            var system = Instantiate(SystemPrefab, transform);
            systemsThisBatch++;

            if (systemsThisBatch > 10) {
                systemsThisBatch = 0;
                await UniTask.WaitForEndOfFrame();
            }

            system.Init(systemInfo);
            system.transform.position = new Vector3(
                (float)(systemInfo.position.x / SystemDistanceScaling),
                (float)(systemInfo.position.y / SystemDistanceScaling),
                (float)(systemInfo.position.z / SystemDistanceScaling));
            system.transform.localScale = Vector3.one * SystemObjectScaling;
            systems.Add(kvp.Key, system);
        }
    }
}