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
            
            var menuOption = Instantiate(menuOptionPrefab, GenerateContainer);
            menuOption.LabelText.text = $"{nameof(Data.PlanetInfos)}";
            menuOption.DetailsText.text = $"{Data.PlanetInfos.Count}/{Constants.PlanetsCount}";
            var progress = new Progress<(float value, string message)>();
            progress.ProgressChanged += (_, tuple) => {
                menuOption.ProgressBar.Value = tuple.value;
                menuOption.ProgressBar.labelText.text = $"Loading Planet Infos";
            };
            menuOption.Switch.onValueChanged.AddListener(
                async value => {
                    if (value) {
                        menuOption.Switch.interactable = false;
                        Data.PlanetInfos = await Data.GetPlanetInfosAsync(progress, Data.SystemIDs);
                    }
                });
            menuOption.gameObject.SetActive(true);
        }

        private string RedForFalseGreenForTrue(bool value) {
            return value ? "<color=green>" : "<color=red>";
        }
    }
}