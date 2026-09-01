---
phase: 01-executable-baseline
plan: 03
subsystem: solo-ui-presentation
tags: [unity, ugui, eventsystem, dotween, lifecycle, tdd]

requires:
  - phase: 01-executable-baseline
    plan: 02
    provides: PlayerHandController input boundary and PlayerHandView presentation boundary
provides:
  - SoloScoringUIController with GUID-preserved scene serialization and explicit solo presentation methods
  - Scene-local forfeit overlay outside GameUIState with Cancel-default native navigation
  - Synchronous modal policy and gameplay-input blocking with direct confirm/cancel event routing
affects: [01-04-solo-manager, 01-05-expansion-tests, solo-mode, scene-wiring]

actuals:
  tokens: 8593
  tasks: 2
  commits: 4

tech-stack:
  added: []
  patterns: [guid-preserving-component-rename, explicit-policy-to-presentation-calls, scene-local-native-modal]

key-files:
  created:
    - Assets/Scripts/UI-Kozeki/SoloScoringUIController.cs
    - Assets/Scripts/UI-Kozeki/SoloScoringUIController.cs.meta
  modified:
    - Assets/Scripts/MahjongGameManager.cs
    - Assets/Scripts/UI-Kozeki/PlayerHandController.cs
    - Assets/Scripts/UI-Kozeki/UiManager.cs
    - Assets/Scenes/SampleScene.unity
    - Assets/Editor/Tests/SoloSessionLifecycleTests.cs

key-decisions:
  - "The existing GameUIManager script GUID remains attached to the renamed SoloScoringUIController asset and scene component."
  - "The forfeit overlay uses the existing scene EventSystem and Buttons; the controller raises direct events and Cancel is selected by default."
  - "PlayerHandController keeps Escape available while a dedicated gameplayInputEnabled flag blocks all ordinary hand input synchronously."

patterns-established:
  - "Presentation boundary: MahjongGameManager sends results only through explicit SoloScoringUIController methods."
  - "Modal boundary: manager policy and input permission change before the scene-local overlay is shown."

requirements-completed: [BASE-05]

coverage:
  - id: D1
    description: "SoloScoringUIController owns the existing solo output references while preserving the former script GUID and ordinary panel behavior."
    requirement: BASE-05
    verification:
      - kind: integration
        ref: "Assets/Editor/Tests/SoloSessionLifecycleTests.cs#two structural lifecycle assertions; Logs/UnityTestGate/20260901-192916-297-results.xml"
        status: pass
    human_judgment: false
  - id: D2
    description: "The scene-local forfeit overlay blocks gameplay synchronously, routes confirm/cancel once, and selects Cancel through native EventSystem navigation."
    requirement: BASE-05
    verification:
      - kind: integration
        ref: "Assets/Editor/Tests/SoloSessionLifecycleTests.cs#ConfirmForfeit_FinalizesOnceWithoutSavingHighScore; Logs/UnityTestGate/20260901-192916-297-results.xml"
        status: pass
    human_judgment: false

duration: 15min
completed: 2026-09-01
status: complete
---

# Phase 1 Plan 3: Solo UI Controller and Forfeit Overlay Summary

**GUID-preserved SoloScoringUIController with explicit solo View ownership and a Cancel-default scene overlay whose input policy is independent of DOTween completion**

## Performance

- **Duration:** 15 min
- **Started:** 2026-09-01T10:15:23Z
- **Completed:** 2026-09-01T10:30:37Z
- **Tasks:** 2
- **Files modified:** 7

## Accomplishments

- Renamed the serialized UI owner to `SoloScoringUIController` without changing GUID `9b978f8eb5c74984b8f91d51ff046652`.
- Moved score, distance, round, call, win, game-over, remaining-time, and hand presentation references behind explicit controller methods.
- Removed `ForfeitConfirmation` from `GameUIState` and the panel map, then wired the existing overlay and Buttons through direct controller events.
- Made modal policy and ordinary-input blocking synchronous while keeping Escape available for cancel and the timer running.

## Task Commits

Each TDD gate and implementation was committed atomically:

1. **Task 1 RED: Define the solo UI rename boundary** - `27cb070` (test)
2. **Task 1 GREEN: Rename the solo UI controller** - `b822ca9` (feat)
3. **Task 2 RED: Define the modal presentation boundary** - `d23c28a` (test)
4. **Task 2 GREEN: Isolate the solo forfeit overlay** - `634a69a` (feat)

## Files Created/Modified

