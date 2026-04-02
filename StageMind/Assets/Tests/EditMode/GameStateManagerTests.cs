using System.Collections.Generic;
using NUnit.Framework;
using StageMind.Tests.EditMode.Mocks;
using UnityEngine;

namespace StageMind.Tests.EditMode
{
    [TestFixture]
    public class GameStateManagerTests
    {
        private StateMachine _stateMachine;
        private List<IGameState> _createdStates;
        private GameObject _gameStateManagerObject;
        private GameStateManager _gameStateManager;

        [SetUp]
        public void SetUp()
        {
            _createdStates = new List<IGameState>();
            _stateMachine = new StateMachine(TestStateFactory);
            _stateMachine.Initialize();

            _gameStateManagerObject = new GameObject("GameStateManagerTests_Object");
            _gameStateManager = _gameStateManagerObject.AddComponent<GameStateManager>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_gameStateManagerObject);
        }

        private IGameState TestStateFactory(GameStateType stateType, GameStateType previousStateType)
        {
            IGameState state = stateType switch
            {
                GameStateType.Paused => new PausedState(null, previousStateType),
                _ => new TestState(stateType)
            };
            _createdStates.Add(state);
            return state;
        }

        [Test]
        public void Initialize_Default_StartsInLobbyLanding()
        {
            Assert.AreEqual(GameStateType.LobbyLanding, _stateMachine.CurrentStateType);
        }

        [Test]
        public void TransitionTo_ValidTransition_ChangesState()
        {
            _stateMachine.TransitionTo(GameStateType.LobbySlidesLoaded);

            Assert.AreEqual(GameStateType.LobbySlidesLoaded, _stateMachine.CurrentStateType);
        }

        [Test]
        public void TransitionTo_ValidTransition_CallsExitOnPreviousState()
        {
            var initialState = (TestState)_createdStates[0];

            _stateMachine.TransitionTo(GameStateType.LobbySlidesLoaded);

            Assert.IsTrue(initialState.ExitCalled);
        }

        [Test]
        public void TransitionTo_ValidTransition_CallsEnterOnNewState()
        {
            _stateMachine.TransitionTo(GameStateType.LobbySlidesLoaded);

            var newState = (TestState)_createdStates[1];
            Assert.IsTrue(newState.EnterCalled);
        }

        [Test]
        public void TransitionTo_ValidTransition_FiresOnStateChanged()
        {
            GameStateType? firedPrevious = null;
            GameStateType? firedCurrent = null;
            _stateMachine.OnStateChanged += (prev, curr) =>
            {
                firedPrevious = prev;
                firedCurrent = curr;
            };

            _stateMachine.TransitionTo(GameStateType.LobbySlidesLoaded);

            Assert.AreEqual(GameStateType.LobbyLanding, firedPrevious);
            Assert.AreEqual(GameStateType.LobbySlidesLoaded, firedCurrent);
        }

        [Test]
        public void TransitionTo_ValidTransition_NotifiesStateAwareListeners()
        {
            var mock = new MockStateAware();
            _stateMachine.RegisterStateAware(mock);

            _stateMachine.TransitionTo(GameStateType.LobbySlidesLoaded);

            Assert.AreEqual(1, mock.ExitedStates.Count);
            Assert.AreEqual(GameStateType.LobbyLanding, mock.ExitedStates[0]);
            Assert.AreEqual(1, mock.EnteredStates.Count);
            Assert.AreEqual(GameStateType.LobbySlidesLoaded, mock.EnteredStates[0]);
        }

        [Test]
        public void TransitionTo_InvalidTransition_DoesNotChangeState()
        {
            _stateMachine.TransitionTo(GameStateType.Rehearsal);

            Assert.AreEqual(GameStateType.LobbyLanding, _stateMachine.CurrentStateType);
        }

        [Test]
        public void TransitionTo_InvalidTransition_DoesNotFireEvent()
        {
            bool eventFired = false;
            _stateMachine.OnStateChanged += (_, _) => eventFired = true;

            _stateMachine.TransitionTo(GameStateType.Rehearsal);

            Assert.IsFalse(eventFired);
        }

        [Test]
        public void RegisterStateAware_AddsListener()
        {
            var mock = new MockStateAware();
            _stateMachine.RegisterStateAware(mock);

            _stateMachine.TransitionTo(GameStateType.LobbySlidesLoaded);

            Assert.AreEqual(1, mock.EnteredStates.Count);
        }

        [Test]
        public void UnregisterStateAware_RemovesListener()
        {
            var mock = new MockStateAware();
            _stateMachine.RegisterStateAware(mock);
            _stateMachine.UnregisterStateAware(mock);

            _stateMachine.TransitionTo(GameStateType.LobbySlidesLoaded);

            Assert.AreEqual(0, mock.EnteredStates.Count);
        }

