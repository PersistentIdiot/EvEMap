using _ProjectEvE.Scripts.Data;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace _ProjectEvE.Scripts.UX {
    public class UIPlanetInfo : MonoBehaviour {
        [SerializeField] private TextMeshProUGUI NameText;
        [SerializeField] private TextMeshProUGUI TypeText;
        
        private PlanetInfo planetInfo;

        public async UniTask InitPlanetInfo(PlanetInfo info) {
            planetInfo = info;
            var typeInfo = await Map.Data.GetTypeInfo(planetInfo.type_id);
            TypeText.text = typeInfo.name;
        }
    }
}