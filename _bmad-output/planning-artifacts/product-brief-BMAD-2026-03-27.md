---
stepsCompleted: [1, 2, 3, 4, 5, 6]
inputDocuments: []
date: 2026-03-27
author: Dominik
---

# Product Brief: BMAD

## Executive Summary

StageMind (working title) is a lightweight VR presentation rehearsal simulator targeting developers and occasional speakers who need to practice their talks in a realistic environment without paying for expensive coaching platforms. Built for Meta Quest with cross-platform potential via Unity + OpenXR, the app embeds a virtual browser on the stage screen so users can open their slides directly from Google Slides, Canva, or any web-based presentation tool — no file uploads, no format conversion, no backend infrastructure. Step onto a virtual stage with a scalable audience and rehearse your actual talk within minutes of installing. Sold as a one-time purchase, it positions itself as "the rehearsal room, not the coaching platform." Tagline: **"Bring your own slides. We bring the stage."**

---

## Core Vision

### Problem Statement

Millions of developers and professionals give public presentations once or twice a year — at local meetups, company conferences, or community events. They have no realistic way to practice. Rehearsing alone at a desk in front of a monitor fails to simulate the pressure of standing on a stage, facing an audience, and advancing through slides in real time. The gap between desk practice and live performance is where confidence breaks down.

### Problem Impact

Underprepared speakers freeze on stage, lose their pacing, fumble slide transitions, and fail to connect with their audience. In an AI-driven era where writing code matters less and selling yourself matters more, the ability to present confidently is becoming a career-defining skill — yet the tools to practice are either nonexistent or built for a different audience entirely.

### Why Existing Solutions Fall Short

Tools like Virtual Speech offer professional-grade VR presentation training, but they're expensive, subscription-based, and overbuilt for the occasional speaker. The casual user who needs to rehearse one talk doesn't want AI coaching analytics, speech pattern analysis, or a monthly fee. They want a stage, an audience, and their slides on the screen behind them. Current solutions serve the power user and price out everyone else.

### Proposed Solution

A lightweight VR presentation simulator for Meta Quest (with cross-platform potential) that delivers:

- **Virtual stage with adjustable audience** — scale from a 20-person meetup room to a 500-seat conference hall
- **Slide integration via virtual browser** — embedded WebView renders on the stage projector screen. Users open Google Slides, Canva, PowerPoint Online, or any web-based presentation tool directly in-app. No file uploads, no format conversion, no backend. Advance slides with VR controller button. Local PDF fallback for offline/power users as a future option.
- **Frictionless onboarding** — the core metric is "time to stage": open app → open your slides in the in-app browser → you're rehearsing. No account creation, no upload steps, no waiting.
- **One-time purchase** — no subscriptions, no recurring costs, buy once and use whenever you need it
- **Static audience MVP** — realistic enough to trigger spatial awareness and stage presence, with audience behavior customization as a future enhancement

### Key Differentiators

1. **"Rehearsal room, not coaching platform"** — stripped to what matters: stage, audience, your slides. No bloat.
2. **"Bring your own slides"** — works with any web-based presentation tool. Platform-agnostic on content — complements the entire presentation ecosystem instead of competing with it.
3. **Affordable one-time purchase** — built for people who speak once a year, not weekly. Priced accordingly.
4. **Cross-platform foundation** — Unity + OpenXR from day one ensures portability beyond Quest.
5. **Developer-community timing** — as AI commoditizes code, personal branding through speaking becomes the new differentiator.

---

## Target Users

### Primary Users

**Persona: Alex, The Occasional Speaker**

- **Demographics:** 20-40 years old, working professional (most likely tech, but not exclusively)
- **Context:** Gets invited to speak at a local meetup, company conference, or community event once or twice a year
- **Emotional state:** Stressed and anxious after accepting the invitation. Public speaking is a common fear, and Alex knows the gap between agreeing to speak and actually delivering is where confidence erodes
- **Current behavior:** Practices at their desk once, maybe twice. Rehearses in front of a monitor, clicking through slides alone. No sense of the room, the audience, or the spatial pressure of a real stage
- **Motivation:** Wants to feel prepared enough that the nerves don't take over. Doesn't want coaching or analytics — just wants to stand on the stage and run through the talk
- **Success vision:** Walking onto the real stage and thinking "I've been here before"
- **VR ownership:** May own a Quest personally, or may access one through a workplace shared device

### Secondary Users

**Meetup/Conference Organizers**
- Recommend StageMind to accepted speakers as a preparation tool
- Word-of-mouth distribution channel

**Company IT/HR/Team Leads**
- Purchase the app and a Quest headset as a shared office resource
- Set up a "rehearsal station" in a meeting room where employees can book time to practice before internal or external presentations

**Speaking Coaches**
- Use StageMind as a lightweight supplement for clients who need stage exposure without booking a real venue

### B2B Growth Vector

The company office scenario — a Quest with StageMind installed as a shared rehearsal station — represents a planned growth path. V1 targets individual buyers with no B2B-specific features. The product naturally serves the company use case without modification. Future growth may include B2B positioning ("empower your team to present better"), companion web services, and freemium tiers to justify ongoing infrastructure costs.

### User Journey

