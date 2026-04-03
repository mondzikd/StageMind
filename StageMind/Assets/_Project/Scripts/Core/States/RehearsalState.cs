using UnityEngine;

namespace StageMind
{
    public class RehearsalState : IGameState
    {
        private readonly GameStateManager _stateManager;

        public RehearsalState(GameStateManager stateManager)
        {
            _stateManager = stateManager;
        }

        public void Enter()
        {
            Debug.Log("[StageMind] Entering Rehearsal state");
        }

        public void Exit()
        {
            Debug.Log("[StageMind] Exiting Rehearsal state");
        }

        public void Update() { }

        public void HandleInput(InputActionType actionType) { }
    }
}
