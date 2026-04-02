# Sprint Change Proposal — 2026-04-02

**Project:** BMAD / StageMind
**Author:** SM (Scrum Master agent)
**Date:** 2026-04-02
**Change Scope:** Moderate
**Status:** Approved

---

## 1. Issue Summary

### Problem Statement

Story 1.3 ("WebView Interface Contract & Vuplex Integration") cannot proceed as originally specified because the Vuplex 3D WebView plugin costs $180, which conflicts with the goal of building a zero-cost MVP to validate the core immersive-rehearsal thesis.

### Discovery Context

Identified during active implementation of Story 1.3. The developer evaluated the cost of Vuplex and proposed a simpler, free alternative: fetching slide images from Google Slides published export URLs and displaying them as textures in Unity.

### Evidence

- Vuplex 3D WebView: $180 commercial plugin required for MVP
- Google Slides public export endpoint: free, stable, widely available
- The core experience hypothesis ("does immersive VR rehearsal feel real enough?") does not require an embedded browser — static slide images on a projector screen are sufficient for MVP validation

---

## 2. Impact Analysis

### Epic Impact

| Epic | Impact | Description |
|------|--------|-------------|
| Epic 1 | **Direct** | Renamed to "Slide Delivery Viability Gate." Stories 1.3 and 1.5 rewritten for image-fetch approach. |
| Epic 2 | **Moderate** | Stories 2.5, 2.6, 2.7 updated: no embedded browser interaction, error messages adapted for image-fetch errors. FR5/FR6 deferred. |
| Epic 3 | **Moderate** | Story 3.3 updated: slide advancement via texture index swap instead of keyboard event dispatch to WebView. |
| Epic 4–6 | **None** | No changes required. |

### Story Impact

| Story | Status | Change |
|-------|--------|--------|
| 1.3 | in-progress | Complete rewrite: `VuplexWebViewController` → `SlideImageController`. Same `IWebViewController` interface preserved. |
| 1.5 | backlog | Rewritten: spike validates image fetch + texture swap instead of WebView rendering. Latency target reduced to <50ms. |
| 2.5 | backlog | Minor: `WebView.LoadUrl()` → `SlideImageController.LoadUrl()`. FR5 deferred. |
| 2.6 | backlog | Significant: removed browser interaction (click/scroll/navigate), replaced with slide preview arrows + slide counter. FR6 deferred. |
| 2.7 | backlog | Moderate: error messages adapted for image-fetch errors. `WebView.Cleanup()` → `SlideImageController.Cleanup()`. |
| 3.3 | backlog | Moderate: `WebView.SendKeyEvent()` → `SlideImageController.SendKeyEvent()` with texture index swap. Latency <50ms. |

### Artifact Conflicts Resolved

| Artifact | Changes Applied |
|----------|----------------|
| **PRD** | Executive summary, implementation considerations, MVP feature set, FR5/FR6 deferred, NFR14–16 updated, risk matrix recalibrated. |
| **Architecture** | Decision 3 rewritten for `SlideImageController`. Directory structure updated. Data flow diagrams updated. Cross-cutting concern renamed. |
| **Epics** | Epic 1 renamed. Stories 1.3, 1.5, 2.5, 2.6, 2.7, 3.3 updated. FR coverage notes adjusted. |
| **UX Design** | No changes needed — image textures on projector/monitor surfaces are visually identical to WebView-rendered content from the user's perspective. |

### Technical Impact

- **Removed dependency:** Vuplex 3D WebView plugin ($180)
- **New implementation:** `SlideImageController` using `UnityWebRequest` to fetch PNG images from Google Slides export URLs
- **Preserved abstraction:** `IWebViewController` interface unchanged — post-MVP browser upgrade (e.g., SimpleUnity3DWebView) remains a drop-in swap
- **Performance improvement:** Texture swap latency <50ms (vs. <200ms for WebView key dispatch)
- **Scope reduction:** FR5 (interactive browser on laptop) and FR6 (browser navigation controls) deferred to post-MVP
- **Risk reduction:** Eliminated WebView stability/performance risk on Quest 3 hardware

---

## 3. Recommended Approach

### Chosen Path: C — Hybrid (Image Fetch MVP + Post-MVP Browser Upgrade)

**Strategy:** Direct Adjustment — modify/add stories within existing sprint plan.

