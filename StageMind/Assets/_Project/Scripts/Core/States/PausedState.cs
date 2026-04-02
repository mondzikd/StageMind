using UnityEngine;

namespace StageMind
{
    public class PausedState : IGameState
    {
        private readonly GameStateManager _stateManager;
        private readonly GameStateType _previousStateType;

        public PausedState(GameStateManager stateManager, GameStateType previousStateType)
        {
            _stateManager = stateManager;
            _previousStateType = previousStateType;
        }

        public GameStateType PreviousStateType => _previousStateType;

        public void Enter()
        {
            Debug.Log("[StageMind] Entering Paused state");
        }

        public void Exit()
        {
            Debug.Log("[StageMind] Exiting Paused state");
        }

        public void Update() { }

        public void HandleInput() { }
    }
}
