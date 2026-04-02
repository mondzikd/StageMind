using System.Collections.Generic;

namespace StageMind.Tests.EditMode.Mocks
{
    public class MockStateAware : IStateAware
    {
        public List<GameStateType> EnteredStates { get; } = new();
        public List<GameStateType> ExitedStates { get; } = new();

        public void OnStateEnter(GameStateType state) => EnteredStates.Add(state);
        public void OnStateExit(GameStateType state) => ExitedStates.Add(state);
    }
}
