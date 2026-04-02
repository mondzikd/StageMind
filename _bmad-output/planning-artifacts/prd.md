---
stepsCompleted: ['step-01-init', 'step-02-discovery', 'step-02b-vision', 'step-02c-executive-summary', 'step-03-success', 'step-04-journeys', 'step-05-domain-skipped', 'step-06-innovation-skipped', 'step-07-project-type', 'step-08-scoping', 'step-09-functional', 'step-10-nonfunctional', 'step-11-polish']
inputDocuments: ['planning-artifacts/product-brief-BMAD-2026-03-27.md']
workflowType: 'prd'
documentCounts:
  briefs: 1
  research: 0
  brainstorming: 0
  projectDocs: 0
classification:
  projectType: 'VR Application (Unity/OpenXR) — mobile_app distribution model'
  domain: 'Presentation Rehearsal / Performance Preparation'
  complexity: 'Low (product-level) — technical risk noted for VR/WebView execution'
  projectContext: 'greenfield'
---

# Product Requirements Document — StageMind

**Author:** Dominik
**Date:** 2026-03-29

## Executive Summary

StageMind is a VR presentation rehearsal simulator for Meta Quest that lets occasional speakers practice their talks on a virtual stage with an audience and their own slides. Built with Unity + OpenXR, the app fetches slides from a published Google Slides link and displays them on the stage screen — no uploads, no format conversion, no backend. Post-MVP, support for additional slide platforms (Canva, PowerPoint Online) and an embedded browser experience are planned via a free open-source WebView integration. The target user speaks publicly once or twice a year at meetups, company events, or community conferences and has no realistic way to simulate the spatial pressure of standing on a real stage. StageMind solves this by providing the experience itself: stand at the podium, see the audience, advance your slides, and run through your talk. Sold as a one-time purchase with no accounts or subscriptions, the app is built by a solo creator using AI-assisted development and targets personal utility first, with commercial viability as a secondary outcome.

### What Makes This Special

Existing VR presentation tools treat public speaking as a skill to be coached — they offer AI analytics, speech pattern scoring, and subscription pricing aimed at power users. StageMind treats it as a space to be occupied and a positive experience to be built. The core insight is that **familiarity and positive association, not instruction, reduce stage anxiety for occasional speakers.** The brain doesn't need feedback or a score; it needs the memory of having been on that stage before — and that memory needs to feel good. StageMind delivers spatial familiarity through VR and reinforces it with positive audience energy regardless of performance, building confidence through encouraging repetition rather than critical analysis. "Bring your own slides. We bring the stage." The product complements the entire presentation ecosystem rather than competing with it, and the deliberate absence of coaching features is the differentiator, not a limitation.

## Project Classification

| Attribute | Value |
|-----------|-------|
| **Project Type** | VR Application (Unity/OpenXR) — mobile_app distribution model (Quest Store APK) |
| **Domain** | Presentation Rehearsal / Performance Preparation |
| **Complexity** | Low (product-level) — technical risk noted for WebView rendering in Unity on Quest hardware |
| **Project Context** | Greenfield — new product, no existing codebase |
| **Platform** | Meta Quest 3 (standalone), cross-platform potential via OpenXR |
| **Business Model** | One-time purchase, no subscriptions, no backend infrastructure |

## Success Criteria

### User Success

- **Spatial rehearsal utility:** The user stands on a virtual stage with their own slides on the projector screen behind them and the experience feels real enough to be useful practice — not a toy, not a tech demo, but a functional rehearsal space.
- **Frictionless slide display:** User opens their presentation via the in-app browser and is rehearsing within minutes. If the slide setup feels cumbersome, the product has failed its core promise.
- **Positive emotional payoff:** The user finishes a rehearsal session feeling more confident than when they started. Post-session reinforcement screen displayed after every completed rehearsal. Specific reinforcement form TBD in UX design.
- **Repeat usage:** User returns to the app the next time they have a presentation to prepare for — the strongest signal of real value.
- **Confidence transfer:** User walks onto the real stage and the nerves are manageable because the spatial experience feels familiar. "I've been here before."

### Business Success

- **Personal utility (primary):** The creator uses the app to prepare for their own presentations. Non-negotiable baseline.
- **Quest Store presence:** App published and available on the Meta Quest Store.
- **Organic traction (stretch):** The app finds an audience beyond the creator through Quest Store discovery.
- **Discovery visibility:** Quest Store search impressions and page views tracked as a leading indicator — distinguishes product problems from discoverability problems.

