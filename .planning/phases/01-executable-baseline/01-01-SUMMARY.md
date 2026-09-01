---
phase: 01-executable-baseline
plan: 01
subsystem: gameplay-lifecycle
tags: [unity, editmode, windows-player, lifecycle, tdd, tmp]

requires: []
provides:
  - Annotated portfolio baseline tag with deterministic seed-1557 trace evidence
  - Idempotent solo timeout/forfeit finalization and clean same-process restart path
  - Licensed five-case EditMode gate, durable Windows BuildReport, and approved GUI evidence
affects: [01-02-lifecycle-expansion, 01-03-final-baseline, solo-mode, windows-build]

actuals:
  tokens: 19642
  tasks: 4
  commits: 7

tech-stack:
  added: []
  patterns: [predefined-Assembly-CSharp-Editor-tests, single-session-owner, durable-build-report]

key-files:
  created:
    - Assets/Editor/Tests/MahjongRoundTraceTests.cs
    - Assets/Editor/Tests/SoloSessionLifecycleTests.cs
    - Assets/Scripts/GameEndReason.cs
    - .planning/phases/01-executable-baseline/01-BASELINE.md
  modified:
    - Assets/Editor/Phase1Build.cs
    - Assets/Scripts/MahjongGameManager.cs
    - Assets/Scripts/UI-Kozeki/PlayerHand.cs
    - Assets/Scripts/UI-Kozeki/GameUIManager.cs
    - Assets/Scripts/UI-Kozeki/UiGameOver.cs
    - Assets/Scenes/SampleScene.unity

key-decisions:
  - "MahjongGameManager remains the sole owner of start, pending-forfeit, finalization, persistence, and restart cleanup."
  - "A private pending-forfeit flag distinguishes confirmation from unrelated Processing state."
  - "Windows builds write a durable report beside the generated Player before the Editor exits, then restore the canonical ignored Temp evidence path."
  - "New Korean lifecycle labels reuse the existing 경기천년 2K TMP atlas and shared material."

patterns-established:
  - "Lifecycle tests: RED first through the official Unity gate, then GREEN with exact discovered case counts."
  - "Evidence: XML, logs, BuildReport, executable, and PID remain ignored while one tracked baseline ledger records live values."

requirements-completed: [BASE-01, BASE-02, BASE-03, BASE-04, BASE-05]

coverage:
  - id: D1
    description: "A bounded seed-1557 solo trace is reproducible through public round APIs."
    requirement: BASE-02
    verification:
      - kind: unit
        ref: "Assets/Editor/Tests/MahjongRoundTraceTests.cs#three trace cases; Temp/phase1/editmode-initial.xml"
        status: pass
    human_judgment: false
  - id: D2
    description: "Confirmed forfeit finalizes once without saving high score and repeated start resets subscriptions and session state."
    requirement: BASE-05
    verification:
      - kind: integration
        ref: "Assets/Editor/Tests/SoloSessionLifecycleTests.cs#two lifecycle cases; Temp/phase1/editmode-initial.xml"
        status: pass
    human_judgment: false
  - id: D3
    description: "The licensed Windows Player completes start, Korean forfeit confirmation, result, menu return, and fresh restart on one PID."
    requirement: BASE-03
    verification:
      - kind: manual_procedural
        ref: "Task 3 approved checklist on Player PID 17432"
        status: pass
    human_judgment: true
    rationale: "Visible timer behavior, Korean glyph rendering, modal input blocking, and one-reaction restart behavior require human observation."

duration: 2d 20h 45m
completed: 2026-09-01
status: complete
---

# Phase 1 Plan 1: Executable Baseline Walking Skeleton Summary

**Deterministic solo tracing, idempotent forfeit/restart ownership, and a licensed Windows Player path verified by five EditMode cases and an approved same-PID GUI run**

## Performance

- **Duration:** 2d 20h 45m across resumed checkpoints
- **Started:** 2026-08-29T21:04:49+09:00
- **Completed:** 2026-09-01T17:49:30+09:00
- **Tasks:** 4
- **Files modified:** 18

## Accomplishments

- Preserved `portfolio-baseline` as an annotated tag at `b18320ec1d9d647900d2173049819bab6bd47175` and added a bounded, machine-readable seed-1557 solo trace.
- Made `MahjongGameManager` own a confirmed-forfeit path, idempotent timeout/forfeit finalization, persistence policy, and clean repeated start while keeping the timer active during confirmation.
- Proved the five selected project cases GREEN, produced a successful Windows x64 BuildReport and executable, and received explicit approval for the same-PID GUI lifecycle with correctly rendered Korean labels.

## Task Commits

Each task was committed atomically:

