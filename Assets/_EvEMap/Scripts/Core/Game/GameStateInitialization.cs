using Sirenix.OdinInspector;

namespace _EvEMap.Scripts.Core {
    public partial class Game {
        [ReadOnly] public GameStates CurrentState = GameStates.Initial;
        private void InitStateMachine() {
            stateMachine = new(GameStates.Initial);
            stateMachine.OnTransitioned(
                transition => {
                    Instance.CurrentState = transition.Destination;
                });
            
            
            stateMachine.Configure(GameStates.Initial)
                .Permit(GameTriggers.EnterMenu, GameStates.Menu)
                .OnExit(
                () => {
                    LoadingManager.LoadScene(MapScene.Name);
                });

            stateMachine.Configure(GameStates.Menu)
                .Permit(GameTriggers.EnterMap, GameStates.Map)
                ;

            stateMachine.Configure(GameStates.Map)
                .Permit(GameTriggers.EnterMenu, GameStates.Menu)
                ;
        }
    }
}