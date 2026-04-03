using UnityEngine;
using UnityEngine.InputSystem;

namespace StageMind
{
    public class InputRouter : MonoBehaviour, IStateAware
    {
        [SerializeField] private GameStateManager _gameStateManager;
        [SerializeField] private HapticFeedback _hapticFeedback;
        [SerializeField] private MonoBehaviour _webViewControllerComponent;

        private StageMindActions _actions;
        private IWebViewController _webViewController;

        private void Awake()
        {
            _actions = new StageMindActions();
            _webViewController = _webViewControllerComponent as IWebViewController;
        }

        private void OnEnable()
        {
            _actions.Gameplay.Enable();
            _actions.UI.Enable();
            _actions.Haptics.Enable();
            SubscribeWebViewEvents();

            _actions.Gameplay.AdvanceSlide.performed += OnAdvanceSlide;
            _actions.Gameplay.PreviousSlide.performed += OnPreviousSlide;
            _actions.Gameplay.PauseMenu.performed += OnPauseMenu;
        }

        private void OnDisable()
        {
            _actions.Gameplay.AdvanceSlide.performed -= OnAdvanceSlide;
            _actions.Gameplay.PreviousSlide.performed -= OnPreviousSlide;
            _actions.Gameplay.PauseMenu.performed -= OnPauseMenu;
            UnsubscribeWebViewEvents();

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
            UnsubscribeWebViewEvents();
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
            if (ShouldBlockInput(InputActionType.AdvanceSlide))
            {
                return;
            }

            _gameStateManager.TryHandleInputAction(InputActionType.AdvanceSlide);
        }

        private void OnPreviousSlide(InputAction.CallbackContext context)
        {
            if (ShouldBlockInput(InputActionType.PreviousSlide))
            {
                return;
            }

            _gameStateManager.TryHandleInputAction(InputActionType.PreviousSlide);
        }

        private void OnPauseMenu(InputAction.CallbackContext context)
        {
            if (ShouldBlockInput(InputActionType.PauseMenu))
            {
                return;
            }

            bool actionHandled = _gameStateManager.TryHandleInputAction(InputActionType.PauseMenu);
            if (ShouldTriggerHaptic(actionHandled, InputActionType.PauseMenu))
            {
                _hapticFeedback.ClickLeft();
            }
        }

        private void OnWebViewKeyEventResult(KeyCode key, bool changed)
        {
            if (!changed || _gameStateManager == null)
            {
                return;
            }

            if (_gameStateManager.CurrentStateType != GameStateType.Rehearsal)
            {
                return;
            }

            if (key == KeyCode.RightArrow || key == KeyCode.LeftArrow)
            {
                _hapticFeedback.ClickRight();
            }
        }

        private bool ShouldBlockInput(InputActionType actionType)
        {
            return _gameStateManager != null &&
                   _gameStateManager.CurrentStateType == GameStateType.Reinforcement;
        }

        private bool ShouldTriggerHaptic(bool actionHandled, InputActionType actionType)
        {
            return actionHandled && !ShouldBlockInput(actionType);
        }

        private void SubscribeWebViewEvents()
        {
            _webViewController ??= _webViewControllerComponent as IWebViewController;
            if (_webViewController == null)
            {
                return;
            }

            _webViewController.OnKeyEventResult -= OnWebViewKeyEventResult;
            _webViewController.OnKeyEventResult += OnWebViewKeyEventResult;
        }

        private void UnsubscribeWebViewEvents()
        {
            if (_webViewController == null)
            {
                return;
            }

            _webViewController.OnKeyEventResult -= OnWebViewKeyEventResult;
        }
    }
}
