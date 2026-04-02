# Story 1.2: Core State Machine & Game State Manager

Status: review

<!-- Note: Validation is optional. Run validate-create-story for quality check before dev-story. -->

## Story

As a developer,
I want a working state machine that manages app states and notifies registered systems of state changes,
So that all StageMind systems can coordinate behavior through a single, testable backbone.

## Acceptance Criteria

1. **Given** the project from Story 1.1 is set up
   **When** the core interfaces are implemented
   **Then** `IGameState` exists with `Enter()`, `Exit()`, `Update()`, and `HandleInput()` methods
   **And** `IStateAware` exists with `OnStateEnter(GameStateType)` and `OnStateExit(GameStateType)` methods
   **And** `GameStateType` enum defines: `LobbyLanding`, `LobbySlidesLoaded`, `Rehearsal`, `Reinforcement`, `Paused`

2. **Given** the interfaces exist
   **When** `GameStateManager` MonoBehaviour is implemented
   **Then** it holds a reference to the current `IGameState`, delegates `Update()` calls to the current state, and dispatches `OnStateChanged(GameStateType previous, GameStateType current)` events to all registered `IStateAware` systems

3. **Given** `GameStateManager` exists
   **When** all 5 state classes are implemented as pure C# (not MonoBehaviour)
   **Then** `LobbyLandingState`, `LobbySlidesLoadedState`, `RehearsalState`, `ReinforcementState`, and `PausedState` each implement `IGameState` with `Enter()` and `Exit()` methods that log their activation
   **And** state classes receive dependencies via constructor parameters (not `FindObjectOfType` or singletons)

4. **Given** the state classes exist
   **When** `TransitionTo()` is called on `GameStateManager`
   **Then** the current state's `Exit()` is called, the new state's `Enter()` is called, and `OnStateChanged` fires with both previous and current state types
   **And** the transition map matches the Architecture spec:
   - `LobbyLanding` → `LobbySlidesLoaded`
   - `LobbySlidesLoaded` → `Rehearsal`
   - `Rehearsal` → `Paused`
   - `Paused` → `Rehearsal`
   - `Paused` → `Reinforcement`
   - `Paused` → `LobbyLanding`
   - `Reinforcement` → `Rehearsal`
   - `Reinforcement` → `LobbyLanding`
   - `LobbySlidesLoaded` → `LobbyLanding`

5. **Given** the state machine is complete
   **When** Edit Mode tests are run
   **Then** `GameStateManagerTests` verify: correct initialization to `LobbyLanding`, valid transitions succeed with correct event dispatch, invalid transitions are rejected, all 5 state classes have tests for `Enter()` and `Exit()` behavior
   **And** `MockStateAware` is implemented in `Tests/EditMode/Mocks/` for verifying event dispatch

## Tasks / Subtasks