        [Test]
        public void TransitionTo_AllValidPaths_Succeeds()
        {
            var validPaths = new[]
            {
                (GameStateType.LobbyLanding, GameStateType.LobbySlidesLoaded),
                (GameStateType.LobbySlidesLoaded, GameStateType.Rehearsal),
                (GameStateType.Rehearsal, GameStateType.Paused),
                (GameStateType.Paused, GameStateType.Rehearsal),
                (GameStateType.Paused, GameStateType.Reinforcement),
                (GameStateType.Paused, GameStateType.LobbyLanding),
                (GameStateType.Reinforcement, GameStateType.Rehearsal),
                (GameStateType.Reinforcement, GameStateType.LobbyLanding),
                (GameStateType.LobbySlidesLoaded, GameStateType.LobbyLanding),
            };

            foreach (var (from, to) in validPaths)
            {
                var sm = new StateMachine(TestStateFactory);
                sm.Initialize();
                NavigateTo(sm, from);

                sm.TransitionTo(to);

                Assert.AreEqual(to, sm.CurrentStateType,
                    $"Transition {from} → {to} should succeed");
            }
        }

        [Test]
        public void TransitionTo_MultipleListeners_AllReceiveEvents()
        {
            var mock1 = new MockStateAware();
            var mock2 = new MockStateAware();
            _stateMachine.RegisterStateAware(mock1);
            _stateMachine.RegisterStateAware(mock2);

            _stateMachine.TransitionTo(GameStateType.LobbySlidesLoaded);

            Assert.AreEqual(1, mock1.EnteredStates.Count);
            Assert.AreEqual(1, mock2.EnteredStates.Count);
        }

        [Test]
        public void GameStateManager_Awake_InitializesToLobbyLanding()
        {
            Assert.AreEqual(GameStateType.LobbyLanding, _gameStateManager.CurrentStateType);
        }

        [Test]
        public void GameStateManager_TransitionTo_ValidTransition_FiresOnStateChanged()
        {
            GameStateType? firedPrevious = null;
            GameStateType? firedCurrent = null;
            _gameStateManager.OnStateChanged += (previous, current) =>
            {
                firedPrevious = previous;
                firedCurrent = current;
            };

            _gameStateManager.TransitionTo(GameStateType.LobbySlidesLoaded);

            Assert.AreEqual(GameStateType.LobbyLanding, firedPrevious);
            Assert.AreEqual(GameStateType.LobbySlidesLoaded, firedCurrent);
        }

        [Test]
        public void GameStateManager_RegisterAndUnregisterStateAware_ForwardsToStateMachine()
        {
            var mock = new MockStateAware();
            _gameStateManager.RegisterStateAware(mock);
            _gameStateManager.TransitionTo(GameStateType.LobbySlidesLoaded);

            Assert.AreEqual(1, mock.EnteredStates.Count);
            Assert.AreEqual(1, mock.ExitedStates.Count);

            _gameStateManager.UnregisterStateAware(mock);
            _gameStateManager.TransitionTo(GameStateType.LobbyLanding);

            Assert.AreEqual(1, mock.EnteredStates.Count);
            Assert.AreEqual(1, mock.ExitedStates.Count);
        }

        private void NavigateTo(StateMachine sm, GameStateType target)
        {
            if (sm.CurrentStateType == target) return;

            switch (target)
            {
                case GameStateType.LobbyLanding:
                    break;
                case GameStateType.LobbySlidesLoaded:
                    sm.TransitionTo(GameStateType.LobbySlidesLoaded);
                    break;
                case GameStateType.Rehearsal:
                    sm.TransitionTo(GameStateType.LobbySlidesLoaded);
                    sm.TransitionTo(GameStateType.Rehearsal);
                    break;
                case GameStateType.Paused:
                    sm.TransitionTo(GameStateType.LobbySlidesLoaded);
                    sm.TransitionTo(GameStateType.Rehearsal);
                    sm.TransitionTo(GameStateType.Paused);
                    break;
                case GameStateType.Reinforcement:
                    sm.TransitionTo(GameStateType.LobbySlidesLoaded);
                    sm.TransitionTo(GameStateType.Rehearsal);
                    sm.TransitionTo(GameStateType.Paused);
                    sm.TransitionTo(GameStateType.Reinforcement);
                    break;
            }
        }

        private class TestState : IGameState
        {
            public GameStateType StateType { get; }
            public bool EnterCalled { get; private set; }
            public bool ExitCalled { get; private set; }

            public TestState(GameStateType stateType)
            {
                StateType = stateType;
            }

            public void Enter() => EnterCalled = true;
            public void Exit() => ExitCalled = true;
            public void Update() { }
            public void HandleInput() { }
        }
    }
}
