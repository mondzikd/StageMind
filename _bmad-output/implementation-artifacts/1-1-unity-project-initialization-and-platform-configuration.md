# Story 1.1: Unity Project Initialization & Platform Configuration

Status: ready-for-dev

## Story

As a developer,
I want a fully configured Unity project targeting Quest 3 with all required packages and project structure in place,
So that I have a clean, correct foundation for building StageMind without configuration issues later.

## Acceptance Criteria

1. **Given** Unity Hub is installed with Unity 6.3 LTS (Android Build Support, OpenJDK, Android SDK & NDK Tools)
   **When** a new project is created from the Universal 3D (URP) template named "StageMind"
   **Then** the project opens in Unity Editor without errors

2. **Given** the new Unity project is open
   **When** build platform is switched to Meta Quest via File → Build Profiles
   **Then** Player Settings are configured: IL2CPP scripting backend, ARM64 target architecture, Vulkan graphics API, Minimum API Level 29, correct Company Name and Product Name

3. **Given** the project is platform-configured
   **When** required packages are installed via Package Manager
   **Then** the following packages are present and resolve without errors: OpenXR (≥1.15.1), Meta XR OpenXR (≥2.2), TextMeshPro, Unity Input System
   **And** Meta XR Simulator is installed from the Unity Asset Store

4. **Given** all packages are installed
   **When** the project folder structure is created under Assets/_Project/
   **Then** the following directories exist: Scripts/Core/, Scripts/WebView/, Scripts/Input/, Scripts/UI/, Scripts/Audio/, Scripts/Environment/, Scripts/Reinforcement/, Scripts/Platform/, Prefabs/UI/, Prefabs/Environment/, Prefabs/Audio/, ScriptableObjects/, Materials/, Textures/, Audio/, Fonts/, Scenes/, Art/, InputActions/, Settings/URP/, Settings/XR/

5. **Given** the folder structure is in place
   **When** assembly definitions are created
   **Then** three .asmdef files exist: StageMind (Scripts/), StageMind.Tests.EditMode (Tests/EditMode/), StageMind.Tests.PlayMode (Tests/PlayMode/) with correct references

6. **Given** the project is fully configured
   **When** a .gitignore appropriate for Unity is added to the project root
   **Then** Library/, Temp/, Logs/, obj/, Builds/, and .utmp/ directories are excluded from version control

## Tasks / Subtasks

