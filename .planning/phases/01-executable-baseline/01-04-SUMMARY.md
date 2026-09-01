---
phase: 01-executable-baseline
plan: 04
subsystem: solo-mode-lifecycle
tags: [unity, lifecycle, scene-root, serialization, tdd]

requires:
  - phase: 01-executable-baseline
    plan: 03
    provides: SoloScoringUIController and direct forfeit presentation boundary
provides:
  - SoloScoringGameManager with the preserved manager script GUID and no legacy runtime facade
  - One inactive-by-default SoloScoringModeRoot controlled by UiManager
  - Symmetric input, UI, timer, and round subscriptions across repeated root cycles
affects: [01-05-expansion-tests, solo-mode, four-player-boundary, scene-wiring]

actuals:
  tokens: 8123
  tasks: 3
  commits: 6

tech-stack:
  added: []
  patterns: [guid-preserving-component-rename, mode-root-lifecycle, symmetric-event-subscriptions]

key-files:
  created:
    - Assets/Scripts/SoloScoringGameManager.cs
    - Assets/Scripts/SoloScoringGameManager.cs.meta
  modified:
    - Assets/Scripts/UI-Kozeki/PlayerHandController.cs
    - Assets/Scripts/UI-Kozeki/UiManager.cs
    - Assets/Scripts/UI-Kozeki/MahjongTileGameObject.cs
    - Assets/Scenes/SampleScene.unity
    - Assets/Editor/Tests/SoloSessionLifecycleTests.cs

key-decisions:
  - "The existing manager script GUID remains attached to SoloScoringGameManager while the legacy MahjongGameManager facade is removed after caller migration."
  - "UiManager activates one serialized SoloScoringModeRoot before session start and disables it only on the existing lobby-return route."
  - "The mode owner is scene-local rather than DontDestroyOnLoad so root disable reliably tears down all solo subscriptions and updates."

patterns-established:
  - "Mode lifecycle: component subscriptions live in OnEnable/OnDisable; StartNewGame resets only one session's state."
  - "Mode ownership: PlayerHandController intents flow directly to SoloScoringGameManager, then results flow to SoloScoringUIController."

requirements-completed: [BASE-05]

coverage:
  - id: D1
    description: "SoloScoringGameManager is the sole named solo lifecycle and policy owner with the original serialized script identity."
    requirement: BASE-05
    verification:
      - kind: integration
        ref: "Assets/Editor/Tests/SoloSessionLifecycleTests.cs#two rename and ownership assertions; Logs/UnityTestGate/20260901-194644-831-results.xml"
        status: pass
    human_judgment: false
  - id: D2
    description: "One inactive scene root controls solo activation, lobby teardown, and exact event subscription counts across repeated cycles."
    requirement: BASE-05
    verification:
      - kind: integration
        ref: "Assets/Editor/Tests/SoloSessionLifecycleTests.cs#StartNewGame_Twice_DetachesAndResetsSession; Logs/UnityTestGate/20260901-194644-831-results.xml"
        status: pass
    human_judgment: false

duration: 15min
completed: 2026-09-01
status: complete
---

# Phase 1 Plan 4: Solo Mode Lifecycle Owner Summary

**GUID-preserved SoloScoringGameManager behind one scene-local mode root with deterministic activation, teardown, and direct input-to-policy-to-presentation routing**

## Performance

- **Duration:** 15 min
- **Started:** 2026-09-01T10:33:00Z
- **Completed:** 2026-09-01T10:47:35Z
- **Tasks:** 3
- **Files modified:** 7

## Accomplishments

- Renamed `MahjongGameManager` to `SoloScoringGameManager` while preserving Unity GUID `83be086a716bef149853d38249179bd7` and migrating every runtime caller.
- Removed the bounded compatibility facade and confirmed no `MahjongGameManager` reference remains under `Assets/Scripts`.
- Added one inactive `SoloScoringModeRoot` containing the manager, solo UI controller, score service, timer, and game canvas/controller hierarchy while leaving the single `EventSystem` outside.
- Made `UiManager` activate the root before `StartNewGame` and disable it on lobby return; game-over presentation keeps the root active.
- Verified exact handler removal and reattachment for controller, UI, timer, old round, and replacement round across a root cycle.

## Task Commits

1. **Task 1 RED: define solo manager rename boundary** - `a94c18a`
2. **Task 1 asset move: preserve manager script and meta identity** - `e0b22b5`
3. **Task 1 GREEN: complete the named owner implementation** - `0a0c599`
4. **Task 2: migrate runtime callers and remove compatibility facade** - `bb4c480`
5. **Task 3 RED: define mode-root lifecycle and subscription contract** - `45db7fc`
6. **Task 3 GREEN: implement the scene-local mode root lifecycle** - `93d7106`

## Files Created/Modified

