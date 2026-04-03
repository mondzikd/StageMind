using System;
using UnityEngine;

namespace StageMind
{
    public class GameStateManager : MonoBehaviour
    {
        public event Action<GameStateType, GameStateType> OnStateChanged
        {
            add => EnsureStateMachineCreated().OnStateChanged += value;
            remove => EnsureStateMachineCreated().OnStateChanged -= value;
        }

        public GameStateType CurrentStateType => EnsureStateMachineCreated().CurrentStateType;

        private StateMachine _stateMachine;
        private bool _isInitialized;

        private void Awake()
        {
            Initialize();
        }

        private void Update()
        {
            if (!_isInitialized)
            {
                Initialize();
            }

            _stateMachine.Update();
        }

        public void TransitionTo(GameStateType target)
        {
            if (!_isInitialized)
            {
                Initialize();
            }

            _stateMachine.TransitionTo(target);
        }

        public void RegisterStateAware(IStateAware listener)
        {
            if (!_isInitialized)
            {
                Initialize();
            }

            _stateMachine.RegisterStateAware(listener);
        }

        public void UnregisterStateAware(IStateAware listener)
        {
            if (!_isInitialized)
            {
                Initialize();
            }

            _stateMachine.UnregisterStateAware(listener);
        }

        public void HandleInputAction(InputActionType actionType)
        {
            if (!_isInitialized)
            {
                Initialize();
            }

            _stateMachine.HandleInputAction(actionType);
        }

        private void Initialize()
        {
            if (_isInitialized)
            {
                return;
            }

            _stateMachine = EnsureStateMachineCreated();
            _stateMachine.Initialize();
            _isInitialized = true;
        }

        private StateMachine EnsureStateMachineCreated()
        {
            _stateMachine ??= new StateMachine(CreateState);
            return _stateMachine;
        }

        private IGameState CreateState(GameStateType stateType, GameStateType previousStateType)
        {
            return stateType switch
            {
                GameStateType.LobbyLanding => new LobbyLandingState(this),
                GameStateType.LobbySlidesLoaded => new LobbySlidesLoadedState(this),
                GameStateType.Rehearsal => new RehearsalState(this),
                GameStateType.Reinforcement => new ReinforcementState(this),
                GameStateType.Paused => new PausedState(this, previousStateType),
                _ => throw new ArgumentOutOfRangeException(nameof(stateType), stateType, null)
            };
        }
    }
}
