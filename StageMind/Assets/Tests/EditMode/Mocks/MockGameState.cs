using System.Collections.Generic;

namespace StageMind.Tests.EditMode.Mocks
{
    public class MockGameState : IGameState
    {
        public List<InputActionType> ReceivedInputActions { get; } = new();
        public int EnterCallCount { get; private set; }
        public int ExitCallCount { get; private set; }

        public void Enter() => EnterCallCount++;
        public void Exit() => ExitCallCount++;
        public void Update() { }
        public bool HandleInput(InputActionType actionType)
        {
            ReceivedInputActions.Add(actionType);
            return true;
        }
    }
}