- **MVP:** `SlideImageController` fetches slide images from Google Slides published URLs via `UnityWebRequest`, stores as `Texture2D[]`, displays via `Graphics.Blit()` to shared `RenderTexture`. Slide navigation = index increment/decrement + texture swap.
- **Post-MVP:** `IWebViewController` abstraction enables drop-in replacement with `SimpleUnity3DWebView` (free, MIT-licensed, Android-compatible) for full embedded browser experience.

**Rationale:**
1. Zero cost for MVP — validates core thesis without financial risk
2. Simpler, more stable — no WebView performance/stability concerns on Quest 3
3. Faster implementation — `UnityWebRequest` + texture swap is well-understood Unity pattern
4. Preserves upgrade path — same interface contract, future browser swap is non-breaking

**Effort Estimate:** Story 1.3 implementation effort reduced (simpler than Vuplex integration). No additional stories needed.

**Risk Assessment:** Low — Google Slides public export URLs are stable and well-documented. Fallback: Google Slides API or server-side rendering.

**Timeline Impact:** Neutral to positive — reduced complexity should accelerate delivery.

---

## 4. Detailed Change Proposals

All edit proposals were reviewed and approved incrementally by the user.

### 4.1 PRD Changes (6 edits applied)
- Executive summary: WebView → slide image fetching for MVP
- Implementation considerations: Vuplex section → "Slide Image Fetching (MVP Approach)" + "Post-MVP Browser Upgrade Path"
- MVP feature set: "slides from WebView" → "slides from fetched images"
- FR5, FR6: marked as deferred
- NFR14–16: updated/deferred for image-fetch approach
- Risk matrix: recalibrated severities (Medium → Low for GPU budget risk)

### 4.2 Architecture Changes (6 edits applied)
- Decision 3: rewritten for `SlideImageController` with MVP + post-MVP sections
- Starter template: Vuplex removed, SimpleUnity3DWebView noted for future
- Directory structure: `SlideImageController.cs` added, `VuplexWebViewController.cs` marked conditional
- Architectural boundary: data flow updated for `UnityWebRequest` → `Texture2D[]` → `Graphics.Blit()`
- Data flow diagram: updated for image-fetch pipeline
- Cross-cutting concern #2: renamed to "Slide Delivery as Swappable Subsystem"

### 4.3 Epics Changes (7 edits applied)
- Epic 1: renamed to "Project Foundation & Slide Delivery Viability Gate"
- Story 1.3: complete rewrite for `SlideImageController` implementation
- Story 1.5: rewritten for slide image fetch spike with <50ms latency target
- Story 2.5: `WebView.LoadUrl()` → `SlideImageController.LoadUrl()`, FR5 deferred
- Story 2.6: browser interaction removed, slide preview with arrows + counter added, FR6 deferred
- Story 2.7: error messages adapted for image-fetch errors, cleanup updated
- Story 3.3: texture index swap replaces WebView key dispatch, <50ms latency

---

## 5. Implementation Handoff

### Change Scope: Moderate

All planning artifacts (PRD, Architecture, Epics) have been updated in-place. The changes affect the currently in-progress story (1.3) and multiple backlog stories.

### Handoff Recipients

| Recipient | Responsibility |
|-----------|---------------|
| **Developer (Dominik)** | Continue Story 1.3 implementation using updated `SlideImageController` spec. The existing `BackendSlideWebViewController` implementation already aligns with this direction. |
| **SM / Create-Story** | When creating story files for 1.5, 2.5, 2.6, 2.7, 3.3 — use updated Epics document as source of truth. |

### Actions Required

1. **Story 1.3 file update** — The existing `1-3-webview-interface-contract-and-vuplex-integration.md` story file should be regenerated or updated to reflect the new acceptance criteria from the Epics document.
2. **Sprint status update** — Rename story keys in `sprint-status.yaml` to reflect new story titles.
3. **Continue development** — Story 1.3 implementation can proceed immediately with the image-fetch approach.

### Success Criteria

- [ ] `SlideImageController` implements `IWebViewController` and fetches slide images from Google Slides published URLs
- [ ] Slide navigation works via index increment/decrement with `Graphics.Blit()` to `RenderTexture`
- [ ] All existing `IWebViewController` tests pass with `SlideImageController`
- [ ] Spike (Story 1.5) validates <50ms texture swap latency on Quest 3 at 72fps
