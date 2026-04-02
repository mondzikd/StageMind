using System;
using UnityEngine;

namespace StageMind
{
    public class GameStateManager : MonoBehaviour
    {
        public event Action<GameStateType, GameStateType> OnStateChanged
        {
            add => _stateMachine.OnStateChanged += value;
            remove => _stateMachine.OnStateChanged -= value;
        }

        public GameStateType CurrentStateType => _stateMachine.CurrentStateType;

        private StateMachine _stateMachine;

        private void Awake()
        {
            Initialize();
        }

        private void Update()
        {
            _stateMachine.Update();
        }

        public void TransitionTo(GameStateType target)
        {
            _stateMachine.TransitionTo(target);
        }

        public void RegisterStateAware(IStateAware listener)
        {
            _stateMachine.RegisterStateAware(listener);
        }

        public void UnregisterStateAware(IStateAware listener)
        {
            _stateMachine.UnregisterStateAware(listener);
        }

        private void Initialize()
        {
            _stateMachine = new StateMachine(CreateState);
            _stateMachine.Initialize();
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
