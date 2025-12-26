using System;
using System.Collections;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _EvEMap.Scripts.Core {
    public class GameStateTriggerer: MonoBehaviour {
        [SerializeField] private GameTriggers trigger;
        public TriggerModes TriggerMode = TriggerModes.Manual;
        [ShowIf(nameof(TriggerMode), TriggerModes.Delayed)] public float Delay = 0.25f;
        private void Awake() {
            if (TriggerMode == TriggerModes.Awake) {
                Trigger();
            }
        }

        private void Start() {
            switch (TriggerMode) {
                case TriggerModes.Manual:
                    break;
                case TriggerModes.Awake:
                    break;
                case TriggerModes.Start:
                    Trigger();
                    break;
                case TriggerModes.Delayed:
                    StartCoroutine(DelayedTrigger(Delay));
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private IEnumerator DelayedTrigger(float delay) {
            yield return new WaitForSecondsRealtime(delay);
            Trigger();
        }

        public void Trigger() {
            Game.FireTrigger(trigger);
        }

        public enum TriggerModes {
            Manual,
            Awake,
            Start,
            Delayed
        }
    }
}