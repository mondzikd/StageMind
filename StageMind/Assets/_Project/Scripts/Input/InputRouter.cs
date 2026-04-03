using UnityEngine;
using UnityEngine.InputSystem;

namespace StageMind
{
    public class InputRouter : MonoBehaviour, IStateAware
    {
        [SerializeField] private GameStateManager _gameStateManager;
        [SerializeField] private HapticFeedback _hapticFeedback;

        private StageMindActions _actions;

        private void Awake()
        {
            _actions = new StageMindActions();
        }

        private void OnEnable()
        {
            _actions.Gameplay.Enable();
            _actions.UI.Enable();
            _actions.Haptics.Enable();

            _actions.Gameplay.AdvanceSlide.performed += OnAdvanceSlide;
            _actions.Gameplay.PreviousSlide.performed += OnPreviousSlide;
            _actions.Gameplay.PauseMenu.performed += OnPauseMenu;
        }

        private void OnDisable()
        {
            _actions.Gameplay.AdvanceSlide.performed -= OnAdvanceSlide;
            _actions.Gameplay.PreviousSlide.performed -= OnPreviousSlide;
            _actions.Gameplay.PauseMenu.performed -= OnPauseMenu;

            _actions.Gameplay.Disable();
            _actions.UI.Disable();
            _actions.Haptics.Disable();
        }

        private void Start()
        {
            _gameStateManager.RegisterStateAware(this);
        }

        private void OnDestroy()
        {
            _gameStateManager.UnregisterStateAware(this);
            _actions?.Dispose();
        }

        public void OnStateEnter(GameStateType state)
        {
            if (state == GameStateType.Reinforcement)
            {
                _actions.Gameplay.AdvanceSlide.Disable();
                _actions.Gameplay.PauseMenu.Disable();
                _actions.Gameplay.PreviousSlide.Disable();
            }
        }

        public void OnStateExit(GameStateType state)
        {
            if (state == GameStateType.Reinforcement)
            {
                _actions.Gameplay.AdvanceSlide.Enable();
                _actions.Gameplay.PauseMenu.Enable();
                _actions.Gameplay.PreviousSlide.Enable();
            }
        }

        private void OnAdvanceSlide(InputAction.CallbackContext context)
        {
            _gameStateManager.HandleInputAction(InputActionType.AdvanceSlide);
            _hapticFeedback.ClickRight();
        }

        private void OnPreviousSlide(InputAction.CallbackContext context)
        {
            _gameStateManager.HandleInputAction(InputActionType.PreviousSlide);
            _hapticFeedback.ClickRight();
        }

        private void OnPauseMenu(InputAction.CallbackContext context)
        {
            _gameStateManager.HandleInputAction(InputActionType.PauseMenu);
            _hapticFeedback.ClickLeft();
        }
    }
}
