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
using Shapes;
using Sirenix.OdinInspector;
using UnityEngine;
using SystemInfo = _ProjectEvE.Scripts.Data.SystemInfo;

public class Map : Singleton<Map> {
    [SerializeField, BoxGroup("References")] private UISystem SystemPrefab;
    public static MapData Data { get => Instance.data; }
    [SerializeField, BoxGroup("References")] private MapData data;

    [BoxGroup("Settings")] public double SystemDistanceScaling = 1000000;
    [BoxGroup("Settings")] public float SystemObjectScaling = 0.125f;

    [SerializeField, BoxGroup("Debug")] private UISystemDictionary systems = new();
    [SerializeField, BoxGroup("Debug")] private UISystem selectedSystem;

    // Start is called before the first frame update
    void Start() {
        InitializeDisplay().Forget();
    }

    private void Update() {
        if (selectedSystem == null || !Data.StargateInfos.TryGetValue(selectedSystem.SystemInfo.system_id, out List<StargateInfo> stargateInfos)) return;

        foreach (var stargateInfo in stargateInfos) {
            if (!Data.SystemInfos.TryGetValue(stargateInfo.system_id, out SystemInfo startSystem)) {
                Debug.Log($"Failed to get {stargateInfo.system_id} from SystemInfos! ToDo: Pull it here");
                continue;
            }

            if (!Data.SystemInfos.TryGetValue(stargateInfo.destination.system_id, out SystemInfo destinationSystem)) {
                Debug.Log($"Failed to get {stargateInfo.destination.system_id} from SystemInfos! ToDo: Pull it here");
                continue;
            }

            var offset = Get3DVectorFromPosition(startSystem.position);
            //offset = Vector3.zero;
            var startPosition = Get3DVectorFromPosition(startSystem.position);
            var endPosition = Get3DVectorFromPosition(destinationSystem.position);


            using (Draw.Command(Camera.main)) {
                // set up static parameters. these are used for all following Draw.Line calls
                Draw.LineGeometry = LineGeometry.Volumetric3D;
                Draw.ThicknessSpace = ThicknessSpace.Pixels;
                Draw.Thickness = 4; // 4px wide

                // set static parameter to draw in the local space of this object
                Draw.Matrix = transform.localToWorldMatrix;

                // draw line
                Draw.Line(startPosition, endPosition, Color.green);
            }
        }
    }

    public void SwitchTo2DMode() {
        Debug.Log($"Switching to 2D Mode");
    }

    public void SwitchTo3DMode() {
        Debug.Log($"Switching to 3D Mode");
    }

    private async UniTask InitializeDisplay() {
        var progress = new Progress<(float value, string message)>();
        progress.ProgressChanged += (_, tuple) => {
            UIManager.Instance.ProgressBar.gameObject.SetActive(true);
            UIManager.Instance.ProgressBar.Value = tuple.value;
            UIManager.Instance.ProgressBar.labelText.text = tuple.message;
            if (Math.Abs(tuple.value - 1f) < Single.MinValue) UIManager.Instance.ProgressBar.gameObject.SetActive(false);
        };
        await Data.InitializeMapData(progress, new HttpClient());
        //await DisplaySystems(Data.SystemInfos);
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
            system.transform.position = Get3DVectorFromPosition(systemInfo.position);
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
            (float)(position.x / Instance.SystemDistanceScaling),
            (float)(-position.z / Instance.SystemDistanceScaling),
            (float)(position.y / Instance.SystemDistanceScaling));
    }

    public static Vector3 Get2DVectorFromPosition(Position position) {
        return new Vector3((float)(position.x / Instance.SystemDistanceScaling), (float)(-position.z / Instance.SystemDistanceScaling), 0);
    }
}