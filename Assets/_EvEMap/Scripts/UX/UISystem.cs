using System;
using Linework.WideOutline;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;
using SystemInfo = _ProjectEvE.Scripts.Data.SystemInfo;

namespace _ProjectEvE.Scripts.UX {
    public class UISystem : MonoBehaviour {
        public SystemInfo SystemInfo;

        [SerializeField] private WideOutlineSettings wideOutlineSettings;
        [FormerlySerializedAs("model")]
        [SerializeField] private Renderer outline;
        [SerializeField] private Renderer model;
        [SerializeField] private bool selected = false;

        public void Init(SystemInfo systemInfo) {
            Debug.Assert(systemInfo != null);

            SystemInfo = systemInfo;
            gameObject.name = $"System ({systemInfo.name})";


            model.material.color = GetColorFromSecurityStatus(systemInfo.security_status);
        }

        private void OnMouseDown() {
            Map.Instance.SelectSystem(this);
        }

        private void OnMouseUp() {
            // SetSelected(false);
        }

        public void SetSelected(bool value) {
            selected = value;

            if (selected) {
                Debug.Log($"{SystemInfo.name} selected!", this);
                var color = GetColorFromSecurityStatus(SystemInfo.security_status);
                wideOutlineSettings.Outlines[0].color = color;
                outline.gameObject.SetActive(true);
            }
            else {
                var color = new Color(0, 0, 0);
                wideOutlineSettings.Outlines[0].color = color;
                outline.gameObject.SetActive(false);
            }
        }
        
        public static Color GetColorFromSecurityStatus(float securityStatus) {
            if (securityStatus < 0) {
                return new Color(Math.Abs(securityStatus), 0, 0);
            }
            else {
                return new Color(0, securityStatus, 0);
            }
        }
    }
}