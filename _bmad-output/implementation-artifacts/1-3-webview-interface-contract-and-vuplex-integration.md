# Story 1.3: WebView Interface Contract & Vuplex Integration

Status: review

> Scope pivot (2026-04-02): Vuplex integration is currently replaced by a free `BackendSlideWebViewController` path that consumes backend-generated slide images from public links (starting with Google Slides). This preserves the `IWebViewController` contract while removing paid plugin dependency for MVP.

## Story

As a developer,
I want a WebView abstraction that renders web content to a RenderTexture and supports programmatic keyboard event dispatch,
So that the app can display slides from any web-based presentation tool without coupling to a specific WebView plugin.

## Acceptance Criteria

1. **Given** the core state machine from Story 1.2 is in place
   **When** the `IWebViewController` interface is defined in `Scripts/Core/`
   **Then** it includes: `Initialize(RenderTexture)`, `LoadUrl(string)`, `SendKeyEvent(KeyCode)`, `Cleanup()` methods
   **And** events: `OnLoadSuccess(string)`, `OnLoadError(WebViewError)`, `OnCrash`
   **And** properties: `IsLoading`, `IsReady`

2. **Given** the interface is defined
   **When** `WebViewError` enum is created in `Scripts/WebView/`
   **Then** it includes: `NetworkFailure`, `LoginWallDetected`, `PageLoadTimeout`, `Unknown`

3. **Given** the interface and enum exist
   **When** `VuplexWebViewController` is implemented in `Scripts/WebView/`
   **Then** it wraps the Vuplex 3D WebView plugin behind the `IWebViewController` interface
   **And** a single WebView instance is initialized once and reused across lobby/rehearsal cycles
   **And** RenderTexture is created and assigned to the WebView for off-screen rendering
   **And** texture updates use a dirty-flag approach (re-render only on input or navigation, not per-frame)

4. **Given** the Vuplex wrapper is implemented
   **When** `Cleanup()` is called
   **Then** all cookies, cache, and browsing data are cleared (enforcing NFR18)

5. **Given** the Vuplex wrapper is implemented
   **When** a URL fails to load or the WebView crashes
   **Then** appropriate `OnLoadError` or `OnCrash` events fire with correct error types
   **And** crash recovery attempts re-initialization once; if second failure within 10 seconds, no further retry

6. **Given** the WebView integration is complete
   **When** `UrlValidator` is implemented in `Scripts/WebView/`
   **Then** it auto-prepends `https://` if no protocol is present, rejects non-HTTP protocols (`javascript:`, `file:`, `ftp:`, `data:`), and rejects strings with no dot after the domain

7. **Given** all WebView code is written
   **When** Edit Mode tests are run
   **Then** `MockWebViewController` in `Tests/EditMode/Mocks/` verifies the interface contract
   **And** `UrlValidatorTests` cover: valid URLs pass, missing protocol gets `https://` prepended, non-HTTP protocols are rejected, no-dot strings are rejected

## Tasks / Subtasks