1. **Discovery:** Alex searches the Meta Quest Store for "presentation practice" after accepting a speaking invitation. Future growth channels include search engine traffic and content marketing targeting people researching presentation skills.
2. **Onboarding:** Installs the app. Opens it. Selects a room size. Opens the in-app browser on the stage screen, navigates to Google Slides, and loads their presentation. Within minutes they're standing on a virtual stage with their slides behind them. No file upload, no account, no friction.
3. **Core Usage:** Adjusts room and audience size to match the expected venue. Runs through the full presentation using VR controller to advance slides in the browser. Repeats sections that feel rough. Does this 2-3 times in the week before the event.
4. **Success Moment ("Aha!"):** Walks onto the real stage at the event and feels a wave of familiarity — "I've been here before." The nerves are manageable. The talk goes well.
5. **Long-term / Retention:** Six months later, Alex gets invited to speak again. This time there's less dread — they know they have a tool to prepare. StageMind reduces the barrier to saying "yes" to speaking opportunities, creating a positive loop: less fear → more speaking → more usage → growing confidence.

### Key Risks & Assumptions (from team review)

- **Assumption to validate:** VR exposure reduces public speaking anxiety for casual users. The entire retention loop depends on this. Early user testing should measure subjective anxiety before/after use.
- **Discovery risk:** Quest Store search volume for "presentation practice" may be low. Organic discovery alone may not drive sufficient adoption. Content marketing and community outreach may be needed earlier than planned.
- **Pricing sensitivity:** One-time purchase price must balance individual impulse-buy accessibility with perceived quality. Too cheap signals low quality to potential B2B buyers. Sweet spot TBD through market testing.

---

## Success Metrics

### User Success

- **Core success indicator:** The user stands on a virtual stage with their own slides displayed behind them on the projector screen and the experience feels real enough to be useful rehearsal — not a toy, not a tech demo, but a functional practice space
- **Frictionless slide display:** User can open their presentation via the in-app browser and be rehearsing within minutes. If the slide setup feels cumbersome, the product has failed its core promise
- **Repeat usage:** User returns to the app the next time they have a presentation to prepare for. This is the strongest signal that the product delivered real value

### Business Objectives

- **Personal utility (primary):** The app works well enough that the creator uses it to prepare for their own presentations. This is the non-negotiable baseline.
- **Quest Store presence:** App is published and available on the Meta Quest Store as a shipped product
- **Organic traction (stretch):** The app finds an audience beyond the creator through Quest Store discovery

### Key Performance Indicators

| KPI | Target | Timeframe |
|-----|--------|-----------|
| App published on Quest Store | Yes | MVP launch |
| Creator uses app for own presentation prep | At least 1 real rehearsal | First 3 months |
| Organic downloads (strangers) | 10 | First 3 months post-launch |
| Repeat usage rate | Any returning user | 12 months |
| Quest Store rating | ≥4.0 stars | 12 months |

### Strategic Note

This is a creator-first product: built to solve the creator's own problem, with commercial viability as a welcome bonus rather than the primary driver. Success is measured first by personal utility, second by learning outcomes, and third by market traction. If the product works for the creator, it likely works for Alex too.

---

## MVP Scope

### Core Features

**1. Lobby Environment**
- Simple VR lobby space where the user lands on app launch
- Embedded browser panel to open and load their presentation (Google Slides, Canva, etc.)
- "Start" button to transition to the stage

**2. Stage Environment**
- One generic conference room / stage setup
- Fixed audience of 50 static attendees seated and facing the speaker
- Projector screen behind the speaker displaying the presentation from the browser opened in lobby
- User stands at a podium/stage position

**3. Slide Control**
- VR controller button press to advance slides forward/backward in the embedded browser
- Slides visible on the projector screen behind the speaker throughout the session

**4. Platform**
- Meta Quest (standalone APK)
- Built with Unity + OpenXR

### Out of Scope for MVP

| Feature | Rationale |
|---------|-----------|
| Audience size selection | Fixed at 50 for MVP; presets (20/50/100/500) in next version |
| Room/venue type selection | Single generic room; additional venues post-MVP |
| Audience behavior/reactions | Static only; behavioral customization is a future enhancement |
| Audio analysis or feedback | No microphone integration; purely visual/spatial rehearsal |
| Presentation timer | Planned as first post-MVP feature |
| Local PDF/file import | Browser-based approach eliminates need; may add as offline fallback later |
| Account system or cloud sync | No backend; standalone app |
| Web companion service | May be unnecessary if browser approach works well |
| Cross-platform (PC VR) | Quest-only for launch; OpenXR foundation enables future porting |
| Speaker notes integration | Post-MVP enhancement |
| Rehearsal recording/review | Post-MVP enhancement |

### MVP Success Criteria

The MVP is successful when:
1. **Creator can rehearse their own talk** — stand on a virtual stage, see their Google Slides on the screen behind them, advance slides with controller, and feel like it's a useful rehearsal
2. **Time to stage < 1 minute** — from app launch to standing on stage with slides loaded
3. **Published on Quest Store** — app is live and discoverable
4. **Stable enough for a full run-through** — no crashes or WebView failures during a 20-30 minute presentation rehearsal

### Future Vision

**Near-term (post-MVP, weeks/months):**
- Audience size presets (20 / 50 / 100 / 500)
- Presentation timer with configurable duration
- Additional venue types (TED-style stage, boardroom, classroom)
- Speaker notes integration (visible to presenter, not audience)

**Mid-term (if traction warrants):**
- Audience behavior customization (nodding, phone checking, restlessness)
- Rehearsal recording and playback for self-review
- Room/audience size slider for fine-grained control
- Cross-platform release (SteamVR / PC VR via OpenXR)

**Conditional (user-driven):**
- Freemium model + web companion (only if browser-based approach proves insufficient)
- B2B features (license management, team analytics)
- Multiplayer / live audience mode (only if users request it)
