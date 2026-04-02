---
stepsCompleted: ['step-01-validate-prerequisites', 'step-02-design-epics', 'step-03-create-stories', 'step-04-final-validation']
inputDocuments:
  - planning-artifacts/prd.md
  - planning-artifacts/architecture.md
  - planning-artifacts/ux-design-specification.md
---

# StageMind - Epic Breakdown

## Overview

This document provides the complete epic and story breakdown for StageMind, decomposing the requirements from the PRD, UX Design, and Architecture into implementable stories.

## Requirements Inventory

### Functional Requirements

- FR1: User can view slide loading instructions upon entering the app, explaining how to share their presentation as a public link
- FR2: User can enter a presentation URL using text input in the lobby
- FR3: User can scan a QR code containing a presentation URL to load slides (conditional MVP — contingent on feasibility spike; UX must accommodate whether or not this ships)
- FR4: User can view their presentation slides rendered in the lobby before entering the stage
- FR5: User can interact with the embedded browser panel using VR controller-based input (pointing, clicking, scrolling, text entry)
- FR6: User can navigate the embedded browser (back, forward, refresh, URL entry) to reach their published presentation
- FR7: User can initiate the transition from lobby to stage when ready to rehearse
- FR8: User can stand on a virtual stage in a conference room environment
- FR9: User can see a seated audience of 50 static attendees facing the stage
- FR10: User can see their presentation slides displayed on a projector screen behind/beside them on stage
- FR11: User can look around the stage environment freely (full 360° head tracking)
- FR12: User experiences a visual transition (fade-to-black) when moving between lobby and stage
- FR13: User can advance slides forward during rehearsal using a VR controller input
- FR14: User can go back to a previous slide during rehearsal using a VR controller input
- FR15: User can end a rehearsal session at any time using a VR controller input, triggering the post-session experience
- FR16: User receives positive reinforcement upon ending a rehearsal session
- FR17: User can choose to rehearse again or return to the lobby after a session ends
- FR18: The lobby landing page instructs users to use public/published slide links before navigating the browser
- FR19: User can recover from a failed slide load without restarting the app (return to lobby, re-enter URL)
- FR20: User is informed when no internet connection is available and slide loading cannot proceed
- FR21: The app operates without requiring any user account, login, or registration
- FR22: The app retains no user-specific data between sessions (no browser history, no cached URLs, no user preferences)
- FR23: Multiple users can use the app on the same device without conflicts or exposure to another user's data

### NonFunctional Requirements

- NFR1: Minimum sustained frame rate of 72fps during all scenes, including active WebView rendering on the stage projector screen
- NFR2: Slide advancement via controller input has less than 200ms perceived latency from button press to visual update on the projector screen
- NFR3: Lobby-to-stage scene transition completes in under 3 seconds with no frame drops below 72fps
- NFR4: App launches and reaches the lobby landing page in under 10 seconds from cold start
- NFR5: Loading indicator displayed while WebView content loads; first slide visible within 10 seconds on a 25 Mbps connection
- NFR6: App remains within Quest 3's thermal comfort zone during a 30-minute session — no thermal warnings, no forced performance reduction or throttling
- NFR7: 30-minute continuous rehearsal session with zero crashes, freezes, or WebView hang events using a published Google Slides deck of up to 60 slides on Quest 3
- NFR8: If network drops mid-session, slides already rendered remain visible on the projector screen; app does not crash; non-intrusive notification appears when connectivity is restored or when browser navigation is attempted
- NFR9: Embedded browser memory usage stays within 15% of initial allocation after 5 consecutive rehearsal sessions with no frame rate degradation below 72fps
- NFR10: Controller battery drain during a 30-minute session does not exceed drain rates of comparable single-player Quest applications
- NFR11: All scene transitions use fade-to-black or cross-fade — no hard cuts, teleportation, or sudden camera movement
- NFR12: User's viewpoint remains stable and grounded at all times — no artificial locomotion, no camera shake, no forced head movement
- NFR13: Stage environment maintains consistent spatial scale matching a mid-size conference room (stage area approximately 4m × 3m, audience seating depth 8–10m, ceiling height 3–4m)
- NFR14: Embedded browser reliably renders published/view-only presentation URLs from Google Slides, Canva, and PowerPoint Online without layout corruption or missing content
- NFR15: VR controller input (pointing, clicking, scrolling) maps correctly to browser interaction without input lag or missed clicks
- NFR16: Embedded browser supports standard web navigation: URL entry, back, forward, and refresh — each action completes within 1 second under normal network conditions
- NFR17: App does not collect or transmit personally identifiable data. Crash reporting, if implemented, must be opt-in with clear user consent
- NFR18: No session state persists after the app is closed — browser cache, cookies, and history cleared on exit
- NFR19: App meets all Meta Quest Store submission requirements including privacy policy, data collection disclosure, content rating, and review guidelines
- NFR20: App package size remains under 2GB to meet Quest Store recommended limits

### Additional Requirements