- `Assets/Scripts/UI-Kozeki/SoloScoringUIController.cs` - Owns ordinary game panels, solo output Views, and the scene-local confirmation overlay.
- `Assets/Scripts/UI-Kozeki/SoloScoringUIController.cs.meta` - Preserves the former UI script GUID.
- `Assets/Scripts/MahjongGameManager.cs` - Owns modal policy and calls the presentation boundary explicitly.
- `Assets/Scripts/UI-Kozeki/PlayerHandController.cs` - Keeps Escape routing active while blocking ordinary gameplay input.
- `Assets/Scripts/UI-Kozeki/UiManager.cs` - Uses the renamed solo UI owner while retaining lobby ownership.
- `Assets/Scenes/SampleScene.unity` - Moves View references, overlay wiring, button routes, and explicit navigation to the controller.
- `Assets/Editor/Tests/SoloSessionLifecycleTests.cs` - Verifies ownership, policy ordering, native scene wiring, and exactly-once event routes.

## Decisions Made

- Reused the existing overlay, Buttons, EventSystem, StandaloneInputModule, and DOTween transition instead of adding a modal framework or input manager.
- Kept `PlayerHandController` as the manager's input boundary and gave `SoloScoringUIController` a direct `PlayerHandView` output reference.
- Removed persistent manager callbacks from the scene Buttons; controller lifecycle wiring now raises one confirm or cancel event consumed by the manager.

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 2 - Missing Critical] Added an explicit ordinary-gameplay input permission to PlayerHandController**
- **Found during:** Task 2
- **Issue:** Manager state alone blocked ordinary actions, but the plan required controller-level synchronous input permission while preserving a second Escape as cancel.
- **Fix:** Added `gameplayInputEnabled` and `SetGameplayInputEnabled`; Escape is evaluated before this guard.
- **Files modified:** `Assets/Scripts/UI-Kozeki/PlayerHandController.cs`
- **Verification:** The forfeit lifecycle test observes the flag as false immediately after the request and still cancels through the existing event route.
- **Committed in:** `634a69a`

**2. [Rule 3 - Blocking] Re-ran Unity after the AssetDatabase refreshed the renamed source path**
- **Found during:** Task 1 GREEN
- **Issue:** The first run used a stale Bee source list and reported missing `GameUIManager.cs` before importing `SoloScoringUIController.cs`.
- **Fix:** Re-ran the identical gate after Unity completed its refresh; no source workaround was added.
- **Files modified:** None
- **Verification:** `Logs/UnityTestGate/20260901-191834-479-results.xml` reports both selected tests GREEN.
- **Committed in:** Not applicable

**3. [Rule 3 - Blocking] Reconciled skipped prose progress update**
- **Found during:** Plan closeout
- **Issue:** `state.update-progress` updated the structured completed-plan count but skipped the prose progress field because the phase scope is unscoped.
- **Fix:** Preserved the SDK counters and reconciled the visible current activity and progress bar to 3/5 plans (60%).
- **Files modified:** `.planning/STATE.md`
- **Verification:** `STATE.md` reports Plan 4 of 5, `completed_plans: 3`, and 60%; `ROADMAP.md` reports 3/5 plans executed.
- **Committed in:** Plan metadata commit

---

**Total deviations:** 3 auto-fixed (1 missing critical functionality, 2 blocking tool-state issues)
**Impact on plan:** The input flag is the minimum correctness mechanism required by D-23/D-26; both tool-state fixes changed no production scope.

## Issues Encountered

- Unity's first post-rename compilation used the stale old source path once; the unchanged retry passed after AssetDatabase refresh.
- EditMode does not guarantee automatic lifecycle invocation for inactive fixture components, so the fixture invokes the production `Awake`/`OnEnable` paths explicitly and verifies idempotent wiring.

## TDD Gate Compliance

- Task 1: `27cb070` RED precedes `b822ca9` GREEN.
- Task 2: `d23c28a` RED precedes `634a69a` GREEN.
- Final gate: `GREEN`, 2 passed, 0 failed, exact planned case names in `Logs/UnityTestGate/20260901-192916-297-results.xml`.

## Known Stubs

None.

## User Setup Required

None - no external service configuration required.

## Next Phase Readiness

- Plan 01-04 can rename the solo game manager against explicit input and presentation boundaries.
- Plan 01-05 can add the reserved expansion cases without changing the two-case lifecycle gate established here.

## Self-Check: PASSED

- All seven listed runtime, scene, meta, and test files exist.
- Commits `27cb070`, `b822ca9`, `d23c28a`, and `634a69a` resolve to commit objects.
- The scene references the preserved controller GUID once, contains one overlay, and has no `ForfeitConfirmation` panel-map entry.

---
*Phase: 01-executable-baseline*
*Completed: 2026-09-01*
