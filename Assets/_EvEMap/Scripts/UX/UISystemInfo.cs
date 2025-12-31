using System;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using SystemInfo = _ProjectEvE.Scripts.Data.SystemInfo;

namespace _ProjectEvE.Scripts.UX {
    public class UISystemInfo: MonoBehaviour {
        public Vector3 Offset;
        public Image Icon;
        public TextMeshProUGUI TitleText;
        public TextMeshProUGUI DescriptionText;

        [SerializeField] private SystemInfo systemInfo;

        public async UniTask SetSystemInfo(SystemInfo info) {
            systemInfo = info;
            var stargateInfos = await Map.Data.GetStargateInfosForSystem(systemInfo);
            TitleText.text = $"{systemInfo.name} <color=#{ColorUtility.ToHtmlStringRGB(UISystem.GetColorFromSecurityStatus(systemInfo.security_status))}>({systemInfo.security_status:N1}</color>)";
            DescriptionText.text = "";
            foreach (var stargateInfo in stargateInfos) {
                DescriptionText.text += $"{stargateInfo.name} \n";
            }
        }

        public void SetPosition(Vector3 position) {
            transform.position = position + Offset;
        }


        private void Update() {
            transform.LookAt(Camera.main!.transform);
            transform.Rotate(0,180,0);
        }
    }
}