### Technical Success

Technical success criteria are formally defined in the Non-Functional Requirements section. Key targets: sustained 72fps (NFR1), < 200ms slide advance latency (NFR2), zero crashes in 30-minute sessions (NFR7), time to stage under 2 minutes for first-time users and under 1 minute for returning users (NFR4, NFR5), and reliable WebView rendering across Google Slides, Canva, and PowerPoint Online (NFR14).

### Measurable Outcomes

| KPI | Target | Timeframe |
|-----|--------|-----------|
| App published on Quest Store | Yes | MVP launch |
| Creator uses app for own presentation prep | At least 1 real rehearsal | First 3 months |
| Organic downloads (strangers) | 10 | First 3 months post-launch |
| Quest Store search impressions | Tracked as leading indicator | First 3 months post-launch |
| Repeat usage rate | Any returning user | 12 months |
| Quest Store rating | ≥ 4.0 stars | 12 months |
| Session completion (no crashes) | 100% for 30-min / ≤60 slides on Quest 3 | MVP launch |
| Time to stage (first-time) | < 2 minutes | MVP launch |
| Time to stage (returning) | < 1 minute | MVP launch |

## User Journeys

### Journey 1: Alex — The Rehearsal That Changes Everything (Success Path)

**Opening Scene:** Alex is a 32-year-old backend developer who just said yes to a 20-minute talk at a local DevOps meetup. It's Tuesday. The event is next Thursday. The excitement from accepting has already curdled into dread. Alex has given exactly two talks in their life — both went poorly. They rehearsed at their desk, clicking through slides on a monitor, mumbling through the content. On stage, everything felt different: the room was bigger, the faces were real, the silence between slides was deafening. Alex froze both times and vowed "never again." But here they are, having said yes again because the topic matters to their career.

**Rising Action:** Alex owns a Quest 3 — bought it for Beat Saber, mostly collecting dust. Searching "presentation practice" on the Quest Store, they find StageMind. One-time purchase, no subscription, no account. Installed in two minutes. They open the app and land in the lobby — a clean VR space with a landing page that explains how to load slides: share your deck as a public link, paste the URL here. Alex grabs their phone, opens Google Slides, taps Share → Publish to web, and copies the URL. Back in VR, they paste it into the lobby browser. Slides load instantly — no login, no friction. Hit "Start." The scene transitions. They're standing on a stage. Fifty people are seated in front of them. Their slides are on the screen behind them. Alex's heart rate actually spikes — the spatial pressure is immediate and involuntary, even though they *know* it's not real.

**Climax:** Alex runs through the full talk. They stumble on slide 7, lose their train of thought at slide 12, and realize slide 16 has a typo. They start over. The second run is smoother. The third run, on Thursday morning before the event, feels almost routine. The audience is still sitting there. The stage is still under their feet. But the panic is gone. It's been replaced by something quieter: familiarity. After the third run, the app delivers a moment of positive reinforcement — and Alex realizes they're actually looking forward to the talk.

**Resolution:** That evening, Alex walks onto the real stage at the meetup. The room holds about 40 people. The setup is different — different chairs, different lighting — but the *feeling* is the same. They've been here before. The talk isn't perfect, but it flows. Slides advance cleanly. Alex makes eye contact with the audience. Afterward, someone says "that was really clear." Six months later, when another invitation arrives, Alex says yes without the dread.

**Requirements revealed:** Lobby landing page with slide loading instructions, embedded browser with URL input, stage environment with audience, slide control via controller, post-session positive reinforcement, session stability for 20-30 minutes, Quest Store discovery and purchase flow.

### Journey 2: Alex — When the Link Isn't Public (Edge Case / Recovery)

**Opening Scene:** Same Alex, second rehearsal session. This time Alex has a new deck in Canva. Rushing to practice, they skip the "publish to web" step and paste the regular Canva edit URL into the lobby browser. The browser shows a Canva login wall. The lobby landing page is right there with the instructions — Alex just didn't read them carefully the first time.

**Rising Action:** Alex glances at the lobby landing page again: "Share your slides as a public link for the best experience." They take off the headset, go to their laptop, and set the Canva presentation to "Anyone with the link can view." Copy the new URL. Back in the headset, they paste it. Slides load clean — no login, no auth. The detour took 30 seconds.

