using _ProjectEvE.Scripts.Utilities;
using Cysharp.Threading.Tasks;
using Evo.UI;
using TMPro;
using UnityEngine;
using SystemInfo = _ProjectEvE.Scripts.Data.SystemInfo;

namespace _ProjectEvE.Scripts.UX {
    public class UIManager : Singleton<UIManager> {
        public ProgressBar ProgressBar;
        public TextMeshProUGUI ProgressBarLabel;

        [SerializeField] private UISystemInfo uiSystemInfo;

        public async UniTask ShowSystemInfo(SystemInfo systemInfo, Vector3 position) {
            await uiSystemInfo.SetSystemInfo(systemInfo);
            uiSystemInfo.SetPosition(position);
            uiSystemInfo.gameObject.SetActive(true);
        }

        public void HideSystemInfo() {
            uiSystemInfo.gameObject.SetActive(false);
        }


        public void SetProgressBarVisibility(bool value) {
            ProgressBar.gameObject.SetActive(value);
        }
    }
}