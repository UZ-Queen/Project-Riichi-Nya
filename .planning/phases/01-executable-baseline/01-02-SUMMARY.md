---
phase: 01-executable-baseline
plan: 02
subsystem: gameplay-input-presentation
tags: [unity, editmode, input, view, lifecycle, tdd]

requires:
  - phase: 01-executable-baseline
    plan: 01
    provides: Proven solo forfeit and same-process restart lifecycle
provides:
  - PlayerHandController input and selection boundary with a separate ForfeitRequested session intent
  - PlayerHandView ownership of tile creation, hand rendering, tsumo presentation, and selection highlighting
  - GUID-preserving controller rename and exact two-case lifecycle regression evidence
affects: [01-03-ui-controller, 01-04-solo-manager, solo-mode, scene-wiring]

actuals:
  tokens: 7224
  tasks: 2
  commits: 4

tech-stack:
  added: []
  patterns: [controller-view-responsibility-split, idempotent-direct-event-subscription, unity-guid-preserving-rename]

key-files:
  created:
    - Assets/Scripts/UI-Kozeki/PlayerHandView.cs
    - Assets/Scripts/UI-Kozeki/PlayerHandView.cs.meta
  modified:
    - Assets/Scripts/UI-Kozeki/PlayerHandController.cs
    - Assets/Scripts/UI-Kozeki/PlayerHandController.cs.meta
    - Assets/Scripts/MahjongGameManager.cs
    - Assets/Scripts/AL-1S/_Enums_Ecchi_Nun_Mari.cs
    - Assets/Scenes/SampleScene.unity
    - Assets/Editor/Tests/SoloSessionLifecycleTests.cs

key-decisions:
  - "The existing hand GameObject and tile hierarchy remain intact; PlayerHandView is a second component wired with its own committed GUID."
  - "MahjongGameManager subscribes directly and idempotently to discard, mahjong-call, and ForfeitRequested events from PlayerHandController."
  - "The PlayerHand asset GUID remains f741381e994254649afad56bd8fdc47a across the controller rename."

patterns-established:
  - "Input boundary: Escape emits ForfeitRequested before the gameplay-state guard and returns in the same frame."
  - "Presentation boundary: controller forwarding methods are the only manager-facing route to PlayerHandView."

requirements-completed: [BASE-05]

coverage:
  - id: D1
    description: "PlayerHandView owns existing tile creation, hand and tsumo rendering, action indicators, and selected highlighting without cloning the scene hierarchy."
    requirement: BASE-05
    verification:
      - kind: integration
        ref: "Assets/Editor/Tests/SoloSessionLifecycleTests.cs#two structural lifecycle assertions; Logs/UnityTestGate/20260901-191031-353-results.xml"
        status: pass
    human_judgment: false
  - id: D2
    description: "PlayerHandController emits a separate ForfeitRequested intent, the manager subscribes once, and PlayerCallType contains only mahjong actions."
    requirement: BASE-05
    verification:
      - kind: integration
        ref: "Assets/Editor/Tests/SoloSessionLifecycleTests.cs#ConfirmForfeit_FinalizesOnceWithoutSavingHighScore; Logs/UnityTestGate/20260901-191031-353-results.xml"
        status: pass
    human_judgment: false

duration: 17min
completed: 2026-09-01
status: complete
---

# Phase 1 Plan 2: Player Hand Controller/View Boundary Summary

**GUID-preserving PlayerHandController/View split with a distinct forfeit session intent and two XML-backed lifecycle regressions remaining GREEN**

## Performance

- **Duration:** 17 min
- **Started:** 2026-09-01T09:54:38Z
- **Completed:** 2026-09-01T10:11:04Z
- **Tasks:** 2
- **Files modified:** 9

## Accomplishments

- Moved tile instantiation, hand and tsumo rendering, action indicators, and selected highlighting into `PlayerHandView` on the existing scene object.
- Renamed the input owner to `PlayerHandController` while preserving the original script GUID and scene component reference.
- Replaced `PlayerCallType.Forfeit` with a direct `ForfeitRequested` session intent that handles Escape before the gameplay-input gate and returns immediately.
- Kept manager subscriptions direct, symmetric, and idempotent; exactly the two planned lifecycle cases pass from committed HEAD.