**Climax:** Alex runs through the talk. Canva's view-only mode renders predictably in the browser. The rehearsal flows without interruption. Alex learns the pattern: always publish first, then rehearse. It becomes muscle memory — the same way you'd check the projector cable before a real talk.

**Resolution:** The edge case is a one-time learning moment, not a recurring frustration. The lobby's default landing page prevents most users from hitting the login wall in the first place. For users who skip the instructions, the recovery is fast and obvious. Future enhancement: detect login/auth pages in the WebView and surface a contextual tip suggesting the user publish their deck as a public link.

**Requirements revealed:** Lobby landing page with slide loading instructions as default first screen, graceful UX when user hits a login wall, QR code scanning as MVP candidate for faster URL input, VR keyboard as baseline URL input method, WebView must handle published/view-only presentation URLs from Google Slides, Canva, and PowerPoint Online.

### Journey 3: Priya — First Time at the Office Rehearsal Station (Shared Device)

**Opening Scene:** Priya is a frontend engineer at a 200-person software company. She's been asked to present her team's quarterly results at the company all-hands next week. A colleague mentions there's a "rehearsal station" in Meeting Room C — a Quest headset with a presentation practice app. Priya has never used VR before. She books the room for 30 minutes during lunch.

**Rising Action:** Priya puts on the Quest and opens StageMind. She didn't install it, doesn't know what it does, and has no context beyond "it helps you practice presentations." The app opens to the lobby landing page — clear instructions: share your slides as a public link, paste the URL here. Priya pulls out her phone, opens her Google Slides deck, publishes to web, and scans the QR code with the Quest's passthrough cameras (or types the URL on the VR keyboard). Slides load. She hits "Start."

**Climax:** Priya is standing on a virtual stage with her slides behind her and 50 people looking at her. She's startled — the spatial presence is stronger than she expected. She stumbles through the first run, but the second attempt is noticeably better. After finishing, the app delivers positive reinforcement. Priya takes off the headset and thinks "that was actually useful." She books the room again for Thursday.

**Resolution:** At the all-hands, Priya presents with more confidence than she expected. She tells two other colleagues about the rehearsal station. Neither of them installed the app either — they just book the room and put on the headset. The product works for users who have zero setup context, zero VR experience, and zero product awareness.

**Requirements revealed:** Zero-configuration standalone experience, first-launch clarity for users with no prior context or VR experience, lobby landing page must be self-explanatory without any onboarding tutorial, multi-user device compatibility (no leftover state from previous sessions), QR code scanning reinforced as high-value for shared device scenario.

### Journey 4: Alex — Six Months Later (Returning User)

**Opening Scene:** It's been six months since Alex last used StageMind. Another speaking invitation arrived — this time a 30-minute talk at a regional tech conference, bigger venue, higher stakes. Alex picks up the Quest, which has been sitting on the shelf. Opens StageMind.

**Rising Action:** The app launches to the lobby landing page — same clean screen, same clear instructions. No state from last time, no "welcome back" flow, no confusion. Alex doesn't need to remember how the app works because there's almost nothing to remember: paste URL, hit Start, rehearse. They publish their new deck, paste the link, and they're on stage within a minute. The workflow is instantly familiar even after months of disuse.

**Climax:** This time the rehearsal feels different — not because the app changed, but because Alex has. The stage doesn't spike their heart rate the way it did the first time. The familiarity is deeper now. Alex focuses on content and pacing rather than managing anxiety. They run through the talk three times across two days, each time refining different sections.

**Resolution:** The conference talk goes well. Alex is more composed than they've ever been on stage. The positive loop has closed: less fear → more speaking → more usage → growing confidence. StageMind has become part of Alex's speaker toolkit — not something they think about often, but something they reach for every time a talk is coming up. The product's value compounds over time through simplicity and consistency.

**Requirements revealed:** Stateless lobby experience, instantly re-learnable interface (lobby landing page serves as both onboarding and refresher), no "welcome back" or "what's new" interruptions, retention validated through simplicity.

### Journey Requirements Summary

