using _ProjectEvE.Scripts.Data;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace _ProjectEvE.Scripts.UX {
    public class UIPlanetInfo : MonoBehaviour {
        [SerializeField] private TextMeshProUGUI NameText;
        [SerializeField] private TextMeshProUGUI TypeText;
        private int planetNumber = -1;
        
        private PlanetInfo planetInfo;

        public async UniTask InitPlanetInfo(PlanetInfo info, int siblingIndex) {
            planetInfo = info;
            var typeInfo = await Map.Data.GetTypeInfo(planetInfo.type_id);
            TypeText.text = typeInfo.name;
        }

        public void SetNameText(string text) {
            NameText.text = text;
        }
    }
}