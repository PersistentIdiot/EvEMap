using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
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
        var progress = new Progress<(float value, string message)>();
        //progress.ProgressChanged += (sender, tuple) => Debug.Log($"Value: {tuple.value}, Message: {tuple.message}");
        progress.ProgressChanged += (sender, tuple) => {
            UIManager.Instance.ProgressBar.Value = tuple.value;
            UIManager.Instance.ProgressBar.labelText.text = tuple.message;
        };

        await Data.InitializeMapData(new HttpClient());
        //await DisplaySystems(Data.SystemInfos);
        await DisplaySystemsAsync(progress);
    }


    private async UniTask<(bool hasValue, float value)> TryGetFloatAsync(IProgress<(float progress, string message)> progress) {
        // do stuff

        float t = 0;

        while (t < 1f) {
            progress.Report((t, "Processing"));
            t += 0.1f;
            await UniTask.Yield();
        }

        if (t > 1f) {
            return (true, t);
        }
        else {
            return (false, default);
        }
    }

    private async UniTask<bool> DisplaySystemsAsync(IProgress<(float progress, string message)> progress) {
        int systemsDisplayed = 0;
        foreach (SystemInfo systemInfo in from kvp in Data.SystemInfos
            select kvp.Value
            into systemInfo
            where systemInfo != null
            where systemInfo.system_id < 30999999
            select systemInfo) {
            progress.Report(((float)systemsDisplayed/data.SystemInfos.Count, $"Loading {systemInfo.name}"));
            
            var system = Instantiate(SystemPrefab, transform);
            system.Init(systemInfo);
            system.transform.position = new Vector3(
                (float)(systemInfo.position.x / SystemDistanceScaling),
                (float)(systemInfo.position.y / SystemDistanceScaling),
                (float)(systemInfo.position.z / SystemDistanceScaling));
            system.transform.localScale = Vector3.one * SystemObjectScaling;
            systems.Add(systemInfo.system_id, system);
            
            
            systemsDisplayed++;
            await UniTask.Yield();
        }

        return true;
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

        foreach (var kvp in systemInfos) {
            var systemInfo = kvp.Value;

            if (systemInfo == null) {
                Debug.Log($"System info for key {kvp.Key} was null");
                continue;
            }

            if (systemInfo.system_id >= 30999999) continue;

            var system = Instantiate(SystemPrefab, transform);
            await UniTask.Yield();

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