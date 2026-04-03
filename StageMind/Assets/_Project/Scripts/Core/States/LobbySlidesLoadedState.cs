using UnityEngine;

namespace StageMind
{
    public class LobbySlidesLoadedState : IGameState
    {
        private readonly GameStateManager _stateManager;

        public LobbySlidesLoadedState(GameStateManager stateManager)
        {
            _stateManager = stateManager;
        }

        public void Enter()
        {
            Debug.Log("[StageMind] Entering LobbySlidesLoaded state");
        }

        public void Exit()
        {
            Debug.Log("[StageMind] Exiting LobbySlidesLoaded state");
        }

        public void Update() { }

        public void HandleInput(InputActionType actionType) { }
    }
}
