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
    public MapData Data;
    public float SystemDistanceScaling = 0.01f;
    public float SystemObjectScaling = 0.125f;

    [SerializeField] private Dictionary<long,UISystem> systems = new();

    // Start is called before the first frame update
    void Start() {
        InitializeDisplay().Forget();
    }

    private async UniTask InitializeDisplay() {
        await Data.InitializeMapData(new HttpClient());
        //await DisplaySystems(Data.SystemInfos);
    }
    
    public async UniTask SelectSystem(UISystem system) {
        for (int i = 0; i < systems.Count; i++) {
            systems[i].SetSelected(false);
        }

        system.SetSelected(true);
        UIManager.Instance.HideSystemInfo();
        await UIManager.Instance.ShowSystemInfo(system.SystemInfo, system.transform.position);
    }

    private async UniTask DisplaySystems(SystemInfosDictionary systemInfos) {
        foreach (var kvp in systemInfos) {
            var systemInfo = kvp.Value;

            if (systemInfo == null) {
                Debug.Log($"System info for key {kvp.Key} was null");
                continue;
            }
            var system = Instantiate(SystemPrefab, transform);
            
            systems.Add(kvp.Key, system);
            system.Init(systemInfo);
            system.transform.position = new Vector3((float)systemInfo.position.x, (float)systemInfo.position.y, (float)systemInfo.position.z) * SystemDistanceScaling;
            system.transform.localScale = Vector3.one * SystemObjectScaling;
        }
    }
}