using UnityEngine;

namespace StageMind
{
    public class RehearsalState : IGameState
    {
        private readonly GameStateManager _stateManager;
        private readonly IWebViewController _webViewController;

        public RehearsalState(GameStateManager stateManager, IWebViewController webViewController = null)
        {
            _stateManager = stateManager;
            _webViewController = webViewController;
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

        public bool HandleInput(InputActionType actionType)
        {
            return actionType switch
            {
                InputActionType.AdvanceSlide => _webViewController?.SendKeyEvent(KeyCode.RightArrow) ?? false,
                InputActionType.PreviousSlide => _webViewController?.SendKeyEvent(KeyCode.LeftArrow) ?? false,
                InputActionType.PauseMenu => true,
                _ => false
            };
        }
    }
}