- [ ] Task 1: Install Unity 6.3 LTS via Unity Hub (AC: #1)
  - [ ] 1.1 Install Unity Hub for Mac if not present
  - [ ] 1.2 Install Unity 6.3 LTS (6000.3.x) with modules: Android Build Support, OpenJDK, Android SDK & NDK Tools
  - [ ] 1.3 Create new project from "Universal 3D (URP)" template, named "StageMind"
  - [ ] 1.4 Open project and confirm no errors in Console
- [ ] Task 2: Configure build platform for Meta Quest 3 (AC: #2)
  - [ ] 2.1 File → Build Profiles → Add Meta Quest profile → Switch Platform
  - [ ] 2.2 Player Settings → Other Settings → Scripting Backend: IL2CPP
  - [ ] 2.3 Player Settings → Other Settings → Target Architectures: ARM64 only (uncheck ARMv7)
  - [ ] 2.4 Player Settings → Other Settings → Graphics APIs: Vulkan (remove OpenGLES3 if present)
  - [ ] 2.5 Player Settings → Other Settings → Minimum API Level: Android 10 (API Level 29)
  - [ ] 2.6 Player Settings → Company Name and Product Name: set appropriately
  - [ ] 2.7 Edit → Project Settings → XR Plug-in Management → enable OpenXR for Android
  - [ ] 2.8 Under OpenXR → add Meta Quest Touch Pro Controller Interaction Profile
- [ ] Task 3: Install required packages (AC: #3)
  - [ ] 3.1 Window → Package Manager → install com.unity.xr.openxr (latest stable, currently 1.16.1)
  - [ ] 3.2 Window → Package Manager → install com.unity.xr.meta-openxr (latest stable, currently 2.4.0)
  - [ ] 3.3 Confirm TextMeshPro is included (bundled with Unity 6)
  - [ ] 3.4 Confirm Unity Input System package is present; if not, install com.unity.inputsystem
  - [ ] 3.5 Import Meta XR Simulator from Unity Asset Store (v85.0 for Mac ARM)
  - [ ] 3.6 Verify all packages resolve without errors in Package Manager and Console
- [ ] Task 4: Create project folder structure (AC: #4)
  - [ ] 4.1 Create Assets/_Project/ root
  - [ ] 4.2 Create all script subdirectories under Scripts/: Core/, Core/States/, Core/Errors/, Core/Utilities/, Core/Extensions/, WebView/, Input/, UI/, Audio/, Environment/, Reinforcement/, Platform/
  - [ ] 4.3 Create asset directories: Prefabs/UI/, Prefabs/Environment/, Prefabs/Audio/, ScriptableObjects/, Materials/Environment/, Materials/Audience/, Materials/UI/, Materials/Furniture/, Textures/, Audio/Ambient/, Audio/Audience/, Audio/Reinforcement/, Fonts/, Scenes/, Art/Characters/, Art/Furniture/, Art/Room/, InputActions/, Settings/URP/, Settings/XR/
  - [ ] 4.4 Create Tests/EditMode/ and Tests/EditMode/Mocks/
  - [ ] 4.5 Create Tests/PlayMode/ and Tests/PlayMode/TestScenes/
  - [ ] 4.6 Delete URP template sample content (SampleScene, default example materials/scripts created by template)
  - [ ] 4.7 Create empty StageMind.unity scene in Scenes/ (single-scene architecture — this is the only scene)
  - [ ] 4.8 Create Assets/Plugins/Vuplex/ placeholder with .gitkeep (Vuplex imported in Story 1.3)
  - [ ] 4.9 Add .gitkeep files to empty directories so they are tracked by git
- [ ] Task 5: Create assembly definitions (AC: #5)
  - [ ] 5.1 Create Assets/_Project/Scripts/StageMind.asmdef — references: Unity defaults, TextMeshPro, Unity Input System, Unity Engine (do NOT reference test assemblies)
  - [ ] 5.2 Create Assets/Tests/EditMode/StageMind.Tests.EditMode.asmdef — references: StageMind.asmdef, UnityEngine.TestRunner, UnityEditor.TestRunner; include platforms: Editor only
  - [ ] 5.3 Create Assets/Tests/PlayMode/StageMind.Tests.PlayMode.asmdef — references: StageMind.asmdef, UnityEngine.TestRunner, UnityEditor.TestRunner; include platforms: any
  - [ ] 5.4 Verify that Unity Editor resolves all assembly references without errors
- [ ] Task 6: Create .gitignore and initialize version control (AC: #6)
  - [ ] 6.1 Add Unity .gitignore to StageMind project root (use GitHub's official Unity template)
  - [ ] 6.2 Ensure the .gitignore includes: /Library/, /Temp/, /Obj/, /Build/, /Builds/, /Logs/, /UserSettings/, /.utmp/, *.apk, *.aab, *.unitypackage
  - [ ] 6.3 Initialize git repository (git init) and make initial commit
- [ ] Task 7: Verification pass (all ACs)
  - [ ] 7.1 Open Unity, confirm no errors or warnings in Console related to packages or configuration
  - [ ] 7.2 Confirm File → Build Profiles shows Meta Quest as active platform
  - [ ] 7.3 Confirm Assets/_Project/ contains all expected directories
  - [ ] 7.4 Confirm assembly definitions resolve (no missing reference errors)
  - [ ] 7.5 Confirm .gitignore excludes Library/, Temp/, etc.

## Dev Notes

### Architecture Compliance

This story establishes the project foundation specified in the Architecture document sections: "Starter Template & Technology Foundation" and "Project Structure & Boundaries". Every subsequent story depends on this foundation. [Source: architecture.md#Starter-Template-&-Technology-Foundation]

**Critical constraints from Architecture:**
- **Single-scene architecture** — create one scene `StageMind.unity` in Scenes/. The entire app uses state changes, not scene loads.
- **Hybrid dependency wiring** — constructor injection for pure C# classes, `[SerializeField]` for MonoBehaviour inter-references. No singletons, no FindObjectOfType, no SendMessage.
- **One class per file** — filename must match class name exactly.
- **No XR Interaction Toolkit** — architecture explicitly rejects it. StageMind uses only point-and-click via custom raycaster.
- **No Meta XR Core SDK (OVRPlugin)** — the `com.unity.xr.meta-openxr` package is the correct path.
- **No Meta XR Interaction SDK** — not needed for three-button controller model.
- **No Meta XR Audio SDK** — Unity built-in spatial audio is sufficient.

### Technical Stack Versions (Verified April 2026)

| Component | Version | Source |
|-----------|---------|--------|
| Unity | 6.3 LTS (6000.3.x) | Architecture spec + web research |
| com.unity.xr.openxr | 1.16.1 (stable) | Unity Registry — spec says ≥1.15.1 |
| com.unity.xr.meta-openxr | 2.4.0 (stable) | Unity Registry — spec says ≥2.2 |
| TextMeshPro | Bundled with Unity 6 | Unity Registry |
| Unity Input System | Bundled/installable | Unity Registry |
| Meta XR Simulator | v85.0 (Mac ARM) | Unity Asset Store |
| Vuplex 3D WebView for Android | v4.15 | Vuplex Store — NOT installed in this story (Phase 0 spike, Story 1.5) |

**Important:** Do NOT install Vuplex 3D WebView in this story. It is a paid commercial plugin ($179.99) that will be integrated in Story 1.3 (WebView Interface Contract & Vuplex Integration) and validated during Story 1.5 (WebView Spike). This story creates only the `Scripts/WebView/` directory and `Plugins/Vuplex/` placeholder.

### Platform Configuration Details

**Player Settings (exact values):**
- Scripting Backend: IL2CPP (required for ARM64 Android — ahead-of-time compilation)
- Target Architectures: ARM64 only
- Graphics APIs: Vulkan (primary) — do NOT include OpenGLES3 alongside
- Minimum API Level: Android 10 (API Level 29) — Quest 3 runs Android-based Horizon OS
- API Compatibility Level: .NET Standard 2.1

**XR Plug-in Management:**
- Enable OpenXR for the Android platform
- Add "Meta Quest Touch Pro Controller Interaction Profile" under OpenXR → Interaction Profiles
- Confirm Meta Quest Feature Group is enabled

### Assembly Definition Configuration

**StageMind.asmdef** (runtime):
```json
{
    "name": "StageMind",
    "rootNamespace": "StageMind",
    "references": [
        "Unity.TextMeshPro",
        "Unity.InputSystem"
    ],
    "includePlatforms": [],
    "excludePlatforms": [],
    "allowUnsafeCode": false,
    "autoReferenced": true
}
```

**StageMind.Tests.EditMode.asmdef**:
```json
{
    "name": "StageMind.Tests.EditMode",
    "rootNamespace": "StageMind.Tests.EditMode",
    "references": [
        "StageMind",
        "UnityEngine.TestRunner",
        "UnityEditor.TestRunner"
    ],
    "includePlatforms": ["Editor"],
    "excludePlatforms": [],
    "allowUnsafeCode": false,
    "overrideReferences": true,
    "precompiledReferences": [
        "nunit.framework.dll"
    ],
    "autoReferenced": false,
    "defineConstraints": [
        "UNITY_INCLUDE_TESTS"
    ]
}
```

**StageMind.Tests.PlayMode.asmdef**:
```json
{
    "name": "StageMind.Tests.PlayMode",
    "rootNamespace": "StageMind.Tests.PlayMode",
    "references": [
        "StageMind",
        "UnityEngine.TestRunner",
        "UnityEditor.TestRunner"
    ],
    "includePlatforms": [],
    "excludePlatforms": [],
    "allowUnsafeCode": false,
    "overrideReferences": true,
    "precompiledReferences": [
        "nunit.framework.dll"
    ],
    "autoReferenced": false,
    "defineConstraints": [
        "UNITY_INCLUDE_TESTS"
    ]
}
```

### .gitignore Template

Use the official GitHub Unity .gitignore template from https://github.com/github/gitignore/blob/main/Unity.gitignore with the following additions for Unity 6:
- `/.utmp/` — new Unity 6 temp directory
- `/Builds/` — build output directory (StageMind-specific)

### Project Structure Notes

The complete directory structure matches the Architecture document exactly. Key notes:

- `Assets/_Project/` is the project root — all StageMind code lives here, never in `Assets/` root
- `Assets/Plugins/Vuplex/` will be populated when Vuplex is imported (Story 1.3) — create a placeholder `.gitkeep` but do NOT import Vuplex yet
- `Assets/Tests/` sits alongside `_Project/`, not inside it — Unity Test Framework convention
- Scene file `StageMind.unity` should be created as an empty scene (the default URP sample scene content should be deleted)
- The URP sample assets created by the template (SampleScene, example materials, etc.) should be deleted to keep the project clean

[Source: architecture.md#Complete-Project-Directory-Structure]

### Mac Apple Silicon Considerations

- Unity 6.3 LTS has reported issues with Android builds on Apple Silicon — monitor for fixes in patch releases
- Meta XR Simulator v85.0 is available natively for Mac ARM
- If Android build issues persist, a Windows VM or CI build machine may be needed for final APK builds
- Known issue with Meta XR Simulator: frame flickers may occur under heavy system load

[Source: architecture.md#Development-Environment-Notes]

### Naming Conventions (for any code created)

While this story is primarily project setup, if any scripts or assets are created:
- Classes/interfaces: PascalCase (`GameStateManager`, `IGameState`)
- Private fields: `_camelCase` (`_currentState`)
- `[SerializeField]` private fields: `_camelCase` (`[SerializeField] private AudioManager _audioManager;`)
- Files/folders: PascalCase (`Scripts/Core/`, `StageMind.unity`)
- Materials: PascalCase descriptive (`WallCream.mat`)

[Source: architecture.md#Naming-Conventions]

### Anti-Patterns to Avoid

Do NOT:
- Install XR Interaction Toolkit (architecture explicitly rejects it)
- Install Meta XR Core SDK (legacy path — use Unity OpenXR Meta package)
- Install Meta XR Interaction SDK (not needed)
- Install Meta XR Audio SDK (not needed)
- Install Vuplex yet (Story 1.3/1.5)
- Create multiple scenes (single-scene architecture)
- Leave URP sample content in the project (delete default SampleScene)
- Use public fields for Inspector exposure (always `[SerializeField] private`)

### References

- [Source: architecture.md#Starter-Template-&-Technology-Foundation] — Template selection, package list, initialization steps
- [Source: architecture.md#Complete-Project-Directory-Structure] — Full directory tree with annotations
- [Source: architecture.md#Assembly-Definitions] — Assembly definition configuration
- [Source: architecture.md#Development-Environment-Notes] — Mac Apple Silicon notes
- [Source: architecture.md#Naming-Conventions] — Code and file naming rules
- [Source: architecture.md#Core-Architectural-Decisions] — Decisions 1-8
- [Source: epics.md#Story-1.1] — Story requirements and acceptance criteria
- [Source: GitHub Unity .gitignore] — https://github.com/github/gitignore/blob/main/Unity.gitignore

## Dev Agent Record

### Agent Model Used

{{agent_model_name_version}}

### Debug Log References

### Completion Notes List

### File List