- The Architecture specifies Universal 3D Template (Unity 6.3 LTS + URP) as the starter — project initialization (Unity Hub setup, project creation, platform configuration, package installation) should be the first implementation story
- Phase 0 WebView spike is the project's go/no-go decision point and must be completed before investing in any other development
- Required Unity packages: OpenXR (≥1.15.1), Meta XR OpenXR (≥2.2), Meta XR Simulator, TextMeshPro, Vuplex 3D WebView for Android
- Platform configuration: IL2CPP scripting backend, ARM64 target, Vulkan graphics API, Android Minimum API Level 29
- GameStateManager + State Pattern with 5 pure C# state classes (LobbyLandingState, LobbySlidesLoadedState, RehearsalState, ReinforcementState, PausedState) and IGameState/IStateAware interfaces
- IWebViewController interface contract: Initialize(), LoadUrl(), SendKeyEvent(), Cleanup(), events for OnLoadSuccess/OnLoadError/OnCrash, WebViewError enum
- Input Action Asset (StageMindActions) with 3 mapped actions: AdvanceSlide (right trigger), PreviousSlide (right B), PauseMenu (left menu button)
- InputRouter MonoBehaviour that routes input through GameStateManager to the current state
- Hybrid dependency wiring: constructor injection for pure C# state classes, [SerializeField] for MonoBehaviour inter-references — no singletons, no FindObjectOfType, no SendMessage
- ScriptableObject configuration assets: ColorPalette (12 colors), AppConfig (timing, dimensions, counts), ReinforcementMessages (15-message pool), AudioConfig (volumes, intervals)
- Centralized ErrorHandler with ErrorEvent struct (Type, UserMessage, GuidanceMessage, Recovery) routing all errors to laptop UI
- Single-scene architecture — lobby and stage are states within one Unity scene, not separate scenes
- Assembly definitions: StageMind (runtime), StageMind.Tests.EditMode (pure C# tests), StageMind.Tests.PlayMode (scene tests)
- Complete project directory structure specified under Assets/_Project/ with 8 script folders (Core, WebView, Input, UI, Audio, Environment, Reinforcement, Platform)
- Naming conventions: PascalCase classes/methods, _camelCase private fields, PascalCase files/folders, one class per file
- Communication patterns: state machine events (primary) + direct interface references (secondary) only — no static events, no global event buses
- Edit Mode tests required for all pure C# classes (state classes, error types, URL validator, etc.)
- Play Mode tests for scene transitions, state system integration, input integration
- Mock implementations required: MockWebViewController, MockStateAware
- Meta XR Simulator for headset-free development on Mac
- WebView texture updates on dirty-flag (re-render only on input/navigation, not per-frame)
- WebView crash recovery: one retry, then "restart app" message (no infinite retry loop)

### UX Design Requirements

- UX-DR1: ColorPalette ScriptableObject with 12 semantic colors — Surface (#F5F0E8), SurfaceAlt (#EDE6D6), TextPrimary (#3A3632), TextSecondary (#7A7570), PrimaryAction (#D4A43A), PrimaryHover (#E8B84B), PrimaryPressed (#B8902E), SecondaryBorder (#A09A94), Error (#C47A5A), Success (#8BA888), ReinforcementGlow (#E8A830), Disabled (#B8B2AA)
- UX-DR2: Typography system using Inter font (Regular 400 + Medium 500) with TextMeshPro SDF rendering at 5 type scale levels — Display ~4cm, Heading ~2.5cm, Body ~1.8cm, Label ~1.6cm, Caption ~1.2cm world height
- UX-DR3: Spacing system with 5 world-space tokens — xs (5mm), sm (1cm), md (2cm), lg (3cm), xl (5cm)
- UX-DR4: TextBlock prefab with TextBlockSize enum variants (Display, Heading, Body, Label, Caption) — one prefab, one font reference, size controlled by enum selector
- UX-DR5: PrimaryButton prefab — rounded rectangle with amber fill (#D4A43A), 5 interaction states (Default, Hovered #E8B84B, Pressed #B8902E, Released flash, Disabled dimmed), minimum 0.08m × 0.04m hit target
- UX-DR6: SecondaryButton prefab — transparent fill with warm gray border (#A09A94), 5 interaction states matching PrimaryButton pattern
- UX-DR7: TextInputField prefab — placeholder text "Paste your slide link here...", Quest TouchScreenKeyboard integration, dual submission paths (keyboard Done key + adjacent "Go" PrimaryButton)
- UX-DR8: Card prefab — styled container with configurable accent color strip, SurfaceAlt (#EDE6D6) background, md (2cm) padding, subtle corner radius, composed with child TextBlock and button prefabs for error display
- UX-DR9: LoadingIndicator prefab — animated spinner or horizontal bar in sage green (#8BA888), fades out on completion
- UX-DR10: StatusIndicator prefab — transient non-interactive text label with UIAnimator-driven fade in (~0.3s), hold (~2s), fade out (~0.5s) — used for "Last slide" indicator on confidence monitor
- UX-DR11: VRButtonInteraction shared script — handles controller ray intersection (hover enter/exit), trigger pull (press/release), visual state transitions, haptic feedback dispatch, disabled state blocking — single script for both button types
- UX-DR12: UIPointerController — controller ray casting from right controller, UI layer intersection testing, current hovered element tracking, pointer visual (line renderer or dot)
- UX-DR13: UIAnimator shared utility — fade (alpha over time), color lerp, scale pulse, configurable duration and easing — used by StatusIndicator, reinforcement transitions, pause menu, button animations
- UX-DR14: Landing page composition on laptop screen — inverted visual hierarchy: instructions (top) → URL input + "Go" button → "Scan QR Code" SecondaryButton → controller diagram (3 TextBlock labels) → "Start Rehearsal" PrimaryButton (disabled until slides load) → "Quit StageMind" SecondaryButton (de-emphasized, bottom)
- UX-DR15: Pause menu composition — gaze-anchored world-space Canvas spawned ~1.5m forward at Menu press then world-locked, semi-transparent warm background dimming scene, 3 vertically stacked SecondaryButtons (Resume / End Session / Return to Lobby) with md spacing
- UX-DR16: Post-session reinforcement — 3-phase sequence: Impact (0–1s: warm amber lights via URP Volume, audience fades ~1s, warm-toned particles drift upward, affirming message in Display typography, trigger and menu inputs disabled), Linger (1–5s: complete stillness, nothing changes), Transition (~5s: "Continue" PrimaryButton fades in via UIAnimator, then "Go Again" PrimaryButton + "Done for Today" SecondaryButton on continue press)
- UX-DR17: 15 reinforcement messages in a pool — random-without-replacement selection within session, pool resets across app launches. Messages span early ("You showed up. That counts.") through later ("The stage knows you now.") emotional stages
- UX-DR18: Error state compositions — 4 scenarios displayed as Card on laptop screen: URL load failure ("Hmm, that link didn't load"), login wall detected via redirect heuristic ("Looks like this page needs a login"), no internet ("We're offline right now" + "Try Again" button), WebView crash (one retry then dead-end "restart app" message)
- UX-DR19: Audio design — 3 MVP audio assets: room tone loop (30–60s seamless HVAC/building hum), audience ambient variations (6–8 one-shot clips 1–3s: seat shift, throat clear, cough, paper rustle — randomized with spatial position variance at 10–20s intervals), reinforcement tone (~2s warm pad/chime). All spatialized via HRTF. Diegetic-only principle. No music.
- UX-DR20: Audience character rendering budget — 1,500–3,000 tris per character, total 75K–150K tris, 1–2 shared 2048×2048 texture atlases, 2 LOD levels (full for rows 1–3 within 6m, simplified for rows 4+), ≤20 draw calls via GPU instancing, 8–12 appearance variations via atlas UV offsets, baked ambient occlusion beneath chairs, no real-time shadows, no animation in MVP
- UX-DR21: Natural minimalism 3D environment — warm cream walls (#F5F0E8–#EDE6D6), light oak/birch wood podium and floor (#C8A96E–#D4B87A), warm charcoal grounding elements (#3A3632–#4A4540), baked lightmaps for ambient warmth, ~4000K warm white ambient light, plausibly real conference room aesthetic
- UX-DR22: Confidence monitor — laptop on podium tilted 20–30° for VR comfort, shared RenderTexture with projector screen (one WebView instance, two display surfaces), interactive in lobby mode only, non-interactive during rehearsal
- UX-DR23: Projector screen lobby state — displays static warm-toned texture with centered StageMind wordmark before slides load (not a mirror of the landing page UI), material swap to shared RenderTexture on successful WebView page load
- UX-DR24: Haptic feedback patterns — subtle click on button press, slide advance (trigger), slide back (B), and menu press. No haptic on non-events (trigger on last slide, B on first slide). Never strong/aggressive vibration.
- UX-DR25: Headset removal behavior — lobby: resume exactly, rehearsal: auto-show pause menu on return, reinforcement: resume with immediate Continue button if in linger phase. Detection via OVRManager.InputFocusLost/Acquired events.
- UX-DR26: Seated mode adaptation — auto-detect via tracked head height (<1.2m from guardian floor threshold), apply Y-axis offset to entire environment parent, maintain correct spatial relationships (podium, audience eye level, laptop tilt). Manual toggle on landing page as fallback.
- UX-DR27: Copy voice consistency — "supportive friend" tone throughout. First person plural for shared states ("we're offline"), second person for achievements ("you showed up"). Contractions always. Sentence case. No technical terms visible to user ("link" not "URL" in error messages).
- UX-DR28: URL validation — auto-prepend https:// if no protocol, reject non-HTTP protocols (javascript:, file:, ftp:, data:) silently by prepending https://, reject strings with no dot after domain. Configurable domain allowlist array for potential Quest Store compliance (not enforced in MVP).
- UX-DR29: QR code scanning flow — "Scan QR Code" SecondaryButton on landing page, passthrough camera activation, visual scan region indicator, haptic confirmation on detection, URL auto-populates and loads automatically, 15-second timeout with gentle prompt, Cancel button. Phase 0 spike dependency for passthrough camera reliability.
- UX-DR30: Button hierarchy — one primary action per screen state. Primary actions at bottom of laptop screen. Pause menu exception: all SecondaryButtons (equal weight, Resume at top position).
- UX-DR31: WebView slide navigation via keyboard event dispatch — Right Arrow key on trigger pull, Left Arrow key on B press. Keyboard input forwarding, not DOM manipulation. Universal across Google Slides, Canva, PowerPoint Online.
- UX-DR32: Accessibility — WCAG AA contrast target (4.5:1) for all text, color independence (no info by color alone), no rapid flashing, one-handed operation (right controller primary), minimal physical movement, UI elements in center of field of view, 72fps as accessibility requirement

### FR Coverage Map

| FR | Epic | Description |
|----|------|-------------|
| FR1 | Epic 2 | Slide loading instructions on landing page |
| FR2 | Epic 2 | URL text input in lobby |
| FR3 | Epic 5 | QR code scanning for URL input |
| FR4 | Epic 2 | Slide preview in lobby before rehearsal |
| FR5 | Epic 2 | Browser interaction via VR controller |
| FR6 | Epic 2 | Browser navigation (back, forward, refresh) |
| FR7 | Epic 3 | Lobby-to-stage transition |
| FR8 | Epic 3 | Virtual stage in conference room |
| FR9 | Epic 3 | 50 static audience attendees |
| FR10 | Epic 3 | Slides on projector screen during rehearsal |
| FR11 | Epic 3 | 360° head tracking |
| FR12 | Epic 3 | Fade-to-black scene transition |
| FR13 | Epic 3 | Advance slides forward via controller |
| FR14 | Epic 3 | Go back to previous slide via controller |
| FR15 | Epic 3 | End rehearsal session via controller |
| FR16 | Epic 4 | Positive reinforcement on session end |
| FR17 | Epic 4 | Choose to rehearse again or return to lobby |
| FR18 | Epic 2 | Landing page instructs public link usage |
| FR19 | Epic 2 | Error recovery without app restart |
| FR20 | Epic 2 | No internet notification |
| FR21 | Epic 2 | No account required |
| FR22 | Epic 2 | No user data retained between sessions |
| FR23 | Epic 2 | Multi-user device compatibility |

## Epic List

### Epic 1: Project Foundation & Slide Delivery Viability Gate
The creator validates that the core technology is viable — published Google Slides images are fetched and displayed as textures inside Unity on Quest 3 at 72fps with slide-advance via texture swap. This is the go/no-go decision for the entire project. Includes Unity project initialization, core state machine backbone, slide delivery interface contract and image-fetching implementation, and spike testing on Quest 3 hardware. The `IWebViewController` abstraction is preserved for a future post-MVP browser upgrade path.
**FRs covered:** None directly (foundational infrastructure). Enables all subsequent FRs.
**Additional Requirements covered:** Project initialization, platform configuration, package installation, GameStateManager + state classes, IWebViewController interface + SlideImageController, Input Action Asset, assembly definitions, project directory structure, spike acceptance criteria testing (NFR1, NFR2, NFR14 validated during spike).

### Epic 2: The Lobby Experience — Setup Your Stage
User opens StageMind, stands in a warm conference room at a podium, reads clear instructions, enters their slide URL, and sees slides load on the laptop screen and projector. When something goes wrong (bad link, no internet, login wall), they get warm, human guidance and can recover without restarting. The app requires no account and retains no data.
**FRs covered:** FR1, FR2, FR4, FR18, FR19, FR20, FR21, FR22, FR23 *(FR5, FR6 deferred to post-MVP — no embedded browser in MVP)*
**Additional Requirements covered:** Design system foundation (UX-DR1–3), all UI prefabs (UX-DR4–10), shared systems (UX-DR11–13), landing page composition (UX-DR14), lobby 3D environment (UX-DR21), podium and confidence monitor setup (UX-DR22), projector screen lobby state (UX-DR23), error state compositions (UX-DR18), URL validation (UX-DR28), copy voice (UX-DR27), room tone audio (UX-DR19 partial), ErrorHandler, stateless/cleanup enforcement.

### Epic 3: The Rehearsal Experience — Stand and Deliver
User presses "Start Rehearsal" and 50 audience members appear instantly. They advance through their slides with the trigger, glance down at the confidence monitor, and can pause anytime via the menu button. The experience is fully immersive — no UI, no feedback, just the stage.
**FRs covered:** FR7, FR8, FR9, FR10, FR11, FR12, FR13, FR14, FR15
**Additional Requirements covered:** Audience system with GPU instancing and 2 LOD levels (UX-DR20), slide control via texture index swap (UX-DR31 adapted), scene transition fade-to-black, pause menu (UX-DR15), haptic feedback patterns (UX-DR24), audience ambient audio (UX-DR19 partial), "Last slide" StatusIndicator (UX-DR10), button hierarchy for pause menu (UX-DR30), input context switching per state.

### Epic 4: The Reinforcement Moment — Feel the Warmth
After ending a session, the user experiences warm amber lighting, an affirming message, and a deliberate moment of stillness. Then they choose to rehearse again or finish for the day. Every completed run-through ends on a positive note.
**FRs covered:** FR16, FR17
**Additional Requirements covered:** Three-phase reinforcement sequence (UX-DR16), 15-message pool with random-without-replacement (UX-DR17), URP post-processing Volume with Color Adjustments, audience fade-out animation, warm-toned particle effect, reinforcement tone audio (UX-DR19 partial), ReinforcementMessages ScriptableObject, "Go Again" / "Done for Today" flow, input disabling during reinforcement, state transitions.

### Epic 5: QR Code Scanning — Skip the Keyboard (Conditional)
User taps "Scan QR Code," sees their real room through passthrough cameras, holds up their phone with a QR code, and the URL auto-populates and loads — no typing required. Time-to-stage drops dramatically.
**FRs covered:** FR3
**Additional Requirements covered:** QRScannerManager with passthrough cameras + ZXing.Net (UX-DR29), scan region indicator, haptic confirmation, 15-second timeout, cancel flow. Conditional on Phase 0 spike confirming passthrough camera QR reliability.

### Epic 6: Launch Readiness — Polish, Accessibility & Quest Store
The app is rock-solid for 30-minute sessions, works seated or standing, handles headset removal gracefully, and is available for purchase on the Meta Quest Store.
**FRs covered:** Cross-cutting NFRs (NFR6, NFR7, NFR9, NFR17, NFR19, NFR20)
**Additional Requirements covered:** Seated mode adaptation (UX-DR26), headset removal handling (UX-DR25), accessibility verification (UX-DR32), performance profiling and thermal testing, memory stability validation, Quest Store submission package (privacy policy, data disclosure, content rating), APK size optimization.

---

## Epic 1: Project Foundation & Slide Delivery Viability Gate

The creator validates that the core technology is viable — published Google Slides images are fetched and displayed as textures inside Unity on Quest 3 at 72fps with slide-advance via texture swap. This is the go/no-go decision for the entire project. Includes Unity project initialization, core state machine backbone, slide delivery interface contract and image-fetching implementation, and spike testing on Quest 3 hardware. The `IWebViewController` abstraction is preserved for a future post-MVP browser upgrade path.

### Story 1.1: Unity Project Initialization & Platform Configuration

As a developer,
I want a fully configured Unity project targeting Quest 3 with all required packages and project structure in place,
So that I have a clean, correct foundation for building StageMind without configuration issues later.

**Acceptance Criteria:**

**Given** Unity Hub is installed with Unity 6.3 LTS (Android Build Support, OpenJDK, Android SDK & NDK Tools)
**When** a new project is created from the Universal 3D (URP) template named "StageMind"
**Then** the project opens in Unity Editor without errors

**Given** the new Unity project is open
**When** build platform is switched to Meta Quest via File → Build Profiles
**Then** Player Settings are configured: IL2CPP scripting backend, ARM64 target architecture, Vulkan graphics API, Minimum API Level 29, correct Company Name and Product Name

**Given** the project is platform-configured
**When** required packages are installed via Package Manager
**Then** the following packages are present and resolve without errors: OpenXR (≥1.15.1), Meta XR OpenXR (≥2.2), TextMeshPro, Unity Input System
**And** Meta XR Simulator is installed from the Unity Asset Store

**Given** all packages are installed
**When** the project folder structure is created under Assets/_Project/
**Then** the following directories exist: Scripts/Core/, Scripts/WebView/, Scripts/Input/, Scripts/UI/, Scripts/Audio/, Scripts/Environment/, Scripts/Reinforcement/, Scripts/Platform/, Prefabs/UI/, Prefabs/Environment/, Prefabs/Audio/, ScriptableObjects/, Materials/, Textures/, Audio/, Fonts/, Scenes/, Art/, InputActions/, Settings/URP/, Settings/XR/

**Given** the folder structure is in place
**When** assembly definitions are created
**Then** three .asmdef files exist: StageMind (Scripts/), StageMind.Tests.EditMode (Tests/EditMode/), StageMind.Tests.PlayMode (Tests/PlayMode/) with correct references

**Given** the project is fully configured
**When** a .gitignore appropriate for Unity is added to the project root
**Then** Library/, Temp/, Logs/, obj/, and Builds/ directories are excluded from version control

### Story 1.2: Core State Machine & Game State Manager

As a developer,
I want a working state machine that manages app states and notifies registered systems of state changes,
So that all StageMind systems can coordinate behavior through a single, testable backbone.

**Acceptance Criteria:**

**Given** the project from Story 1.1 is set up
**When** the core interfaces are implemented
**Then** IGameState exists with Enter(), Exit(), Update(), and HandleInput() methods
**And** IStateAware exists with OnStateEnter(GameStateType) and OnStateExit(GameStateType) methods
**And** GameStateType enum defines: LobbyLanding, LobbySlidesLoaded, Rehearsal, Reinforcement, Paused

**Given** the interfaces exist
**When** GameStateManager MonoBehaviour is implemented
**Then** it holds a reference to the current IGameState, delegates Update() calls to the current state, and dispatches OnStateChanged(GameStateType previous, GameStateType current) events to all registered IStateAware systems

**Given** GameStateManager exists
**When** all 5 state classes are implemented as pure C# (not MonoBehaviour)
**Then** LobbyLandingState, LobbySlidesLoadedState, RehearsalState, ReinforcementState, and PausedState each implement IGameState with Enter() and Exit() methods that log their activation
**And** state classes receive dependencies via constructor parameters (not FindObjectOfType or singletons)

**Given** the state classes exist
**When** TransitionTo() is called on GameStateManager
**Then** the current state's Exit() is called, the new state's Enter() is called, and OnStateChanged fires with both previous and current state types
**And** the transition map matches the Architecture spec: LobbyLanding→LobbySlidesLoaded, LobbySlidesLoaded→Rehearsal, Rehearsal→Paused, Paused→Rehearsal, Paused→Reinforcement, Paused→LobbyLanding, Reinforcement→Rehearsal, Reinforcement→LobbyLanding, LobbySlidesLoaded→LobbyLanding

**Given** the state machine is complete
**When** Edit Mode tests are run
**Then** GameStateManagerTests verify: correct initialization to LobbyLanding, valid transitions succeed with correct event dispatch, invalid transitions are rejected, all 5 state classes have tests for Enter() and Exit() behavior
**And** MockStateAware is implemented in Tests/EditMode/Mocks/ for verifying event dispatch

### Story 1.3: Slide Delivery Interface Contract & Image Fetch Implementation

As a developer,
I want a slide delivery abstraction that fetches slide images and renders them to a RenderTexture with programmatic slide navigation,
So that the app can display slides from Google Slides published links without coupling to a specific delivery mechanism, and the interface supports swapping in a browser-based implementation post-MVP.

**Acceptance Criteria:**

**Given** the core state machine from Story 1.2 is in place
**When** the IWebViewController interface is defined in Scripts/Core/
**Then** it includes: Initialize(RenderTexture), LoadUrl(string), SendKeyEvent(KeyCode), Cleanup() methods
**And** events: OnLoadSuccess(string), OnLoadError(WebViewError), OnCrash
**And** properties: IsLoading, IsReady

**Given** the interface is defined
**When** WebViewError enum is created in Scripts/WebView/
**Then** it includes: NetworkFailure, LoginWallDetected, PageLoadTimeout, Unknown

**Given** the interface and enum exist
**When** SlideImageController is implemented in Scripts/WebView/
**Then** it implements IWebViewController by fetching slide images from Google Slides published URLs via UnityWebRequest
**And** LoadUrl() parses the Google Slides presentation ID, fetches all slide images as Texture2D objects, and fires OnLoadSuccess when complete
**And** SendKeyEvent(RightArrow) increments the slide index and updates the RenderTexture via Graphics.Blit(); SendKeyEvent(LeftArrow) decrements
**And** a shared RenderTexture is updated only when the slide index changes (not per-frame)

**Given** the SlideImageController is implemented
**When** Cleanup() is called
**Then** all cached Texture2D objects are destroyed and slide index is reset (enforcing NFR18)

**Given** the SlideImageController is implemented
**When** a URL fails to load (HTTP error, DNS failure, or timeout)
**Then** appropriate OnLoadError events fire with correct error types (NetworkFailure for HTTP errors, PageLoadTimeout for timeout)
**And** if the fetched response is HTML instead of image data (non-published URL), OnLoadError fires with LoginWallDetected

**Given** the slide delivery integration is complete
**When** UrlValidator is implemented in Scripts/WebView/
**Then** it auto-prepends https:// if no protocol is present, rejects non-HTTP protocols (javascript:, file:, ftp:, data:), and rejects strings with no dot after the domain

**Given** all slide delivery code is written
**When** Edit Mode tests are run
**Then** MockWebViewController in Tests/EditMode/Mocks/ verifies the interface contract
**And** UrlValidatorTests cover: valid URLs pass, missing protocol gets https:// prepended, non-HTTP protocols are rejected, no-dot strings are rejected

### Story 1.4: Input System Foundation

As a developer,
I want a controller input system that routes actions to the current game state with haptic feedback,
So that slide advancement, navigation, and pause controls work correctly in each app state.

**Acceptance Criteria:**

**Given** the state machine from Story 1.2 is in place
**When** the StageMindActions Input Action Asset is created in InputActions/
**Then** it defines: AdvanceSlide (bound to right trigger), PreviousSlide (bound to right B button), PauseMenu (bound to left menu button), UIPoint (bound to right controller position/rotation), UIClick (bound to right trigger)
**And** "Generate C# Class" is enabled, producing StageMindActions.cs

**Given** the Input Action Asset exists
**When** InputRouter MonoBehaviour is implemented in Scripts/Input/
**Then** it subscribes to Input Action callbacks and routes them through GameStateManager to the current state's HandleInput() method
**And** during ReinforcementState, AdvanceSlide and PauseMenu actions are disabled (inputs blocked per UX spec)

**Given** the InputRouter is implemented
**When** HapticFeedback is implemented in Scripts/Input/
**Then** it dispatches subtle haptic clicks via OpenXR haptic API on trigger pull, B press, and menu press
**And** no haptic fires on non-events (trigger on last slide, B on first slide)

**Given** the input system is complete
**When** Edit Mode tests are run
**Then** InputRouter routing logic is verified: actions dispatch to the correct state handler based on current GameStateType
**And** input blocking during ReinforcementState is verified

### Story 1.5: Slide Image Fetch Spike — Quest 3 Hardware Validation

As a creator,
I want to confirm that slide images fetched from Google Slides render reliably as textures in VR on Quest 3 hardware at acceptable performance levels,
So that I have confidence the product is technically viable before investing in further development.

**Acceptance Criteria:**

**Given** a Unity build with SlideImageController and a test scene containing a quad with slide images rendered to a RenderTexture is deployed to Quest 3
**When** a published Google Slides deck of 30+ slides is loaded via LoadUrl()
**Then** all slide images are fetched and the first slide renders as a texture on the quad without corruption or missing content

**Given** the slides are loaded
**When** SendKeyEvent(RightArrow) is called to advance the slide index and swap the displayed texture
**Then** the perceived latency from key dispatch to visual update on the RenderTexture is less than 50ms (texture swap)

**Given** slide textures are loaded and displayed
**When** frame rate is measured via Unity Profiler or OVR Metrics Tool
**Then** sustained frame rate remains at or above 72fps with no frame drops below 72fps during slide display or advancement

**Given** the spike test scene is running
**When** Google Slides published URLs with various deck sizes (10, 30, 60 slides) are tested
**Then** all decks fetch reliably and display without missing slides. Canva and PowerPoint Online deferred to post-MVP.

**Given** the spike scene is running continuously
**When** a 30-minute session is completed with periodic slide advancement
**Then** no crashes or freezes occur
**And** memory usage remains stable (no growth pattern indicating leaks from cached textures)

**Given** QR code scanning feasibility is being evaluated
**When** Quest 3 passthrough cameras are activated and a QR code is displayed at arm's length (~0.5m)
**Then** the result is documented: reliable scanning confirms FR3 for MVP, unreliable scanning defers FR3 to post-MVP

**Given** all spike criteria have been tested
**When** results are compiled
**Then** a go/no-go decision is documented: pass on all 5 slide fetch criteria → proceed to Phase 1; fail on any criterion → evaluate alternative export mechanisms (Google Slides API, server-side rendering)

---

## Epic 2: The Lobby Experience — Setup Your Stage

User opens StageMind, stands in a warm conference room at a podium, reads clear instructions, enters their slide URL, and sees slides load on the laptop screen and projector. When something goes wrong (bad link, no internet, login wall), they get warm, human guidance and can recover without restarting. The app requires no account and retains no data.

### Story 2.1: Design System Foundation — Color, Typography & Spacing Tokens

As a developer,
I want a centralized design token system that defines all colors, typography, and spacing values as ScriptableObject assets,
So that every UI element in the app draws from a single source of truth and visual consistency is guaranteed.

**Acceptance Criteria:**

**Given** the project structure from Epic 1 is in place
**When** ColorPalette ScriptableObject class is created in Scripts/Core/
**Then** it defines 12 public Color fields: Surface (#F5F0E8), SurfaceAlt (#EDE6D6), TextPrimary (#3A3632), TextSecondary (#7A7570), PrimaryAction (#D4A43A), PrimaryHover (#E8B84B), PrimaryPressed (#B8902E), SecondaryBorder (#A09A94), Error (#C47A5A), Success (#8BA888), ReinforcementGlow (#E8A830), Disabled (#B8B2AA)
**And** a ColorPalette.asset instance is created in ScriptableObjects/ with all hex values set

**Given** the ColorPalette asset exists
**When** AppConfig ScriptableObject class is created in Scripts/Core/
**Then** it includes spacing tokens (xs: 0.005f, sm: 0.01f, md: 0.02f, lg: 0.03f, xl: 0.05f), timing values (reinforcement linger: 5f, fade speeds, WebView timeout: 15f), spatial dimensions (stage 4m×3m, audience depth 8-10m, ceiling 3-4m), audience count (50), and haptic intensity values
**And** an AppConfig.asset instance is created in ScriptableObjects/ with all values set

**Given** color and config assets exist
**When** Inter font SDF assets are created
**Then** Inter-Regular SDF.asset (weight 400) and Inter-SemiBold SDF.asset (weight 500) exist in Fonts/
**And** both use TextMeshPro's SDF rendering for crisp VR text at any viewing angle

### Story 2.2: UI Interaction System — Pointer, Buttons & Animator

As a developer,
I want a controller-based UI interaction system that handles ray casting, button states, and UI animations,
So that all world-space UI elements respond consistently to controller input with proper visual and haptic feedback.

**Acceptance Criteria:**

**Given** the design tokens from Story 2.1 and input system from Story 1.4 are in place
**When** UIPointerController MonoBehaviour is implemented in Scripts/UI/
**Then** it casts a ray from the right controller's position/rotation, tests intersection against world-space UI Canvases on a dedicated UI layer, tracks the currently hovered element, and renders a pointer visual (line renderer or dot indicator)

**Given** UIPointerController exists
**When** VRButtonInteraction MonoBehaviour is implemented in Scripts/UI/
**Then** it handles 5 states for any button it's attached to: Default, Hovered (brightness increase on ray intersection), Pressed (color darken + visual depress on trigger pull), Released (brief flash/bounce-back confirming action), Disabled (dimmed, no hover response)
**And** haptic feedback dispatches a subtle click on press via HapticFeedback from Story 1.4
**And** the script works identically for both PrimaryButton and SecondaryButton prefabs

**Given** VRButtonInteraction exists
**When** UIAnimator MonoBehaviour is implemented in Scripts/UI/
**Then** it provides reusable methods for: fade (alpha over time), color lerp (between two colors over duration), and scale pulse (brief scale increase and return)
**And** all methods accept configurable duration and use smooth easing
**And** coroutine references are stored and stopped on OnDisable() or state exit

### Story 2.3: UI Prefab Library — Input, Cards & Indicators

As a developer,
I want a complete set of UI prefabs built from the design tokens,
So that all screen compositions can be assembled from consistent, reusable atomic components.

**Acceptance Criteria:**

**Given** the design tokens from Story 2.1 and interaction system from Story 2.2 are in place
**When** TextBlock prefab is created in Prefabs/UI/
**Then** it uses a TextMeshPro text component with Inter font SDF and a TextBlockSize enum selector in the Inspector choosing between: Display (~0.04m), Heading (~0.025m), Body (~0.018m), Label (~0.016m), Caption (~0.012m) world height
**And** text color defaults to TextPrimary (#3A3632) from ColorPalette

**Given** TextBlock exists
**When** PrimaryButton prefab is created in Prefabs/UI/
**Then** it has a rounded rectangle background with PrimaryAction amber fill (#D4A43A), a TextBlock child for the label (Inter Medium 500), minimum world-space hit target of 0.08m × 0.04m, and VRButtonInteraction attached with state colors configured (Default #D4A43A, Hovered #E8B84B, Pressed #B8902E, Disabled from ColorPalette)

**Given** TextBlock exists
**When** SecondaryButton prefab is created in Prefabs/UI/
**Then** it has a transparent background with warm gray border (#A09A94), a TextBlock child for the label (Inter Medium 500), VRButtonInteraction attached with appropriate state visuals (border brightens on hover, darkens on press)

**Given** button prefabs exist
**When** TextInputField prefab is created in Prefabs/UI/
**Then** it displays placeholder text "Paste your slide link here..." in TextSecondary color, highlights its border on focus, triggers Quest TouchScreenKeyboard via the TouchScreenKeyboard API when tapped, and displays user-entered text replacing the placeholder
**And** submission is supported via both the keyboard Done/Enter key and an adjacent "Go" PrimaryButton

**Given** TextBlock and button prefabs exist
**When** Card prefab is created in Prefabs/UI/
**Then** it has a SurfaceAlt (#EDE6D6) background, configurable accent color strip (defaults to none), md (0.02m) padding, subtle corner radius, and accepts child elements (TextBlock, buttons) via Unity layout groups

**Given** UIAnimator exists
**When** LoadingIndicator prefab is created in Prefabs/UI/
**Then** it displays an animated spinner or horizontal bar in Success sage green (#8BA888) and fades out on completion via UIAnimator

**Given** UIAnimator exists
**When** StatusIndicator prefab is created in Prefabs/UI/
**Then** it displays a transient text label that fades in (~0.3s), holds (~2s), and fades out (~0.5s) via UIAnimator, and is non-interactive

### Story 2.4: Lobby 3D Environment — The Empty Conference Room

As a user,
I want to stand in a warm, realistic conference room when I open StageMind,
So that the space feels welcoming and familiar — like arriving early to set up before a talk.

**Acceptance Criteria:**

**Given** the project is configured for Quest 3 VR
**When** the conference room environment is built in the StageMind.unity scene
**Then** it includes: warm cream walls (#F5F0E8–#EDE6D6, matte), light wood or warm-toned matte stage floor, warm charcoal carpet/floor accent (#3A3632–#4A4540), ceiling panels (#F8F4EE), and the overall space matches a mid-size conference room (stage area ~4m×3m, audience seating depth 8-10m, ceiling height 3-4m per NFR13)

**Given** the room geometry exists
**When** the podium is placed at the speaker position
**Then** it uses light oak/birch wood material (#C8A96E–#D4B87A) with visible natural grain, has a laptop on top tilted 20-30° above flat for comfortable VR downward glance, and the laptop screen has a world-space Canvas mapped to its surface

**Given** the podium and laptop exist
**When** the projector screen is placed behind/beside the stage
**Then** it displays a static warm-toned texture with centered "StageMind" wordmark in warm dark gray — not mirroring the laptop UI
**And** the screen uses a standard conference projector screen frame

**Given** all geometry and materials are in place
**When** lighting is baked
**Then** the room uses baked lightmaps with ~4000K warm white ambient light, creating a "golden hour indoors" feel — bright but not harsh, warm but not orange
**And** no real-time lights are used (performance requirement)

**Given** the environment is complete
**When** the user spawns into the scene in VR
**Then** they are positioned at the podium facing the empty seats, with the laptop in front (on the podium) and the projector screen behind them
**And** the room tone audio loop (30-60s seamless HVAC/building hum) plays at quiet ambient volume via a spatial audio source

**Given** audience chairs are part of the environment
**When** the lobby state is active
**Then** 50 audience chairs are visible but audience characters are hidden (SetActive false on AudienceRoot)

### Story 2.5: Landing Page & URL Input — Load Your Slides

As a user,
I want to see clear instructions on the laptop screen and easily enter my slide URL,
So that I can load my presentation and start rehearsing within minutes.

**Acceptance Criteria:**

**Given** the lobby environment from Story 2.4 is set up and the UI prefabs from Story 2.3 exist
**When** the landing page Canvas composition is created on the laptop screen
**Then** it follows the inverted visual hierarchy (top to bottom): TextBlock heading with warm instructional text ("Share your slides as a public link, then paste the link below"), TextBlock body with brief how-to, TextInputField with "Paste your slide link here..." placeholder, adjacent "Go" PrimaryButton, "Scan QR Code" SecondaryButton (visible but disabled with "(coming soon)" label if QR not yet implemented), controller diagram as 3 TextBlock labels ("Right trigger: Next slide / Right B: Previous / Left Menu: Pause"), "Start Rehearsal" PrimaryButton (disabled until slides load), and "Quit StageMind" SecondaryButton (de-emphasized at bottom)

**Given** the landing page is displayed
**When** the user points at the TextInputField and pulls the trigger
**Then** the Quest TouchScreenKeyboard opens, the field border highlights, and the user can type or paste a URL
**And** copy-paste from Quest's system clipboard works

**Given** a URL is entered in the input field
**When** the user presses "Go" or the keyboard Done/Enter key
**Then** UrlValidator checks the input (auto-prepends https:// if needed, rejects non-HTTP protocols and no-dot strings)
**And** if validation passes, LoadingIndicator appears and SlideImageController.LoadUrl() is called to fetch slide images
**And** if validation fails, an inline error message appears: "That doesn't look like a link. Try pasting the full URL from your browser."

**Given** the landing page is displayed
**When** the user points at "Quit StageMind" and pulls the trigger
**Then** Application.Quit() is called immediately with no confirmation dialog

**Given** the URL input and landing page are functional
**When** the experience is tested end-to-end
**Then** all copy voice follows "supportive friend" tone — contractions, sentence case, no technical terms visible to the user ("link" not "URL" in user-facing text)
**And** FR1, FR2, FR18 are satisfied *(FR5 deferred — no embedded browser interaction in MVP)*

### Story 2.6: Slide Preview & Lobby-to-Loaded Transition

As a user,
I want to see my slides appear on the laptop and projector screen after loading,
So that I can verify my presentation looks correct before starting rehearsal.

**Acceptance Criteria:**

**Given** a URL has been submitted and SlideImageController.LoadUrl() is fetching slide images
**When** the controller reports OnLoadSuccess
**Then** the first slide image appears on the laptop screen (Texture2D rendered to RenderTexture mapped to laptop Canvas)
**And** the projector screen material swaps from the static StageMind wordmark texture to the shared RenderTexture, now mirroring the laptop content at larger scale
**And** the LoadingIndicator fades out via UIAnimator
**And** a slide counter ("1 / N") is displayed on the laptop screen

**Given** slides are loaded on the laptop screen
**When** the user interacts with the laptop via controller pointing
**Then** they can tap left/right arrows on the laptop UI to preview different slides
**And** the projector screen mirrors the same slide passively (shared RenderTexture updates via Graphics.Blit)

**Given** slides are successfully loaded
**When** the lobby state transitions to LobbySlidesLoaded
**Then** the "Start Rehearsal" PrimaryButton changes from disabled to enabled (amber fill, interactive)
**And** FR4 is satisfied *(FR6 deferred — no embedded browser navigation in MVP)*

**Given** the user is in LobbySlidesLoaded state
**When** they enter a new URL in the TextInputField
**Then** the state transitions back to LobbyLanding, the new URL loads, and on success returns to LobbySlidesLoaded with the new slides displayed

### Story 2.7: Error Handling & Stateless Design

As a user,
I want warm, helpful guidance when something goes wrong with my slides,
So that I can fix the issue and get back to rehearsing without frustration or confusion.

**Acceptance Criteria:**

**Given** the ErrorHandler MonoBehaviour is implemented in Scripts/Core/Errors/
**When** any system encounters an error
**Then** it creates an ErrorEvent (Type, UserMessage, GuidanceMessage, Recovery) and sends it to ErrorHandler
**And** ErrorHandler renders the error as a Card composition (terracotta accent) on the laptop screen with warm, human copy

**Given** a URL fails to load (HTTP 4xx/5xx, DNS failure, or timeout after 15 seconds)
**When** SlideImageController reports OnLoadError with NetworkFailure
**Then** an error Card appears on the laptop: "Hmm, that link didn't load." with guidance "Make sure your slides are shared as a public link."
**And** the entered URL persists in the input field for editing
**And** FR19 is satisfied

**Given** the fetched response is HTML instead of image data (non-published / private URL)
**When** SlideImageController reports OnLoadError with LoginWallDetected
**Then** an error Card appears: "Looks like these slides aren't published yet. Try sharing your slides as a public link instead." with how-to steps visible

**Given** the device has no internet connection
**When** Application.internetReachability returns NotReachable before a slide fetch attempt
**Then** an error Card appears: "We're offline right now. You'll need internet to load your slides." with a "Try Again" PrimaryButton
**And** FR20 is satisfied

**Given** the slide fetch encounters repeated failures
**When** a retry is attempted once automatically and also fails within 10 seconds
**Then** an error Card appears: "Something went wrong loading your slides. Try closing and reopening StageMind." with no further retry button (dead-end directing to app restart)

**Given** any error is displayed
**When** the user fixes the issue and retries
**Then** recovery always loops back to URL input — no dead ends (except double-failure), no app restart required

**Given** the app is running
**When** no user account, login, or registration is required at any point
**Then** FR21 is satisfied
**And** no authentication infrastructure exists in the codebase

**Given** the app is closed or a new session begins
**When** SlideImageController.Cleanup() is called on app exit (OnApplicationQuit)
**Then** all cached slide textures are destroyed and slide index is reset
**And** no user-specific data persists between sessions (no PlayerPrefs, no local files, no cached URLs)
**And** FR22, FR23 are satisfied — multiple users on the same device see no trace of previous sessions

---

## Epic 3: The Rehearsal Experience — Stand and Deliver

User presses "Start Rehearsal" and 50 audience members appear instantly. They advance through their slides with the trigger, glance down at the confidence monitor, and can pause anytime via the menu button. The experience is fully immersive — no UI, no feedback, just the stage.

### Story 3.1: Audience System — 50 Characters with GPU Instancing

As a developer,
I want 50 audience characters pre-placed in the scene with GPU instancing and LOD levels,
So that the audience can be toggled on/off instantly while maintaining 72fps on Quest 3.

**Acceptance Criteria:**

**Given** the conference room environment from Story 2.4 exists
**When** 50 audience character GameObjects are placed in the scene under an AudienceRoot parent
**Then** characters are seated in rows facing the stage at plausible conference room spacing within the audience seating depth (8-10m)
**And** each character uses a low-poly mesh (1,500-3,000 triangles) in a static seated pose

**Given** all 50 characters are placed
**When** GPU instancing is configured
**Then** all characters share a single mesh and a single material with a shared texture atlas (2048×2048)
**And** 8-12 unique appearances are achieved via atlas UV offsets for hair, clothing color, and skin tone variation
**And** total draw calls for all 50 audience members combined are ≤20

**Given** GPU instancing is configured
**When** LOD levels are set up
**Then** rows 1-3 (within 6m of the speaker) use the full-detail mesh (LOD0)
**And** rows 4+ use a simplified mesh at ~50% triangle count (LOD1)

**Given** characters are placed and instanced
**When** baked ambient occlusion is applied beneath the chairs
**Then** characters appear grounded in the space without any real-time shadows

**Given** AudienceRoot contains all 50 characters
**When** AudienceController MonoBehaviour toggles AudienceRoot.SetActive(false)
**Then** all 50 characters disappear in a single frame with zero render cost while disabled
**And** AudienceRoot.SetActive(true) makes them all appear instantly (one frame — no fade-in, no animation)

**Given** the audience is visible
**When** frame rate is measured on Quest 3 with the audience active
**Then** sustained 72fps is maintained with headroom for WebView rendering (total audience triangles 75K-150K)

### Story 3.2: Start Rehearsal — Audience Appears & Scene Transition

As a user,
I want the audience to appear the instant I press "Start Rehearsal,"
So that the spatial pressure of facing a room full of people kicks in immediately — this is the product delivering its core promise.

**Acceptance Criteria:**

**Given** the user is in LobbySlidesLoaded state with slides loaded on the laptop
**When** the user points at "Start Rehearsal" PrimaryButton and pulls the trigger
**Then** a haptic click fires and the button shows its pressed/released animation

**Given** "Start Rehearsal" is pressed
**When** the state transitions from LobbySlidesLoaded to Rehearsal
**Then** a fade-to-black transition occurs (SceneTransitionController using FadeOverlay.mat) completing in under 3 seconds with no frame drops below 72fps (NFR3, NFR11)

**Given** the fade-to-black completes
**When** the scene emerges from black
**Then** AudienceController sets AudienceRoot active — 50 audience members appear immediately facing the speaker
**And** the audience is visible in the user's forward field of view for maximum heart-spike impact
**And** slide advance input is disabled until the transition fully completes

**Given** the audience is visible
**When** the rehearsal state is fully entered
**Then** the laptop screen switches from the landing page Canvas to confidence monitor mode — displaying the current slide via the shared RenderTexture (non-interactive)
**And** the projector screen continues showing slides to the audience via the same RenderTexture
**And** the controller diagram and all lobby UI elements are hidden
**And** FR7, FR8, FR9, FR10, FR11, FR12 are satisfied

**Given** the user is in rehearsal
**When** they look around freely
**Then** full 360° head tracking works via OpenXR with stable, grounded viewpoint — no artificial locomotion, no camera shake (NFR12)

### Story 3.3: Slide Control & Rehearsal Immersion

As a user,
I want to advance and reverse my slides with simple controller inputs during rehearsal,
So that I can run through my talk naturally — just like clicking a physical presentation remote.

**Acceptance Criteria:**

**Given** the user is in Rehearsal state with slides displayed
**When** the user pulls the right trigger
**Then** InputRouter dispatches to RehearsalState which calls SlideImageController.SendKeyEvent(RightArrow), incrementing the slide index
**And** the next slide texture is blitted to the shared RenderTexture, updating both projector screen and confidence monitor
**And** perceived latency from trigger pull to visual update is less than 50ms (texture swap — NFR2)
**And** a subtle haptic click fires on the right controller

**Given** the user is in Rehearsal state
**When** the user presses the right B button
**Then** SlideImageController.SendKeyEvent(LeftArrow) is dispatched, decrementing the slide index
**And** the previous slide texture is blitted to the shared RenderTexture, updating both screens
**And** a subtle haptic click fires

**Given** the user is on the last slide
**When** the user pulls the right trigger
**Then** nothing happens — no slide change, no error (slide index stays at max)
**And** a "Last slide" StatusIndicator appears at the bottom of the confidence monitor (laptop screen), fading in (~0.3s), holding (~2s), and fading out (~0.5s) via UIAnimator
**And** no haptic fires (non-event)

**Given** the user is on the first slide
**When** the user presses the right B button
**Then** nothing happens — silent non-event, no haptic, no indicator (slide index stays at 0)

**Given** the user is actively rehearsing
**When** no controller input occurs
**Then** zero UI is visible — no HUD, no floating panels, no feedback. Pure spatial immersion. The user is on a stage, not in an app.
**And** FR13, FR14 are satisfied

### Story 3.4: Pause Menu — The Escape Hatch

As a user,
I want to pause my rehearsal and choose to resume, end the session, or return to the lobby,
So that I have control over my rehearsal without accidental exits or disruptive interruptions.

**Acceptance Criteria:**

**Given** the user is in Rehearsal state
**When** the user presses the left Menu button
**Then** the state transitions to Paused
**And** a gaze-anchored world-space Canvas spawns at ~1.5m in the user's forward direction at the moment of press, then becomes world-locked (stays in place, not head-tracked)
**And** a haptic click fires on the left controller
**And** a semi-transparent warm overlay dims the scene behind the menu

**Given** the pause menu is displayed
**When** the user sees the menu options
**Then** three vertically stacked SecondaryButtons are shown with md (0.02m) spacing: "Resume" (top), "End Session" (middle), "Return to Lobby" (bottom)
**And** all three use SecondaryButton styling (equal visual weight — no primary button, per UX-DR30 pause menu exception)

**Given** the pause menu is displayed
**When** the user selects "Resume"
**Then** the pause menu disappears instantly, the state transitions back to Rehearsal, and the user continues at their current slide

**Given** the pause menu is displayed
**When** the user selects "End Session"
**Then** the pause menu closes and the state transitions to Reinforcement (handled in Epic 4)
**And** FR15 is satisfied

**Given** the pause menu is displayed
**When** the user selects "Return to Lobby"
**Then** the audience fades out over ~1s (AudienceController lerps shared material alpha then SetActive false)
**And** the state transitions to LobbyLanding
**And** the laptop returns to showing the landing page with slides still loaded (WebView state preserved)

**Given** no pause menu input method other than the Menu button exists
**When** the user is in rehearsal with no menu visible
**Then** there is no way to accidentally exit rehearsal — the pause menu is the sole escape hatch

### Story 3.5: Audience Ambient Audio — Spatial Presence

As a user,
I want to hear subtle audience sounds during rehearsal — an occasional cough, a chair shift, quiet rustling,
So that the audience feels spatially present, not just visually placed, deepening the rehearsal realism.

**Acceptance Criteria:**

**Given** the AudioManager and AudienceAudioController MonoBehaviours are implemented in Scripts/Audio/
**When** the Rehearsal state is entered
**Then** AudienceAudioController begins playing audience ambient one-shot clips at random intervals between 10-20 seconds

**Given** audience ambient clips are configured
**When** a clip plays
**Then** it is randomly selected from a pool of 6-8 variations (seat shift, throat clear, quiet cough, paper rustle)
**And** each clip plays from a randomized spatial position within the audience seating area (not always from the same spot)
**And** spatialization uses HRTF via Unity's spatial audio system — a cough from the left of the audience sounds like it comes from the left

**Given** audience audio is playing
**When** volume levels are set
**Then** all audience ambient sounds sit well below conversation volume — felt, not heard. If the user is speaking at normal presentation volume, they should barely notice the sounds.

**Given** the AudioConfig ScriptableObject is implemented in Scripts/Audio/
**When** audio parameters are configured
**Then** ambient volume levels, audience sound interval range (min: 10s, max: 20s), and reinforcement tone settings are stored in AudioConfig.asset in ScriptableObjects/

**Given** the user transitions from Rehearsal to any other state
**When** AudienceAudioController receives OnStateExit for Rehearsal
**Then** audience ambient sounds stop playing and any in-progress clip fades out gracefully

---

## Epic 4: The Reinforcement Moment — Feel the Warmth

After ending a session, the user experiences warm amber lighting, an affirming message, and a deliberate moment of stillness. Then they choose to rehearse again or finish for the day. Every completed run-through ends on a positive note — the emotional capstone of the product.

### Story 4.1: Reinforcement Sequence — Impact, Linger & Transition

As a user,
I want to experience a warm, affirming moment after finishing a rehearsal,
So that every session ends on a positive note and I associate the stage with accomplishment rather than anxiety.

**Acceptance Criteria:**

**Given** the user selects "End Session" from the pause menu
**When** the state transitions to Reinforcement
**Then** Phase 1 (Impact, 0-1s) begins immediately: stage lights shift to warm golden/amber via URP post-processing Volume (ColorAdjustments weight lerps from 0→1 over ~0.5s using color filter #E8A830), the audience fades out over ~1s (AudienceController lerps shared material alpha then SetActive false), warm-toned particles or light motes begin drifting upward with graceful slow-float physics
**And** an affirming message appears in Display typography (~4cm, Inter Medium 500, centered in visual field)
**And** right trigger and left menu button inputs are disabled — controller actions blocked to prevent accidental disruption of the emotional moment

**Given** Phase 1 has completed
**When** 1-5 seconds elapse
**Then** Phase 2 (Linger) is active: nothing changes. No animations starting, no buttons appearing, no visual movement. Complete stillness.
**And** this deliberate pause communicates "there's no rush — this moment is for you"

**Given** Phase 2 has completed (~5 seconds total since reinforcement began)
**When** the linger period ends
**Then** Phase 3 (Transition) begins: a "Continue" PrimaryButton gently fades in via UIAnimator — not urgent, not demanding attention
**And** if the user doesn't act, the warm space persists indefinitely with the message and "Continue" button visible

### Story 4.2: Message Pool & Selection System

As a user,
I want to see a different affirming message each time I finish a rehearsal,
So that the encouragement feels genuine and fresh rather than repetitive.

**Acceptance Criteria:**

**Given** the ReinforcementMessages ScriptableObject class is created in Scripts/Reinforcement/
**When** the asset is configured in ScriptableObjects/ReinforcementMessages.asset
**Then** it contains a pool of 15 messages:
1. "You showed up and ran through it. That counts."
2. "Another run in the books. You're more ready than you think."
3. "Practice doesn't have to be perfect. You showed up. That's what counts."
4. "That's another rehearsal done. The stage knows you now."
5. "Every time you stand here, the real stage feels a little more like home."
6. "You just did the thing most people only think about doing."
7. "Not bad for someone who was nervous a few minutes ago."
8. "The hardest part was starting. You already did that."
9. "One more run-through, and this stage is yours."
10. "You know this material. Now you know this room, too."
11. "Familiarity is a quiet kind of confidence."
12. "The audience didn't faze you as much that time. You noticed, right?"
13. "This is what preparation actually feels like."
14. "You're building a memory your body will remember on the real stage."
15. "Done. And you can always come back."

**Given** the message pool exists
**When** RandomWithoutRepeat utility in Scripts/Core/Utilities/ selects a message
**Then** messages are chosen randomly without replacement within a session — the user never sees the same message twice in consecutive run-throughs
**And** the pool resets across app launches (no persistence)

**Given** the RandomWithoutRepeat utility is implemented
**When** Edit Mode tests are run (RandomWithoutRepeatTests)
**Then** tests verify: all messages appear before any repeats, pool resets correctly, distribution is reasonably uniform, pool exhaustion and reset work for back-to-back sessions exceeding 15 runs

### Story 4.3: Post-Reinforcement Flow — Go Again or Done for Today

As a user,
I want to choose whether to rehearse again or finish after the reinforcement moment,
So that I can run through my talk multiple times without friction or wrap up when I'm ready.

**Acceptance Criteria:**

**Given** the user presses "Continue" during Phase 3 of the reinforcement sequence
**When** the continue action fires
**Then** two buttons appear: "Go Again" PrimaryButton and "Done for Today" SecondaryButton
**And** the affirming message remains visible above the buttons

**Given** the reinforcement tone audio asset exists (warm pad/chime, ~2s, gentle attack, slow decay)
**When** the reinforcement sequence begins (Phase 1 Impact)
**Then** the reinforcement tone plays once, accompanying the lighting shift
**And** no other audio plays during the reinforcement — room tone fades to silence over ~1s, audience sounds have already stopped

**Given** the choice buttons are displayed
**When** the user selects "Go Again"
**Then** the post-processing warm amber returns to normal, the state transitions to Rehearsal, the audience reappears (SetActive true), and the user resumes at their current slide position
**And** the user navigates back to slide 1 manually using B button if desired (mirroring real presenter behavior — no forced reset)
**And** FR16 is satisfied

**Given** the choice buttons are displayed
**When** the user selects "Done for Today"
**Then** the post-processing returns to lobby warmth, the state transitions to LobbyLanding, the user is back in the empty conference room
**And** the laptop shows the landing page with slides still loaded (WebView state preserved at current slide)
**And** the user can navigate slides, load a new URL, start a new rehearsal, or quit
**And** FR17 is satisfied

**Given** the "Done for Today" label is used
**When** the user reads it
**Then** the wording implies "you'll be back" — a small narrative promise embedded in a UI element, not "Quit" or "Exit"

---

## Epic 5: QR Code Scanning — Skip the Keyboard (Conditional)

User taps "Scan QR Code," sees their real room through passthrough cameras, holds up their phone with a QR code, and the URL auto-populates and loads — no typing required. Time-to-stage drops dramatically, especially on shared devices. Conditional on Phase 0 spike confirming passthrough camera QR code scanning reliability.

### Story 5.1: QR Scanner Integration — Passthrough Camera & Decoding

As a developer,
I want a QR code scanning system that uses Quest 3's passthrough cameras to decode URLs from QR codes,
So that the app can offer a frictionless alternative to VR keyboard text entry.

**Acceptance Criteria:**

**Given** the Phase 0 spike (Story 1.5) confirmed passthrough camera QR scanning is reliable
**When** QRScannerManager MonoBehaviour is implemented in Scripts/Platform/
**Then** it activates Quest 3's passthrough camera feed via the Meta Spatial SDK (OVRCameraRig passthrough layer)
**And** it captures frames from the passthrough camera and scans for QR codes using a decoding library (e.g., ZXing.Net for Unity)
**And** no internet connection is required for the scan itself — only for loading the decoded URL afterward

**Given** the QR scanner is active
**When** a QR code is detected in the camera feed
**Then** the encoded URL string is extracted and returned to the calling system
**And** a haptic confirmation fires on the right controller

**Given** the QR scanner is active
**When** a visual scan region indicator is displayed
**Then** the user sees a clear overlay indicating where to hold the QR code within the passthrough view

**Given** the QR scanner is active
**When** no QR code is detected within 15 seconds
**Then** a gentle prompt appears: "No code found. Hold the QR code closer or try typing the link instead."
**And** the prompt follows supportive friend copy voice — no technical language

**Given** the QR scanner is active
**When** the user wants to cancel
**Then** a "Cancel" button is visible at all times during scanning
**And** pressing Cancel closes passthrough mode and returns to the landing page with no side effects

**Given** the Phase 0 spike determined QR scanning is NOT reliable
**When** the landing page is displayed
**Then** the "Scan QR Code" SecondaryButton shows "(coming soon)" text and remains in Disabled state (dimmed, no hover response)

### Story 5.2: QR Code to Slide Loading — End-to-End Flow

As a user,
I want to scan a QR code from my phone and have my slides load automatically,
So that I can skip the VR keyboard entirely and get to rehearsing faster.

**Acceptance Criteria:**

**Given** the QR scanner from Story 5.1 is working and the landing page from Story 2.5 exists
**When** the user points at the "Scan QR Code" SecondaryButton on the landing page and pulls the trigger
**Then** passthrough mode activates — the user sees their real room through the headset
**And** the scan region indicator appears
**And** the landing page UI is hidden during scanning

**Given** passthrough is active and the user holds up a phone/laptop showing a QR code
**When** the QR code is successfully decoded
**Then** the extracted URL auto-populates the TextInputField on the landing page
**And** passthrough mode closes, returning the user to the VR lobby environment
**And** the URL loads automatically — same flow as manual "Go" button submission (UrlValidator → LoadingIndicator → WebView.LoadUrl())
**And** no additional user action is required between scan and slide loading

**Given** the QR code contains an invalid URL (no dot, non-HTTP protocol)
**When** the decoded string is passed through UrlValidator
**Then** normal validation applies — auto-prepend https:// if missing, reject invalid formats with the same warm error messages as manual entry

**Given** the full QR-to-slides flow is working
**When** tested end-to-end on Quest 3 hardware
**Then** the complete flow (tap Scan → hold up phone → QR detected → slides load) completes in under 15 seconds
**And** FR3 is satisfied

---

## Epic 6: Launch Readiness — Polish, Accessibility & Quest Store

The app is rock-solid for 30-minute sessions, works seated or standing, handles headset removal gracefully, and is available for purchase on the Meta Quest Store.

### Story 6.1: Seated Mode Adaptation

As a user,
I want StageMind to work correctly whether I'm standing or sitting,
So that I can rehearse comfortably regardless of my physical setup or ability.

**Acceptance Criteria:**

**Given** the user launches StageMind while seated
**When** the app reads the tracked head height relative to the guardian floor at launch
**Then** if the head height is below 1.2m, the app assumes seated mode and applies a vertical offset to the entire environment parent object (single Y-axis transform)
**And** the podium, laptop, audience eye level, and all spatial relationships shift proportionally so the experience feels correct from a seated position

**Given** seated mode is active
**When** the user looks at the podium
**Then** the podium is at the same relative chest-height position as when standing
**And** the laptop tilt angle is adjusted to maintain a comfortable downward glance (~15-25° from seated eye height)
**And** the audience appears at the user's seated eye level — not above them

**Given** the auto-detection may be wrong (e.g., user is standing on a lower surface)
**When** the landing page is displayed
**Then** a manual standing/seated toggle is available (SecondaryButton or similar control)
**And** toggling immediately applies or removes the Y-axis offset

**Given** seated mode is configured
**When** the full rehearsal flow is tested while seated
**Then** all interactions work identically: slide advancement, pause menu, reinforcement sequence, lobby navigation
**And** UX-DR26 is satisfied

### Story 6.2: Headset Removal Handling

As a user,
I want the app to handle gracefully when I take off the headset mid-session,
So that I can check my phone, fix slide sharing settings, or take a break without losing my place.

**Acceptance Criteria:**

**Given** the app registers for OVRManager.InputFocusLost and OVRManager.InputFocusAcquired events via PlatformManager
**When** the user removes the headset during lobby state (landing page or slides loaded)
**Then** the app pauses via Quest's proximity sensor
**And** on return, the app resumes exactly where left — WebView and input field preserve state, no action needed

**Given** the user removes the headset during Rehearsal state
**When** InputFocusLost fires, a flag is set
**Then** on InputFocusAcquired, the flag is checked and the pause menu appears automatically
**And** the user sees Resume / End Session / Return to Lobby — preventing the disorienting experience of being thrust back onto a stage mid-sentence

**Given** the user removes the headset during Reinforcement state
**When** they put the headset back on
**Then** the app resumes in the reinforcement state
**And** if the user was in the linger phase, the "Continue" button is shown immediately (the stillness timer does not reset — the pause was long enough)

**Given** headset removal handling is implemented
**When** tested across all app states
**Then** no crashes, state corruption, or unexpected behavior occurs on removal and return
**And** UX-DR25 is satisfied

### Story 6.3: Performance Profiling & Stability Validation

As a creator,
I want the app to run rock-solid for 30-minute sessions without frame drops, crashes, or thermal throttling,
So that rehearsal sessions are never interrupted by technical issues.

**Acceptance Criteria:**

**Given** the complete app is deployed to Quest 3 with all systems active (audience, WebView, audio, post-processing)
**When** a 30-minute continuous rehearsal session is run with a published Google Slides deck of 60 slides
**Then** zero crashes, freezes, or WebView hang events occur (NFR7)
**And** sustained frame rate remains at or above 72fps throughout the entire session (NFR1)

**Given** the app is running a 30-minute session
**When** thermal status is monitored
**Then** no thermal warnings, no forced performance reduction or throttling occur (NFR6)

**Given** the app runs 5 consecutive lobby→stage→lobby→stage cycles
**When** memory usage is measured after each cycle
**Then** WebView memory usage stays within 15% of initial allocation with no frame rate degradation (NFR9)

**Given** the final APK is built
**When** the package size is measured
**Then** it remains under 2GB (NFR20)

**Given** any performance issues are discovered during profiling
**When** optimizations are applied
**Then** they follow the established architecture: GPU instancing for audience, dirty-flag WebView rendering, baked lighting only, selective post-processing (reinforcement only), no real-time shadows

### Story 6.4: Quest Store Submission Package

As a creator,
I want all Meta Quest Store requirements met and accessibility verified,
So that the app can be submitted for review and made available for purchase.

**Acceptance Criteria:**

**Given** the app is feature-complete
**When** the privacy policy is created
**Then** it explicitly declares "this app does not collect or transmit user data" — no accounts, no analytics, no telemetry (NFR17)
**And** the policy is hosted at an accessible URL for the store listing

**Given** the privacy policy exists
**When** the Quest Store data collection disclosure is prepared
**Then** it declares "this app does not collect or transmit user data" per Meta's requirements

**Given** store metadata is prepared
**When** content rating is assessed
**Then** the lowest applicable rating is selected — no violence, no user-generated content displayed to others, no social features, suitable for general audiences

**Given** all store requirements are documented
**When** the submission package is assembled
**Then** it includes: APK, privacy policy URL, data collection disclosure, content rating, app description, screenshots, and clear justification that the WebView is a slide rendering tool within a VR experience (not the primary UI) per store compliance notes

**Given** the app is ready for accessibility verification
**When** all text/background color pairs are checked
**Then** warm dark gray (#3A3632) on warm cream (#F5F0E8) meets WCAG AA contrast ratio (4.5:1) on Quest 3 LCD panels

**Given** accessibility is being verified
**When** all UI interactions are tested
**Then** no information is conveyed by color alone — all buttons have text labels, all errors include descriptive text
**And** no rapid flashing or high-contrast strobing occurs — all visual transitions are gentle
**And** one-handed operation is confirmed: all interactions use right controller only (trigger + B), left used only for Menu
**And** important UI elements are placed in center of field of view (naturally achieved by laptop-on-podium design)
**And** NFR19 and UX-DR32 are satisfied
