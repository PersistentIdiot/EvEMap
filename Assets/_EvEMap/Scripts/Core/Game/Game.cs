using _ProjectEvE.Scripts.Utilities;
using Cysharp.Threading.Tasks;
using Michsky.LSS;
using Sirenix.OdinInspector;
using Stateless;
using Unity.VisualScripting;
using UnityEngine;

namespace _EvEMap.Scripts.Core {
    public partial class Game: _ProjectEvE.Scripts.Utilities.Singleton<Game> {
        public GameStates InitialState = GameStates.Initial;
        [SerializeField] private SceneField MenuScene;
        [SerializeField] private SceneField MapScene;
        [SerializeField] private LSS_Manager LoadingManager;
        private StateMachine<GameStates, GameTriggers> stateMachine;

        private void OnEnable() {
            if (stateMachine == null) {
                InitStateMachine();
            }
        }

        public static async UniTask FireTrigger(GameTriggers trigger) {
            await Instance.stateMachine.FireAsync(trigger);
        }
    }
}