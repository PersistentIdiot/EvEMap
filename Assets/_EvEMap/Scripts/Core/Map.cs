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
        progress.ProgressChanged += (_, tuple) => {
            UIManager.Instance.ProgressBar.gameObject.SetActive(true);
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
        // Grab only non-null K-Space systems
        var systemsToDisplay = Data.SystemInfos.Values
            .Where(s => s != null && s.system_id < 30999999)
            .ToList();

        // Init variables for progress bar
        int total = systemsToDisplay.Count;
        int systemsDisplayed = 0;


       await  UniTask.WaitForSeconds(1f);

        foreach (var systemInfo in systemsToDisplay) {
            await UniTask.NextFrame();
            var system = Instantiate(SystemPrefab, transform);
            
            // Give UISystem its SystemInfo
            system.Init(systemInfo);
            
            // Update position and scale
            system.transform.position = new Vector3(
                (float)(systemInfo.position.x / SystemDistanceScaling),
                (float)(systemInfo.position.y / SystemDistanceScaling),
                (float)(systemInfo.position.z / SystemDistanceScaling));
            system.transform.localScale = Vector3.one * SystemObjectScaling;
            
            // Add system to dictionary for later use
            systems.Add(systemInfo.system_id, system);
            
            var t = (float)systemsDisplayed/total;
            Debug.Log($"{systemsDisplayed}/{total}, {t*100:F0}%");
            systemsDisplayed++;
            await UniTask.NextFrame();
            
            // Update progress bar via callback
            progress.Report(((float)systemsDisplayed/total, $"Loading {systemInfo.name}"));
        }
        UIManager.Instance.ProgressBar.gameObject.SetActive(false);
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