1. **Task 1 RED: Add failing deterministic trace cases** - `2eebf7a` (test)
2. **Task 1 GREEN: Add seeded trace and Windows build boundary** - `d88f86d` (feat)
3. **Task 1 build repair: Ignore empty sound groups** - `ad3c5e7` (fix)
4. **Task 2 RED: Add failing lifecycle tests** - `081ab1d` (test)
5. **Task 2 GREEN: Wire solo session lifecycle** - `9ca5909` (feat)
6. **Task 3 repair: Use Korean font for lifecycle labels** - `7a783e9` (fix)
7. **Task 4: Record approved executable baseline** - `8df5c24` (docs)

## Files Created/Modified

- `Assets/Editor/Tests/MahjongRoundTraceTests.cs` - Bounded seed trace, first-mismatch diagnostics, and trace-field contract.
- `Assets/Editor/Tests/SoloSessionLifecycleTests.cs` - Initial forfeit idempotency and repeated-start regressions.
- `Assets/Editor/Phase1Build.cs` - Strict Windows x64 build entry with durable successful-report evidence.
- `Assets/Scripts/GameEndReason.cs` - Explicit timeout and forfeit result reason.
- `Assets/Scripts/MahjongGameManager.cs` - Single session owner and idempotent finalization path.
- `Assets/Scripts/UI-Kozeki/PlayerHand.cs` - Symmetric lifecycle subscriptions and input gating.
- `Assets/Scripts/UI-Kozeki/GameUIManager.cs` - Forfeit-confirmation panel state.
- `Assets/Scripts/UI-Kozeki/UiGameOver.cs` - Korean end-reason presentation.
- `Assets/Scenes/SampleScene.unity` - Confirm/cancel modal, result reason label, callbacks, and existing Korean TMP resources.
- `.planning/phases/01-executable-baseline/01-BASELINE.md` - Live before/after evidence ledger.

## Decisions Made

- Kept all application lifecycle ownership in the existing `MahjongGameManager`; no second lifecycle or modal abstraction was introduced.
- Used an explicit pending-forfeit flag because `GameState.Processing` already covers non-modal domain work.
- Kept raw test/build/player artifacts ignored and recorded only their live-derived values in the tracked baseline ledger.
- Reused the project's Korean TMP atlas after human verification exposed LiberationSans missing-glyph boxes.

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 3 - Blocking] Ignored empty serialized sound groups**
- **Found during:** Task 1 Windows build verification
- **Issue:** Empty serialized sound groups caused `SoundArchive` initialization failure and blocked the executable boundary.
- **Fix:** Safely ignored empty groups without changing valid sound lookup behavior.
- **Files modified:** `Assets/Scripts/SoundArchive/SoundArchive.cs`
- **Verification:** Windows Player build completed successfully.
- **Committed in:** `ad3c5e7`

**2. [Rule 3 - Blocking] Preserved BuildReport before explicit Editor exit**
- **Found during:** Task 2 Windows build verification
- **Issue:** `-quit` could terminate managed execution before report writing, and Unity shutdown removed a report first created under its managed `Temp` directory.
- **Fix:** Removed `-quit`, wrote and flushed the report beside the generated Player, exited explicitly after success, and restored the canonical ignored `Temp/phase1/build-report.txt` evidence path.
- **Files modified:** `Assets/Editor/Phase1Build.cs`
- **Verification:** BuildReport recorded `Succeeded`, the fresh executable existed, and the Player launched.
- **Committed in:** `9ca5909`

**3. [Rule 1 - Bug] Assigned the existing Korean TMP font to new labels**
- **Found during:** Task 3 human GUI verification
- **Issue:** Four new Korean labels used LiberationSans and rendered as missing-glyph boxes.
- **Fix:** Reused `경기천년 2K` and its existing shared material for only those four labels.
- **Files modified:** `Assets/Scenes/SampleScene.unity`
- **Verification:** Five-case GREEN gate, successful rebuild, and explicit repeated human GUI approval.
- **Committed in:** `7a783e9`

---

**Total deviations:** 3 auto-fixed (1 bug, 2 blocking issues)
**Impact on plan:** Each change was required for truthful build or GUI verification and introduced no new package, font asset, lifecycle owner, or feature scope.

## Issues Encountered

- Unity licensing initially blocked batch execution. The user completed the visible licensing action; subsequent official test and build runs resolved entitlements and passed.
- An unfiltered suite included unrelated package self-tests after concurrent package changes. The official project filter discovered exactly the planned three trace and two lifecycle cases, all passing.

## Authentication Gates

- **Task 2 Unity licensing:** Required the user to complete Unity Hub licensing interactively. Outcome: resolved; fresh test and build logs support `Licensing status: PASS`.

## User Setup Required

None - no external service configuration required.

## Next Phase Readiness

- Plan 01-02 can expand the same lifecycle fixture and production owner without changing the proven scene path.
- The only remaining working-tree modifications are preserved user-owned planning/package/ProjectSettings changes.

## Self-Check: PASSED

- All listed created/modified files exist.
- All seven task and evidence commits resolve to commit objects.

---
*Phase: 01-executable-baseline*
*Completed: 2026-09-01*
