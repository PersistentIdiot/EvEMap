using _ProjectEvE.Scripts.Data;
using _ProjectEvE.Scripts.Utilities;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace _ProjectEvE.Scripts.UX {
    public class UISystemInfoPanel : Singleton<UISystemInfoPanel> {
        [SerializeField, BoxGroup("References")] private CanvasGroup canvasGroup;
        [SerializeField, BoxGroup("References")] private Transform planetInfoContainer;
        [SerializeField, BoxGroup("References")] private Transform moonInfoContainer;

        [SerializeField, BoxGroup("References")] private UIPlanetInfo planetInfoPrefab;

        private Data.SystemInfo currentSystem;

        
        public static void InitSystemInfo(Data.SystemInfo systemInfo) {
            Instance.currentSystem = systemInfo;
            Instance.InitInfoDisplay().Forget();
        }

        public static void SetVisibility(bool value) {
            //Instance.gameObject.SetActive(value);
            Instance.canvasGroup.DOFade(value ? 1 : 0, 0.25f).onComplete += () => {
                Instance.canvasGroup.interactable = value;
                Instance.canvasGroup.blocksRaycasts = value;
            };
        }

        private async UniTask InitInfoDisplay() {
            ClearInfoDisplay();
            
            // Load planet info
            foreach (var planet in currentSystem.planets) {
                if (Map.Data.PlanetInfos.TryGetValue(planet.planet_id, out PlanetInfo planetInfo)) {
                    var uiPlanetInfo = Instantiate(planetInfoPrefab, planetInfoContainer);
                    await uiPlanetInfo.InitPlanetInfo(planetInfo);
                }
                else {
                    Debug.Log($"Failed to get PlanetInfo for Planet ID: {planet.planet_id}");
                }
            }
            SetVisibility(true);
        }

        private void ClearInfoDisplay() {
            for (int i = planetInfoContainer.childCount - 1; i >= 0; i--) {
                Destroy(planetInfoContainer.GetChild(i).gameObject);
            }
        }
    }
}