- `Assets/Scripts/SoloScoringGameManager.cs` - Renamed solo lifecycle, policy, timer, round, and finalization owner.
- `Assets/Scripts/SoloScoringGameManager.cs.meta` - Preserved original manager GUID across the asset rename.
- `Assets/Scripts/UI-Kozeki/PlayerHandController.cs` - Uses the named solo owner for session events and state gating.
- `Assets/Scripts/UI-Kozeki/UiManager.cs` - Owns serialized mode-root activation and lobby teardown.
- `Assets/Scripts/UI-Kozeki/MahjongTileGameObject.cs` - Uses the renamed manager's existing tile database boundary.
- `Assets/Scenes/SampleScene.unity` - Contains one inactive solo root, updated hierarchy, serialized root reference, and renamed manager object.
- `Assets/Editor/Tests/SoloSessionLifecycleTests.cs` - Verifies renamed asset resolution, root structure, and exact subscription counts.

## Decisions Made

- Kept the manager's existing GUID and moved the `.cs` and `.meta` together instead of replacing the component.
- Kept `UiManager` outside the mode root as the lobby/start owner; no registry, base manager, second scene, or mode abstraction was added.
- Removed `DontDestroyOnLoad` from the solo manager because a mode-root child must remain scene-local for disable-based lifecycle cleanup.

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 3 - Blocking] Retried Unity after the first post-rename stale source-path compile**
- **Found during:** Task 1 GREEN verification
- **Issue:** Unity's first import still referenced the removed `Assets/Scripts/MahjongGameManager.cs` path and produced CS2001 before tests ran.
- **Fix:** Re-ran the exact unchanged gate after AssetDatabase refresh; compilation and both selected tests passed.
- **Files modified:** None
- **Verification:** `Logs/UnityTestGate/20260901-193749-350-results.xml` is GREEN with 2 passed and 0 failed.
- **Committed in:** N/A (verification-only recovery)

**2. [Rule 3 - Blocking] Completed Task 1 content after a rename-only staging pathspec split**
- **Found during:** Task 1 GREEN commit
- **Issue:** Including the already-missing old source path in `git add` stopped the staging command, while the prior `git mv` remained staged and committed alone.
- **Fix:** Preserved the non-destructive rename commit and immediately committed the remaining type/test content separately without rewriting shared history.
- **Files modified:** `Assets/Scripts/SoloScoringGameManager.cs`, `Assets/Editor/Tests/SoloSessionLifecycleTests.cs`
- **Verification:** Subsequent Task 1, Task 2, Task 3, and final gates all passed the exact two selected cases.
- **Committed in:** `e0b22b5`, `0a0c599`

**3. [Rule 3 - Blocking] Reconciled visible STATE progress after an SDK scope skip**
- **Found during:** Plan closeout
- **Issue:** `state.update-progress` updated the structured completed-plan count but skipped the prose activity and progress bar because the phase scope is unscoped.
- **Fix:** Preserved SDK counters and reconciled the visible activity and progress bar to Plan 5 of 5 with 4/5 plans complete (80%).
- **Files modified:** `.planning/STATE.md`
- **Verification:** `STATE.md` reports Plan 5 of 5, `completed_plans: 4`, and 80%; `ROADMAP.md` reports 4/5 plans executed.
- **Committed in:** Plan state metadata commit

---

**Total deviations:** 3 auto-fixed blocking tool/import issues
**Impact on plan:** Production scope and behavior did not expand; the only implementation effect was one extra Task 1 commit.

## Issues Encountered

- Unity required one unchanged retry after the GUID-preserving script rename refreshed its generated source list.
- The mode root had to remain under the existing main Canvas so the reused `Game Canvas` retained its rendering ancestry; non-UI mode services are children of that root and remain scene-local.

## TDD Gate Compliance

- Task 1: `a94c18a` RED precedes `0a0c599` GREEN, with `e0b22b5` preserving the file/meta move between them.
- Task 3: `45db7fc` RED precedes `93d7106` GREEN.
- Final gate: `GREEN`, 2 passed, 0 failed, exact planned case names in `Logs/UnityTestGate/20260901-194644-831-results.xml`.

## Known Stubs

None.

## User Setup Required

None - no external service configuration required.

## Next Phase Readiness

- Plan 01-05 can add the reserved thirteen-case expansion against the stable named solo owner and mode-root lifecycle.
- The four-player phase can add its own manager/root without inheriting solo timer, score, UI, or finalization policy.

## Self-Check: PASSED

- All seven listed runtime, scene, meta, and test files exist.
- Commits `a94c18a`, `e0b22b5`, `0a0c599`, `bb4c480`, `45db7fc`, and `93d7106` resolve to commit objects.
- The final XML-backed gate reports the exact two planned cases GREEN, and the imported manager MonoScript resolves to `SoloScoringGameManager`.

---
*Phase: 01-executable-baseline*
*Completed: 2026-09-01*