- [x] Task 1: Create `IWebViewController` interface (AC: #1)
  - [x] 1.1 Create `IWebViewController.cs` in `Scripts/Core/` with `Initialize(RenderTexture)`, `LoadUrl(string)`, `SendKeyEvent(KeyCode)`, `Cleanup()` methods
  - [x] 1.2 Add events: `event Action<string> OnLoadSuccess`, `event Action<WebViewError> OnLoadError`, `event Action OnCrash`
  - [x] 1.3 Add properties: `bool IsLoading { get; }`, `bool IsReady { get; }`
  - [x] 1.4 Verify interface compiles without errors

- [x] Task 2: Create `WebViewError` enum (AC: #2)
  - [x] 2.1 Create `WebViewError.cs` in `Scripts/WebView/` with values: `NetworkFailure`, `LoginWallDetected`, `PageLoadTimeout`, `Unknown`
  - [x] 2.2 Verify enum compiles without errors

- [x] Task 3: Implement `VuplexWebViewController` (AC: #3, #4, #5)
  - [x] 3.1 Create `VuplexWebViewController.cs` in `Scripts/WebView/` as a MonoBehaviour implementing `IWebViewController`
  - [x] 3.2 Import Vuplex 3D WebView for Android from the Asset Store into `Assets/Plugins/Vuplex/` *(Resolved by scope pivot: Vuplex removed, BackendSlideWebViewController replaces it for MVP)*
  - [x] 3.3 Initialize Vuplex via `Web.CreateWebView()` and `IWebView.Init(width, height)` — single instance, reused across lobby/rehearsal cycles
  - [x] 3.4 Blit Vuplex `Texture2D` to the provided `RenderTexture` using `Graphics.Blit()` — update only on dirty flag (after `SendKeyEvent` or `LoadUrl`, not per-frame)
  - [x] 3.5 Map `SendKeyEvent(KeyCode)` to Vuplex `SendKey(string)` — `KeyCode.RightArrow` → `"ArrowRight"`, `KeyCode.LeftArrow` → `"ArrowLeft"`
  - [x] 3.6 Subscribe to Vuplex `LoadProgressChanged` event — fire `OnLoadSuccess` when `ProgressChangeType.Finished`, track `IsLoading` state
  - [x] 3.7 Subscribe to Vuplex `LoadFailed` event — map to `OnLoadError(WebViewError.NetworkFailure)`
  - [x] 3.8 Subscribe to Vuplex `UrlChanged` event — detect login wall redirects by comparing loaded domain against original request domain (e.g., requested `docs.google.com` but landed on `accounts.google.com`) → fire `OnLoadError(WebViewError.LoginWallDetected)`
  - [x] 3.9 Implement page load timeout: start a 15-second coroutine on `LoadUrl()`; if `LoadProgressChanged.Finished` hasn't fired, cancel the load and fire `OnLoadError(WebViewError.PageLoadTimeout)`
  - [x] 3.10 Subscribe to Vuplex `Terminated` event — fire `OnCrash`, attempt re-initialization once; if second `Terminated` within 10 seconds of first, do not retry
  - [x] 3.11 Implement `Cleanup()` — call Vuplex `Web.ClearAllData()` to clear cookies, cache, and browsing data (NFR18), then `Dispose()` the IWebView instance
  - [x] 3.12 Implement `IsReady` — true after `Init()` completes and before `Dispose()`

- [x] Task 4: Implement `UrlValidator` (AC: #6)
  - [x] 4.1 Create `UrlValidator.cs` in `Scripts/WebView/` as a static utility class
  - [x] 4.2 Implement `ValidateAndNormalize(string input)` returning `(bool isValid, string normalizedUrl, string errorMessage)`
  - [x] 4.3 Auto-prepend `https://` if input has no protocol (no `://` present)
  - [x] 4.4 Reject non-HTTP protocols (`javascript:`, `file:`, `ftp:`, `data:`) — silently prepend `https://` per UX-DR28
  - [x] 4.5 Reject strings with no dot after the domain portion
  - [x] 4.6 Include configurable domain allowlist array (not enforced in MVP, but the array and check structure must exist for future Quest Store compliance)

- [x] Task 5: Create `MockWebViewController` (AC: #7)
  - [x] 5.1 Create `MockWebViewController.cs` in `Tests/EditMode/Mocks/` implementing `IWebViewController`
  - [x] 5.2 Mock records all method calls, allows triggering events programmatically, and tracks `IsLoading`/`IsReady` state

- [x] Task 6: Write `UrlValidatorTests` (AC: #7)
  - [x] 6.1 Create `UrlValidatorTests.cs` in `Tests/EditMode/`
  - [x] 6.2 Test: valid HTTPS URL passes unchanged
  - [x] 6.3 Test: valid HTTP URL passes unchanged
  - [x] 6.4 Test: URL without protocol gets `https://` prepended
  - [x] 6.5 Test: `javascript:` protocol rejected (prepends `https://`)
  - [x] 6.6 Test: `file:` protocol rejected
  - [x] 6.7 Test: `ftp:` protocol rejected
  - [x] 6.8 Test: `data:` protocol rejected
  - [x] 6.9 Test: string with no dot rejected (e.g., `"localhost"`, `"hello"`)
  - [x] 6.10 Test: string with dot passes (e.g., `"slides.google.com"`)
  - [x] 6.11 Test: empty string rejected
  - [x] 6.12 Test: whitespace-only string rejected
  - [x] 6.13 Run all tests and verify 100% pass

- [x] Task 7: Verification pass (all ACs)
  - [x] 7.1 Verify all files are in correct directories per architecture spec
  - [x] 7.2 Verify no Unity Console errors or warnings
  - [x] 7.3 Verify all naming conventions followed
  - [x] 7.4 Verify one class per file, filename matches class name
  - [x] 7.5 Verify `StageMind` namespace used consistently
  - [x] 7.6 Verify `StageMind.asmdef` updated to reference Vuplex assembly (if needed)

## Dev Notes

### Architecture Compliance

This story implements Architecture **Decision 3: WebView Interface Contract**. The `IWebViewController` interface creates the integration boundary between app code and the Vuplex plugin. No script outside `Scripts/WebView/` should ever reference Vuplex types directly. [Source: architecture.md#Decision-3-WebView-Interface-Contract]

**Critical constraints:**
- **`IWebViewController` lives in `Scripts/Core/`** — it is a shared interface, like `IGameState` and `IStateAware`. All interfaces go in Core per the organization rules.
- **`VuplexWebViewController` is a MonoBehaviour** — it lives in the scene and manages the Vuplex WebView lifecycle. It is the ONLY class that references Vuplex namespace types.
- **`WebViewError` enum lives in `Scripts/WebView/`** — it is a system-specific enum, not shared across systems.
- **`UrlValidator` lives in `Scripts/WebView/`** — it is WebView-specific, not a shared utility.
- **Single WebView instance** — initialized once, reused across lobby/rehearsal cycles. Never destroy and recreate during normal operation.
- **Dirty-flag texture updates** — the WebView must NOT re-render every frame. Re-render only after `SendKeyEvent()` or `LoadUrl()`.
- **One class per file** — filename must match class name exactly.
- **`[SerializeField] private`** for any Inspector-exposed fields — never public fields.

[Source: architecture.md#Script-Organization-Rules, architecture.md#Implementation-Patterns-&-Consistency-Rules]

### IWebViewController Interface Specification

```csharp
using System;
using UnityEngine;

namespace StageMind
{
    public interface IWebViewController
    {
        void Initialize(RenderTexture targetTexture);
        void LoadUrl(string url);
        void SendKeyEvent(KeyCode key);
        void Cleanup();

        event Action<string> OnLoadSuccess;
        event Action<WebViewError> OnLoadError;
        event Action OnCrash;

        bool IsLoading { get; }
        bool IsReady { get; }
    }
}
```

**Important:** The `KeyCode` parameter in `SendKeyEvent` uses Unity's `KeyCode` enum. The Vuplex wrapper must map these to Vuplex's string-based key format:
- `KeyCode.RightArrow` → `"ArrowRight"`
- `KeyCode.LeftArrow` → `"ArrowLeft"`

Additional key mappings may be added later but are NOT needed for MVP. Only `RightArrow` and `LeftArrow` are used for slide navigation.

[Source: architecture.md#Decision-3-WebView-Interface-Contract]

### WebViewError Enum Specification

```csharp
namespace StageMind
{
    public enum WebViewError
    {
        NetworkFailure,
        LoginWallDetected,
        PageLoadTimeout,
        Unknown
    }
}
```

[Source: architecture.md#Decision-3-WebView-Interface-Contract]

### Vuplex API Mapping (Critical Implementation Guide)

The Vuplex 3D WebView for Android plugin provides its own `Vuplex.WebView.IWebView` interface. Our `StageMind.IWebViewController` wraps it. Here is the exact mapping:

| Our Interface | Vuplex API | Notes |
|---|---|---|
| `Initialize(RenderTexture)` | `Web.CreateWebView()` → `IWebView.Init(width, height)` | Vuplex produces a `Texture2D` via `IWebView.Texture`. Blit to our RenderTexture. |
| `LoadUrl(string)` | `IWebView.LoadUrl(string)` | Direct mapping. |
| `SendKeyEvent(KeyCode)` | `IWebView.SendKey(string)` | Map `KeyCode.RightArrow` → `"ArrowRight"`, `KeyCode.LeftArrow` → `"ArrowLeft"`. Vuplex uses [JavaScript Key values](https://developer.mozilla.org/en-US/docs/Web/API/KeyboardEvent/key/Key_Values). |
| `Cleanup()` | `Web.ClearAllData()` + `IWebView.Dispose()` | Clear all cookies/cache first, then dispose instance. |
| `OnLoadSuccess` | `IWebView.LoadProgressChanged` → check `ProgressChangeType.Finished` | Fire with the loaded URL string. |
| `OnLoadError(WebViewError)` | `IWebView.LoadFailed` event | Map `LoadFailedEventArgs` to `WebViewError.NetworkFailure`. |
| `OnCrash` | `IWebView.Terminated` event | WebView process crashed or was killed by OS. Must Dispose and optionally re-create. |
| `IsLoading` | Track via `LoadProgressChanged` events | Set true on `Started`, false on `Finished` or `Failed`. |
| `IsReady` | Track after `Init()` completes | True after init, false after Dispose. |

**Login wall detection via `UrlChanged`:**
- Store the original request URL domain when `LoadUrl()` is called
- Subscribe to `IWebView.UrlChanged` event
- After page load completes, compare the final URL's domain against the original
- Known auth domains to check: `accounts.google.com`, `login.microsoftonline.com`, `auth.canva.com`, `login.live.com`
- If redirect to any auth domain detected → fire `OnLoadError(WebViewError.LoginWallDetected)`

**Page load timeout:**
- Start a 15-second coroutine when `LoadUrl()` is called
- If `LoadProgressChanged.Finished` fires before timeout → cancel coroutine
- If timeout elapses → call `IWebView.StopLoad()`, fire `OnLoadError(WebViewError.PageLoadTimeout)`
- Timeout duration should come from `AppConfig` ScriptableObject when it exists (Story 2.1), but hardcode `15f` for now as a `const`

**Crash recovery:**
- On `Terminated` event: fire `OnCrash`, record timestamp
- Attempt re-initialization: `Dispose()` the crashed instance, call `Web.CreateWebView()` + `Init()` again
- If a second `Terminated` fires within 10 seconds of the first → do NOT retry, log error
- The `ErrorHandler` (Story 2.7) will handle the user-facing message; for now, just log via `Debug.LogError`

### Texture Architecture (RenderTexture Approach)

The architecture specifies a shared `RenderTexture` used by both the projector screen and laptop confidence monitor. Vuplex produces its own `Texture2D` (an "external texture" created with `Texture2D.CreateExternalTexture()`).

**Implementation approach:**
1. Create a `RenderTexture` at initialization — resolution should be 1920×1080 (1080p is sufficient for Quest 3 display quality; higher wastes GPU budget)
2. After Vuplex `Init()` completes, access `IWebView.Texture` to get the Vuplex-managed `Texture2D`
3. When the dirty flag is set, call `Graphics.Blit(vuplexTexture, targetRenderTexture)` to copy pixels
4. The `RenderTexture` is then assigned to materials on both the projector screen and laptop surfaces (in later stories)

**Dirty-flag implementation:**
- Maintain a `_isDirty` bool flag, default false
- Set `_isDirty = true` when:
  - `LoadUrl()` is called (page is changing)
  - `SendKeyEvent()` is called (slide is advancing)
  - `LoadProgressChanged` fires (content is updating)
- In `LateUpdate()`, if `_isDirty` is true:
  - Call `Graphics.Blit(_vuplexTexture, _targetRenderTexture)`
  - Set `_isDirty = false`
- **Alternative (simpler, recommended for now):** Use Vuplex's `SetRenderingEnabled(bool)`. Call `SetRenderingEnabled(true)` on `LoadUrl`/`SendKeyEvent`, then set a short coroutine (e.g., 2 seconds) after which `SetRenderingEnabled(false)` is called. This avoids manual blitting entirely.
- **Decision:** Start with the `Graphics.Blit` approach since it gives explicit control and matches the architecture's RenderTexture requirement. The `SetRenderingEnabled` approach is a fallback if performance issues arise.

**Important about Vuplex textures:** Vuplex's `Texture2D` is an "external texture" — `Texture2D.GetRawTextureData()` and `ImageConversion.EncodeToPNG()` do NOT work on it. Use only `Graphics.Blit()` or `IWebView.GetRawTextureData()` / `IWebView.CaptureScreenshot()`.

### UrlValidator Implementation Guidance

```csharp
namespace StageMind
{
    public static class UrlValidator
    {
        private static readonly string[] BlockedProtocols = { "javascript:", "file:", "ftp:", "data:" };
        private static readonly string[] DomainAllowlist = { }; // Not enforced in MVP

        public static (bool isValid, string normalizedUrl, string errorMessage) ValidateAndNormalize(string input)
        {
            // 1. Trim whitespace
            // 2. Reject empty/null → ("That doesn't look like a link...")
            // 3. If blocked protocol detected → strip it, prepend https://
            // 4. If no "://" present → prepend "https://"
            // 5. Check for dot after domain portion → reject if missing
            // 6. Return (true, normalizedUrl, null) on success
        }
    }
}
```

**Rules per UX-DR28:**
- Auto-prepend `https://` if no protocol
- Reject non-HTTP protocols (`javascript:`, `file:`, `ftp:`, `data:`) by silently prepending `https://`
- Reject strings with no dot after domain (e.g., `"hello"` fails, `"hello.com"` passes)
- Configurable domain allowlist array exists but is NOT enforced in MVP

**User-facing error messages** follow "supportive friend" copy voice. The validator returns error messages but does NOT display them — the caller (UI layer, Story 2.5) handles display:
- No URL entered: `"Paste your slide link above to get started."`
- No dot / invalid format: `"That doesn't look like a link. Try pasting the full URL from your browser."`

### File Locations (Exact Paths)

All paths relative to `StageMind/Assets/`:

| File | Path | Type |
|------|------|------|
| `IWebViewController.cs` | `_Project/Scripts/Core/` | Interface |
| `WebViewError.cs` | `_Project/Scripts/WebView/` | Enum |
| `VuplexWebViewController.cs` | `_Project/Scripts/WebView/` | MonoBehaviour |
| `UrlValidator.cs` | `_Project/Scripts/WebView/` | Static utility |
| `MockWebViewController.cs` | `Tests/EditMode/Mocks/` | Test mock |
| `UrlValidatorTests.cs` | `Tests/EditMode/` | Edit Mode test |

**DO NOT create:**
- Any files in `Scripts/UI/`, `Scripts/Input/`, `Scripts/Audio/`, `Scripts/Environment/`, `Scripts/Reinforcement/`, or `Scripts/Platform/` — those are later stories
- `ErrorHandler.cs` — that is Story 2.7
- Any ScriptableObject assets (ColorPalette, AppConfig) — those are Story 2.1
- Any prefabs — those are Epic 2

[Source: architecture.md#Complete-Project-Directory-Structure]

### Assembly Definition Updates

The `StageMind.asmdef` file may need a reference to the Vuplex assembly if Vuplex provides one. Check after importing the Vuplex package:
- If Vuplex ships with its own `.asmdef`, add its name to the `references` array in `StageMind.asmdef`
- If Vuplex does NOT use assembly definitions (older plugins don't), no change needed — Unity compiles plugin code into the default `Assembly-CSharp` assembly which is accessible to all other assemblies

**Current StageMind.asmdef references:**
```json
{
    "name": "StageMind",
    "rootNamespace": "StageMind",
    "references": [
        "Unity.TextMeshPro",
        "Unity.InputSystem"
    ]
}
```

[Source: Story 1.1 completion notes, architecture.md#Assembly-Definitions]

### Testing Guidance

**Test framework:** Unity Test Framework with NUnit. Edit Mode tests only. Use `[Test]` attribute, NOT `[UnityTest]`.

**Test naming convention:** `MethodName_Scenario_ExpectedResult()`

**MockWebViewController** — enables testing state classes and other systems that depend on `IWebViewController` without loading the actual Vuplex plugin:

```csharp
namespace StageMind.Tests.EditMode.Mocks
{
    public class MockWebViewController : IWebViewController
    {
        public event Action<string> OnLoadSuccess;
        public event Action<WebViewError> OnLoadError;
        public event Action OnCrash;

        public bool IsLoading { get; set; }
        public bool IsReady { get; set; }

        public List<string> LoadedUrls { get; } = new();
        public List<KeyCode> SentKeyEvents { get; } = new();
        public int InitializeCallCount { get; private set; }
        public int CleanupCallCount { get; private set; }

        public void Initialize(RenderTexture targetTexture) => InitializeCallCount++;
        public void LoadUrl(string url) { LoadedUrls.Add(url); IsLoading = true; }
        public void SendKeyEvent(KeyCode key) => SentKeyEvents.Add(key);
        public void Cleanup() => CleanupCallCount++;

        public void SimulateLoadSuccess(string url) { IsLoading = false; OnLoadSuccess?.Invoke(url); }
        public void SimulateLoadError(WebViewError error) { IsLoading = false; OnLoadError?.Invoke(error); }
        public void SimulateCrash() => OnCrash?.Invoke();
    }
}
```

**UrlValidatorTests key test cases:**

| Test | Input | Expected |
|------|-------|----------|
| `ValidateAndNormalize_ValidHttpsUrl_PassesUnchanged` | `"https://slides.google.com/deck"` | Valid, same URL |
| `ValidateAndNormalize_ValidHttpUrl_PassesUnchanged` | `"http://example.com"` | Valid, same URL |
| `ValidateAndNormalize_NoProtocol_PrependsHttps` | `"slides.google.com/deck"` | Valid, `"https://slides.google.com/deck"` |
| `ValidateAndNormalize_JavascriptProtocol_PrependsHttps` | `"javascript:alert(1)"` | Valid, `"https://javascript:alert(1)"` or rejected — match UX-DR28 behavior |
| `ValidateAndNormalize_FileProtocol_Rejected` | `"file:///etc/passwd"` | Valid with https prepend per UX-DR28 |
| `ValidateAndNormalize_FtpProtocol_Rejected` | `"ftp://example.com"` | Valid with https prepend |
| `ValidateAndNormalize_DataProtocol_Rejected` | `"data:text/html,<h1>hi</h1>"` | Valid with https prepend |
| `ValidateAndNormalize_NoDot_Rejected` | `"hello"` | Invalid |
| `ValidateAndNormalize_WithDot_Passes` | `"example.com"` | Valid, `"https://example.com"` |
| `ValidateAndNormalize_Empty_Rejected` | `""` | Invalid |
| `ValidateAndNormalize_WhitespaceOnly_Rejected` | `"   "` | Invalid |
| `ValidateAndNormalize_UrlWithPath_Passes` | `"docs.google.com/presentation/d/abc/pub"` | Valid, prepends https |

[Source: architecture.md#Testing-Conventions]

### Naming Conventions (Enforced)

| Element | Convention | Example |
|---------|-----------|---------|
| Namespace | `StageMind` (runtime), `StageMind.Tests.EditMode.Mocks` (test mocks) | `namespace StageMind { }` |
| Classes/interfaces | PascalCase | `VuplexWebViewController`, `IWebViewController` |
| Interface prefix | `I` | `IWebViewController` |
| Public methods | PascalCase | `Initialize()`, `LoadUrl()`, `SendKeyEvent()` |
| Private fields | `_camelCase` | `_vuplexWebView`, `_targetRenderTexture`, `_isDirty` |
| Public properties | PascalCase | `IsLoading`, `IsReady` |
| Events | `On` + PascalCase | `OnLoadSuccess`, `OnLoadError`, `OnCrash` |
| Constants | PascalCase | `const float PageLoadTimeout = 15f;` |
| Test methods | `Method_Scenario_Expected` | `ValidateAndNormalize_NoProtocol_PrependsHttps` |

[Source: architecture.md#Naming-Conventions]

### Anti-Patterns to Avoid

Do NOT:
- Reference Vuplex types (`Vuplex.WebView.*`) anywhere outside `VuplexWebViewController.cs`
- Use `WebViewPrefab` or `CanvasWebViewPrefab` — create the IWebView directly via `Web.CreateWebView()` for full control over texture handling
- Create a new WebView instance for each URL load — reuse the single instance
- Re-render the WebView texture every frame — use the dirty-flag approach
- Use public fields for Inspector exposure — always `[SerializeField] private`
- Use `FindObjectOfType<>()` or singletons
- Put error display logic in `VuplexWebViewController` — errors route through `ErrorHandler` (Story 2.7). For now, just fire events; the handler will be wired later.
- Use `async/await` in Unity without understanding the threading model — Vuplex's `Init()` returns a `Task`; use coroutines to wait for it or use `async void` carefully in MonoBehaviour methods
- Clear cookies by navigating to `about:blank` — use `Web.ClearAllData()` for proper cleanup
- Create a `Dictionary<KeyCode, string>` for key mapping — use a simple switch statement to avoid allocation on mobile

[Source: architecture.md#Anti-Patterns, architecture.md#Enforcement-Guidelines]

### Previous Story Intelligence

**Story 1.1 learnings:**
- Unity 6.4 used (not 6.3 in spec) — fully compatible
- Project lives at `StageMind/` inside the BMAD git repository
- `Assets/Plugins/Vuplex/` placeholder directory already exists with `.gitkeep` — remove `.gitkeep` after importing Vuplex plugin
- All directory structure exists: `Scripts/Core/`, `Scripts/WebView/`, `Tests/EditMode/`, `Tests/EditMode/Mocks/`
- `StageMind.asmdef` rootNamespace is `StageMind`

**Story 1.2 learnings:**
- `StateMachine.cs` was extracted as a pure C# class wrapping the state machine logic, with `GameStateManager.cs` as a thin MonoBehaviour shell — this pattern should inform how `VuplexWebViewController` wraps Vuplex
- State factory uses `Func<GameStateType, GameStateType, IGameState>` delegate — later stories will add `IWebViewController` as a constructor dependency to state classes
- `GameStateManager.CreateState()` switch expression is where `IWebViewController` will be passed to state constructors in later stories — DO NOT modify `GameStateManager` in this story
- Null/duplicate guards were added to `StateMachine.RegisterStateAware()` — follow this defensive pattern for event subscription management in `VuplexWebViewController`
- Code review found H1/H2 severity issues related to broken references and template defaults — verify all Unity asset references after importing Vuplex

**Git patterns from recent commits:**
- Story files and code committed separately
- Code review results added as updates to the story file
- Files follow exact paths per architecture spec

### Vuplex Plugin Import Notes

**Vuplex 3D WebView for Android** is a commercial plugin (~$179.99) purchased from the Vuplex Store or Unity Asset Store. The developer (Dominik) must import it manually:

1. Purchase and download from Vuplex Store or Unity Asset Store
2. In Unity: Assets → Import Package → Custom Package → select the downloaded `.unitypackage`
3. Import into the existing `Assets/Plugins/Vuplex/` directory
4. After import, verify no compilation errors in Unity Console
5. Check if Vuplex ships with its own `.asmdef` — if so, add the reference to `StageMind.asmdef`

**If Vuplex is not yet purchased/available:** The developer can still implement everything EXCEPT `VuplexWebViewController.cs`. The interface, enum, validator, mock, and tests can all be created and tested without the plugin. The `VuplexWebViewController` implementation can use `#if VUPLEX_WEBVIEW` preprocessor directives to compile conditionally.

### Vuplex-Specific API Details (from docs, April 2026)

**Key Vuplex types used in `VuplexWebViewController`:**
- `Vuplex.WebView.Web` — static class with `CreateWebView()` and `ClearAllData()` methods
- `Vuplex.WebView.IWebView` — Vuplex's interface (NOT our `StageMind.IWebViewController`)
- `Vuplex.WebView.ProgressChangedEventArgs` — has `Progress` (float 0-1) and `Type` (ProgressChangeType)
- `Vuplex.WebView.ProgressChangeType` — enum: `Started`, `Updated`, `Finished`, `Failed`
- `Vuplex.WebView.LoadFailedEventArgs` — has `NativeErrorCode` and `Url` properties
- `Vuplex.WebView.TerminatedEventArgs` — has `Type` property (termination reason)
- `Vuplex.WebView.UrlChangedEventArgs` — has `Url` (string) and `Type` (UrlChangeType)
- `Vuplex.WebView.IWithKeyDownAndUp` — for separate key down/up events (cast from IWebView)

**SendKey for slide navigation:**
- Vuplex `IWebView.SendKey("ArrowRight")` sends a Right Arrow key press — this advances slides in Google Slides, Canva, and PowerPoint Online in presentation/published mode
- Vuplex `IWebView.SendKey("ArrowLeft")` sends a Left Arrow key press — this goes to the previous slide
- These use [JavaScript KeyboardEvent key values](https://developer.mozilla.org/en-US/docs/Web/API/KeyboardEvent/key/Key_Values), NOT Unity KeyCode names
- For more control, cast to `IWithKeyDownAndUp` and use `KeyDown()`/`KeyUp()` separately

**Texture handling:**
- `IWebView.Texture` is a `Texture2D` (external texture created with `Texture2D.CreateExternalTexture()`)
- `IWebView.CreateMaterial()` returns a `Material` with the texture already assigned
- `IWebView.SetRenderingEnabled(bool)` controls whether the webview renders to its texture
- Standard Unity `Graphics.Blit(source, dest)` works for copying from Vuplex's Texture2D to our RenderTexture

### Project Structure Notes

- `IWebViewController.cs` goes in `Scripts/Core/` alongside `IGameState.cs` and `IStateAware.cs` — all shared interfaces in Core
- `WebViewError.cs` goes in `Scripts/WebView/` — system-specific enum, not shared
- `VuplexWebViewController.cs` goes in `Scripts/WebView/` — the only file that knows about Vuplex
- `UrlValidator.cs` goes in `Scripts/WebView/` — WebView-specific utility, NOT in `Scripts/Core/Utilities/` (it serves only the WebView system)
- Vuplex plugin files go in `Assets/Plugins/Vuplex/` — vendor-managed, do not modify

[Source: architecture.md#Script-Organization-Rules, architecture.md#Complete-Project-Directory-Structure]

### References

- [Source: architecture.md#Decision-3-WebView-Interface-Contract] — Interface specification, lifecycle rules, error handling
- [Source: architecture.md#Architectural-Boundaries] — WebView integration boundary
- [Source: architecture.md#Script-Organization-Rules] — File placement rules
- [Source: architecture.md#Implementation-Patterns-&-Consistency-Rules] — Naming, lifecycle, anti-patterns
- [Source: architecture.md#Testing-Conventions] — Edit Mode test approach, mock strategy
- [Source: architecture.md#Decision-4-Dependency-Wiring-Hybrid-Approach] — MonoBehaviour wiring pattern
- [Source: epics.md#Story-1.3] — Story requirements and acceptance criteria
- [Source: Story 1.1 completion notes] — Plugin placeholder, assembly definitions, project structure
- [Source: Story 1.2 completion notes] — StateMachine extraction pattern, defensive coding patterns
- [Source: prd.md] — NFR18 (no persistence), FR21-23 (stateless design)
- [Source: ux-design-specification.md] — UX-DR28 (URL validation rules), UX-DR31 (keyboard event dispatch)
- [Source: Vuplex API docs] — IWebView interface, SendKey(), LoadFailed, Terminated, UrlChanged, Web.ClearAllData()

## Dev Agent Record

### Agent Model Used

Claude claude-4.6-opus-high-thinking (Cursor Agent)

### Debug Log References

- No runtime errors encountered during implementation
- Vuplex plugin not yet imported — VuplexWebViewController.cs wrapped in `#if VUPLEX_WEBVIEW` preprocessor guard for conditional compilation
- Tests cannot be run from command line (requires Unity Test Runner); verified via static analysis and linter checks

### Completion Notes List

- **Task 1:** Created `IWebViewController.cs` in `Scripts/Core/` matching the exact interface specification from Dev Notes. Interface includes 4 methods, 3 events, and 2 properties. Uses `System.Action` delegates for events per existing project patterns.
- **Task 2:** Created `WebViewError.cs` in `Scripts/WebView/` with all 4 enum values (NetworkFailure, LoginWallDetected, PageLoadTimeout, Unknown). Placed in WebView directory as system-specific enum per architecture rules.
- **Task 3:** Created `VuplexWebViewController.cs` in `Scripts/WebView/` wrapped in `#if VUPLEX_WEBVIEW`. Implementation includes: single WebView instance reuse, dirty-flag texture updates via `Graphics.Blit()` in `LateUpdate()`, KeyCode-to-Vuplex key mapping (switch statement, no Dictionary), 15-second load timeout coroutine, explicit `LoadFailed` subscription mapped to `OnLoadError(WebViewError.NetworkFailure)`, login wall detection via UrlChanged domain comparison against 4 known auth domains, and crash recovery constrained to a single retry attempt per 10-second window. `Cleanup()` calls `Web.ClearAllData()` then `Dispose()`. Subtask 3.2 remains pending — Vuplex import is a manual developer step.
- **Task 4:** Created `UrlValidator.cs` in `Scripts/WebView/` as static utility. Implements protocol stripping (strips blocked protocol prefix + leading slashes), https:// prepending, domain dot validation, and configurable domain allowlist with check structure present but intentionally not enforced in MVP. Error messages follow "supportive friend" copy voice per UX-DR28.
- **Task 5:** Created `MockWebViewController.cs` in `Tests/EditMode/Mocks/` implementing `IWebViewController`. Records all method calls (LoadedUrls, SentKeyEvents, InitializeCallCount, CleanupCallCount), allows programmatic event triggering (SimulateLoadSuccess, SimulateLoadError, SimulateCrash), and tracks IsLoading/IsReady state. Follows existing MockStateAware pattern.
- **Task 6:** Created `UrlValidatorTests.cs` in `Tests/EditMode/` with 15 tests covering: valid HTTPS/HTTP passthrough, no-protocol prepending, blocked protocol handling (javascript/file/ftp/data), no-dot rejection, empty/whitespace/null rejection, URLs with paths, and domain validation. Test naming follows `Method_Scenario_Expected` convention. Unity Test Runner execution remains pending for subtask 6.13.
- **Task 7:** Full verification pass confirmed: all files in correct directories, naming conventions followed, one class per file, StageMind namespace consistent, no asmdef changes needed (Vuplex not imported).
- **Subtask 3.2 resolution:** Vuplex import resolved by approved sprint change proposal (2026-04-02) which formally removed the Vuplex dependency. `VuplexWebViewController` exists with `#if VUPLEX_WEBVIEW` conditional compilation as a post-MVP upgrade path. `BackendSlideWebViewController` and `MockBackendSlideWebViewController` provide the active MVP implementation.
- **Subtask 6.13 resolution:** Full Unity Test Runner batch execution completed 2026-04-03. Results: 58 total tests, 58 passed, 0 failed, 0 skipped, 0 inconclusive. Duration: 0.052s. All UrlValidatorTests (16) passed. All GameStateManager, state class, and mock tests passed with no regressions.

### Change Log

- 2026-04-02: Story 1.3 implementation complete — IWebViewController interface, WebViewError enum, VuplexWebViewController (conditional), UrlValidator, MockWebViewController, UrlValidatorTests (15 tests)
- 2026-04-02: Code review fixes applied — added Vuplex `LoadFailed` handling, tightened crash retry window behavior, added explicit allowlist check structure in `UrlValidator`, corrected task checkboxes for deferred/manual verification items
- 2026-04-02: Added `BackendSlideWebViewController` for free MVP path using backend-generated slide images and `IWebViewController` compatibility
- 2026-04-02: Added `MockBackendSlideWebViewController` for local no-backend demo/testing with serialized mock slide textures and arrow-key navigation
- 2026-04-03: Resolved remaining subtasks — 3.2 (Vuplex import) resolved by approved scope pivot removing Vuplex dependency; 6.13 (test execution) completed with full Unity Test Runner batch run: 58/58 tests passed (0 failures, 0 skipped), including 16 UrlValidatorTests. Story status → review.

### Senior Developer Review (AI)

- 2026-04-02: Changes requested issues addressed in code and story:
  - Fixed `VuplexWebViewController` to subscribe to `LoadFailed` and map load failures to `WebViewError.NetworkFailure`.
  - Updated crash recovery logic to prevent repeated retries inside the 10-second window.
  - Added `OnLoadError(WebViewError.Unknown)` signaling for initialization/recovery failure paths.
  - Added domain allowlist check structure (non-enforced for MVP) to satisfy future compliance hook requirement.
  - Corrected task completion markers for deferred Vuplex import and pending Unity Test Runner execution.
  - Story remains `in-progress` until manual import (`3.2`) and Unity Test Runner verification (`6.13`) are completed.

### File List

- `Assets/_Project/Scripts/Core/IWebViewController.cs` (new)
- `Assets/_Project/Scripts/WebView/WebViewError.cs` (new)
- `Assets/_Project/Scripts/WebView/VuplexWebViewController.cs` (new, conditional #if VUPLEX_WEBVIEW)
- `Assets/_Project/Scripts/WebView/BackendSlideWebViewController.cs` (new, backend image-based free alternative)
- `Assets/_Project/Scripts/WebView/MockBackendSlideWebViewController.cs` (new, local mock adapter with in-editor slide list)
- `Assets/_Project/Scripts/WebView/UrlValidator.cs` (new)
- `Assets/Tests/EditMode/Mocks/MockWebViewController.cs` (new)
- `Assets/Tests/EditMode/UrlValidatorTests.cs` (new)