| Capability Area | Revealed By | Priority |
|----------------|-------------|----------|
| Lobby landing page with slide loading instructions (default first screen) | Journey 1, 2, 3, 4 | MVP |
| Embedded browser with URL input (VR keyboard baseline) | Journey 1, 2 | MVP |
| QR code scanning for URL input | Journey 2, 3 | MVP candidate (spike feasibility) |
| Stage environment with 50 static audience | Journey 1, 3 | MVP |
| Slide control via VR controller | Journey 1 | MVP |
| Post-session positive reinforcement | Journey 1, 3 | MVP |
| Session stability (30 min continuous) | Journey 1 | MVP |
| Quest Store listing and purchase flow | Journey 1 | MVP |
| WebView compatibility with published/view-only links (Google Slides, Canva, PowerPoint Online) | Journey 2 | MVP |
| Graceful login-wall handling with contextual guidance | Journey 2 | MVP |
| Zero-configuration standalone experience | Journey 3, 4 | MVP |
| Stateless lobby (no user-specific state between sessions) | Journey 3, 4 | MVP |
| Multi-user device compatibility | Journey 3 | MVP |
| First-launch clarity for zero-context users | Journey 3 | MVP |
| Contextual WebView auth detection with publish tip | Journey 2 | Post-MVP |

## VR Application Specific Requirements

### Project-Type Overview

StageMind is a standalone VR application built with Unity + OpenXR, distributed as an APK through the Meta Quest Store. The app is a single-user, offline-capable experience with one network dependency: the embedded WebView requires internet access to load slides from web-based presentation tools. No backend, no user accounts, no cloud services, no social features.

### Platform Requirements

| Requirement | Detail |
|-------------|--------|
| **Target Hardware** | Meta Quest 3 only for MVP — Quest 2 backport evaluated post-MVP based on performance profiling |
| **Runtime** | Unity + OpenXR, standalone Android APK |
| **Minimum Frame Rate** | 72fps sustained (Quest comfort requirement) |
| **Network** | Required for WebView slide loading; app itself runs standalone |
| **Storage** | Minimal — no local data persistence, no user state, no cached content in MVP |
| **Cross-Platform** | OpenXR foundation enables future SteamVR / PC VR porting; not in MVP scope |

### Device Features & Permissions

| Feature | MVP Usage | Permission Required |
|---------|-----------|-------------------|
| **VR Controllers** | Primary input — slide advancement, UI interaction, browser navigation | Standard (included) |
| **Internet Access** | WebView slide loading from Google Slides, Canva, PowerPoint Online | android.permission.INTERNET |
| **Passthrough Cameras** | QR code scanning for URL input (MVP candidate — spike feasibility) | Camera permission (if implemented) |
| **Spatial Audio** | Not in MVP — post-MVP enhancement for audience/room ambiance | None for MVP |
| **Microphone** | Not in MVP — future option for self-hearing or recording | None for MVP |

### Store Compliance

| Aspect | Approach |
|--------|----------|
| **Distribution** | Meta Quest Store (main store, not App Lab) |
| **Content Rating** | Lowest applicable — no violence, no UGC displayed to others, no social features |
| **WebView Usage** | WebView is a slide rendering tool within a VR experience, not the primary UI. Framing must be clear in store submission. |
| **Review Process** | Meta Quest Store curation review required; plan for review cycles in timeline |
| **Privacy Policy** | Required for store listing. Explicit "no data collection" declaration — no accounts, no analytics, no telemetry in MVP. |
| **Data Collection Disclosure** | Must declare "this app does not collect or transmit user data" in store listing and privacy policy per Meta requirements. |
| **Age Rating** | No age-restricted content; suitable for general audiences |

### Implementation Considerations

- **Slide Image Fetching (MVP Approach):** The MVP skips an embedded browser entirely. Users provide a published Google Slides link, and the app fetches individual slide images via Google Slides' public export endpoints using Unity's `UnityWebRequest`. Each slide becomes a `Texture2D` displayed on the projector screen and confidence monitor. Slide advancement swaps textures — trivially fast and stable. This approach eliminates the highest-risk architectural dependency (WebView rendering on mobile VR GPU) and reduces MVP cost to zero.

- **Slide Fetch Spike Acceptance Criteria:**
  1. Fetches all slides from a published Google Slides deck (30+ slides) as individual images on Quest 3
  2. Slide advance via controller input with < 50ms perceived latency (texture swap)
  3. Maintains 72fps while slide textures are loaded and displayed
  4. Handles published Google Slides URLs reliably (Canva and PowerPoint Online deferred to post-MVP)
  5. No memory leaks during a 30-minute continuous session with periodic slide navigation

