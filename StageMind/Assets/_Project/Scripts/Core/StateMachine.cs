using System;
using System.Collections.Generic;
using UnityEngine;

namespace StageMind
{
    public class StateMachine
    {
        public event Action<GameStateType, GameStateType> OnStateChanged;

        public GameStateType CurrentStateType { get; private set; }

        private IGameState _currentState;
        private readonly List<IStateAware> _stateAwareListeners = new();
        private readonly Func<GameStateType, GameStateType, IGameState> _stateFactory;

        private static readonly HashSet<(GameStateType From, GameStateType To)> _allowedTransitions = new()
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

        public StateMachine(Func<GameStateType, GameStateType, IGameState> stateFactory)
        {
            _stateFactory = stateFactory;
        }

        public void Initialize()
        {
            if (_currentState != null)
            {
                return;
            }

            CurrentStateType = GameStateType.LobbyLanding;
            _currentState = _stateFactory(GameStateType.LobbyLanding, GameStateType.LobbyLanding);
            _currentState.Enter();
        }

        public void TransitionTo(GameStateType target)
        {
            if (!_allowedTransitions.Contains((CurrentStateType, target)))
            {
                Debug.LogWarning($"[StageMind] Invalid transition: {CurrentStateType} → {target}");
                return;
            }

            var previousStateType = CurrentStateType;

            _currentState.Exit();
            CurrentStateType = target;
            _currentState = _stateFactory(target, previousStateType);
            _currentState.Enter();

            for (int i = 0; i < _stateAwareListeners.Count; i++)
            {
                var listener = _stateAwareListeners[i];
                if (listener == null)
                {
                    continue;
                }

                listener.OnStateExit(previousStateType);
                listener.OnStateEnter(target);
            }

            OnStateChanged?.Invoke(previousStateType, target);
        }

        public void Update()
        {
            _currentState?.Update();
        }

        public void RegisterStateAware(IStateAware listener)
        {
            if (listener == null)
            {
                Debug.LogWarning("[StageMind] Ignoring null IStateAware listener registration.");
                return;
            }

            if (_stateAwareListeners.Contains(listener))
            {
                return;
            }

            _stateAwareListeners.Add(listener);
        }

        public void UnregisterStateAware(IStateAware listener)
        {
            if (listener == null)
            {
                return;
            }

            _stateAwareListeners.Remove(listener);
        }
    }
}
