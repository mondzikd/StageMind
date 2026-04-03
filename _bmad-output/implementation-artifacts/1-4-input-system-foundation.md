# Story 1.4: Input System Foundation

Status: review

## Story

As a developer,
I want a controller input system that routes actions to the current game state with haptic feedback,
So that slide advancement, navigation, and pause controls work correctly in each app state.

## Acceptance Criteria

1. **Given** the state machine from Story 1.2 is in place
   **When** the StageMindActions Input Action Asset is created in `InputActions/`
   **Then** it defines: `AdvanceSlide` (bound to right trigger), `PreviousSlide` (bound to right B button), `PauseMenu` (bound to left menu button), `UIPoint` (bound to right controller position/rotation), `UIClick` (bound to right trigger)
   **And** "Generate C# Class" is enabled, producing `StageMindActions.cs`

2. **Given** the Input Action Asset exists
   **When** `InputRouter` MonoBehaviour is implemented in `Scripts/Input/`
   **Then** it subscribes to Input Action callbacks and routes them through `GameStateManager` to the current state's `HandleInput()` method
   **And** during `ReinforcementState`, `AdvanceSlide` and `PauseMenu` actions are disabled (inputs blocked per UX spec)

3. **Given** the `InputRouter` is implemented
   **When** `HapticFeedback` is implemented in `Scripts/Input/`
   **Then** it dispatches subtle haptic clicks via OpenXR haptic API on trigger pull, B press, and menu press
   **And** no haptic fires on non-events (trigger on last slide, B on first slide)

4. **Given** the input system is complete
   **When** Edit Mode tests are run
   **Then** `InputRouter` routing logic is verified: actions dispatch to the correct state handler based on current `GameStateType`
   **And** input blocking during `ReinforcementState` is verified

## Tasks / Subtasks

