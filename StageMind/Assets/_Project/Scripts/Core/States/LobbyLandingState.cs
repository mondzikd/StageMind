using UnityEngine;

namespace StageMind
{
    public class LobbyLandingState : IGameState
    {
        private readonly GameStateManager _stateManager;

        public LobbyLandingState(GameStateManager stateManager)
        {
            _stateManager = stateManager;
        }

        public void Enter()
        {
            Debug.Log("[StageMind] Entering LobbyLanding state");
        }

        public void Exit()
        {
            Debug.Log("[StageMind] Exiting LobbyLanding state");
        }

        public void Update() { }

        public bool HandleInput(InputActionType actionType) => true;
    }
}
