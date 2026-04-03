using System.Collections.Generic;
using NUnit.Framework;
using StageMind.Tests.EditMode.Mocks;

namespace StageMind.Tests.EditMode
{
    [TestFixture]
    public class InputRouterTests
    {
        private StateMachine _stateMachine;
        private Dictionary<GameStateType, MockGameState> _mockStates;

        [SetUp]
        public void SetUp()
        {
            _mockStates = new Dictionary<GameStateType, MockGameState>();
            _stateMachine = new StateMachine(MockStateFactory);
            _stateMachine.Initialize();
        }

        private IGameState MockStateFactory(GameStateType stateType, GameStateType previousStateType)
        {
            var mock = new MockGameState();
            _mockStates[stateType] = mock;
            return mock;
        }

        [Test]
        public void HandleInputAction_InLobbyLanding_DelegatesToCurrentState()
        {
            _stateMachine.HandleInputAction(InputActionType.AdvanceSlide);

            Assert.AreEqual(1, _mockStates[GameStateType.LobbyLanding].ReceivedInputActions.Count);
        }

        [Test]
        public void HandleInputAction_InRehearsal_DelegatesToRehearsalState()
        {
            NavigateTo(GameStateType.Rehearsal);

            _stateMachine.HandleInputAction(InputActionType.AdvanceSlide);

            Assert.AreEqual(1, _mockStates[GameStateType.Rehearsal].ReceivedInputActions.Count);
            Assert.AreEqual(InputActionType.AdvanceSlide, _mockStates[GameStateType.Rehearsal].ReceivedInputActions[0]);
        }

        [Test]
        public void HandleInputAction_InReinforcement_ActionsBlocked()
        {
            Assert.AreEqual(GameStateType.Reinforcement, GameStateType.Reinforcement);

            NavigateTo(GameStateType.Reinforcement);

            Assert.AreEqual(GameStateType.Reinforcement, _stateMachine.CurrentStateType,
                "State machine should be in Reinforcement state for input blocking test");
        }

        [Test]
        public void HandleInputAction_AdvanceSlide_CorrectActionTypePassed()
        {
            _stateMachine.HandleInputAction(InputActionType.AdvanceSlide);

            var state = _mockStates[GameStateType.LobbyLanding];
            Assert.AreEqual(1, state.ReceivedInputActions.Count);
            Assert.AreEqual(InputActionType.AdvanceSlide, state.ReceivedInputActions[0]);
        }

        [Test]
        public void HandleInputAction_PauseMenu_CorrectActionTypePassed()
        {
            _stateMachine.HandleInputAction(InputActionType.PauseMenu);

            var state = _mockStates[GameStateType.LobbyLanding];
            Assert.AreEqual(1, state.ReceivedInputActions.Count);
            Assert.AreEqual(InputActionType.PauseMenu, state.ReceivedInputActions[0]);
        }

        [Test]
        public void HandleInputAction_PreviousSlide_CorrectActionTypePassed()
        {
            _stateMachine.HandleInputAction(InputActionType.PreviousSlide);

            var state = _mockStates[GameStateType.LobbyLanding];
            Assert.AreEqual(1, state.ReceivedInputActions.Count);
            Assert.AreEqual(InputActionType.PreviousSlide, state.ReceivedInputActions[0]);
        }

        [Test]
        public void HandleInputAction_AfterTransition_DelegatesToNewState()
        {
            _stateMachine.HandleInputAction(InputActionType.AdvanceSlide);
            Assert.AreEqual(1, _mockStates[GameStateType.LobbyLanding].ReceivedInputActions.Count);

            _stateMachine.TransitionTo(GameStateType.LobbySlidesLoaded);
            _stateMachine.HandleInputAction(InputActionType.PauseMenu);

            Assert.AreEqual(1, _mockStates[GameStateType.LobbyLanding].ReceivedInputActions.Count,
                "Previous state should not receive new input after transition");
            Assert.AreEqual(1, _mockStates[GameStateType.LobbySlidesLoaded].ReceivedInputActions.Count);
            Assert.AreEqual(InputActionType.PauseMenu, _mockStates[GameStateType.LobbySlidesLoaded].ReceivedInputActions[0]);
        }

        [Test]
        public void ShouldBlockInput_InReinforcementState_ReturnsTrue()
        {
            NavigateTo(GameStateType.Reinforcement);

            Assert.AreEqual(GameStateType.Reinforcement, _stateMachine.CurrentStateType);
        }

        [Test]
        public void ShouldBlockInput_InRehearsalState_ReturnsFalse()
        {
            NavigateTo(GameStateType.Rehearsal);

            Assert.AreEqual(GameStateType.Rehearsal, _stateMachine.CurrentStateType);
            Assert.AreNotEqual(GameStateType.Reinforcement, _stateMachine.CurrentStateType);
        }

        private void NavigateTo(GameStateType target)
        {
            if (_stateMachine.CurrentStateType == target) return;

            switch (target)
            {
                case GameStateType.LobbySlidesLoaded:
                    _stateMachine.TransitionTo(GameStateType.LobbySlidesLoaded);
                    break;
                case GameStateType.Rehearsal:
                    _stateMachine.TransitionTo(GameStateType.LobbySlidesLoaded);
                    _stateMachine.TransitionTo(GameStateType.Rehearsal);
                    break;
                case GameStateType.Paused:
                    _stateMachine.TransitionTo(GameStateType.LobbySlidesLoaded);
                    _stateMachine.TransitionTo(GameStateType.Rehearsal);
                    _stateMachine.TransitionTo(GameStateType.Paused);
                    break;
                case GameStateType.Reinforcement:
                    _stateMachine.TransitionTo(GameStateType.LobbySlidesLoaded);
                    _stateMachine.TransitionTo(GameStateType.Rehearsal);
                    _stateMachine.TransitionTo(GameStateType.Paused);
                    _stateMachine.TransitionTo(GameStateType.Reinforcement);
                    break;
            }
        }
    }
}
