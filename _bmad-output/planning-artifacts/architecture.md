---
stepsCompleted: [1, 2, 3, 4, 5, 6, 7, 8]
inputDocuments:
  - planning-artifacts/product-brief-BMAD-2026-03-27.md
  - planning-artifacts/prd.md
  - planning-artifacts/prd-validation-report.md
  - planning-artifacts/ux-design-specification.md
workflowType: 'architecture'
lastStep: 8
status: 'complete'
completedAt: '2026-03-31'
project_name: 'StageMind'
user_name: 'Dominik'
date: '2026-03-31'
---

# Architecture Decision Document — StageMind

_This document builds collaboratively through step-by-step discovery. Sections are appended as we work through each architectural decision together._

## Project Context Analysis

### Requirements Overview

**Functional Requirements:**

23 FRs across 6 capability areas. The architectural weight is concentrated in three areas:

1. **Lobby & Onboarding (FR1-FR7):** Browser integration, URL input, slide preview, QR code scanning. The WebView is the architectural centerpiece — it must render on a world-space surface, accept controller input for navigation, and support programmatic keyboard event dispatch for slide advancement.

2. **Stage & Spatial Experience (FR8-FR12):** Single conference room environment with 50 static audience members. Shared `RenderTexture` between projector screen and laptop confidence monitor. Scene transition is a state change (audience appears), not a scene load.

3. **Privacy & Multi-User (FR21-FR23):** Zero persistence architecture. No accounts, no cached state, no browser history. Every session starts clean. This is a constraint that simplifies the architecture (no data layer) but must be enforced at the WebView level (clear cookies/cache on exit).

The remaining areas (Rehearsal Control FR13-FR15, Post-Session FR16-FR17, Error Handling FR18-FR20) are relatively lightweight architecturally — they involve input mapping, UI state transitions, and WebView error callbacks.

**Non-Functional Requirements:**

20 NFRs. The architecturally dominant ones:

| NFR | Constraint | Architectural Impact |
|-----|-----------|---------------------|
| NFR1 | 72fps sustained with WebView active | Drives rendering pipeline decisions, character LOD strategy, lighting approach (baked vs. real-time) |
| NFR2 | < 200ms slide advance latency | WebView keyboard event dispatch must be near-instant |
| NFR6 | No thermal throttling in 30-min sessions | Constrains total GPU/CPU budget; affects character count, texture resolution, WebView update frequency |
| NFR7 | Zero crashes in 30-min sessions with 60 slides | WebView stability is the primary risk; requires crash detection and recovery |
| NFR9 | Memory stable across 5 consecutive sessions | WebView memory management is the primary concern; requires lifecycle management |
| NFR14 | Reliable rendering of Google Slides, Canva, PowerPoint Online | WebView must handle modern web content without layout corruption |
| NFR18 | No state persists after app close | WebView cache/cookies/history must be cleared on exit |

**Scale & Complexity:**