## Task Commits

Each TDD gate and implementation was committed atomically:

1. **Task 1 RED: Define the hand rendering boundary** - `ffb4106` (test)
2. **Task 1 GREEN: Extract hand rendering view** - `a091010` (feat)
3. **Task 2 RED: Define the forfeit input boundary** - `060a928` (test)
4. **Task 2 GREEN: Separate the forfeit session intent** - `d100521` (feat)

## Files Created/Modified

- `Assets/Scripts/UI-Kozeki/PlayerHandView.cs` - Owns the existing tile objects and all hand presentation mutations.
- `Assets/Scripts/UI-Kozeki/PlayerHandController.cs` - Owns keyboard polling, selection state, and discard/call/forfeit intents.
- `Assets/Scripts/MahjongGameManager.cs` - Subscribes directly and idempotently to the three controller intent channels.
- `Assets/Scripts/AL-1S/_Enums_Ecchi_Nun_Mari.cs` - Removes session-level forfeit from `PlayerCallType`.
- `Assets/Scenes/SampleScene.unity` - Wires the new view component to existing prefab, holders, indicators, and controller.
- `Assets/Editor/Tests/SoloSessionLifecycleTests.cs` - Verifies both responsibility boundaries and the production event route within the two lifecycle cases.

## Decisions Made

- Kept both components on the existing hand GameObject and reused the current prefab, holders, and action indicators; no prefab, pooling, seat, or input-context abstraction was added.
- Preserved `f741381e994254649afad56bd8fdc47a` by moving the controller source and meta together.
- Used direct controller-to-manager events and `-=`/`+=` subscription normalization so repeated enable paths cannot duplicate an intent handler.

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 3 - Blocking] Reconciled skipped prose progress update**
- **Found during:** Plan closeout
- **Issue:** `state.update-progress` updated the structured completed-plan count but skipped the prose progress field because it classified the phase scope as unscoped.
- **Fix:** Preserved the SDK-updated counters and reconciled the visible current-plan activity and progress bar to 2/5 plans (40%).
- **Files modified:** `.planning/STATE.md`
- **Verification:** `STATE.md` reports Plan 3 of 5, `completed_plans: 2`, and 40% progress; `ROADMAP.md` reports 2/5 plans executed.
- **Committed in:** plan metadata commit

---

**Total deviations:** 1 auto-fixed (1 blocking issue)
**Impact on plan:** Documentation-only reconciliation; runtime scope and verification are unchanged.

## Issues Encountered

- Unity's first run after adding and later renaming a script compiled a stale AssetDatabase source list before importing the new path. Each log then showed a successful refresh/recompile; rerunning the identical gate produced valid XML.
- EditMode did not automatically invoke the inactive fixture manager's `OnEnable` path. The fixture now invokes that production lifecycle method explicitly and asserts exactly three manager-owned handlers across discard, call, and forfeit events.

## TDD Gate Compliance

- Task 1: `ffb4106` RED precedes `a091010` GREEN.
- Task 2: `060a928` RED precedes `d100521` GREEN.
- Final gate: `GREEN`, 2 passed, 0 failed, exact planned case names in `Logs/UnityTestGate/20260901-191031-353-results.xml`.

## User Setup Required

None - no external service configuration required.

## Next Phase Readiness

- Plan 01-03 can rename and isolate the in-game UI controller while keeping `PlayerHandController -> MahjongGameManager -> UI` as the proven route.
- No package, asmdef, prefab extraction, generalized hand system, or additional scene was introduced.

## Self-Check: PASSED

- All listed created and modified runtime/test files exist.
- Commits `ffb4106`, `a091010`, `060a928`, and `d100521` resolve to commit objects.
- The scene references the preserved controller GUID and the committed view GUID exactly once each.

---
*Phase: 01-executable-baseline*
*Completed: 2026-09-01*
