using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using StageMind.Tests.EditMode.Mocks;
using UnityEngine;

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
            var (router, managerObject, routerObject) = CreateInitializedInputRouter();

            try
            {
                NavigateManagerTo(managerObject.GetComponent<GameStateManager>(), GameStateType.Reinforcement);

                router.OnStateEnter(GameStateType.Reinforcement);
                var actions = ReadPrivateField<StageMindActions>(router, "_actions");

                Assert.IsFalse(actions.Gameplay.AdvanceSlide.enabled);
                Assert.IsFalse(actions.Gameplay.PreviousSlide.enabled);
                Assert.IsFalse(actions.Gameplay.PauseMenu.enabled);
            }
            finally
            {
                Object.DestroyImmediate(routerObject);
                Object.DestroyImmediate(managerObject);
            }
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
            var (router, managerObject, routerObject) = CreateInitializedInputRouter();

            try
            {
                var manager = managerObject.GetComponent<GameStateManager>();
                NavigateManagerTo(manager, GameStateType.Reinforcement);

                bool shouldBlock = InvokeShouldBlockInput(router, InputActionType.AdvanceSlide);
                Assert.IsTrue(shouldBlock);
            }
            finally
            {
                Object.DestroyImmediate(routerObject);
                Object.DestroyImmediate(managerObject);
            }
        }

        [Test]
        public void ShouldBlockInput_InRehearsalState_ReturnsFalse()
        {
            var (router, managerObject, routerObject) = CreateInitializedInputRouter();

            try
            {
                var manager = managerObject.GetComponent<GameStateManager>();
                NavigateManagerTo(manager, GameStateType.Rehearsal);

                bool shouldBlock = InvokeShouldBlockInput(router, InputActionType.AdvanceSlide);
                Assert.IsFalse(shouldBlock);
            }
            finally
            {
                Object.DestroyImmediate(routerObject);
                Object.DestroyImmediate(managerObject);
            }
        }

        [Test]
        public void OnStateExit_Reinforcement_ReEnablesGameplayActions()
        {
            var (router, managerObject, routerObject) = CreateInitializedInputRouter();

            try
            {
                router.OnStateEnter(GameStateType.Reinforcement);
                router.OnStateExit(GameStateType.Reinforcement);

                var actions = ReadPrivateField<StageMindActions>(router, "_actions");
                Assert.IsTrue(actions.Gameplay.AdvanceSlide.enabled);
                Assert.IsTrue(actions.Gameplay.PreviousSlide.enabled);
                Assert.IsTrue(actions.Gameplay.PauseMenu.enabled);
            }
            finally
            {
                Object.DestroyImmediate(routerObject);
                Object.DestroyImmediate(managerObject);
            }
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

        private static (InputRouter router, GameObject managerObject, GameObject routerObject) CreateInitializedInputRouter()
        {
            var managerObject = new GameObject("InputRouterTests_Manager");
            managerObject.AddComponent<GameStateManager>();

            var routerObject = new GameObject("InputRouterTests_Router");
            var router = routerObject.AddComponent<InputRouter>();
            SetPrivateField(router, "_gameStateManager", managerObject.GetComponent<GameStateManager>());

            // Ensure private StageMindActions instance is created for action enable/disable assertions.
            InvokePrivateMethod(router, "Awake");

            return (router, managerObject, routerObject);
        }

        private static void NavigateManagerTo(GameStateManager manager, GameStateType target)
        {
            if (manager.CurrentStateType == target)
            {
                return;
            }

            switch (target)
            {
                case GameStateType.LobbySlidesLoaded:
                    manager.TransitionTo(GameStateType.LobbySlidesLoaded);
                    break;
                case GameStateType.Rehearsal:
                    manager.TransitionTo(GameStateType.LobbySlidesLoaded);
                    manager.TransitionTo(GameStateType.Rehearsal);
                    break;
                case GameStateType.Paused:
                    manager.TransitionTo(GameStateType.LobbySlidesLoaded);
                    manager.TransitionTo(GameStateType.Rehearsal);
                    manager.TransitionTo(GameStateType.Paused);
                    break;
                case GameStateType.Reinforcement:
                    manager.TransitionTo(GameStateType.LobbySlidesLoaded);
                    manager.TransitionTo(GameStateType.Rehearsal);
                    manager.TransitionTo(GameStateType.Paused);
                    manager.TransitionTo(GameStateType.Reinforcement);
                    break;
            }
        }

        private static bool InvokeShouldBlockInput(InputRouter router, InputActionType actionType)
        {
            var method = typeof(InputRouter).GetMethod("ShouldBlockInput", BindingFlags.Instance | BindingFlags.NonPublic);
            return (bool)method.Invoke(router, new object[] { actionType });
        }

        private static void InvokePrivateMethod(object instance, string methodName)
        {
            var method = instance.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
            method?.Invoke(instance, null);
        }

        private static T ReadPrivateField<T>(object instance, string fieldName) where T : class
        {
            var field = instance.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            return field?.GetValue(instance) as T;
        }

        private static void SetPrivateField(object instance, string fieldName, object value)
        {
            var field = instance.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            field?.SetValue(instance, value);
        }
    }
}