- Primary domain: VR Application (Unity/C#/OpenXR on Quest 3)
- Complexity level: Low-to-medium — concentrated technical risk (WebView + mobile GPU performance) in a simple product structure
- Estimated architectural components: ~8-10 major systems (Scene State Manager, WebView Controller, Input System, UI Framework, Audio Manager, Environment/Lighting, Audience System, Reinforcement System, Platform Integration, QR Scanner)

### Technical Constraints & Dependencies

| Constraint | Source | Impact |
|-----------|--------|--------|
| Quest 3 hardware (Snapdragon XR2 Gen 2) | Platform | Mobile GPU/CPU thermal budget governs all rendering decisions |
| Unity + OpenXR | PRD | Engine and runtime locked; architecture must use Unity patterns (MonoBehaviour, ScriptableObjects, Prefabs) |
| WebView plugin (TBD by spike) | PRD/UX | The chosen WebView solution (Vuplex, Android WebView bridge, or CEF) will dictate the browser integration architecture |
| No backend / no network services | PRD | Standalone app; only network dependency is WebView loading external URLs |
| Quest Store distribution | PRD | APK size < 2GB; must pass Meta's curation review; WebView usage must be justified |
| Stateless design | PRD | No local storage, no PlayerPrefs, no persistent data of any kind |
| Single-developer resource model | PRD | Architecture must be simple enough for one person to build and maintain |
| Meta SDK dependencies | UX | OVRManager for headset events, Meta audio spatializer for HRTF, TouchScreenKeyboard for text input, passthrough API for QR scanning |
| Quest system keyboard reliability | Platform | `TouchScreenKeyboard` API has known reliability issues across Quest OS versions — input focus management between keyboard and WebView is a dependency risk |
| APK asset budget | Distribution | Character meshes, texture atlases, audio clips, environment geometry, and baked lightmaps must fit within the < 2GB package size constraint alongside the WebView plugin |

### Cross-Cutting Concerns Identified

1. **Scene State Machine & Transition Orchestration** — The app has 5 distinct states (Lobby/Landing, Lobby/SlidesLoaded, Rehearsal, Reinforcement, Paused). Each transition is a coordinated multi-system event: audience visibility, lighting state, audio state, UI visibility, and input routing all change simultaneously. A centralized state orchestrator that sequences these transitions is required — independent systems reacting to a shared enum risks race conditions and partially-transitioned states. The state machine logic must be pure C# separable from MonoBehaviour to enable Edit Mode testing without booting the full app.

2. **Slide Delivery as Swappable Subsystem** — The slide delivery mechanism is isolated behind `IWebViewController`, making it swappable without affecting the rest of the app. The MVP implementation (`SlideImageController`) fetches slide images via HTTP and manages a texture array — dramatically simpler than a full WebView runtime. The architecture preserves the interface contract (initialize, load URL, dispatch key events for navigation, handle errors, cleanup) so a post-MVP browser-based implementation can be swapped in without touching other systems. This abstraction proved its value when the original Vuplex plan was replaced with image fetching.

3. **Performance Budget Management** — Affects every system. The 72fps target on mobile VR is the most pervasive constraint. Character rendering, WebView update frequency, UI rendering, audio processing, and post-processing effects all compete for the same GPU/CPU budget. Frame time profiling is a critical quality gate on every build.

4. **Input Context Switching** — Controller input meaning changes across states: trigger advances slides during rehearsal, activates buttons in lobby, is disabled during reinforcement. A context-aware input routing system managed by the state machine is needed.

5. **Platform Integration Surface** — Multiple Meta/Quest APIs are used across different features: OVRManager (headset events), TouchScreenKeyboard (text input), passthrough cameras (QR scanning), audio spatializer (HRTF), system menu button interception. These touch different systems but share platform dependency management.

6. **Testability** — VR spatial presence cannot be unit-tested, but state transitions, WebView integration, memory stability, and performance budgets can. The state machine must be testable in isolation (pure C# separable from Unity lifecycle). WebView integration tests run in a harness. Memory stability is verified by measuring delta across 5 consecutive lobby→stage cycles. Performance profiling serves as a manual or automated gate.

## Starter Template & Technology Foundation

### Primary Technology Domain

**Unity VR Application (C# / OpenXR)** targeting Meta Quest 3 as a standalone Android APK.

Unlike web or mobile projects, Unity VR has no CLI scaffolding tools. The "starter" decision is which Unity project template, render pipeline, SDK packages, and project structure conventions to adopt.

### Starter Options Considered

| Option | Description | Assessment |
|--------|------------|------------|
| **Unity VR Template** | Pre-configured with XR Interaction Toolkit, locomotion, teleportation, grab systems, sample scene | **Rejected** — includes substantial interaction infrastructure StageMind doesn't need. Adds complexity and learning burden for a newcomer. Teleportation, grab interactors, and socket systems are all anti-patterns for this project. |
| **Universal 3D Template + Manual Quest Setup** | Clean URP project with no VR pre-configuration. Add only the packages needed. | **Selected** — gives full control, avoids unnecessary dependencies, and forces understanding of each component added. Best for learning and for a project with an intentionally minimal interaction model. |
| **Community VR Templates** (e.g., Fist Full of Shrimp) | Pre-built boilerplate with hand models, teleportation, organized folders | **Rejected** — designed for general-purpose VR with interaction patterns StageMind explicitly avoids. Would need to strip more than it provides. |

### Selected Starter: Universal 3D Template (Unity 6.3 LTS + URP)

**Rationale:**

1. **Minimal complexity** — a clean foundation that includes only what's needed. Critical for a first-time Unity developer.
2. **URP included** — Universal Render Pipeline is required for Quest 3 mobile GPU performance. Built-in RP is too heavyweight; HDRP is not supported on mobile.
3. **Full control** — each package added is a deliberate, understood choice. No "what does this component do and can I delete it?" confusion.
4. **Best-documented path** — Meta's official setup documentation starts from the Universal 3D template.

### Initialization Approach

**Step 1 — Unity Hub Setup:**

```
Install Unity Hub (Mac)
Install Unity 6.3 LTS with:
  - Android Build Support
  - OpenJDK
  - Android SDK & NDK Tools
```

**Step 2 — Project Creation:**

```
Unity Hub → New Project → Universal 3D (URP)
Project name: StageMind
```

**Step 3 — Platform Configuration:**

```
File → Build Profiles → Add Meta Quest profile → Switch Platform
Player Settings:
  - Company Name / Product Name
  - Minimum API Level: 29 (Android 10)
  - Scripting Backend: IL2CPP
  - Target Architectures: ARM64
  - Graphics API: Vulkan (primary)
```

**Step 4 — Package Installation:**

| Package | Source | Purpose |
|---------|--------|---------|
| `com.unity.xr.openxr` (≥1.15.1) | Unity Registry | Core OpenXR runtime — VR rendering, tracking, input |
| `com.unity.xr.meta-openxr` (≥2.2) | Unity Registry | Quest-specific OpenXR extensions — passthrough, system keyboard, headset events |
| Meta XR Simulator | Unity Asset Store (free) | Development without physical headset — simulates Quest 3 on Mac |
| TextMeshPro | Unity Registry (included) | SDF text rendering — non-negotiable for VR text legibility |
| ~~Vuplex 3D WebView for Android~~ | ~~Asset Store (commercial)~~ | *Removed for MVP — replaced by `SlideImageController` using `UnityWebRequest` for slide image fetching. Post-MVP: [SimpleUnity3DWebView](https://github.com/t-34400/SimpleUnity3DWebView) (GitHub, MIT license, free) identified as potential browser upgrade.* |

**Packages explicitly NOT included:**

| Package | Reason for Exclusion |
|---------|---------------------|
| XR Interaction Toolkit | Designed for complex VR interactions (grab, teleport, socket). StageMind uses only point-and-click — a custom lightweight raycaster is simpler and more appropriate. |
| Meta XR Core SDK (OVRPlugin) | Legacy path. Unity OpenXR Meta package is the recommended approach for new projects. |
| Meta XR Interaction SDK | Designed for hand tracking, grab interactions, poke interactions. None of these apply to StageMind's three-button controller model. |
| Meta XR Audio SDK | StageMind uses Unity's built-in spatial audio with HRTF. Meta's audio SDK adds features not needed for ambient room tone and occasional audience sounds. |

### Architectural Decisions Provided by Starter

**Language & Runtime:**

- C# (Unity's scripting language — no alternative)
- .NET Standard 2.1 API compatibility level
- IL2CPP ahead-of-time compilation for Quest (required for ARM64 Android)

**Render Pipeline:**

- Universal Render Pipeline (URP) with Vulkan graphics API
- URP Renderer configured for mobile VR: HDR disabled, post-processing selective (only for reinforcement color temperature shift), Vulkan primary API
- Baked lighting (lightmaps) for environment warmth at zero runtime cost

**Testing Framework:**

- Unity Test Framework (included) — supports Edit Mode tests (pure C# logic, no scene) and Play Mode tests (scene-dependent behavior)
- Edit Mode tests for state machine, input routing logic, and WebView interface contract
- Play Mode tests for scene transitions, UI composition, and integration

**Code Organization:**

```
Assets/
├── _Project/
│   ├── Scripts/
│   │   ├── Core/           # State machine, event system, interfaces
│   │   ├── WebView/        # WebView controller, error handling, lifecycle
│   │   ├── Input/          # Input router, controller mapping, context switching
│   │   ├── UI/             # VRButtonInteraction, UIPointerController, UIAnimator
│   │   ├── Audio/          # AudioManager, spatial audio, ambient system
│   │   ├── Environment/    # AudienceController, LightingController, scene setup
│   │   ├── Reinforcement/  # Post-session sequence, message pool, visual effects
│   │   └── Platform/       # Quest integration, keyboard, QR scanner, headset events
│   ├── Prefabs/
│   │   ├── UI/             # 8 atomic prefabs (TextBlock, PrimaryButton, etc.)
│   │   ├── Environment/    # Podium, chairs, audience characters, projector screen
│   │   └── Audio/          # Audio source prefabs with spatial settings
│   ├── ScriptableObjects/
│   │   ├── ColorPalette.asset
│   │   ├── ReinforcementMessages.asset
│   │   └── AppConfig.asset  # Runtime configuration (audience count, timing, etc.)
│   ├── Materials/           # URP materials for environment
│   ├── Textures/            # Texture atlases, lightmaps
│   ├── Audio/               # Room tone, audience ambients, reinforcement tone
│   ├── Fonts/               # Inter font SDF assets
│   ├── Scenes/
│   │   └── StageMind.unity  # Single scene — lobby and stage are states, not scenes
│   └── Art/                 # 3D models (audience characters, furniture, room)
├── Tests/
│   ├── EditMode/            # Pure C# tests — state machine, input routing
│   └── PlayMode/            # Scene tests — transitions, UI, integration
└── Plugins/
    └── Vuplex/              # WebView plugin (installed via Asset Store)
```

**Development Experience:**

- Meta XR Simulator for headset-free development on Mac
- Unity Editor Play Mode for scene testing (non-VR preview)
- Unity Profiler for frame time analysis
- Android Logcat for runtime debugging on device (when Quest 3 is available)

### Development Environment Notes

**Mac Apple Silicon Considerations:**

- Unity 6.3 LTS has reported issues with Android builds on Apple Silicon — monitor for fixes in patch releases
- Meta XR Simulator v85.0 is available natively for Mac ARM
- If Android build issues persist, a Windows VM or CI build machine may be needed for final APK builds

**Quest 3 Hardware Requirement:**

The Meta XR Simulator enables most development work without a headset, but the following require physical hardware:

- WebView performance profiling under real GPU constraints
- Thermal throttling testing (NFR6)
- VR comfort validation (spatial scale, text legibility, button hit targets)
- Final Quest Store submission testing
- QR code scanning via passthrough cameras

Acquiring a Quest 3 before the Phase 0 WebView spike is strongly recommended — the spike's acceptance criteria require testing on actual Quest 3 hardware.

**Note:** Project initialization using this setup should be the first implementation story.

## Core Architectural Decisions

### Decision Priority Analysis

**Critical Decisions (Block Implementation):**

1. Scene State Machine Pattern — State Pattern with C# classes
2. Input System — Unity Input System (action-based)
3. WebView Interface Contract — Abstracted behind `IWebViewController`

**Important Decisions (Shape Architecture):**

4. Dependency Wiring — Hybrid manual injection + Inspector references
5. Audience Character Management — Pre-placed, toggle visibility
6. Post-Processing Strategy — URP Volume with Color Adjustments
7. Error Handling Strategy — Centralized ErrorHandler routing to laptop UI
8. Configuration Management — ScriptableObject assets

**Deferred Decisions (Post-MVP):**

- Audience animation system (idle breathing, head shifts) — no animation in MVP
- Audio middleware selection — Unity built-in spatial audio sufficient for MVP ambient sounds
- Analytics/telemetry framework — no data collection in MVP (NFR17)
- Cross-platform abstraction layer — Quest 3 only for MVP; OpenXR foundation handles future portability

### Decision 1: Scene State Machine — State Pattern

| Attribute | Value |
|-----------|-------|
| **Decision** | State Pattern with C# classes |
| **Rationale** | Clean separation of state logic, fully testable in Edit Mode without Unity scene, maps to standard CS patterns, each state encapsulates its own transition logic |
| **Affects** | Every system in the app — UI, input, audio, lighting, audience, WebView |

**Implementation:**

- `IGameState` interface with `Enter()`, `Exit()`, `Update()` methods
- Concrete state classes: `LobbyLandingState`, `LobbySlidesLoadedState`, `RehearsalState`, `ReinforcementState`, `PausedState`
- `GameStateManager` (MonoBehaviour) holds current state, delegates lifecycle calls, and dispatches `OnStateChanged` events to registered `IStateAware` systems
- State classes are pure C# — no MonoBehaviour inheritance. They receive dependencies via constructor injection (WebView controller, input router, UI manager references)
- Transition sequencing handled within `Enter()`/`Exit()` methods — each state knows what to activate/deactivate, ensuring coordinated multi-system transitions

**State Transition Map:**

```
LobbyLanding → LobbySlidesLoaded    (URL loads successfully)
LobbySlidesLoaded → Rehearsal        ("Start Rehearsal" pressed)
Rehearsal → Paused                   (Menu button pressed)
Paused → Rehearsal                   ("Resume" selected)
Paused → Reinforcement              ("End Session" selected)
Paused → LobbyLanding               ("Return to Lobby" selected)
Reinforcement → Rehearsal            ("Go Again" selected)
Reinforcement → LobbyLanding        ("Done for Today" selected)
LobbySlidesLoaded → LobbyLanding    (new URL entered / error recovery)
```

### Decision 2: Input System — Unity Input System (Action-Based)

| Attribute | Value |
|-----------|-------|
| **Decision** | Unity Input System (new) with Input Action Assets |
| **Rationale** | Modern standard, native OpenXR integration, action-based model maps cleanly to StageMind's three-button design, supports future remapping |
| **Affects** | Input routing, controller mapping, state-dependent input behavior |

**Implementation:**

- Input Action Asset defines three actions: `AdvanceSlide` (right trigger), `PreviousSlide` (right B), `PauseMenu` (left menu button)
- `InputRouter` (MonoBehaviour) subscribes to Input Action callbacks and routes them through the `GameStateManager` — the current state determines which handler receives the action
- During Reinforcement state, `AdvanceSlide` and `PauseMenu` actions are disabled (controller inputs blocked per UX spec)
- Haptic feedback dispatched alongside input actions via OpenXR haptic API — subtle click on trigger/B/menu, no haptic on non-events (trigger on last slide, B on first slide)

### Decision 3: WebView Interface Contract

| Attribute | Value |
|-----------|-------|
| **Decision** | Abstract `IWebViewController` interface with swappable implementations |
| **Rationale** | Testability (mock in Edit Mode tests), swappability (change slide delivery mechanism without touching app code), clean integration boundary. MVP uses `SlideImageController` (image fetching). Post-MVP can swap in a browser-based implementation (e.g., SimpleUnity3DWebView). |
| **Affects** | WebView subsystem, error handling, slide navigation, RenderTexture management |

**Interface Contract:**

```csharp
public interface IWebViewController
{
    void Initialize(RenderTexture targetTexture);
    void LoadUrl(string url);
    void SendKeyEvent(KeyCode key);  // Arrow keys for slide navigation
    void Cleanup();                   // Clear cache/cookies, release resources

    event Action<string> OnLoadSuccess;    // URL loaded, page ready
    event Action<WebViewError> OnLoadError; // Load failure with error type
    event Action OnCrash;                   // WebView process crashed

    bool IsLoading { get; }
    bool IsReady { get; }
}

public enum WebViewError
{
    NetworkFailure,     // DNS, timeout, HTTP errors
    LoginWallDetected,  // Redirect to auth domain detected
    PageLoadTimeout,    // 15-second load timeout exceeded
    Unknown
}
```

**MVP Implementation — `SlideImageController`:**

- Implements `IWebViewController` by fetching slide images from Google Slides published URLs via `UnityWebRequest`
- `LoadUrl(string url)` parses the Google Slides presentation ID from the URL, fetches all slide images as `Texture2D` objects, and fires `OnLoadSuccess` when complete
- `SendKeyEvent(KeyCode.RightArrow)` increments the slide index and updates the `RenderTexture` with the next slide texture. `SendKeyEvent(KeyCode.LeftArrow)` decrements.
- `RenderTexture` shared between projector screen and laptop confidence monitor meshes — updated via `Graphics.Blit()` when slide index changes
- `Cleanup()` destroys cached `Texture2D` objects and resets slide index (NFR18)
- No crash recovery complexity — HTTP fetch failures are handled via `OnLoadError`
- `IsLoading` is true while slide images are being fetched; `IsReady` is true after all slides are loaded

**Post-MVP Browser Implementation Path:**

- The `IWebViewController` interface is designed to support a full browser implementation
- [SimpleUnity3DWebView](https://github.com/t-34400/SimpleUnity3DWebView) (MIT license, free) has been identified as a potential browser-based implementation
- A future `SimpleWebViewController` would wrap this library, restoring embedded browser capabilities (FR5, FR6) and multi-platform slide support (Canva, PowerPoint Online)
- The Vuplex `VuplexWebViewController` implementation remains in the codebase (behind `#if VUPLEX_WEBVIEW`) as an additional option if commercial licensing becomes acceptable

### Decision 4: Dependency Wiring — Hybrid Approach

| Attribute | Value |
|-----------|-------|
| **Decision** | Manual constructor injection for pure C# layer + `[SerializeField]` Inspector references for MonoBehaviours |
| **Rationale** | No DI framework overhead at this project scale, explicit dependencies for testability, idiomatic Unity pattern for scene wiring |
| **Affects** | All system initialization, testing approach |

**Implementation:**

- **Pure C# classes** (state classes, input routing logic, WebView interface, error types): receive dependencies via constructor parameters. Fully testable with mocks.
- **MonoBehaviours** (managers that live in the scene): wire to each other via `[SerializeField]` fields assigned in the Unity Inspector. The `GameStateManager` MonoBehaviour is the composition root — it holds references to all system MonoBehaviours and passes them into state constructors during initialization.
- **No static singletons.** No `GameManager.Instance` pattern. Every dependency is explicit.

### Decision 5: Audience Character Management — Pre-Placed Toggle

| Attribute | Value |
|-----------|-------|
| **Decision** | All 50 characters pre-placed in scene, toggled via `SetActive()` |
| **Rationale** | Instant visibility change (one frame — matches UX "immediate appearance" requirement), zero instantiation frame spike, disabled objects have zero render cost, simplest approach |
| **Affects** | Scene setup, audience controller, performance budget |

**Implementation:**

- 50 audience GameObjects pre-placed in scene hierarchy under an `AudienceRoot` parent
- `AudienceController` toggles `AudienceRoot.SetActive()` — single call controls all 50
- Characters use GPU instancing (shared mesh, shared material with atlas UV offsets for appearance variation)
- 2 LOD levels: full detail (rows 1-3, within 6m) and simplified (rows 4+)
- Baked ambient occlusion beneath chairs for grounding (no real-time shadows)
- Fade-out on session end: `AudienceController` lerps a shared material alpha over ~1s, then `SetActive(false)`

### Decision 6: Post-Processing — URP Volume

| Attribute | Value |
|-----------|-------|
| **Decision** | URP Post-Processing Volume with Color Adjustments for reinforcement lighting shift |
| **Rationale** | URP-native tool for exactly this use case, minimal GPU cost, tuneable in Editor, single parameter controls warmth |
| **Affects** | Reinforcement visual effect, URP configuration |

**Implementation:**

- Global URP Volume with `ColorAdjustments` override
- `ReinforcementController` lerps Volume weight from 0 → 1 over ~0.5s using `UIAnimator`
- Color filter set to warm amber (#E8A830) in the Volume profile
- Post-processing otherwise disabled in URP settings for performance (HDR off, no bloom, no ambient occlusion, no anti-aliasing post-process)
- The reinforcement warm shift is the only post-processing effect in the entire app

### Decision 7: Error Handling — Centralized ErrorHandler

| Attribute | Value |
|-----------|-------|
| **Decision** | Centralized `ErrorHandler` that receives errors from all systems and routes to laptop UI |
| **Rationale** | All errors display on the laptop screen as Card compositions (per UX spec), consistent warm tone, no silent failures |
| **Affects** | WebView errors, network errors, platform errors, UI error display |

**Implementation:**

- `ErrorHandler` receives `ErrorEvent` objects with type, user-facing message, and recovery action
- WebView reports errors through `IWebViewController` events → `ErrorHandler` maps to UX-spec error messages
- Network connectivity checked via `Application.internetReachability` before WebView load attempts
- All error messages follow "supportive friend" copy voice — no technical language visible to user
- Recovery always loops back to URL input — no dead ends
- WebView crash recovery: one retry, then "restart app" dead-end (per UX spec)

### Decision 8: Configuration — ScriptableObject Assets

| Attribute | Value |
|-----------|-------|
| **Decision** | ScriptableObject assets for all tunable configuration |
| **Rationale** | Single source of truth, editable in Inspector without code changes, Unity-idiomatic data container pattern |
| **Affects** | Visual consistency, timing, messages, spatial dimensions |

**Configuration Assets:**

| Asset | Contents |
|-------|----------|
| `ColorPalette` | 12 semantic colors from UX spec (Surface, TextPrimary, PrimaryAction, Error, etc.) |
| `ReinforcementMessages` | 15-message pool with random-without-replacement selection logic |
| `AppConfig` | Timing values (reinforcement linger: 5s, fade speeds, WebView timeout: 15s), audience count (50), spatial dimensions (stage 4m×3m, audience depth 8-10m, ceiling 3-4m), haptic intensity values |
| `AudioConfig` | Ambient volume levels, audience sound interval range (10-20s), reinforcement tone settings |

### Decision Impact Analysis

**Implementation Sequence:**

1. **GameStateManager + State classes** — the backbone everything else plugs into
2. **InputRouter + Input Action Asset** — enables controller interaction
3. **UIPointerController + VRButtonInteraction** — enables UI interaction
4. **WebView integration** (Phase 0 spike first, then full integration) — the critical path
5. **Environment setup** (room, podium, projector, audience) — the visual foundation
6. **AudienceController** — lobby/rehearsal state switching
7. **LightingController** — baked lighting + reinforcement post-processing
8. **AudioManager** — ambient room tone and audience sounds
9. **ReinforcementController** — post-session sequence
10. **ErrorHandler** — error routing and display
11. **Platform integration** — system keyboard, headset events, QR scanner

**Cross-Component Dependencies:**

```
GameStateManager ← (all systems register as IStateAware)
    ├── InputRouter ← Unity Input System
    ├── UIManager ← UIPointerController, VRButtonInteraction, UIAnimator
    ├── WebViewController ← IWebViewController (Vuplex wrapper)
    ├── AudienceController ← AudienceRoot toggle
    ├── LightingController ← URP Volume
    ├── AudioManager ← Spatial audio sources
    ├── ReinforcementController ← Messages SO, UIAnimator, LightingController
    ├── ErrorHandler ← WebViewController events, network status
    └── PlatformManager ← OVR events, keyboard, QR scanner
```

## Implementation Patterns & Consistency Rules

### Conflict Points Identified

6 areas where AI agents working on different stories could make incompatible choices:

1. C# and Unity naming conventions
2. Script and file organization
3. Inter-system communication patterns
4. MonoBehaviour lifecycle and Unity patterns
5. Error handling flow
6. Testing conventions

### 1. Naming Conventions

**C# Code (follows Microsoft C# conventions):**

| Element | Convention | Example |
|---------|-----------|---------|
| Classes, interfaces, enums | PascalCase | `GameStateManager`, `IGameState`, `WebViewError` |
| Interfaces | `I` prefix + PascalCase | `IGameState`, `IStateAware`, `IWebViewController` |
| Public methods | PascalCase | `Enter()`, `LoadUrl()`, `SendKeyEvent()` |
| Private methods | PascalCase (no underscore prefix) | `HandleTransition()`, `ValidateUrl()` |
| Public properties | PascalCase | `IsLoading`, `IsReady`, `CurrentState` |
| Private fields | `_camelCase` (underscore prefix) | `_currentState`, `_webViewController`, `_isInitialized` |
| `[SerializeField]` private fields | `_camelCase` (same as private fields) | `[SerializeField] private AudioManager _audioManager;` |
| Local variables | camelCase | `slideUrl`, `errorMessage`, `targetState` |
| Method parameters | camelCase | `void LoadUrl(string url)`, `void TransitionTo(IGameState nextState)` |
| Constants | PascalCase | `const float FadeDuration = 0.5f;` |
| Enums | PascalCase (type and values) | `enum GameStateType { LobbyLanding, Rehearsal }` |
| Events | `On` + PascalCase | `event Action OnStateChanged`, `event Action<WebViewError> OnLoadError` |
| ScriptableObject assets | PascalCase in filename | `ColorPalette.asset`, `AppConfig.asset` |

**Files and Folders:**

| Element | Convention | Example |
|---------|-----------|---------|
| C# script files | PascalCase, match class name exactly | `GameStateManager.cs`, `IWebViewController.cs` |
| Folders | PascalCase | `Scripts/Core/`, `Scripts/WebView/`, `Prefabs/UI/` |
| Prefab files | PascalCase | `PrimaryButton.prefab`, `TextBlock.prefab` |
| Scene files | PascalCase | `StageMind.unity` |
| Materials | PascalCase descriptive | `WallCream.mat`, `PodiumOak.mat`, `AudienceAtlas.mat` |
| Textures | PascalCase with suffix | `AudienceAtlas_Diffuse.png`, `Room_Lightmap.exr` |
| Audio clips | PascalCase descriptive | `RoomToneLoop.wav`, `AudienceCough01.wav` |

**Unity Scene Hierarchy:**

| Element | Convention | Example |
|---------|-----------|---------|
| Root objects | PascalCase, prefixed by system | `--- Environment ---`, `--- UI ---`, `--- Audio ---` |
| System managers | PascalCase + "Manager" or "Controller" | `GameStateManager`, `AudioManager`, `AudienceController` |
| Grouped objects | Parent with descriptive name | `AudienceRoot`, `LobbyUI`, `PodiumSetup` |

### 2. Script Organization Rules

**One class per file.** No exceptions. The filename must match the class name exactly.

**Script placement follows the project structure from Step 3:**

| Script Type | Location | Rule |
|------------|----------|------|
| Interfaces | `Scripts/Core/` | All interfaces in Core, regardless of which system uses them |
| State classes | `Scripts/Core/States/` | One file per state |
| Enums shared across systems | `Scripts/Core/` | Shared enums in Core |
| System-specific enums | With their system | `Scripts/WebView/WebViewError.cs` |
| MonoBehaviour managers | System folder | `Scripts/Audio/AudioManager.cs` |
| ScriptableObject definitions | System folder | `Scripts/Core/AppConfig.cs` (class), `ScriptableObjects/AppConfig.asset` (instance) |
| Helper/utility classes | `Scripts/Core/Utilities/` | Only truly shared utilities |
| Extension methods | `Scripts/Core/Extensions/` | Grouped by type being extended |

**No "Utils" dumping ground.** If a utility is used by only one system, it lives in that system's folder.

### 3. Communication Patterns

**Systems communicate through two mechanisms only:**

**Mechanism 1: State Machine Events (primary)**

```csharp
public event Action<GameStateType, GameStateType> OnStateChanged; // (previous, current)

public interface IStateAware
{
    void OnStateEnter(GameStateType state);
    void OnStateExit(GameStateType state);
}
```

All coordinated behavior (what happens when entering rehearsal, leaving lobby, etc.) flows through the state machine. Systems never coordinate directly with each other for state transitions.

**Mechanism 2: Direct Interface References (secondary)**

For non-state interactions (e.g., ErrorHandler needs to show a message on the UI, InputRouter needs to dispatch haptics), systems hold explicit references to the interfaces they depend on. These references are injected via constructor (pure C#) or `[SerializeField]` (MonoBehaviour).

**Forbidden communication patterns:**

- No static events or global event buses — all events are instance-scoped
- No `FindObjectOfType<>()` at runtime — all references established during initialization
- No string-based messaging (`SendMessage()`, `BroadcastMessage()`) — invisible dependencies
- No singletons (`Instance` pattern) — all dependencies explicit

### 4. MonoBehaviour & Unity Patterns

**Lifecycle Method Rules:**

| Method | Usage |
|--------|-------|
| `Awake()` | Self-initialization only — set up internal state, never reference other objects |
| `Start()` | Cross-references — register with GameStateManager, establish inter-system references |
| `OnEnable()` / `OnDisable()` | Subscribe/unsubscribe from events only |
| `Update()` | Avoid where possible. Prefer event-driven patterns. If needed, only in the `GameStateManager` which delegates to the current state's `Update()` |
| `OnDestroy()` | Cleanup — unsubscribe events, release resources |

**`[SerializeField]` Rules:**

- Always use `[SerializeField] private` — never public fields for Inspector exposure
- Add `[Tooltip("description")]` for non-obvious references
- Group serialized fields with `[Header("Section Name")]` for Inspector readability

**Coroutine Rules:**

- Use coroutines for timed sequences (reinforcement phases, fade animations)
- Always store coroutine references and stop them in `OnDisable()` or state exit
- Never use coroutines for game logic that needs to be testable — use `UIAnimator` for animation timing

**Prefab Rules:**

- Prefabs are self-contained — they must work when instantiated with no additional setup
- Script references within a prefab use `[SerializeField]` to internal children, never external scene objects
- Scene-level references (e.g., connecting a prefab to a manager) are wired at the scene level, not inside the prefab

### 5. Error Handling Flow

**All errors follow one path:**

```
System detects error
  → Creates ErrorEvent (type, userMessage, recoveryAction)
  → Sends to ErrorHandler
  → ErrorHandler maps to Card composition on laptop UI
  → User sees warm, human error message with clear next action
```

**ErrorEvent structure:**

```csharp
public struct ErrorEvent
{
    public ErrorType Type;           // Network, WebView, Platform
    public string UserMessage;       // "Hmm, that link didn't load"
    public string GuidanceMessage;   // "Make sure your slides are shared as a public link"
    public ErrorRecovery Recovery;   // RetryUrl, ShowInstructions, RestartApp
}
```

**Rules:**

- Systems never show error UI directly — always route through `ErrorHandler`
- User-facing messages use "supportive friend" voice — never technical language
- Every error includes a recovery path — no dead-end error states (except WebView double-crash → restart app)
- Debug-level errors go to `Debug.LogWarning()` / `Debug.LogError()` — never visible to user
- No `try/catch` blocks that swallow exceptions silently — catch, log, and route to `ErrorHandler`

### 6. Testing Conventions

**Test File Location:**

| Test Type | Location | Naming |
|-----------|----------|--------|
| Edit Mode (pure C#) | `Tests/EditMode/` | `{ClassName}Tests.cs` — e.g., `GameStateManagerTests.cs` |
| Play Mode (scene) | `Tests/PlayMode/` | `{SystemName}IntegrationTests.cs` — e.g., `WebViewIntegrationTests.cs` |

**Test Method Naming:**

```csharp
[Test]
public void MethodName_Scenario_ExpectedResult()
{
    // e.g., TransitionTo_FromLobbyToRehearsal_ActivatesAudience()
    // e.g., LoadUrl_InvalidProtocol_ReturnsNetworkFailure()
}
```

**Testing Rules:**

- State machine logic is tested in Edit Mode (no scene required) using mocked `IStateAware` implementations
- `IWebViewController` is mocked in all tests except dedicated WebView integration tests
- No test should depend on `Update()` timing — use explicit method calls
- Tests verify state transitions, event dispatching, and error routing — not visual output
- Each state class has tests for its `Enter()`, `Exit()`, and input handling

### Enforcement Guidelines

**All AI Agents MUST:**

1. Follow the naming conventions table exactly — no exceptions for "personal preference"
2. Place scripts in the correct folder per the organization rules — never dump in root `Scripts/`
3. Route all errors through `ErrorHandler` — never create custom error display logic
4. Communicate between systems only via state machine events or explicit interface references — no singletons, no globals, no `FindObjectOfType`
5. Use `[SerializeField] private` for Inspector-exposed fields — never public fields
6. Write Edit Mode tests for all pure C# classes — state classes, error types, configuration logic
7. Never use `Update()` in system scripts — the `GameStateManager` owns the update loop and delegates to the current state

**Anti-Patterns (Reject in Code Review):**

| Anti-Pattern | Why It's Wrong | Correct Pattern |
|-------------|---------------|-----------------|
| `public static GameManager Instance` | Hidden dependency, untestable | Explicit `[SerializeField]` or constructor injection |
| `FindObjectOfType<AudioManager>()` | Fragile, order-dependent, runtime cost | `[SerializeField] private AudioManager _audioManager;` |
| `gameObject.SendMessage("OnError")` | String-based, no compile-time safety | Direct interface method call or typed event |
| `public float fadeSpeed = 0.5f;` | Public field exposes internal state | `[SerializeField] private float _fadeSpeed = 0.5f;` |
| `try { } catch { }` (empty catch) | Swallows errors silently | Catch, log, and route to ErrorHandler |
| `Debug.Log("error: " + message)` shown to user | Technical message leaks to UX | Route through ErrorHandler with user-facing copy |

## Project Structure & Boundaries

### Requirements to Structure Mapping

**FR Category → System Mapping:**

| FR Category | System | Directory |
|------------|--------|-----------|
| Lobby & Onboarding (FR1-FR7) | WebView, UI, Input, Platform | `Scripts/WebView/`, `Scripts/UI/`, `Scripts/Input/`, `Scripts/Platform/` |
| Stage & Spatial Experience (FR8-FR12) | Environment, Core (states) | `Scripts/Environment/`, `Scripts/Core/States/` |
| Rehearsal Control (FR13-FR15) | Input, Core (states) | `Scripts/Input/`, `Scripts/Core/States/` |
| Post-Session Experience (FR16-FR17) | Reinforcement, Core (states) | `Scripts/Reinforcement/`, `Scripts/Core/States/` |
| Error Handling & Guidance (FR18-FR20) | Core (ErrorHandler), UI | `Scripts/Core/Errors/`, `Scripts/UI/` |
| Privacy & Multi-User (FR21-FR23) | WebView (cleanup), Core | `Scripts/WebView/`, `Scripts/Core/` |

**Cross-Cutting NFRs → System Mapping:**

| NFR Concern | Affected Systems | Primary Owner |
|------------|-----------------|---------------|
| 72fps (NFR1, NFR6) | All rendering systems | Environment (LOD, instancing), WebView (dirty-flag) |
| Slide latency (NFR2) | WebView, Input | `Scripts/WebView/VuplexWebViewController.cs` |
| Memory stability (NFR9) | WebView lifecycle | `Scripts/WebView/VuplexWebViewController.cs` |
| Privacy/cleanup (NFR17-18) | WebView cleanup | `Scripts/WebView/VuplexWebViewController.cs` |
| VR comfort (NFR11-13) | Core transitions, Environment | `Scripts/Core/States/`, `Scripts/Environment/` |

### Complete Project Directory Structure

```
StageMind/
├── .gitignore                              # Unity gitignore (Library/, Temp/, Logs/, obj/, Builds/)
├── README.md                               # Project setup instructions, architecture overview link
├── ProjectSettings/                        # Unity-managed — build profiles, input, quality, player, XR settings
│   └── (Unity-managed files)
│
├── Packages/
│   └── manifest.json                       # Unity package dependencies (OpenXR, Meta OpenXR, TMP, Input System)
│
├── Assets/
│   ├── _Project/
│   │   ├── Scenes/
│   │   │   └── StageMind.unity             # Single scene — all states coexist, toggled by GameStateManager
│   │   │
│   │   ├── Scripts/
│   │   │   ├── StageMind.asmdef                # Root assembly definition — all runtime code
│   │   │   ├── Core/
│   │   │   │   ├── IGameState.cs               # State interface: Enter(), Exit(), Update(), HandleInput()
│   │   │   │   ├── IStateAware.cs              # Observer interface: OnStateEnter(), OnStateExit()
│   │   │   │   ├── IWebViewController.cs       # WebView abstraction contract
│   │   │   │   ├── GameStateType.cs            # Enum: LobbyLanding, LobbySlidesLoaded, Rehearsal, Reinforcement, Paused
│   │   │   │   ├── GameStateManager.cs         # MonoBehaviour — composition root, state lifecycle, event dispatch
│   │   │   │   ├── AppConfig.cs                # ScriptableObject definition — timing, dimensions, counts
│   │   │   │   ├── ColorPalette.cs             # ScriptableObject definition — 12 semantic colors
│   │   │   │   ├── Errors/
│   │   │   │   │   ├── ErrorType.cs                # Enum: Network, WebView, Platform
│   │   │   │   │   ├── ErrorRecovery.cs            # Enum: RetryUrl, ShowInstructions, RestartApp
│   │   │   │   │   ├── ErrorEvent.cs               # Struct: Type, UserMessage, GuidanceMessage, Recovery
│   │   │   │   │   └── ErrorHandler.cs             # MonoBehaviour — receives ErrorEvents, routes to UI
│   │   │   │   ├── States/
│   │   │   │   │   ├── LobbyLandingState.cs        # Pure C# — shows landing page, URL input, instructions
│   │   │   │   │   ├── LobbySlidesLoadedState.cs   # Pure C# — shows slide preview, enables "Start Rehearsal"
│   │   │   │   │   ├── RehearsalState.cs           # Pure C# — audience visible, slide control active
│   │   │   │   │   ├── ReinforcementState.cs       # Pure C# — warm shift, message, input blocked
│   │   │   │   │   └── PausedState.cs              # Pure C# — overlay menu, resume/end/return options
│   │   │   │   ├── Utilities/
│   │   │   │   │   └── RandomWithoutRepeat.cs      # Shared utility — message pool selection
│   │   │   │   └── Extensions/
│   │   │   │       └── (reserved for future shared extensions)
│   │   │   │
│   │   │   ├── WebView/
│   │   │   │   ├── SlideImageController.cs          # MVP IWebViewController — fetches Google Slides images via UnityWebRequest
│   │   │   │   ├── VuplexWebViewController.cs       # Post-MVP IWebViewController wrapping Vuplex (conditional #if VUPLEX_WEBVIEW)
│   │   │   │   ├── WebViewError.cs                  # Enum: NetworkFailure, LoginWallDetected, PageLoadTimeout, Unknown
│   │   │   │   └── UrlValidator.cs                  # URL protocol/format validation before load
│   │   │   │
│   │   │   ├── Input/
│   │   │   │   ├── InputRouter.cs                  # MonoBehaviour — subscribes to Input Actions, routes via current state
│   │   │   │   └── HapticFeedback.cs               # OpenXR haptic dispatch for controller vibration
│   │   │   │
│   │   │   ├── UI/
│   │   │   │   ├── UIPointerController.cs          # MonoBehaviour — raycaster for world-space UI interaction
│   │   │   │   ├── VRButtonInteraction.cs          # MonoBehaviour — handles button hover/press/release states
│   │   │   │   └── UIAnimator.cs                   # MonoBehaviour — fade, scale, color transitions for UI elements
│   │   │   │
│   │   │   ├── Audio/
│   │   │   │   ├── AudioManager.cs                 # MonoBehaviour — ambient room tone, state-driven audio
│   │   │   │   ├── AudienceAudioController.cs      # MonoBehaviour — periodic audience sounds (coughs, shifts)
│   │   │   │   └── AudioConfig.cs                  # ScriptableObject definition — volumes, intervals
│   │   │   │
│   │   │   ├── Environment/
│   │   │   │   ├── AudienceController.cs           # MonoBehaviour — toggles AudienceRoot visibility
│   │   │   │   ├── LightingController.cs           # MonoBehaviour — URP Volume weight for reinforcement shift
│   │   │   │   └── SceneTransitionController.cs    # MonoBehaviour — fade-to-black overlay for state transitions
│   │   │   │
│   │   │   ├── Reinforcement/
│   │   │   │   ├── ReinforcementController.cs      # MonoBehaviour — orchestrates post-session sequence
│   │   │   │   └── ReinforcementMessages.cs        # ScriptableObject definition — message pool
│   │   │   │
│   │   │   └── Platform/
│   │   │       ├── PlatformManager.cs              # MonoBehaviour — headset events, app lifecycle
│   │   │       ├── KeyboardManager.cs              # MonoBehaviour — Quest system keyboard integration
│   │   │       └── QRScannerManager.cs             # MonoBehaviour — passthrough camera QR code scanning
│   │   │
│   │   ├── InputActions/
│   │   │   ├── StageMindActions.inputactions       # Input Action Asset — AdvanceSlide, PreviousSlide, PauseMenu, UIPoint, UIClick
│   │   │   └── StageMindActions.cs                 # Auto-generated C# wrapper (generated by Unity when "Generate C# Class" enabled)
│   │   │
│   │   ├── Prefabs/
│   │   │   ├── UI/
│   │   │   │   ├── TextBlock.prefab                # Atomic — TMP text with standard VR sizing
│   │   │   │   ├── PrimaryButton.prefab            # Atomic — "Start Rehearsal", "Go Again" actions
│   │   │   │   ├── SecondaryButton.prefab          # Atomic — "Done for Today", "Return to Lobby"
│   │   │   │   ├── GhostButton.prefab              # Atomic — text-only interactive ("back" actions)
│   │   │   │   ├── Card.prefab                     # Composition — groups icon + heading + body + action
│   │   │   │   ├── InputField.prefab               # Atomic — URL text input field
│   │   │   │   ├── LoadingIndicator.prefab         # Atomic — progress indicator for WebView loading
│   │   │   │   └── IconElement.prefab              # Atomic — icon display component
│   │   │   ├── Environment/
│   │   │   │   ├── AudienceMember.prefab           # GPU-instanced audience character
│   │   │   │   ├── Podium.prefab                   # Stage podium with confidence monitor surface
│   │   │   │   ├── ProjectorScreen.prefab          # Rear projector screen with RenderTexture surface
│   │   │   │   └── Chair.prefab                    # Audience seating (instanced)
│   │   │   └── Audio/
│   │   │       ├── AmbientSource.prefab            # Spatial audio source for room tone
│   │   │       └── AudienceSource.prefab           # Spatial audio source for audience sounds
│   │   │
│   │   ├── ScriptableObjects/
│   │   │   ├── ColorPalette.asset                  # Instance — 12 UX-spec semantic colors
│   │   │   ├── AppConfig.asset                     # Instance — timing, dimensions, counts, haptic values
│   │   │   ├── ReinforcementMessages.asset         # Instance — 15-message pool
│   │   │   └── AudioConfig.asset                   # Instance — ambient volumes, audience sound intervals
│   │   │
│   │   ├── Materials/
│   │   │   ├── Environment/
│   │   │   │   ├── WallCream.mat                   # Room walls
│   │   │   │   ├── FloorWood.mat                   # Stage/room floor
│   │   │   │   ├── CeilingPanel.mat                # Ceiling tiles
│   │   │   │   └── PodiumOak.mat                   # Podium surface
│   │   │   ├── Audience/
│   │   │   │   └── AudienceAtlas.mat               # GPU-instanced shared material with atlas UVs
│   │   │   ├── UI/
│   │   │   │   ├── ProjectorScreenMat.mat          # RenderTexture material for projector
│   │   │   │   ├── LaptopScreenMat.mat             # RenderTexture material for confidence monitor
│   │   │   │   └── FadeOverlay.mat                 # Unlit black material for fade-to-black transitions
│   │   │   └── Furniture/
│   │   │       └── ChairFabric.mat                 # Audience chair material
│   │   │
│   │   ├── Textures/
│   │   │   ├── AudienceAtlas_Diffuse.png           # Texture atlas for audience appearance variation
│   │   │   └── Room_Lightmap.exr                   # Baked lightmap for environment
│   │   │
│   │   ├── Audio/
│   │   │   ├── Ambient/
│   │   │   │   └── RoomToneLoop.wav                # Continuous room ambient loop
│   │   │   ├── Audience/
│   │   │   │   ├── AudienceCough01.wav             # Periodic audience sound
│   │   │   │   ├── AudienceCough02.wav
│   │   │   │   ├── AudienceShift01.wav             # Chair shift sound
│   │   │   │   └── AudienceShift02.wav
│   │   │   └── Reinforcement/
│   │   │       └── ReinforcementTone.wav           # Warm tone for positive reinforcement
│   │   │
│   │   ├── Fonts/
│   │   │   ├── Inter-Regular SDF.asset             # TMP SDF font — body text
│   │   │   └── Inter-SemiBold SDF.asset            # TMP SDF font — headings, buttons
│   │   │
│   │   ├── Art/
│   │   │   ├── Characters/
│   │   │   │   ├── AudienceMember_LOD0.fbx         # Full detail (rows 1-3)
│   │   │   │   └── AudienceMember_LOD1.fbx         # Simplified (rows 4+)
│   │   │   ├── Furniture/
│   │   │   │   ├── Podium.fbx
│   │   │   │   ├── Chair.fbx
│   │   │   │   └── ProjectorScreen.fbx
│   │   │   └── Room/
│   │   │       └── ConferenceRoom.fbx              # Room shell geometry
│   │   │
│   │   └── Settings/
│   │       ├── URP/
│   │       │   ├── StageMind_URPAsset.asset         # URP pipeline settings (mobile VR config)
│   │       │   ├── StageMind_Renderer.asset         # URP renderer configuration
│   │       │   └── ReinforcementVolume.asset        # URP Volume profile — ColorAdjustments warm shift
│   │       └── XR/
│   │           └── OpenXRSettings.asset             # OpenXR feature configuration
│   │
│   ├── Tests/
│   │   ├── EditMode/
│   │   │   ├── StageMind.Tests.EditMode.asmdef     # Assembly definition — references StageMind.asmdef
│   │   │   ├── GameStateManagerTests.cs            # State transitions, event dispatch, initialization
│   │   │   ├── LobbyLandingStateTests.cs           # Enter/Exit behavior, URL input handling
│   │   │   ├── LobbySlidesLoadedStateTests.cs      # Enter/Exit, start rehearsal transition
│   │   │   ├── RehearsalStateTests.cs              # Slide navigation, pause handling
│   │   │   ├── ReinforcementStateTests.cs          # Sequence timing, message selection, input blocking
│   │   │   ├── PausedStateTests.cs                 # Menu options, resume/end/return transitions
│   │   │   ├── ErrorHandlerTests.cs                # Error routing, message mapping, recovery paths
│   │   │   ├── UrlValidatorTests.cs                # URL format validation, protocol checks
│   │   │   ├── RandomWithoutRepeatTests.cs         # Pool exhaustion, reset, distribution
│   │   │   └── Mocks/
│   │   │       ├── MockWebViewController.cs        # IWebViewController mock for state tests
│   │   │       └── MockStateAware.cs               # IStateAware mock for GameStateManager tests
│   │   └── PlayMode/
│   │       ├── StageMind.Tests.PlayMode.asmdef     # Assembly definition — references StageMind.asmdef
│   │       ├── SceneTransitionIntegrationTests.cs  # Fade-to-black, state transitions in scene
│   │       ├── StateSystemIntegrationTests.cs      # IStateAware implementations respond to real state transitions
│   │       ├── InputIntegrationTests.cs            # InputRouter dispatches to correct state handler
│   │       ├── WebViewIntegrationTests.cs          # Vuplex initialization, load, cleanup (requires device)
│   │       └── TestScenes/
│   │           └── IntegrationTestScene.unity      # Minimal scene with GameStateManager + essential system MonoBehaviours
│   │
│   └── Plugins/
│       └── (reserved for post-MVP browser plugin integration)
│
└── Builds/                                         # All build outputs (git-ignored)
    └── Quest/                                      # APK output directory
```

### Architectural Boundaries

**Integration Boundary: WebView**

The WebView plugin (Vuplex) is isolated behind `IWebViewController`. No script outside `Scripts/WebView/` directly references Vuplex types. If the plugin changes or is replaced, only `VuplexWebViewController.cs` is modified.

```
App Code ──→ IWebViewController (interface in Core/)
                    │
        ┌───────────┴───────────┐
  SlideImageController      VuplexWebViewController
  (MVP — image fetching)    (post-MVP — conditional)
        │                        │
  UnityWebRequest           Vuplex Plugin or
  (Unity built-in)          SimpleUnity3DWebView
```

**Integration Boundary: Platform/Quest APIs**

All Meta/Quest SDK calls are isolated in `Scripts/Platform/`. Other systems never call `OVRManager`, `TouchScreenKeyboard`, or passthrough APIs directly.

```
App Code ──→ PlatformManager / KeyboardManager / QRScannerManager
                    │
              Meta XR OpenXR Plugin / Unity OpenXR
```

**Component Communication Boundaries:**

```
┌──────────────────────────────────────────────────────────┐
│ GameStateManager (composition root)                      │
│   Owns: current IGameState, dispatches OnStateChanged    │
│   Wired to all IStateAware systems via [SerializeField]  │
├──────────────────────────────────────────────────────────┤
│ State-Driven Systems (implement IStateAware)             │
│   AudioManager, AudienceController, LightingController,  │
│   UIPointerController, InputRouter, ReinforcementCtrl    │
│   ─ React to state enter/exit events                     │
│   ─ Never call each other directly for state behavior    │
├──────────────────────────────────────────────────────────┤
│ Support Systems (referenced directly via interface)       │
│   ErrorHandler ← receives ErrorEvent from any system     │
│   UIAnimator ← called by systems needing animations      │
│   HapticFeedback ← called by InputRouter                │
└──────────────────────────────────────────────────────────┘
```

**Data Flow:**

```
URL Input → KeyboardManager → SlideImageController.LoadUrl()
                                    │
                              Parses Google Slides ID → Fetches slide images via UnityWebRequest
                                    │
                              OnLoadSuccess ──→ GameStateManager transitions to LobbySlidesLoaded
                              OnLoadError  ──→ ErrorHandler ──→ UI displays error Card
                                    │
                              Slide textures[] ──→ Graphics.Blit(current) ──→ RenderTexture
                                    │                                            │
                                    │                              ProjectorScreen material
                                    │                              LaptopScreen material (shared)
                                    │
Controller Input → InputRouter → Current State.HandleInput()
                                    │
                              SlideImageController.SendKeyEvent(Arrow) ──→ increments slide index
                              Graphics.Blit(slides[newIndex], renderTexture) ──→ display updates
```

### Assembly Definitions

| Assembly | References | Purpose |
|----------|-----------|---------|
| `StageMind` (`Scripts/StageMind.asmdef`) | Unity defaults + TextMeshPro + Input System | All runtime scripts |
| `StageMind.Tests.EditMode` | `StageMind` + Unity Test Framework | Pure C# unit tests |
| `StageMind.Tests.PlayMode` | `StageMind` + Unity Test Framework | Scene integration tests |

### Development Workflow

**Build Process:**

1. **Editor Development:** Unity Editor with Meta XR Simulator — test state transitions, UI layout, input routing without headset
2. **Device Build:** File → Build Profiles → Quest → Build and Run → produces APK in `Builds/Quest/`
3. **Testing:** Edit Mode tests run in Editor (no scene). Play Mode tests require Editor Play Mode. WebView integration tests require Quest 3 device.

## Architecture Validation Results

### Coherence Validation

**Decision Compatibility:** All technology choices are compatible: Unity 6.3 LTS + URP + OpenXR + Meta XR OpenXR plugin + Vuplex + Unity Input System + TextMeshPro. The State Pattern with pure C# classes is compatible with the hybrid DI approach (constructor injection for states, `[SerializeField]` for MonoBehaviours). ScriptableObjects for configuration is standard Unity. No version conflicts or incompatible API surfaces detected.

**Pattern Consistency:** The naming conventions (Microsoft C# standard), communication patterns (state machine events + direct interface references), error handling flow (centralized ErrorHandler), and testing approach (Edit Mode for pure C# / Play Mode for scene integration) are internally consistent. The anti-pattern list in the enforcement guidelines directly reinforces the communication pattern decisions — no contradictions.

**Structure Alignment:** The project tree maps cleanly to every architectural decision. Each of the 8 system folders corresponds to an identified architectural component. The `Core/Errors/` and `Core/States/` subfolders prevent overcrowding. Assembly definitions enable clean test isolation. The WebView and Platform integration boundaries are structurally enforced by directory separation.

### Requirements Coverage Validation

**Functional Requirements — 23/23 covered:**

| FR | Architectural Support | Primary System |
|----|----------------------|----------------|
| FR1 (instructions) | `LobbyLandingState` + UI prefabs | Core/States, UI |
| FR2 (URL input) | `KeyboardManager` + `IWebViewController.LoadUrl()` | Platform, WebView |
| FR3 (QR code) | `QRScannerManager` (conditional on spike) | Platform |
| FR4 (slide preview) | `RenderTexture` shared to laptop surface | WebView, Materials |
| FR5 (browser interaction) | `UIPointerController` + WebView | UI, WebView |
| FR6 (browser navigation) | `IWebViewController` methods | WebView |
| FR7 (lobby to stage) | `GameStateManager` state transition | Core |
| FR8 (virtual stage) | `ConferenceRoom.fbx` + environment setup | Environment, Art |
| FR9 (50 audience) | `AudienceController` + `AudienceMember.prefab` (GPU instanced) | Environment |
| FR10 (slides on projector) | `RenderTexture` → `ProjectorScreenMat` | WebView, Materials |
| FR11 (360° tracking) | OpenXR head tracking (platform-provided) | Platform |
| FR12 (visual transition) | `SceneTransitionController` + `FadeOverlay.mat` | Environment |
| FR13 (advance slides) | `InputRouter` → `WebView.SendKeyEvent(RightArrow)` | Input, WebView |
| FR14 (previous slide) | `InputRouter` → `WebView.SendKeyEvent(LeftArrow)` | Input, WebView |
| FR15 (end rehearsal) | `InputRouter` → `PausedState` transition | Input, Core |
| FR16 (reinforcement) | `ReinforcementController` + `ReinforcementState` | Reinforcement, Core |
| FR17 (rehearse again/return) | State transitions from `ReinforcementState` | Core |
| FR18 (landing instructions) | `LobbyLandingState` default UI composition | Core, UI |
| FR19 (error recovery) | `ErrorHandler` → state loops back to `LobbyLandingState` | Core/Errors |
| FR20 (no internet) | `Application.internetReachability` → `ErrorHandler` | Core/Errors |
| FR21 (no account) | Stateless design — no auth infrastructure | Architecture-wide |
| FR22 (no persistence) | `IWebViewController.Cleanup()` clears all data | WebView |
| FR23 (multi-user) | `Cleanup()` on exit + no `PlayerPrefs` | WebView, Core |

**Non-Functional Requirements — 20/20 covered:**

| NFR | Architectural Support |
|-----|----------------------|
| NFR1 (72fps) | URP mobile config, GPU instancing, baked lighting, WebView dirty-flag rendering |
| NFR2 (<200ms slide) | `SendKeyEvent()` direct dispatch, dirty-flag texture update |
| NFR3 (<3s transition) | Single-scene state change (no scene load), fade overlay |
| NFR4 (<10s cold start) | Single scene, no dynamic loading |
| NFR5 (loading indicator) | `LoadingIndicator.prefab` |
| NFR6 (no throttling) | Performance budget management, selective post-processing |
| NFR7 (zero crashes) | WebView crash detection + one-retry recovery |
| NFR8 (network drop) | `ErrorHandler` + slides remain on `RenderTexture` |
| NFR9 (memory stability) | WebView lifecycle management, single instance reuse |
| NFR10 (controller battery) | Minimal input processing, event-driven |
| NFR11 (fade transitions) | `SceneTransitionController` enforces fade-to-black |
| NFR12 (stable viewpoint) | No locomotion, no camera manipulation |
| NFR13 (spatial scale) | `AppConfig` ScriptableObject with dimension constants |
| NFR14 (reliable rendering) | Vuplex WebView + `IWebViewController` contract |
| NFR15 (controller mapping) | `InputRouter` + `StageMindActions.inputactions` |
| NFR16 (browser navigation) | `IWebViewController` methods |
| NFR17 (no PII) | No analytics, no data collection infrastructure |
| NFR18 (no persistence) | `Cleanup()` clears cookies/cache/history |
| NFR19 (Quest Store) | Platform configuration, APK build pipeline |
| NFR20 (<2GB APK) | Asset budget awareness, LOD, texture atlases |

### Implementation Readiness Validation

**Decision Completeness:** All 8 core decisions documented with rationale, implementation approach, and cross-component impact. The `IWebViewController` interface contract specified at code level. State transition map covers all valid transitions. Enforcement guidelines and anti-patterns table provide explicit rules for AI agents.

**Structure Completeness:** Project tree lists every expected file with annotations. Assembly definitions, test structure, and build output defined. No placeholder directories.

**Pattern Completeness:** All 6 identified conflict areas have documented resolution patterns with concrete code examples. Lifecycle method rules, `[SerializeField]` rules, coroutine rules, and prefab rules cover Unity-specific patterns that AI agents would otherwise guess at inconsistently.

### Gap Analysis Results

**No critical gaps found.** No important gaps found.

**Minor observations (non-blocking, implementation-level):**

| Item | Observation | Resolution |
|------|------------|------------|
| Confidence monitor surface | Laptop is part of `Podium.prefab`, not a separate prefab | Noted in prefab description — sufficient |
| RenderTexture parameters | Resolution/format not specified at architecture level | Agent determines based on Quest 3 GPU budget during implementation |
| FR3 spike outcome | QR scanning conditional; `QRScannerManager.cs` in tree regardless | PRD marks as conditional — architecture correctly flexible |
| Pause menu composition | No dedicated pause overlay prefab — composed from atomic prefabs | Consistent with compositional UI approach |

### Architecture Completeness Checklist

**Requirements Analysis**

- [x] Project context thoroughly analyzed (23 FRs, 20 NFRs, 4 user journeys)
- [x] Scale and complexity assessed (low-to-medium, ~8-10 systems)
- [x] Technical constraints identified (10 constraints documented)
- [x] Cross-cutting concerns mapped (6 concerns identified)

**Architectural Decisions**

- [x] Critical decisions documented with rationale (3 critical, 5 important)
- [x] Technology stack fully specified (Unity 6.3 LTS, URP, OpenXR, Vuplex, Input System)
- [x] Integration patterns defined (WebView boundary, Platform boundary)
- [x] Performance considerations addressed (dirty-flag, GPU instancing, baked lighting)

**Implementation Patterns**

- [x] Naming conventions established (C#, files, scene hierarchy)
- [x] Structure patterns defined (one class per file, system folders)
- [x] Communication patterns specified (state events + direct references, forbidden patterns)
- [x] Process patterns documented (error flow, lifecycle methods, testing)

**Project Structure**

- [x] Complete directory structure defined (all files annotated)
- [x] Component boundaries established (WebView, Platform isolation)
- [x] Integration points mapped (data flow diagrams)
- [x] Requirements to structure mapping complete (FR→system, NFR→system)

### Architecture Readiness Assessment

**Overall Status:** READY FOR IMPLEMENTATION

**Confidence Level:** High — all requirements traced, all decisions coherent, patterns specific enough for AI agent consistency.

**Key Strengths:**

- Clean WebView isolation behind `IWebViewController` enables plugin swappability and testability
- Pure C# state classes fully testable in Edit Mode without Unity scene
- Hybrid DI approach balances Unity idioms with testability
- Single-scene architecture eliminates scene loading complexity
- Explicit anti-pattern list prevents common Unity mistakes by AI agents

**Areas for Future Enhancement (Post-MVP):**

- Audience animation system (idle breathing, head shifts)
- Audio middleware for more sophisticated spatial audio
- Analytics/telemetry framework (opt-in, per NFR17)
- Cross-platform abstraction layer for non-Quest headsets
- Multiple environment variants (different room sizes, contexts)

### Implementation Handoff

**AI Agent Guidelines:**

- Follow all architectural decisions exactly as documented
- Use implementation patterns consistently across all components
- Respect project structure and boundaries — place files in correct system folders
- Route all errors through `ErrorHandler` — never create custom error display
- Communicate between systems only via state machine events or explicit interface references
- Refer to this document for all architectural questions

**First Implementation Priority:**

1. Unity project initialization (Universal 3D Template + package installation + platform configuration)
2. `GameStateManager` + state classes — the backbone everything else plugs into
3. Phase 0 WebView spike — validate Vuplex on Quest 3 before building dependent systems
