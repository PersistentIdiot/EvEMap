using System;
using System.Net.Http;
using System.Reflection.Emit;
using _ProjectEvE.Scripts.Data;
using _ProjectEvE.Scripts.Utilities;
using Cysharp.Threading.Tasks;
using Evo.UI;
using UnityEngine;
using Constants = _EvEMap.Scripts.Core.Constants;

namespace _ProjectEvE.Scripts.UX {
    public class UIMainMenu : Singleton<UIMainMenu> {
        [SerializeField] private MapData Data;
        [SerializeField] private Transform GenerateContainer;
        [SerializeField] private Transform VerifyContainer;
        [SerializeField] private Transform MapContainer;
        [SerializeField] private Transform SettingsContainer;

        [SerializeField] private UIMenuLoadOption menuOptionPrefab;

        private void Start() {
            InitGenerateContainer();
        }

        private void InitGenerateContainer() {
            HttpClient client = new HttpClient();
            string colorString = "";
            CreateGenerateOption(nameof(Data.ConstellationIDs), Data.ConstellationIDs.Count, Constants.ConstellationCount, OnGenerateConstellationIDsClick);
            CreateGenerateOption(nameof(Data.RegionIDs), Data.RegionIDs.Count, Constants.RegionCount, OnGenerateRegionIDsClick);
            CreateGenerateOption(nameof(Data.SystemIDs), Data.SystemIDs.Count, Constants.SystemCount, OnGenerateSystemIDsClick);
            CreateGenerateOption(nameof(Data.ConstellationInfos), Data.ConstellationInfos.Count, Constants.ConstellationCount, OnGenerateConstellationInfosClick);
            CreateGenerateOption(nameof(Data.RegionInfos), Data.RegionInfos.Count, Constants.RegionCount, OnGenerateRegionInfosClick);
            CreateGenerateOption(nameof(Data.SystemInfos), Data.SystemInfos.Count, Constants.SystemCount, OnGenerateSystemInfosClick);

            void CreateGenerateOption(string label, int minCount, int maxCount, Action onClickAction) {
                bool initialized = minCount >= maxCount;
                colorString = RedForFalseGreenForTrue(initialized);
                string detailsText = $"{colorString} {minCount} / {maxCount}</color>";
                var generateConstellationDataMenuOption = CreateMenuOption(GenerateContainer, label, detailsText, onClickAction);
                generateConstellationDataMenuOption.Switch.SetValue(initialized, false);
                generateConstellationDataMenuOption.Switch.interactable = !initialized;
            }

            async void OnGenerateConstellationIDsClick() {
                Data.ConstellationIDs = await Data.GetConstellationIDs(client);
            }
            async void OnGenerateRegionIDsClick() {
                Data.RegionIDs = await Data.GetRegionIDs(client);
            }
            async void OnGenerateSystemIDsClick() {
                Data.SystemIDs = await Data.GetSystemIDs(client);
            }
            async void OnGenerateConstellationInfosClick() {
                Debug.Log($"Generating Constellation Infos");
                var infos  = await Data.GetConstellationInfos(Data.ConstellationIDs,client);
                Debug.Log($"Constellation infos loaded");
                Data.ConstellationInfos = infos;
            }
            async void OnGenerateRegionInfosClick() {
                Data.RegionInfos = await Data.GetRegionInfos(Data.RegionIDs,client);
            }
            async void OnGenerateSystemInfosClick() {
                Data.SystemInfos = await Data.GetKspaceInfos(Data.SystemIDs,client);
            }
        }


        private UIMenuLoadOption CreateMenuOption(Transform container, string labelText = null, string detailsText = null, Action onClick = null) {
            var menuOption = Instantiate(menuOptionPrefab, container);

            if (!string.IsNullOrEmpty(labelText)) menuOption.LabelText.text = labelText;
            if (!string.IsNullOrEmpty(detailsText)) menuOption.DetailsText.text = detailsText;
            if (onClick != null) menuOption.Switch.onSwitchOn.AddListener(onClick.Invoke);
            menuOption.gameObject.SetActive(true);

            return menuOption;
        }

        private string RedForFalseGreenForTrue(bool value) {
            return value ? "<color=green>" : "<color=red>";
        }
    }
}