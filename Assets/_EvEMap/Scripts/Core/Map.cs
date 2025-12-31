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
using DG.Tweening;
using Shapes;
using Sirenix.OdinInspector;
using UnityEngine;
using SystemInfo = _ProjectEvE.Scripts.Data.SystemInfo;


public class Map : Singleton<Map> {
    public int MapDebugMode = 1;
    [SerializeField, BoxGroup("References")] private UISystem SystemPrefab;
    public static MapData Data { get => Instance.data; }
    [SerializeField, BoxGroup("References")] private MapData data;

    [BoxGroup("Settings")] public float SystemObjectScaling = 0.125f;
    [BoxGroup("Settings")] public float MinZoomAmount = 0.1f;
    [BoxGroup("Settings")] public float MaxZoomAmount = 100f;

    [SerializeField, BoxGroup("Debug")] private UISystemDictionary systems = new();
    [SerializeField, BoxGroup("Debug")] private UISystem selectedSystem;
    [SerializeField, BoxGroup("Debug")] private MapModes MapMode;

    // Start is called before the first frame update
    void Start() {
        DOTween.SetTweensCapacity(10000, 100);
        InitializeDisplay().Forget();
    }

    private void Update() {
        if (selectedSystem == null || !Data.StargateInfos.TryGetValue(selectedSystem.SystemInfo.system_id, out List<StargateInfo> stargateInfos)) return;

        foreach (var stargateInfo in stargateInfos) {
            if(!systems.TryGetValue(stargateInfo.system_id,out UISystem startSystem)) continue;
            if (!systems.TryGetValue(stargateInfo.destination.system_id, out UISystem destinationSystem)) continue;
            
            var startPosition = startSystem.transform.position;
            var endPosition = destinationSystem.transform.position;


            using (Draw.Command(Camera.main)) {
                // set up static parameters. these are used for all following Draw.Line calls
                Draw.LineGeometry = LineGeometry.Volumetric3D;
                Draw.ThicknessSpace = ThicknessSpace.Pixels;
                Draw.Thickness = 4; // 4px wide

                Draw.ResetMatrix();

                // draw line
                Draw.Line(startPosition, endPosition, Color.green);
            }
        }
    }

    public void SwitchTo2DMode() {
        Debug.Log($"Switching to 2D Mode");

        foreach (var kvp in systems) {
            var system = kvp.Value;
            var systemInfo = system.SystemInfo;
            DOTween.Kill(system.transform);
            system.transform.DOMove(Get2DVectorFromPosition(systemInfo.position), 1, true);
        }
    }

    public void SwitchTo3DMode() {
        Debug.Log($"Switching to 3D Mode");

        foreach (var kvp in systems) {
            var system = kvp.Value;
            var systemInfo = system.SystemInfo;
            DOTween.Kill(system.transform);
            system.transform.DOMove(Get3DVectorFromPosition(systemInfo.position), 1, true);
        }
    }

    public void SetZoomAmount(float amount) {
        Debug.Log("Zooming!");
        amount = Mathf.Clamp(amount, MinZoomAmount, MaxZoomAmount);

        foreach (var kvp in systems) {
            var system = kvp.Value;
            var systemInfo = system.SystemInfo;
            var endPosition = MapMode switch {
                MapModes.TwoDimensions => Get2DVectorFromPosition(systemInfo.position) * amount,
                MapModes.ThreeDimensions => Get3DVectorFromPosition(systemInfo.position) * amount,
                _ => throw new ArgumentOutOfRangeException()
            };

            DOTween.Kill(system.transform);
            system.transform.DOMove(endPosition, 0.25f, true);
        }
    }

    private async UniTask InitializeDisplay() {
        var progress = new Progress<(float value, string message)>();
        progress.ProgressChanged += (_, tuple) => {
            UIManager.Instance.ProgressBar.gameObject.SetActive(true);
            UIManager.Instance.ProgressBar.Value = tuple.value;
            UIManager.Instance.ProgressBar.labelText.text = tuple.message;
            if (tuple.value == 1f) UIManager.Instance.ProgressBar.gameObject.SetActive(false);
        };
        await Data.InitializeMapData(progress, new HttpClient());
        await DisplaySystemsAsync(progress);
    }


    private async UniTask DisplaySystemsAsync(IProgress<(float progress, string message)> progress) {
        // Grab only non-null K-Space systems
        var systemsToDisplay = Data.SystemInfos.Values.Where(s => s != null && s.system_id < 30999999).ToList();

        // Init variables for progress bar
        int total = systemsToDisplay.Count;
        int systemsDisplayed = 0;


        await UniTask.WaitForSeconds(1f);

        foreach (var systemInfo in systemsToDisplay) {
            var system = Instantiate(SystemPrefab, transform);
            await UniTask.NextFrame();
            // Give UISystem its SystemInfo
            system.Init(systemInfo);

            // Update position and scale
            var systemPosition = MapMode switch {
                MapModes.TwoDimensions => Get2DVectorFromPosition(systemInfo.position),
                MapModes.ThreeDimensions => Get3DVectorFromPosition(systemInfo.position),
                _ => throw new ArgumentOutOfRangeException()
            };
            system.transform.position = systemPosition;
            system.transform.localScale = Vector3.one * SystemObjectScaling;

            // Add system to dictionary for later use
            systems.Add(systemInfo.system_id, system);
            systemsDisplayed++;

            // Update progress bar via callback
            progress.Report(((float)systemsDisplayed / total, $"Loading {systemInfo.name}"));
        }

        progress.Report((1f, "Finished loading systems"));
        UIManager.Instance.ProgressBar.gameObject.SetActive(false);
    }

    public async UniTask SelectSystem(UISystem system) {
        foreach (var kvp in systems) {
            kvp.Value.SetSelected(false);
        }

        selectedSystem = system;
        system.SetSelected(true);
        UIManager.Instance.HideSystemInfo();
        await UIManager.Instance.ShowSystemInfo(system.SystemInfo, system.transform.position);
    }

    public static Vector3 Get3DVectorFromPosition(Position position) {
        return new Vector3(
            (float)(position.x / Constants.DefaultSystemScaling),
            (float)(-position.z / Constants.DefaultSystemScaling),
            (float)(position.y / Constants.DefaultSystemScaling));
    }

    public static Vector3 Get2DVectorFromPosition(Position position) {
        return new Vector3((float)(position.x / Constants.DefaultSystemScaling), (float)(-position.z / Constants.DefaultSystemScaling), 0);
    }


    private enum MapModes {
        TwoDimensions,
        ThreeDimensions
    }
}