- [x] Task 1: Create core interfaces and enum (AC: #1)
  - [x] 1.1 Create `IGameState.cs` in `Scripts/Core/` with `Enter()`, `Exit()`, `Update()`, `HandleInput()` methods
  - [x] 1.2 Create `IStateAware.cs` in `Scripts/Core/` with `OnStateEnter(GameStateType)`, `OnStateExit(GameStateType)` methods
  - [x] 1.3 Create `GameStateType.cs` in `Scripts/Core/` with enum values: `LobbyLanding`, `LobbySlidesLoaded`, `Rehearsal`, `Reinforcement`, `Paused`
  - [x] 1.4 Verify all three files compile without errors in Unity Editor
- [x] Task 2: Implement GameStateManager (AC: #2, #4)
  - [x] 2.1 Create `GameStateManager.cs` in `Scripts/Core/` as a MonoBehaviour
  - [x] 2.2 Implement `_currentState` (private `IGameState` field), `CurrentStateType` (public `GameStateType` property)
  - [x] 2.3 Implement `OnStateChanged` event as `event Action<GameStateType, GameStateType>`
  - [x] 2.4 Implement `RegisterStateAware(IStateAware)` and `UnregisterStateAware(IStateAware)` methods using a `List<IStateAware>`
  - [x] 2.5 Implement `TransitionTo(GameStateType)` method: validate transition against allowed map, call `Exit()` on current, set new state, call `Enter()` on new, dispatch `OnStateChanged` to all registered `IStateAware` and fire the event
  - [x] 2.6 Implement allowed transition validation using a `HashSet<(GameStateType, GameStateType)>` — reject invalid transitions with `Debug.LogWarning`
  - [x] 2.7 Implement `Update()` lifecycle that delegates to `_currentState.Update()`
  - [x] 2.8 Implement `Initialize()` called from `Awake()` that creates initial `LobbyLandingState` and calls `Enter()`
- [x] Task 3: Implement 5 state classes (AC: #3)
  - [x] 3.1 Create `LobbyLandingState.cs` in `Scripts/Core/States/` — pure C# implementing `IGameState`, constructor receives `GameStateManager` reference, `Enter()` and `Exit()` log activation via `Debug.Log`
  - [x] 3.2 Create `LobbySlidesLoadedState.cs` in `Scripts/Core/States/` — same pattern
  - [x] 3.3 Create `RehearsalState.cs` in `Scripts/Core/States/` — same pattern
  - [x] 3.4 Create `ReinforcementState.cs` in `Scripts/Core/States/` — same pattern
  - [x] 3.5 Create `PausedState.cs` in `Scripts/Core/States/` — same pattern, stores previous `GameStateType` for "Resume" transition
  - [x] 3.6 Each state class: `HandleInput()` is a no-op stub for now (input routing added in Story 1.4), `Update()` is empty (state-specific update logic added in later epics)
- [x] Task 4: Create test mocks (AC: #5)
  - [x] 4.1 Create `MockStateAware.cs` in `Tests/EditMode/Mocks/` implementing `IStateAware` — records all calls to `OnStateEnter` and `OnStateExit` with state types for assertion
- [x] Task 5: Write Edit Mode tests (AC: #5)
  - [x] 5.1 Create `GameStateManagerTests.cs` in `Tests/EditMode/` — test initialization to `LobbyLanding`, valid transitions fire events correctly, invalid transitions are rejected and logged, multiple `IStateAware` listeners receive events
  - [x] 5.2 Create `LobbyLandingStateTests.cs` — test `Enter()` and `Exit()` behavior
  - [x] 5.3 Create `LobbySlidesLoadedStateTests.cs` — test `Enter()` and `Exit()` behavior
  - [x] 5.4 Create `RehearsalStateTests.cs` — test `Enter()` and `Exit()` behavior
  - [x] 5.5 Create `ReinforcementStateTests.cs` — test `Enter()` and `Exit()` behavior
  - [x] 5.6 Create `PausedStateTests.cs` — test `Enter()` and `Exit()` behavior, previous state tracking
  - [x] 5.7 Run all Edit Mode tests in Unity and verify 100% pass
- [x] Task 6: Verification pass (all ACs)
  - [x] 6.1 Verify all files are in correct directories per architecture spec
  - [x] 6.2 Verify no Unity Console errors or warnings
  - [x] 6.3 Verify all naming conventions followed (PascalCase classes, _camelCase private fields)
  - [x] 6.4 Verify one class per file, filename matches class name
  - [x] 6.5 Verify `StageMind` namespace used consistently across all new files

## Dev Notes

### Architecture Compliance

This story implements Architecture **Decision 1: Scene State Machine — State Pattern**. The `GameStateManager` is the composition root and backbone that every subsequent system plugs into. [Source: architecture.md#Decision-1-Scene-State-Machine-State-Pattern]

**Critical constraints:**
- **State classes are pure C#** — NOT MonoBehaviour. They receive dependencies via constructor parameters. This enables Edit Mode testing without booting a Unity scene.
- **GameStateManager IS a MonoBehaviour** — it lives in the scene, owns the current state, delegates `Update()`, and dispatches events. It is the composition root.
- **Hybrid dependency wiring** — constructor injection for pure C# state classes, `[SerializeField]` for MonoBehaviour inter-references. No singletons, no `FindObjectOfType`, no `SendMessage`.
- **No static events or global event buses** — all events are instance-scoped on `GameStateManager`.
- **One class per file** — filename must match class name exactly.
- **`[SerializeField] private`** for any Inspector-exposed fields — never public fields.
- **No `Update()` in system scripts** — only `GameStateManager` owns the update loop and delegates to the current state.

[Source: architecture.md#Implementation-Patterns-&-Consistency-Rules]

### File Locations (Exact Paths)

All paths relative to `StageMind/Assets/`:

| File | Path | Type |
|------|------|------|
| `IGameState.cs` | `_Project/Scripts/Core/` | Interface |
| `IStateAware.cs` | `_Project/Scripts/Core/` | Interface |
| `IWebViewController.cs` | `_Project/Scripts/Core/` | Interface (NOT in this story — Story 1.3) |
| `GameStateType.cs` | `_Project/Scripts/Core/` | Enum |
| `GameStateManager.cs` | `_Project/Scripts/Core/` | MonoBehaviour |
| `LobbyLandingState.cs` | `_Project/Scripts/Core/States/` | Pure C# |
| `LobbySlidesLoadedState.cs` | `_Project/Scripts/Core/States/` | Pure C# |
| `RehearsalState.cs` | `_Project/Scripts/Core/States/` | Pure C# |
| `ReinforcementState.cs` | `_Project/Scripts/Core/States/` | Pure C# |
| `PausedState.cs` | `_Project/Scripts/Core/States/` | Pure C# |
| `MockStateAware.cs` | `Tests/EditMode/Mocks/` | Test mock |
| `GameStateManagerTests.cs` | `Tests/EditMode/` | Edit Mode test |
| `LobbyLandingStateTests.cs` | `Tests/EditMode/` | Edit Mode test |
| `LobbySlidesLoadedStateTests.cs` | `Tests/EditMode/` | Edit Mode test |
| `RehearsalStateTests.cs` | `Tests/EditMode/` | Edit Mode test |
| `ReinforcementStateTests.cs` | `Tests/EditMode/` | Edit Mode test |
| `PausedStateTests.cs` | `Tests/EditMode/` | Edit Mode test |

**DO NOT create:**
- `IWebViewController.cs` — that is Story 1.3
- Any files in `Scripts/WebView/`, `Scripts/Input/`, `Scripts/UI/`, etc. — those are later stories
- Any ScriptableObject assets — those are Epic 2 stories
- Any `.meta` files manually — Unity generates these automatically

[Source: architecture.md#Complete-Project-Directory-Structure]

### Interface Specifications

**IGameState:**
```csharp
namespace StageMind
{
    public interface IGameState
    {
        void Enter();
        void Exit();
        void Update();
        void HandleInput();
    }
}
```

**IStateAware:**
```csharp
namespace StageMind
{
    public interface IStateAware
    {
        void OnStateEnter(GameStateType state);
        void OnStateExit(GameStateType state);
    }
}
```

**GameStateType:**
```csharp
namespace StageMind
{
    public enum GameStateType
    {
        LobbyLanding,
        LobbySlidesLoaded,
        Rehearsal,
        Reinforcement,
        Paused
    }
}
```

[Source: architecture.md#Decision-1-Scene-State-Machine-State-Pattern, architecture.md#Communication-Patterns]

### GameStateManager Implementation Guidance

```csharp
namespace StageMind
{
    public class GameStateManager : MonoBehaviour
    {
        // Event for external listeners (non-IStateAware)
        public event Action<GameStateType, GameStateType> OnStateChanged;

        public GameStateType CurrentStateType { get; private set; }

        private IGameState _currentState;
        private readonly List<IStateAware> _stateAwareListeners = new();

        // Allowed transitions — use HashSet for O(1) lookup, NOT Dictionary (avoids enum boxing allocation on mobile)
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
    }
}
```

**Key implementation details:**
- `TransitionTo(GameStateType target)` validates against `_allowedTransitions` before executing. If invalid, log a warning and return without transitioning.
- Store the `previousStateType` before transitioning for event dispatch.
- Call `_currentState.Exit()` → update `CurrentStateType` → create new state instance → `_currentState = newState` → `_currentState.Enter()` → notify all `IStateAware` listeners → fire `OnStateChanged` event.
- `Update()` MonoBehaviour lifecycle delegates to `_currentState?.Update()`.
- For state creation: use a factory method or switch on `GameStateType` to instantiate the correct concrete state class, passing `this` (the `GameStateManager`) as a constructor dependency. In this story, states only need the `GameStateManager` reference. Later stories will add more constructor dependencies as systems are built.

**Performance note on transition map:** Do NOT use `Dictionary<GameStateType, HashSet<GameStateType>>` — enum-key dictionaries cause 40 bytes allocation per lookup due to boxing on mobile platforms. The `HashSet<(GameStateType, GameStateType)>` tuple approach avoids this since tuples of value types are not boxed.

### State Class Implementation Pattern

Each state follows this pattern:

```csharp
namespace StageMind
{
    public class LobbyLandingState : IGameState
    {
        private readonly GameStateManager _stateManager;

        public LobbyLandingState(GameStateManager stateManager)
        {
            _stateManager = stateManager;
        }

        public void Enter()
        {
            Debug.Log("[StageMind] Entering LobbyLanding state");
        }

        public void Exit()
        {
            Debug.Log("[StageMind] Exiting LobbyLanding state");
        }

        public void Update() { }

        public void HandleInput() { }
    }
}
```

**PausedState special behavior:** Store the `GameStateType` of the state that was active before pausing. This is needed for "Resume" functionality (transition back to the paused-from state). Pass the previous state type via constructor:

```csharp
public class PausedState : IGameState
{
    private readonly GameStateManager _stateManager;
    private readonly GameStateType _previousStateType;

    public PausedState(GameStateManager stateManager, GameStateType previousStateType)
    {
        _stateManager = stateManager;
        _previousStateType = previousStateType;
    }

    public GameStateType PreviousStateType => _previousStateType;
    // ...
}
```

### Testing Guidance

**Test framework:** Unity Test Framework with NUnit. Edit Mode tests only (no scene required). Use `[Test]` attribute, NOT `[UnityTest]` — coroutines are not needed for pure C# state logic.

**Test naming convention:** `MethodName_Scenario_ExpectedResult()`

**GameStateManagerTests key test cases:**

| Test | Verifies |
|------|----------|
| `Initialize_Default_StartsInLobbyLanding` | Initial state is `LobbyLanding` |
| `TransitionTo_ValidTransition_ChangesState` | e.g., `LobbyLanding` → `LobbySlidesLoaded` succeeds |
| `TransitionTo_ValidTransition_CallsExitOnPreviousState` | `Exit()` called on old state |
| `TransitionTo_ValidTransition_CallsEnterOnNewState` | `Enter()` called on new state |
| `TransitionTo_ValidTransition_FiresOnStateChanged` | Event dispatched with correct previous and current types |
| `TransitionTo_ValidTransition_NotifiesStateAwareListeners` | All registered `IStateAware` receive `OnStateExit` then `OnStateEnter` |
| `TransitionTo_InvalidTransition_DoesNotChangeState` | e.g., `LobbyLanding` → `Rehearsal` is rejected |
| `TransitionTo_InvalidTransition_DoesNotFireEvent` | No event on invalid transition |
| `RegisterStateAware_AddsListener` | Listener receives subsequent events |
| `UnregisterStateAware_RemovesListener` | Listener stops receiving events |
| `TransitionTo_AllValidPaths_Succeeds` | Iterate all 9 valid transitions and verify each succeeds |

**Testing GameStateManager without MonoBehaviour:** Since `GameStateManager` is a MonoBehaviour, you cannot use `new GameStateManager()` in Edit Mode tests. Two approaches:

1. **Preferred:** Extract the pure state machine logic into a separate non-MonoBehaviour class (e.g., `StateMachine`) that `GameStateManager` wraps. Test `StateMachine` directly. `GameStateManager` becomes a thin MonoBehaviour shell.
2. **Alternative:** Use `new GameObject().AddComponent<GameStateManager>()` in tests, but clean up the `GameObject` in `[TearDown]`.

**Approach 1 is strongly recommended** — it keeps the state machine logic fully testable without any Unity lifecycle dependencies, which is the architectural intent ("state machine logic must be pure C# separable from MonoBehaviour").

**MockStateAware implementation:**

```csharp
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
```

**Individual state tests** — each state class test file verifies:
- `Enter()` executes without exceptions
- `Exit()` executes without exceptions
- `Update()` executes without exceptions (no-op for now)
- `HandleInput()` executes without exceptions (no-op for now)
- Constructor correctly stores references

[Source: architecture.md#Testing-Conventions, architecture.md#Decision-1-Scene-State-Machine-State-Pattern]

### Assembly Definition References

Runtime code (`StageMind.asmdef`) already references `Unity.TextMeshPro` and `Unity.InputSystem`. No additional assembly references needed for this story.

Test code (`StageMind.Tests.EditMode.asmdef`) already references `StageMind`, `UnityEngine.TestRunner`, `UnityEditor.TestRunner`, and `nunit.framework.dll`. No changes needed.

[Source: Story 1.1 completion notes]

### Naming Conventions (Enforced)

| Element | Convention | Example |
|---------|-----------|---------|
| Namespace | `StageMind` (runtime), `StageMind.Tests.EditMode.Mocks` (test mocks) | `namespace StageMind { }` |
| Classes/interfaces | PascalCase | `GameStateManager`, `IGameState` |
| Interface prefix | `I` | `IGameState`, `IStateAware` |
| Public methods | PascalCase | `Enter()`, `TransitionTo()` |
| Private fields | `_camelCase` | `_currentState`, `_stateAwareListeners` |
| Public properties | PascalCase | `CurrentStateType` |
| Enum type + values | PascalCase | `GameStateType.LobbyLanding` |
| Events | `On` + PascalCase | `OnStateChanged` |
| Test methods | `Method_Scenario_Expected` | `TransitionTo_ValidTransition_ChangesState` |

[Source: architecture.md#Naming-Conventions]

### Anti-Patterns to Avoid

Do NOT:
- Make state classes inherit from `MonoBehaviour` — they must be pure C#
- Use `public static GameStateManager Instance` singleton pattern
- Use `FindObjectOfType<>()` to locate systems
- Use `SendMessage()` or `BroadcastMessage()`
- Use public fields for Inspector exposure — always `[SerializeField] private`
- Create `static` events or a global event bus
- Put `Update()` in any script besides `GameStateManager`
- Use `Dictionary<enum, ...>` for transition lookup — causes boxing allocation on mobile
- Create interfaces in `Scripts/WebView/` or other system folders — all interfaces go in `Scripts/Core/`
- Create `IWebViewController` — that is Story 1.3's responsibility

[Source: architecture.md#Anti-Patterns, architecture.md#Enforcement-Guidelines]

### Previous Story Intelligence

**Story 1.1 learnings:**
- Unity 6.4 used (not 6.3) — fully compatible, no impact on state machine implementation
- Project lives inside the BMAD git repository at `StageMind/` subdirectory
- All directory structure already exists: `Scripts/Core/`, `Scripts/Core/States/`, `Tests/EditMode/`, `Tests/EditMode/Mocks/`
- Assembly definitions are already configured and verified working
- StageMind.asmdef `rootNamespace` is set to `StageMind` — use this namespace consistently
- Tests asmdef `rootNamespace` is `StageMind.Tests.EditMode` — use this for test files
- `.gitkeep` files exist in empty directories — remove them when adding real files to those directories (or leave them; Unity ignores them)
- Code review found: always verify Unity Console has no errors after changes, verify build settings aren't broken by new code

### Project Structure Notes

- All new scripts go under `StageMind/Assets/_Project/Scripts/Core/` and `StageMind/Assets/_Project/Scripts/Core/States/`
- Tests go under `StageMind/Assets/Tests/EditMode/` and `StageMind/Assets/Tests/EditMode/Mocks/`
- The `_Project/` prefix is intentional — keeps StageMind code separate from plugin/Unity-generated content
- `Assets/Tests/` sits alongside `_Project/`, not inside it — Unity Test Framework convention

[Source: architecture.md#Complete-Project-Directory-Structure, Story 1.1 completion notes]

### References

- [Source: architecture.md#Decision-1-Scene-State-Machine-State-Pattern] — State pattern rationale, implementation approach, transition map
- [Source: architecture.md#Communication-Patterns] — `IStateAware` event dispatch, forbidden patterns
- [Source: architecture.md#Implementation-Patterns-&-Consistency-Rules] — Naming, organization, lifecycle rules, anti-patterns
- [Source: architecture.md#Complete-Project-Directory-Structure] — Exact file locations for all scripts
- [Source: architecture.md#Testing-Conventions] — Edit Mode test approach, naming, mock strategy
- [Source: architecture.md#Decision-4-Dependency-Wiring-Hybrid-Approach] — Constructor injection for state classes
- [Source: epics.md#Story-1.2] — Story requirements and acceptance criteria
- [Source: Story 1.1 completion notes] — Unity version, assembly definitions, directory structure confirmation

## Dev Agent Record

### Agent Model Used

Claude claude-4.6-opus (Cursor)

### Debug Log References

None — no blocking issues encountered during implementation.

### Completion Notes List

- Implemented core interfaces (`IGameState`, `IStateAware`) and `GameStateType` enum exactly per architecture spec
- Followed Dev Notes Approach 1 (strongly recommended): extracted pure state machine logic into `StateMachine.cs` (non-MonoBehaviour), keeping `GameStateManager.cs` as a thin MonoBehaviour shell. This enables full Edit Mode testing without Unity lifecycle dependencies
- `StateMachine` uses `Func<GameStateType, GameStateType, IGameState>` factory delegate for state creation — the second parameter passes the previous state type, which `PausedState` uses to track the state that was active before pausing
- All 5 state classes are pure C# with constructor injection per architecture spec; `HandleInput()` and `Update()` are no-op stubs as specified
- Transition map uses `HashSet<(GameStateType, GameStateType)>` tuple approach to avoid enum boxing allocation on mobile platforms (40 bytes per lookup with Dictionary)
- `GameStateManagerTests` includes 12 tests covering: initialization, valid/invalid transitions, event dispatch, listener registration/unregistration, all 9 valid transition paths
- Individual state tests verify Enter/Exit/Update/HandleInput execute without exceptions, interface compliance, and PausedState previous state tracking
- `MockStateAware` records all state enter/exit calls for assertion in tests
- All naming conventions followed: PascalCase classes, _camelCase private fields, `On` prefix for events, `Method_Scenario_Expected` test naming

### Change Log

- 2026-04-01: Implemented Story 1.2 — Core State Machine & Game State Manager (all 6 tasks, all 5 ACs)

### File List

**New files (paths relative to `StageMind/Assets/`):**

| File | Path | Type |
|------|------|------|
| `GameStateType.cs` | `_Project/Scripts/Core/` | Enum |
| `IGameState.cs` | `_Project/Scripts/Core/` | Interface |
| `IStateAware.cs` | `_Project/Scripts/Core/` | Interface |
| `StateMachine.cs` | `_Project/Scripts/Core/` | Pure C# (extracted state machine logic) |
| `GameStateManager.cs` | `_Project/Scripts/Core/` | MonoBehaviour (thin shell) |
| `LobbyLandingState.cs` | `_Project/Scripts/Core/States/` | Pure C# |
| `LobbySlidesLoadedState.cs` | `_Project/Scripts/Core/States/` | Pure C# |
| `RehearsalState.cs` | `_Project/Scripts/Core/States/` | Pure C# |
| `ReinforcementState.cs` | `_Project/Scripts/Core/States/` | Pure C# |
| `PausedState.cs` | `_Project/Scripts/Core/States/` | Pure C# |
| `MockStateAware.cs` | `Tests/EditMode/Mocks/` | Test mock |
| `GameStateManagerTests.cs` | `Tests/EditMode/` | Edit Mode test |
| `LobbyLandingStateTests.cs` | `Tests/EditMode/` | Edit Mode test |
| `LobbySlidesLoadedStateTests.cs` | `Tests/EditMode/` | Edit Mode test |
| `RehearsalStateTests.cs` | `Tests/EditMode/` | Edit Mode test |
| `ReinforcementStateTests.cs` | `Tests/EditMode/` | Edit Mode test |
| `PausedStateTests.cs` | `Tests/EditMode/` | Edit Mode test |
