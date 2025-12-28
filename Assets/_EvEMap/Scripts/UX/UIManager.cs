using System;
using System.Globalization;
using _ProjectEvE.Scripts.Data;
using _ProjectEvE.Scripts.Utilities;
using Cysharp.Threading.Tasks;
using Evo.UI;
using TMPro;
using UnityEngine;
using Constants = _EvEMap.Scripts.Core.Constants;
using RegionInfo = _ProjectEvE.Scripts.Data.RegionInfo;
using SystemInfo = _ProjectEvE.Scripts.Data.SystemInfo;

namespace _ProjectEvE.Scripts.UX {
    public class UIManager : Singleton<UIManager> {
        public MapData MapData;
        public Dropdown SystemSearchDropdown;
        public Dropdown ConstellationSearchDropdown;
        public Dropdown RegionSearchDropdown;

        public ProgressBar ProgressBar;
        public TextMeshProUGUI ProgressBarLabel;

        [SerializeField] private UISystemInfo uiSystemInfo;

        private void Start() {
            InitDropdowns().Forget();
        }

        private async UniTask InitDropdowns() {
            RegionSearchDropdown.onItemSelected.AddListener(OnRegionDropdownItemSelected);
            ConstellationSearchDropdown.onItemSelected.AddListener(OnConstellationDropdownItemSelected);
            await PopulateRegions();
        }

        private void OnRegionDropdownItemSelected(int index) {
            //PopulateConstellationsFromRegion(index).Forget();
        }

        private void OnConstellationDropdownItemSelected(int index) {
            //PopulateSystemsFromConstellation(index).Forget();
        }


        public async UniTask ShowSystemInfo(SystemInfo systemInfo, Vector3 position) {
            uiSystemInfo.SetPosition(position);
            uiSystemInfo.gameObject.SetActive(true);
            await uiSystemInfo.SetSystemInfo(systemInfo);
        }

        public void HideSystemInfo() {
            uiSystemInfo.gameObject.SetActive(false);
        }


        public void SetProgressBarVisibility(bool value) {
            ProgressBar.gameObject.SetActive(value);
        }


        private async UniTask PopulateRegions() {
            RegionSearchDropdown.ClearAllItems();

            SetProgressBarVisibility(true);

            for (int i = 0; i < MapData.RegionIDs.Count; i++) {
                var regionID = MapData.RegionIDs[i];

                ProgressBarLabel.text = $"Populating regions...";
                ProgressBar.Value = (float)i / Constants.RegionCount;

                if (MapData.RegionInfos.TryGetValue(regionID, out RegionInfo regionInfo) && regionInfo != null) {
                    RegionSearchDropdown.AddItem(
                        new Dropdown.Item(regionInfo.name),
                        true,
                        () => {
                            ONClickAction(regionInfo);
                        });
                    await UniTask.NextFrame();
                }
                else {
                    var newRegionInfo = await MapData.GetRegionInfo(regionID);

                    if (newRegionInfo != null) {
                        RegionSearchDropdown.AddItem(newRegionInfo.name);
                        MapData.RegionInfos.Add(regionID, newRegionInfo);
                    }
                    else {
                        Debug.Log($"Failed to pull from EvE");
                    }
                }
            }



            SetProgressBarVisibility(false);

            async void ONClickAction(RegionInfo regionInfo) {
                await PopulateConstellationsFromRegion(regionInfo);
            }
        }


        private async UniTask PopulateConstellationsFromRegion(RegionInfo regionInfo) {
            ConstellationSearchDropdown.ClearAllItems();
            SetProgressBarVisibility(true);

            for (int index = 0; index < regionInfo.constellations.Length; index++) {
                long constellationID = regionInfo.constellations[index];

                ProgressBarLabel.text = $"Populating constellations...";
                ProgressBar.Value = (float)index / regionInfo.constellations.Length;

                if (MapData.ConstellationInfos.TryGetValue(constellationID, out ConstellationInfo constellationInfo)) {
                    ConstellationSearchDropdown.AddItem(
                        new Dropdown.Item(constellationInfo.name),
                        true,
                        () => {
                            OnClickAction(constellationInfo);
                        });
                }
                else {
                    var newConstellationInfo = await MapData.GetConstellationInfo(constellationID);

                    if (newConstellationInfo != null) {
                        ConstellationSearchDropdown.AddItem(newConstellationInfo.name);
                        MapData.ConstellationInfos.Add(constellationID, newConstellationInfo);
                    }
                    else {
                        Debug.LogError($"Failed to find ID ({constellationID}) in {nameof(MapData.ConstellationInfos)}. ToDo: Load here");
                    }
                }
            }


            SetProgressBarVisibility(false);

            async void OnClickAction(ConstellationInfo constellationInfo) {
                Debug.Log($"Wtf");
                await PopulateSystemsFromConstellation(constellationInfo);
            }
        }

        private async UniTask PopulateSystemsFromConstellation(ConstellationInfo constellationInfo) {
            Debug.Log($"Populating systems. Count: {constellationInfo.systems.Length}");
            SystemSearchDropdown.ClearAllItems();
            SetProgressBarVisibility(true);

            for (int index = 0; index < constellationInfo.systems.Length; index++) {
                long systemID = constellationInfo.systems[index];

                ProgressBarLabel.text = $"Populating systems...";
                ProgressBar.Value = (float)index / constellationInfo.systems.Length;

                if (MapData.SystemInfos.TryGetValue(systemID, out SystemInfo systemInfo)) {
                    SystemSearchDropdown.AddItem(
                        new Dropdown.Item(systemInfo.name),
                        true,
                        () => {
                            Debug.Log($"Button Clicked");
                        });
                }
                else {
                    var newSystemInfo = await MapData.GetSystemInfo(systemID);

                    if (newSystemInfo != null) {
                        SystemSearchDropdown.AddItem(
                            new Dropdown.Item(newSystemInfo.name),
                            true,
                            () => {
                                Debug.Log($"Button Clicked");
                            });
                        MapData.SystemInfos.Add(systemID, newSystemInfo);
                    }
                    else {
                        Debug.LogError($"Failed to find ID ({systemID}) in {nameof(MapData.SystemInfos)}. ");
                    }
                }
            }


            SetProgressBarVisibility(false);
        }
    }
}