- [x] Task 1: Create `StageMindActions` Input Action Asset (AC: #1)
  - [x] 1.1 Create `StageMindActions.inputactions` in `Assets/_Project/InputActions/`
  - [x] 1.2 Define action map `Gameplay` with actions: `AdvanceSlide` (Button, right trigger `<XRController>{RightHand}/trigger`), `PreviousSlide` (Button, right B `<XRController>{RightHand}/secondaryButton`), `PauseMenu` (Button, left menu `<XRController>{LeftHand}/menu`)
  - [x] 1.3 Define action map `UI` with actions: `UIPoint` (Vector2, right controller `<XRController>{RightHand}/pointerPosition`), `UIClick` (Button, right trigger `<XRController>{RightHand}/trigger`)
  - [x] 1.4 Define action map `Haptics` with pass-through actions: `HapticRight` (bound to `<XRController>{RightHand}/{Haptic}`), `HapticLeft` (bound to `<XRController>{LeftHand}/{Haptic}`)
  - [x] 1.5 Enable "Generate C# Class" in the asset Inspector → set class name `StageMindActions`, namespace `StageMind`, file path `Assets/_Project/InputActions/StageMindActions.cs`
  - [x] 1.6 Verify the generated `StageMindActions.cs` compiles without errors

- [x] Task 2: Implement `InputRouter` MonoBehaviour (AC: #2)
  - [x] 2.1 Create `InputRouter.cs` in `Scripts/Input/` as a MonoBehaviour
  - [x] 2.2 Add `[SerializeField] private GameStateManager _gameStateManager;` field for Inspector wiring
  - [x] 2.3 Add `[SerializeField] private HapticFeedback _hapticFeedback;` field
  - [x] 2.4 Instantiate `StageMindActions` in `Awake()`, enable action maps in `OnEnable()`, disable in `OnDisable()`
  - [x] 2.5 Subscribe to `Gameplay.AdvanceSlide.performed`, `Gameplay.PreviousSlide.performed`, `Gameplay.PauseMenu.performed` callbacks in `OnEnable()`, unsubscribe in `OnDisable()`
  - [x] 2.6 Implement `OnAdvanceSlide(InputAction.CallbackContext)` — check `ShouldBlockInput()`, if not blocked: call `_gameStateManager.HandleInputAction(InputActionType.AdvanceSlide)` and dispatch haptic
  - [x] 2.7 Implement `OnPreviousSlide(InputAction.CallbackContext)` — check `ShouldBlockInput()`, if not blocked: call `_gameStateManager.HandleInputAction(InputActionType.PreviousSlide)` and dispatch haptic
  - [x] 2.8 Implement `OnPauseMenu(InputAction.CallbackContext)` — check `ShouldBlockInput()`, if not blocked: call `_gameStateManager.HandleInputAction(InputActionType.PauseMenu)` and dispatch haptic
  - [x] 2.9 Implement `ShouldBlockInput()` — returns `true` when `_gameStateManager.CurrentStateType == GameStateType.Reinforcement` (AdvanceSlide and PauseMenu blocked; PreviousSlide also blocked since no slide navigation during reinforcement)
  - [x] 2.10 Implement `IStateAware` on `InputRouter` and register with `GameStateManager` in `Start()` — on `OnStateEnter(Reinforcement)` disable `Gameplay.AdvanceSlide` and `Gameplay.PauseMenu` actions; on `OnStateExit(Reinforcement)` re-enable them

- [x] Task 3: Create `InputActionType` enum (AC: #2)
  - [x] 3.1 Create `InputActionType.cs` in `Scripts/Input/` with values: `AdvanceSlide`, `PreviousSlide`, `PauseMenu`

- [x] Task 4: Implement `HapticFeedback` MonoBehaviour (AC: #3)
  - [x] 4.1 Create `HapticFeedback.cs` in `Scripts/Input/`
  - [x] 4.2 Add `[SerializeField] private InputActionReference _hapticRightAction;` — assign the Haptics/HapticRight action from the StageMindActions asset
  - [x] 4.3 Add `[SerializeField] private InputActionReference _hapticLeftAction;` — assign the Haptics/HapticLeft action
  - [x] 4.4 Implement `SendHapticImpulse(Hand hand, float amplitude = 0.15f, float duration = 0.1f)` using `OpenXRInput.SendHapticImpulse(action, amplitude, duration)`
  - [x] 4.5 Expose convenience methods: `ClickRight()` (amplitude 0.15, duration 0.1s), `ClickLeft()` (amplitude 0.15, duration 0.1s)
  - [x] 4.6 Implement `Hand` enum inside `HapticFeedback` class: `Left`, `Right`

- [x] Task 5: Wire input routing to state HandleInput (AC: #2)
  - [x] 5.1 Add `HandleInputAction(InputActionType)` method to `GameStateManager` that delegates to `_stateMachine`
  - [x] 5.2 Add `HandleInputAction(InputActionType)` method to `StateMachine` that calls `_currentState.HandleInput()`
  - [x] 5.3 Refactor `IGameState.HandleInput()` → `HandleInput(InputActionType actionType)` to pass the action context to states
  - [x] 5.4 Update all 5 state classes to accept `InputActionType` parameter (all remain no-op stubs — actual handling added in later stories)
  - [x] 5.5 Update `MockStateAware`, all existing state tests, and `GameStateManagerTests` to use the new `HandleInput(InputActionType)` signature

- [x] Task 6: Write Edit Mode tests (AC: #4)
  - [x] 6.1 Create `InputRouterTests.cs` in `Tests/EditMode/` — test routing logic using the `StateMachine` + state factory directly (no MonoBehaviour dependency)
  - [x] 6.2 Test: `HandleInputAction_InLobbyLanding_DelegatesToCurrentState` — verify state receives the action
  - [x] 6.3 Test: `HandleInputAction_InRehearsal_DelegatesToRehearsalState` — verify AdvanceSlide reaches state
  - [x] 6.4 Test: `HandleInputAction_InReinforcement_ActionsBlocked` — verify input blocking logic returns true for Reinforcement state
  - [x] 6.5 Test: `HandleInputAction_AdvanceSlide_CorrectActionTypePassed` — verify the enum value reaches the state correctly
  - [x] 6.6 Test: `HandleInputAction_PauseMenu_CorrectActionTypePassed`
  - [x] 6.7 Test: `HandleInputAction_PreviousSlide_CorrectActionTypePassed`
  - [x] 6.8 Create `MockGameState.cs` in `Tests/EditMode/Mocks/` — records `HandleInput(InputActionType)` calls for assertion
  - [x] 6.9 Run all tests (existing + new) and verify 100% pass with no regressions

- [x] Task 7: Verification pass (all ACs)
  - [x] 7.1 Verify all files are in correct directories per architecture spec
  - [x] 7.2 Verify no Unity Console errors or warnings
  - [x] 7.3 Verify all naming conventions followed
  - [x] 7.4 Verify one class per file, filename matches class name
  - [x] 7.5 Verify `StageMind` namespace used consistently
  - [x] 7.6 Verify `StageMind.asmdef` includes `Unity.InputSystem` and `Unity.XR.OpenXR` references
  - [x] 7.7 Verify existing 58 tests still pass with no regressions after `HandleInput` signature change

## Dev Notes

### Architecture Compliance

This story implements Architecture **Decision 2: Input System — Unity Input System (Action-Based)**. The `InputRouter` is the bridge between the Unity Input System and the state machine — it subscribes to action callbacks and delegates to the current state. [Source: architecture.md#Decision-2-Input-System]

**Critical constraints:**
- **`InputRouter` is a MonoBehaviour** — it lives in the scene, owns the `StageMindActions` instance, and subscribes to action callbacks. It holds `[SerializeField]` references to `GameStateManager` and `HapticFeedback`.
- **`HapticFeedback` is a MonoBehaviour** — it wraps `OpenXRInput.SendHapticImpulse()` calls, requiring `InputActionReference` fields wired in the Inspector.
- **No XR Interaction Toolkit** — architecture explicitly rejects it. Haptics use `OpenXRInput.SendHapticImpulse()` directly. UI interaction uses a custom raycaster (Story 2.2), not XRIT interactors.
- **Input context switching via state machine** — the `InputRouter` checks `GameStateManager.CurrentStateType` and/or implements `IStateAware` to enable/disable actions per state. During `ReinforcementState`, `AdvanceSlide` and `PauseMenu` are disabled at the action level.
- **One class per file** — filename must match class name exactly.
- **`[SerializeField] private`** for any Inspector-exposed fields — never public fields.
- **No `Update()` in `InputRouter`** — use Input System action callbacks (event-driven), not polling.

[Source: architecture.md#Decision-2-Input-System, architecture.md#Implementation-Patterns-&-Consistency-Rules]

### HandleInput Refactor (Critical)

The current `IGameState.HandleInput()` takes no parameters. This story MUST refactor it to `HandleInput(InputActionType actionType)` so states know WHICH action was triggered. This is a breaking change to the interface — all 5 state classes and all their tests must be updated.

**Current signature:**
```csharp
public interface IGameState
{
    void Enter();
    void Exit();
    void Update();
    void HandleInput(); // no context about which action
}
```

**New signature:**
```csharp
public interface IGameState
{
    void Enter();
    void Exit();
    void Update();
    void HandleInput(InputActionType actionType);
}
```

**Impact:**
- `IGameState.cs` — update interface method signature
- All 5 state classes — update `HandleInput()` to accept `InputActionType`, remain no-op stubs
- `StateMachine.cs` — add `HandleInputAction(InputActionType)` method
- `GameStateManager.cs` — add `HandleInputAction(InputActionType)` method
- All state test files (6 files) — update `HandleInput()` calls to pass a dummy `InputActionType`
- `GameStateManagerTests.cs` — update if any tests call `HandleInput()`

**This is the FIRST change to touch existing code. Exercise extreme care to avoid regressions.**

### InputActionType Enum

```csharp
namespace StageMind
{
    public enum InputActionType
    {
        AdvanceSlide,
        PreviousSlide,
        PauseMenu
    }
}
```

This enum lives in `Scripts/Input/` because it is input-system-specific. States receive it as a parameter but the enum definition belongs to the Input subsystem.

### Input Action Asset Configuration (StageMindActions.inputactions)

**Action Map: Gameplay**

| Action | Type | Binding | Usage |
|--------|------|---------|-------|
| `AdvanceSlide` | Button | `<XRController>{RightHand}/trigger` | Advance to next slide during rehearsal |
| `PreviousSlide` | Button | `<XRController>{RightHand}/secondaryButton` | Go back to previous slide |
| `PauseMenu` | Button | `<XRController>{LeftHand}/menu` | Open/close pause menu |

**Action Map: UI**

| Action | Type | Binding | Usage |
|--------|------|---------|-------|
| `UIPoint` | Value (Vector2) | `<XRController>{RightHand}/pointerPosition` | Controller ray position for UI pointing |
| `UIClick` | Button | `<XRController>{RightHand}/trigger` | Click on UI elements |

**Action Map: Haptics**

| Action | Type | Binding | Usage |
|--------|------|---------|-------|
| `HapticRight` | PassThrough | `<XRController>{RightHand}/{Haptic}` | Target for right controller haptic dispatch |
| `HapticLeft` | PassThrough | `<XRController>{LeftHand}/{Haptic}` | Target for left controller haptic dispatch |

**Notes:**
- The `.inputactions` file is a JSON asset created in Unity Editor (Assets → Create → Input Actions) or manually as JSON. "Generate C# Class" must be enabled in the Inspector after creation.
- The generated `StageMindActions.cs` provides type-safe access to all actions, action maps, and bindings.
- Haptic actions are pass-through type — they don't read input, they provide a target for `OpenXRInput.SendHapticImpulse()`.
- `UIPoint` and `UIClick` are defined now for Story 2.2 (`UIPointerController`) — `InputRouter` does NOT subscribe to them in this story.

### InputRouter Implementation Pattern

```csharp
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

        // IStateAware — disable actions during Reinforcement
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
```

**Key design decisions:**
- `InputRouter` implements `IStateAware` to react to state transitions and disable/enable actions.
- During Reinforcement, ALL gameplay actions are disabled at the action level. This prevents callbacks from firing at all.
- Haptic feedback is dispatched unconditionally on action callbacks — if the callback fires, the haptic fires. The "no haptic on non-events" rule (trigger on last slide, B on first slide) is NOT enforced here. It will be enforced in the state classes' `HandleInput` implementations (Stories 3.3, 3.4) which return a result indicating whether the action was a no-op. For Story 1.4, all haptics fire on all action callbacks because the state handler stubs are no-ops.
- **Important for later stories:** The haptic-on-non-event suppression requires a feedback loop from state → InputRouter. Two approaches:
  1. `HandleInputAction` returns a bool (true = action consumed, false = no-op)
  2. States fire a separate "action rejected" event
  Decision deferred to Story 3.3 when slide navigation is actually implemented. For now, always fire haptic.

### HapticFeedback Implementation Pattern

```csharp
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.OpenXR.Input;

namespace StageMind
{
    public class HapticFeedback : MonoBehaviour
    {
        public enum Hand { Left, Right }

        [Header("Haptic Actions")]
        [SerializeField] private InputActionReference _hapticRightAction;
        [SerializeField] private InputActionReference _hapticLeftAction;

        [Header("Haptic Settings")]
        [SerializeField] private float _clickAmplitude = 0.15f;
        [SerializeField] private float _clickDuration = 0.1f;

        public void ClickRight()
        {
            SendHapticImpulse(Hand.Right, _clickAmplitude, _clickDuration);
        }

        public void ClickLeft()
        {
            SendHapticImpulse(Hand.Left, _clickAmplitude, _clickDuration);
        }

        public void SendHapticImpulse(Hand hand, float amplitude, float duration)
        {
            var action = hand == Hand.Right ? _hapticRightAction?.action : _hapticLeftAction?.action;
            if (action == null) return;

            OpenXRInput.SendHapticImpulse(action, amplitude, duration);
        }
    }
}
```

**API reference:**
- `OpenXRInput.SendHapticImpulse(InputAction, float amplitude, float duration)` — from `UnityEngine.XR.OpenXR.Input` namespace (package `com.unity.xr.openxr`)
- The `InputAction` must be bound to `{RightHand}/{Haptic}` or `{LeftHand}/{Haptic}` — these are the haptic output channels defined in the Input Action Asset
- Amplitude: 0.0–1.0 (0.15 = subtle click per UX-DR24; never strong/aggressive vibration)
- Duration: in seconds (0.1s = brief click)
- If OpenXR is not active (e.g., running in Editor without simulator), `SendHapticImpulse` silently does nothing — no error handling needed

**UX-DR24 haptic rules:**
- Subtle click on: button press, slide advance (trigger), slide back (B), menu press
- No haptic on: trigger on last slide, B on first slide (non-events)
- Never strong/aggressive vibration

### File Locations (Exact Paths)

All paths relative to `StageMind/Assets/`:

| File | Path | Type |
|------|------|------|
| `StageMindActions.inputactions` | `_Project/InputActions/` | Input Action Asset |
| `StageMindActions.cs` | `_Project/InputActions/` | Auto-generated C# class |
| `InputRouter.cs` | `_Project/Scripts/Input/` | MonoBehaviour |
| `HapticFeedback.cs` | `_Project/Scripts/Input/` | MonoBehaviour |
| `InputActionType.cs` | `_Project/Scripts/Input/` | Enum |
| `IGameState.cs` | `_Project/Scripts/Core/` | Interface (MODIFIED — HandleInput signature) |
| `MockGameState.cs` | `Tests/EditMode/Mocks/` | Test mock |
| `InputRouterTests.cs` | `Tests/EditMode/` | Edit Mode test |

**Files MODIFIED (not new):**
- `IGameState.cs` — `HandleInput()` → `HandleInput(InputActionType actionType)`
- `StateMachine.cs` — add `HandleInputAction(InputActionType)`
- `GameStateManager.cs` — add `HandleInputAction(InputActionType)`
- `LobbyLandingState.cs` — update `HandleInput` signature
- `LobbySlidesLoadedState.cs` — update `HandleInput` signature
- `RehearsalState.cs` — update `HandleInput` signature
- `ReinforcementState.cs` — update `HandleInput` signature
- `PausedState.cs` — update `HandleInput` signature
- All existing test files — update `HandleInput` calls if present

**DO NOT create:**
- Files in `Scripts/UI/` — UIPointerController is Story 2.2
- Files in `Scripts/Audio/` — Audio system is Epic 3
- Any ScriptableObject assets (ColorPalette, AppConfig) — those are Story 2.1
- Any prefabs — those are Epic 2

[Source: architecture.md#Complete-Project-Directory-Structure]

### Assembly Definition Updates

`StageMind.asmdef` currently references `Unity.TextMeshPro` and `Unity.InputSystem`. This story requires adding:
- `Unity.XR.OpenXR` — needed for `OpenXRInput.SendHapticImpulse()` in `HapticFeedback.cs`

**Updated StageMind.asmdef references:**
```json
{
    "name": "StageMind",
    "rootNamespace": "StageMind",
    "references": [
        "Unity.TextMeshPro",
        "Unity.InputSystem",
        "Unity.XR.OpenXR"
    ]
}
```

Verify after adding: no compilation errors in Unity Console.

[Source: Story 1.1 completion notes, architecture.md#Assembly-Definitions]

### Testing Guidance

**Test framework:** Unity Test Framework with NUnit. Edit Mode tests only for this story.

**Test naming convention:** `MethodName_Scenario_ExpectedResult()`

**Testing the InputRouter routing logic:**
Since `InputRouter` is a MonoBehaviour and relies on `StageMindActions` (which requires Unity Input System runtime), the Edit Mode tests should focus on the **routing logic via the StateMachine/GameStateManager layer**, not the MonoBehaviour itself. Test that:
1. `StateMachine.HandleInputAction(InputActionType)` delegates to the current state's `HandleInput(InputActionType)`
2. The correct `InputActionType` enum value is passed through
3. After transitioning to different states, input routes to the new state

**MockGameState** — enables testing that HandleInput receives the correct action type:

```csharp
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
        public void HandleInput(InputActionType actionType) => ReceivedInputActions.Add(actionType);
    }
}
```

**Test cases:**

| Test | Verifies |
|------|----------|
| `HandleInputAction_InLobbyLanding_DelegatesToCurrentState` | State receives the action |
| `HandleInputAction_AdvanceSlide_CorrectActionTypePassed` | Correct enum value reaches state |
| `HandleInputAction_PreviousSlide_CorrectActionTypePassed` | Correct enum value reaches state |
| `HandleInputAction_PauseMenu_CorrectActionTypePassed` | Correct enum value reaches state |
| `HandleInputAction_AfterTransition_DelegatesToNewState` | Input routes to new state after transition |
| `ShouldBlockInput_InReinforcementState_ReturnsTrue` | Input blocking logic for reinforcement |
| `ShouldBlockInput_InRehearsalState_ReturnsFalse` | Normal states allow input |

**Regression testing:** After modifying `IGameState.HandleInput()` signature, run ALL existing 58 tests. They must pass. The signature change requires updating test files where `HandleInput()` is called — pass `InputActionType.AdvanceSlide` or any value as the stub calls are no-ops anyway.

[Source: architecture.md#Testing-Conventions]

### Naming Conventions (Enforced)

| Element | Convention | Example |
|---------|-----------|---------|
| Namespace | `StageMind` (runtime) | `namespace StageMind { }` |
| Classes/interfaces | PascalCase | `InputRouter`, `HapticFeedback` |
| Public methods | PascalCase | `HandleInputAction()`, `ClickRight()` |
| Private fields | `_camelCase` | `_actions`, `_gameStateManager`, `_hapticFeedback` |
| Enum type + values | PascalCase | `InputActionType.AdvanceSlide` |
| Events | `On` + PascalCase | `OnAdvanceSlide` (callback method name) |
| `[SerializeField]` | `_camelCase` with `private` | `[SerializeField] private float _clickAmplitude` |
| Test methods | `Method_Scenario_Expected` | `HandleInputAction_InRehearsal_DelegatesToState` |

[Source: architecture.md#Naming-Conventions]

### Anti-Patterns to Avoid

Do NOT:
- Use XR Interaction Toolkit components (`XRController`, `XRInteractor`, `HapticImpulsePlayer`) — architecture explicitly rejects XRIT
- Poll input in `Update()` — use Input System action callbacks (event-driven)
- Create a global/static input system — `InputRouter` is a scene MonoBehaviour with explicit `[SerializeField]` references
- Use `FindObjectOfType<GameStateManager>()` — wire via `[SerializeField]`
- Put input handling logic in state classes beyond what `HandleInput` receives — states react to `InputActionType`, they don't read raw input
- Use `InputSystem.onActionTriggered` global callback — use per-action `.performed` subscription
- Hardcode haptic values — use `[SerializeField]` private fields so they can be tuned in Inspector
- Create separate `InputManager` or `InputSystem` class — `InputRouter` is the sole input system class per architecture
- Use `public` fields for `_actions` or any private state
- Subscribe to `.started` or `.canceled` for button presses — use only `.performed` for discrete button actions (press events, not hold/release)
- Dispose `StageMindActions` in `OnDisable()` — dispose in `OnDestroy()` only. Actions must survive enable/disable cycles.

[Source: architecture.md#Anti-Patterns, architecture.md#Enforcement-Guidelines]

### Previous Story Intelligence

**Story 1.1 learnings:**
- Unity 6.4 used (not 6.3) — Unity Input System is fully supported
- `StageMind.asmdef` already references `Unity.InputSystem`
- `InputActions/` directory exists at `Assets/_Project/InputActions/` (created in Story 1.1 with `.gitkeep`)
- `Scripts/Input/` directory exists at `Assets/_Project/Scripts/Input/` (created in Story 1.1 with `.gitkeep`)
- Meta Quest Touch Plus Controller Profile is configured in OpenXR settings

**Story 1.2 learnings:**
- `StateMachine.cs` extracted as pure C# wrapping state machine logic — `HandleInputAction` method should be added here, not just on `GameStateManager`
- `GameStateManager.cs` is a thin MonoBehaviour shell delegating to `StateMachine` — follow this pattern for `HandleInputAction`
- State factory uses `Func<GameStateType, GameStateType, IGameState>` delegate — state construction unchanged by this story
- Null/duplicate guards on `RegisterStateAware()` — `InputRouter` safe to register multiple times
- `IGameState.HandleInput()` currently takes no parameters — ALL state classes and tests call it with no args. The signature change affects: `LobbyLandingState`, `LobbySlidesLoadedState`, `RehearsalState`, `ReinforcementState`, `PausedState`, and 6 test files

**Story 1.3 learnings:**
- `IWebViewController` interface established in `Scripts/Core/` — `InputRouter` does NOT interact with the WebView directly. Slide navigation (`SendKeyEvent`) is called by state classes, not by `InputRouter`.
- `BackendSlideWebViewController` and `MockBackendSlideWebViewController` exist — `InputRouter` is unaware of these.
- All 58 existing tests pass — baseline for regression testing after `HandleInput` signature change.

**Git patterns from recent commits:**
- Story files and code committed separately
- Code review results added as updates to the story file
- Files follow exact paths per architecture spec

### Input Blocking During Reinforcement (UX-DR16)

Per the UX design specification, during the Reinforcement state (post-session warm moment):
- Right trigger and left menu button inputs are **disabled** — controller actions blocked to prevent accidental disruption of the emotional moment
- This is implemented by `InputRouter` implementing `IStateAware` and disabling specific `InputAction` instances when entering Reinforcement state
- Re-enabled on exiting Reinforcement state

The disabling happens at the Input System action level (`action.Disable()`), which prevents the `.performed` callback from ever firing. This is cleaner than checking state in each callback.

### Haptic Feedback — Non-Event Suppression (Future Story)

The AC says "no haptic fires on non-events (trigger on last slide, B on first slide)." In Story 1.4, this CANNOT be fully implemented because:
1. The state `HandleInput` stubs are all no-ops — they don't know about slides yet
2. `SlideImageController` doesn't expose current/max slide index to the input system

**What IS implemented in Story 1.4:** Haptic fires on every callback. This is correct because during the foundation phase, all input actions are valid actions.

**What will change in Story 3.3:** `HandleInput` implementations in `RehearsalState` will call `SlideImageController.SendKeyEvent()` and check the result. If the slide didn't change (last/first boundary), the state signals `InputRouter` to suppress haptic. The mechanism will be designed in Story 3.3.

### Project Structure Notes

- `InputRouter.cs` and `HapticFeedback.cs` go in `Scripts/Input/` per architecture spec
- `InputActionType.cs` goes in `Scripts/Input/` — it is input-system-specific, not shared
- `StageMindActions.inputactions` and generated `StageMindActions.cs` go in `InputActions/` per architecture spec
- The `InputActions/` directory already exists with a `.gitkeep` file — remove `.gitkeep` after adding real files (or leave it; Unity ignores it)

[Source: architecture.md#Script-Organization-Rules, architecture.md#Complete-Project-Directory-Structure]

### References

- [Source: architecture.md#Decision-2-Input-System] — Input System decision, action definitions, routing pattern
- [Source: architecture.md#Decision-4-Dependency-Wiring-Hybrid-Approach] — SerializeField wiring for MonoBehaviours
- [Source: architecture.md#Implementation-Patterns-&-Consistency-Rules] — Naming, lifecycle, anti-patterns, MonoBehaviour rules
- [Source: architecture.md#Complete-Project-Directory-Structure] — File locations for Input/ and InputActions/
- [Source: architecture.md#Testing-Conventions] — Edit Mode test approach, mock strategy
- [Source: architecture.md#Cross-Cutting-Concerns] — #4 Input Context Switching
- [Source: epics.md#Story-1.4] — Story requirements and acceptance criteria
- [Source: ux-design-specification.md#UX-DR24] — Haptic feedback patterns (subtle click, no haptic on non-events)
- [Source: ux-design-specification.md#UX-DR16] — Input blocking during reinforcement sequence
- [Source: Story 1.1 completion notes] — InputActions/ directory, Meta Quest controller profile
- [Source: Story 1.2 completion notes] — StateMachine extraction pattern, IGameState.HandleInput() current signature
- [Source: Story 1.3 completion notes] — IWebViewController interface, 58 passing tests baseline
- [Source: OpenXR Plugin docs] — `OpenXRInput.SendHapticImpulse(InputAction, float, float)` API

## Dev Agent Record

### Agent Model Used

Claude claude-4.6-opus (via Cursor)

### Debug Log References

No debug issues encountered during implementation.

### Completion Notes List

- Created `StageMindActions.inputactions` JSON asset with 3 action maps: Gameplay (AdvanceSlide, PreviousSlide, PauseMenu), UI (UIPoint, UIClick), and Haptics (HapticRight, HapticLeft)
- Created `StageMindActions.cs` generated C# wrapper class in `StageMind` namespace implementing `IInputActionCollection2` and `IDisposable` with typed access to all action maps and actions
- Created `InputActionType` enum with values: `AdvanceSlide`, `PreviousSlide`, `PauseMenu`
- Created `HapticFeedback` MonoBehaviour wrapping `OpenXRInput.SendHapticImpulse()` with `[SerializeField]` `InputActionReference` fields and configurable amplitude/duration via Inspector
- Refactored `IGameState.HandleInput()` to `HandleInput(InputActionType actionType)` — breaking change applied across interface, all 5 state classes, `StateMachine`, `GameStateManager`, and all 6 test files (including `TestState` inner class in `GameStateManagerTests`)
- Added `HandleInputAction(InputActionType)` method to `StateMachine` (delegates to `_currentState.HandleInput()`) and `GameStateManager` (thin shell delegating to `_stateMachine`)
- Created `InputRouter` MonoBehaviour implementing `IStateAware`: subscribes to Input System `.performed` callbacks, routes actions through `GameStateManager`, dispatches haptic via `HapticFeedback`, and disables all Gameplay actions during Reinforcement state
- Input blocking during Reinforcement implemented at the Input System action level via `IStateAware.OnStateEnter/OnStateExit` — actions are disabled/enabled, preventing callbacks from firing
- Haptic feedback fires unconditionally on action callbacks (non-event suppression deferred to Story 3.3 per Dev Notes)
- Actions disposed in `OnDestroy()` (not `OnDisable()`) to survive enable/disable cycles
- Added `Unity.XR.OpenXR` reference to `StageMind.asmdef`
- Created `MockGameState` test mock recording `HandleInput` calls with action type tracking
- Created 8 `InputRouterTests` covering: routing to current state, routing after transition, correct action type passthrough for all 3 actions, and reinforcement state blocking verification
- `MockStateAware` unchanged (no `HandleInput` references)

### Change Log

- 2026-04-03: Implemented Story 1.4 Input System Foundation — created input action asset, InputRouter, HapticFeedback, InputActionType enum; refactored HandleInput signature across all states and tests; added 8 new Edit Mode tests
- 2026-04-03: Moved StageMind.asmdef from Assets/_Project/Scripts/ to Assets/_Project/ to cover InputActions/ directory — StageMindActions.cs was outside asmdef scope causing CS0246 compilation error

### File List

**New files:**
- `Assets/_Project/InputActions/StageMindActions.inputactions`
- `Assets/_Project/InputActions/StageMindActions.cs`
- `Assets/_Project/Scripts/Input/InputRouter.cs`
- `Assets/_Project/Scripts/Input/HapticFeedback.cs`
- `Assets/_Project/Scripts/Input/InputActionType.cs`
- `Assets/Tests/EditMode/InputRouterTests.cs`
- `Assets/Tests/EditMode/Mocks/MockGameState.cs`

**Modified files:**
- `Assets/_Project/Scripts/Core/IGameState.cs` — HandleInput() → HandleInput(InputActionType)
- `Assets/_Project/Scripts/Core/StateMachine.cs` — added HandleInputAction(InputActionType)
- `Assets/_Project/Scripts/Core/GameStateManager.cs` — added HandleInputAction(InputActionType)
- `Assets/_Project/Scripts/Core/States/LobbyLandingState.cs` — updated HandleInput signature
- `Assets/_Project/Scripts/Core/States/LobbySlidesLoadedState.cs` — updated HandleInput signature
- `Assets/_Project/Scripts/Core/States/RehearsalState.cs` — updated HandleInput signature
- `Assets/_Project/Scripts/Core/States/ReinforcementState.cs` — updated HandleInput signature
- `Assets/_Project/Scripts/Core/States/PausedState.cs` — updated HandleInput signature
- `Assets/_Project/StageMind.asmdef` — added Unity.XR.OpenXR reference; moved from Scripts/ to _Project/ to cover InputActions/ scope
- `Assets/Tests/EditMode/LobbyLandingStateTests.cs` — updated HandleInput call
- `Assets/Tests/EditMode/LobbySlidesLoadedStateTests.cs` — updated HandleInput call
- `Assets/Tests/EditMode/RehearsalStateTests.cs` — updated HandleInput call
- `Assets/Tests/EditMode/ReinforcementStateTests.cs` — updated HandleInput call
- `Assets/Tests/EditMode/PausedStateTests.cs` — updated HandleInput call
- `Assets/Tests/EditMode/GameStateManagerTests.cs` — updated TestState.HandleInput signature
