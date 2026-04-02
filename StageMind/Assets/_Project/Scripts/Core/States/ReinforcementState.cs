using UnityEngine;

namespace StageMind
{
    public class ReinforcementState : IGameState
    {
        private readonly GameStateManager _stateManager;

        public ReinforcementState(GameStateManager stateManager)
        {
            _stateManager = stateManager;
        }

        public void Enter()
        {
            Debug.Log("[StageMind] Entering Reinforcement state");
        }

        public void Exit()
        {
            Debug.Log("[StageMind] Exiting Reinforcement state");
        }

        public void Update() { }

        public void HandleInput() { }
    }
}