- **Go/No-Go Decision Point:** If slide image fetching from Google Slides published URLs does not work reliably on Quest 3, alternative export mechanisms (e.g., Google Slides API image export, server-side rendering) are evaluated. The bar is lower than the original WebView approach — HTTP image fetching is well-understood technology.

- **Post-MVP Browser Upgrade Path:** The `IWebViewController` interface abstraction is preserved in the codebase. A free open-source WebView library ([SimpleUnity3DWebView](https://github.com/t-34400/SimpleUnity3DWebView), MIT license) has been identified as a potential Vuplex replacement. A post-MVP spike can evaluate this library for full embedded browser support, unlocking Canva, PowerPoint Online, and a richer lobby browsing experience.

- **Scene Transitions:** Lobby-to-stage transition must use fade-to-black or cross-fade. Hard cuts cause VR discomfort. Under 3 seconds, no frame drops below 72fps.

- **Stateless Design:** No user data persistence by design. Each session starts clean — no browser history, no cached URLs, no user preferences. Serves both the privacy model and multi-user shared device scenario.

## Project Scoping & Phased Development

### MVP Strategy & Philosophy

**MVP Approach:** Experience MVP — deliver the minimum that creates a believable spatial rehearsal experience with the user's own slides. The product proves one thesis: VR spatial familiarity with positive reinforcement reduces presentation anxiety for occasional speakers.

**Resource Model:** Solo creator with AI-assisted development. All scope decisions assume a single developer. No parallel workstreams, no dedicated QA, no design team.

**Timeline:** Ship when ready. No artificial deadline. Quality and stability take priority over speed — a broken VR experience is worse than no product.

### MVP Feature Set (Phase 1)

**Core User Journeys Supported:**
- Journey 1 (Alex — success path): Full rehearsal flow from lobby to stage to positive reinforcement
- Journey 2 (Alex — edge case): Login wall recovery via public slide links
- Journey 3 (Priya — shared device): Zero-context first use on an office Quest
- Journey 4 (Alex — returning user): Instant re-familiarization after months away

**Must-Have Capabilities:**

| # | Capability | Rationale |
|---|-----------|-----------|
| 1 | Lobby with landing page and embedded browser | Entry point; slide loading instructions prevent friction |
| 2 | VR keyboard URL input | Baseline method to enter slide URLs |
| 3 | Stage environment with 50 static audience | Core spatial pressure simulation |
| 4 | Projector screen rendering slides from fetched images | The "bring your own slides" promise — MVP uses Google Slides published link image export |
| 5 | Slide advance/back via VR controller | Essential rehearsal interaction |
| 6 | Post-session positive reinforcement | Positive association — core differentiator |
| 7 | Lobby-to-stage fade transition | VR comfort requirement (< 3s, no frame drops) |
| 8 | Stateless design (no user data, no cached state) | Supports multi-user, privacy, simplicity |
| 9 | Quest Store submission package | Privacy policy, data disclosure, content rating |

**MVP Candidate (Spike Feasibility):**

| # | Capability | Rationale |
|---|-----------|-----------|
| 1 | QR code scanning for URL input | Significantly improves time-to-stage; high value for shared device scenario |

### Development Sequencing

**Phase 0: Viability Gate (Before All Other Work)**

The WebView spike is the project's go/no-go decision point. Must be completed and evaluated before investing in any other development.

1. **WebView Spike** — Evaluate Vuplex, Android WebView bridge, and CEF against the five acceptance criteria using a real 30-slide Google Slides deck on Quest 3.
2. **Decision:** If spike passes → proceed to Phase 1. If spike fails → project pauses for alternative slide delivery research.

**Phase 1: MVP Build (After Spike Passes)**

1. Lobby environment with landing page and browser panel
2. Stage environment with audience and projector screen
3. Slide control via VR controller
4. Scene transition (fade-to-black)
5. Post-session positive reinforcement
6. QR code scanning (if spike confirms feasibility)
7. Performance optimization and stability testing
8. Quest Store submission preparation and review

### Post-MVP Features

**Phase 2 (Growth):**
- Audience size presets (20 / 50 / 100 / 500)
- Presentation timer with configurable duration
- Additional venue types (TED-style stage, boardroom, classroom)
- Speaker notes integration (visible to presenter, not audience)
- Audience behavior customization (nodding, phone checking, restlessness)
- Enhanced positive feedback tied to audience reactions
- Natural session end detection (auto-detect advancement past final slide)
- Slide position indicator (current slide of total)
- Restart presentation from first slide
- Onboarding guidance for optimal slide formatting
- Contextual WebView auth detection with publish tip
- Login wall detection with contextual guidance
- Companion shortcut guide — in-app instructions for using Quest's built-in browser to copy/paste URLs into StageMind
- Clipboard sharing support — documentation and onboarding tip for Meta's phone-to-Quest clipboard sync via the Meta app

**Phase 3 (Expansion):**
- Spatial audio for room ambiance and audience presence
- Microphone passthrough for self-hearing during rehearsal
- Offline presentation library (download and cache published decks)
- Rehearsal recording and playback for self-review
- Room/audience size slider for fine-grained control
- Cross-platform release (SteamVR / PC VR via OpenXR)
- Quest 2 backport (if performance profiling supports it)
- Personal progress tracking (private, non-competitive)
- Optional anonymous community milestones

**Phase 4 (Conditional — User-Driven):**
- Freemium model + web companion (only if browser approach proves insufficient)
- B2B features (license management, team analytics)
- Multiplayer / live audience mode (only if users request it)

### Risk Mitigation Strategy

**Technical Risks:**

| Risk | Severity | Mitigation |
|------|----------|------------|
| Google Slides image export URLs change or become unreliable | **Medium** | Published Google Slides decks expose slide images via well-known public endpoints. If Google changes the export format, the `SlideImageController` is the only component that needs updating. The `IWebViewController` interface abstraction isolates the rest of the app. A post-MVP browser integration (via free open-source WebView library) provides an alternative slide delivery path. |
| 50 audience members + slide textures exceed Quest 3 GPU budget | Low | Slide textures are static images — dramatically lower GPU cost than a live WebView. Profile early; reduce audience count or LOD if needed. 72fps is non-negotiable. |
| Quest Store review rejects WebView-heavy app | Medium | Frame WebView as slide rendering tool, not primary UI. Prepare clear justification for reviewers. |
| Scene transition causes frame drops or discomfort | Low | Standard VR pattern (fade-to-black); test early in development. |

**Market Risks:**

| Risk | Severity | Mitigation |
|------|----------|------------|
| Low Quest Store search volume for "presentation practice" | High | Track search impressions as leading indicator. Content marketing and community outreach if organic discovery is insufficient. |
| One-time purchase price misalignment | Medium | Market test pricing. Start at a reasonable mid-point; adjust based on conversion data. |
| Users don't perceive VR rehearsal as meaningfully better than desk practice | Medium | The entire thesis rests on spatial familiarity. Early user testing should measure subjective anxiety before/after. |

**Resource Risks:**

| Risk | Severity | Mitigation |
|------|----------|------------|
| Solo developer — single point of failure | Medium | AI-assisted development reduces time per task. No artificial deadlines reduce burnout risk. |
| Scope creep from "just one more feature" | Medium | MVP boundary is firm: 9 must-have capabilities, nothing else. Growth features wait for post-launch validation. |
| Quest Store review cycles add unpredictable delays | Low | Submit early; plan for at least one revision cycle. |

## Functional Requirements

### Lobby & Onboarding

- **FR1:** User can view slide loading instructions upon entering the app, explaining how to share their presentation as a public link
- **FR2:** User can enter a presentation URL using text input in the lobby
- **FR3:** User can scan a QR code containing a presentation URL to load slides *(conditional MVP — contingent on feasibility spike; UX must accommodate whether or not this ships)*
- **FR4:** User can view their presentation slides rendered in the lobby before entering the stage
- **FR5:** ~~User can interact with the embedded browser panel using VR controller-based input (pointing, clicking, scrolling, text entry)~~ *Deferred to post-MVP — no embedded browser in MVP. Slides are fetched as images from the published URL.*
- **FR6:** ~~User can navigate the embedded browser (back, forward, refresh, URL entry) to reach their published presentation~~ *Deferred to post-MVP — no browser navigation in MVP. User enters URL directly and slides load automatically.*
- **FR7:** User can initiate the transition from lobby to stage when ready to rehearse

### Stage & Spatial Experience

- **FR8:** User can stand on a virtual stage in a conference room environment
- **FR9:** User can see a seated audience of 50 static attendees facing the stage
- **FR10:** User can see their presentation slides displayed on a projector screen behind/beside them on stage
- **FR11:** User can look around the stage environment freely (full 360° head tracking)
- **FR12:** User experiences a visual transition (fade-to-black) when moving between lobby and stage

### Rehearsal Control

- **FR13:** User can advance slides forward during rehearsal using a VR controller input
- **FR14:** User can go back to a previous slide during rehearsal using a VR controller input
- **FR15:** User can end a rehearsal session at any time using a VR controller input, triggering the post-session experience

### Post-Session Experience

- **FR16:** User receives positive reinforcement upon ending a rehearsal session
- **FR17:** User can choose to rehearse again or return to the lobby after a session ends

### Error Handling & Guidance

- **FR18:** The lobby landing page instructs users to use public/published slide links before navigating the browser
- **FR19:** User can recover from a failed slide load without restarting the app (return to lobby, re-enter URL)
- **FR20:** User is informed when no internet connection is available and slide loading cannot proceed

### Privacy & Multi-User

- **FR21:** The app operates without requiring any user account, login, or registration
- **FR22:** The app retains no user-specific data between sessions (no browser history, no cached URLs, no user preferences)
- **FR23:** Multiple users can use the app on the same device without conflicts or exposure to another user's data

## Non-Functional Requirements

### Performance

- **NFR1:** Minimum sustained frame rate of 72fps during all scenes, including active WebView rendering on the stage projector screen
- **NFR2:** Slide advancement via controller input has less than 200ms perceived latency from button press to visual update on the projector screen
- **NFR3:** Lobby-to-stage scene transition completes in under 3 seconds with no frame drops below 72fps
- **NFR4:** App launches and reaches the lobby landing page in under 10 seconds from cold start
- **NFR5:** Loading indicator displayed while WebView content loads; first slide visible within 10 seconds on a 25 Mbps connection
- **NFR6:** App remains within Quest 3's thermal comfort zone during a 30-minute session — no thermal warnings, no forced performance reduction or throttling

### Reliability

- **NFR7:** 30-minute continuous rehearsal session with zero crashes, freezes, or WebView hang events using a published Google Slides deck of up to 60 slides on Quest 3
- **NFR8:** If network drops mid-session, slides already rendered remain visible on the projector screen; app does not crash; non-intrusive notification appears when connectivity is restored or when browser navigation is attempted
- **NFR9:** Embedded browser memory usage stays within 15% of initial allocation after 5 consecutive rehearsal sessions (lobby → stage → lobby → stage cycle) with no frame rate degradation below 72fps
- **NFR10:** Controller battery drain during a 30-minute session does not exceed drain rates of comparable single-player Quest applications under similar input frequency

### VR Comfort

- **NFR11:** All scene transitions use fade-to-black or cross-fade — no hard cuts, teleportation, or sudden camera movement that could induce motion sickness
- **NFR12:** User's viewpoint remains stable and grounded at all times — no artificial locomotion, no camera shake, no forced head movement
- **NFR13:** Stage environment maintains consistent spatial scale matching a mid-size conference room (stage area approximately 4m × 3m, audience seating depth 8–10m, ceiling height 3–4m) — no disproportionate elements that break spatial presence

### Integration

- **NFR14:** Slide image fetcher reliably processes published Google Slides URLs, extracting and displaying all slides without missing or corrupted images. Canva and PowerPoint Online support deferred to post-MVP.
- **NFR15:** ~~VR controller input (pointing, clicking, scrolling) maps correctly to browser interaction without input lag or missed clicks~~ *Deferred to post-MVP — no embedded browser interaction in MVP.*
- **NFR16:** ~~Embedded browser supports standard web navigation: URL entry, back, forward, and refresh — each action completes within 1 second under normal network conditions~~ *Deferred to post-MVP — no browser navigation in MVP.*

### Privacy & Data

- **NFR17:** App does not collect or transmit personally identifiable data. Crash reporting, if implemented, must be opt-in with clear user consent and must not include any user content or browsing data
- **NFR18:** No session state persists after the app is closed — browser cache, cookies, and history cleared on exit. *Known UX trade-off: returning users must re-enter their URL. "Last session URL" convenience is a candidate for future phases.*

### Distribution

- **NFR19:** App meets all Meta Quest Store submission requirements including privacy policy, data collection disclosure, content rating, and review guidelines
- **NFR20:** App package size remains under 2GB to meet Quest Store recommended limits for optimal download and installation experience
