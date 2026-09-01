---
phase: 01-executable-baseline
plan: 05
subsystem: executable-baseline-evidence
tags: [unity, editmode, windows-player, lifecycle, tdd, human-verification]

requires:
  - phase: 01-executable-baseline
    plan: 04
    provides: SoloScoringGameManager and scene-local solo mode root lifecycle
provides:
  - Exact three-case trace and thirteen-case lifecycle XML-backed GREEN evidence
  - Successful Windows x64 Player build with durable BuildReport and approved PID-20000 GUI lifecycle
  - Final before/after baseline ledger for the refactored solo ownership path
affects: [phase-02-rule-correctness, phase-07-portfolio-evidence, windows-build, solo-mode]

actuals:
  tokens: 5944
  tasks: 3
  commits: 4

tech-stack:
  added: []
  patterns: [callable-private-input-seam, exact-filtered-unity-gates, fail-closed-human-evidence]

key-files:
  created:
    - .planning/phases/01-executable-baseline/01-05-SUMMARY.md
  modified:
    - Assets/Editor/Tests/SoloSessionLifecycleTests.cs
    - Assets/Scripts/UI-Kozeki/PlayerHandController.cs
    - .planning/phases/01-executable-baseline/01-BASELINE.md

key-decisions:
  - "PlayerHandController routes Update through one private HandleEscape seam so the real same-frame Escape branch is assertion-testable without adding an input abstraction."
  - "The final project evidence is the explicit aggregate 3 + 13 = 16 from two independently filtered XML results, never the unfiltered package suite total."
  - "GUI PASS is recorded only from the user's exact approval of all eight same-PID steps on Player PID 20000."

patterns-established:
  - "Lifecycle gates assert exact case names and counts in parseable XML before accepting GREEN."
  - "Build and GUI evidence remain local and ignored while one tracked ledger records live-derived values."

requirements-completed: [BASE-01, BASE-02, BASE-03, BASE-04, BASE-05]

coverage:
  - id: D1
    description: "The annotated baseline tag, descriptor equality, seed trace, and runtime editor-import boundary remain intact."
    requirement: BASE-01, BASE-02, BASE-03, BASE-04
    verification:
      - kind: integration
        ref: "Logs/UnityTestGate/20260901-200231-641-results.xml; portfolio-baseline -> b18320ec1d9d647900d2173049819bab6bd47175"
        status: pass
    human_judgment: false
  - id: D2
    description: "Thirteen named lifecycle regressions cover modal, timeout, subscription, lobby, restart, rendering, and input-order contracts."
    requirement: BASE-05
    verification:
      - kind: integration
        ref: "Logs/UnityTestGate/20260901-200143-063-results.xml"
        status: pass
    human_judgment: false
  - id: D3
    description: "The final Windows Player completed the eight-step start, cancel, forfeit, lobby, and restart sequence on PID 20000."
    requirement: BASE-05
    verification:
      - kind: manual_procedural
        ref: "Task 2 exact approval; .planning/phases/01-executable-baseline/01-BASELINE.md"
        status: pass
    human_judgment: true
    rationale: "Visible input focus, timer movement, Korean result text, and duplicate-free same-process restart require human observation."

duration: 1h 35min
completed: 2026-09-01
status: complete
---

# Phase 1 Plan 5: Final Executable Baseline Summary

**Exact 3+13 Unity regression evidence, a successful Windows Player build, and an explicitly approved same-PID refactored solo lifecycle**

## Performance

- **Duration:** 1h 35min across the blocking human checkpoint
- **Started:** 2026-09-01T10:51:54Z
- **Completed:** 2026-09-01T12:27:21Z
- **Tasks:** 3
- **Files modified:** 3

## Accomplishments

- Expanded `SoloSessionLifecycleTests` from two to exactly thirteen named post-refactor cases covering input ordering, modal policy, timer priority, event symmetry, scene serialization, result rendering, lobby teardown, and clean restart.
- Kept `MahjongRoundTraceTests` independently GREEN at exactly three cases with `seed=1557 acceptedActions=112 PASS`.
- Revalidated the annotated `portfolio-baseline` target, zero descriptor drift, and zero runtime `UnityEditor` imports.
- Built `Builds/phase1/RiichiNya.exe` successfully with zero errors and preserved its durable BuildReport.
- Recorded the user's exact approval of all eight same-PID GUI observations on Player PID `20000`.

## Task Commits

1. **Task 1 RED: add thirteen lifecycle regressions** - `a24b4a4`
2. **Task 1 GREEN: expose the production Escape branch** - `3645939`
3. **Task 1 evidence: stage automated/build evidence with GUI pending** - `27b57d8`
4. **Task 3: seal the explicitly approved GUI baseline** - `2c47d5f`

Task 2 was the blocking-human GUI checkpoint and produced no file change or commit.

## Files Created/Modified

- `Assets/Editor/Tests/SoloSessionLifecycleTests.cs` - Thirteen exact lifecycle cases using production methods and the loaded `SampleScene` serialization.
- `Assets/Scripts/UI-Kozeki/PlayerHandController.cs` - Minimal private Escape seam preserving the existing first-branch immediate return.
- `.planning/phases/01-executable-baseline/01-BASELINE.md` - Final live-derived automated, build, PID, and approved GUI ledger.

## Decisions Made

- Added only a private boolean Escape seam rather than a generalized input context, wrapper, interface, or new package.
- Kept the trace and lifecycle runs separate so package self-tests cannot inflate the project-authored evidence count.
- Preserved raw XML, logs, build outputs, and PID files as ignored local artifacts; only the Markdown ledger is tracked.

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 2 - Missing Critical Functionality] Added a callable production Escape seam**
- **Found during:** Task 1 RED authoring
- **Issue:** Legacy `Input.GetKeyDown` could not be synthesized reliably in EditMode, so the planned same-frame Escape return could not be invoked as a real production branch.
- **Fix:** Routed `Update` through one private `HandleEscape(bool)` method and asserted that a consumed Escape prevents discard and mahjong-call intents.
- **Files modified:** `Assets/Scripts/UI-Kozeki/PlayerHandController.cs`, `Assets/Editor/Tests/SoloSessionLifecycleTests.cs`
- **Commit:** `3645939`

**2. [Rule 3 - Blocking] Used the loaded scene's EventSystem lifecycle for Cancel selection**
- **Found during:** Task 1 RED verification
- **Issue:** A synthetic EditMode EventSystem was not registered as `EventSystem.current`, producing fixture errors instead of a valid RED assertion.
- **Fix:** Loaded `SampleScene.unity` additively, invoked its real EventSystem lifecycle, and verified the real scene controller selects the serialized Cancel button.
- **Files modified:** `Assets/Editor/Tests/SoloSessionLifecycleTests.cs`
- **Commit:** `a24b4a4`

**3. [Rule 3 - Blocking] Restored the canonical Temp BuildReport from its durable copy**
- **Found during:** Task 3 continuation
- **Issue:** Unity cleaned `Temp/phase1/build-report.txt` between the checkpoint and continuation while `Builds/phase1/build-report.txt` remained intact.
- **Fix:** Restored the canonical ignored Temp evidence path from the durable byte-for-byte report before sealing the ledger.
- **Files modified:** None tracked
- **Commit:** N/A (ignored evidence restoration)

**4. [Rule 3 - Blocking] Reconciled visible STATE progress after the SDK scope skip**
- **Found during:** Plan closeout
- **Issue:** `state.update-progress` updated the structured completed-plan count but skipped the prose activity and progress bar because the phase scope is unscoped.
- **Fix:** Preserved SDK counters and verifying status while reconciling the visible activity and progress bar to 5/5 plans and 100%.
- **Files modified:** `.planning/STATE.md`
- **Commit:** Plan state metadata commit

---

**Total deviations:** 4 auto-fixed (1 missing critical testability seam, 3 blocking evidence/fixture/state issues)
**Impact on plan:** No feature scope, package, assembly descriptor, scene, or public API was added; the production change is one private branch seam.

## Issues Encountered

- The first synthetic EventSystem attempts produced fixture errors rather than RED; the final RED gate discovered all thirteen cases and failed only the intended missing Escape seam assertion.
- Player window handles were transient after launcher return. The final launcher kept PID `20000` alive through the checkpoint, and the user explicitly approved the complete same-PID sequence.

## TDD Gate Compliance

- RED: `Logs/UnityTestGate/20260901-200033-928-results.xml` discovered 13 cases, with 12 passing and the missing production Escape seam failing by assertion.
- GREEN: `Logs/UnityTestGate/20260901-200143-063-results.xml` reports exactly 13 passed and 0 failed after `3645939`.
- Independent trace gate: `Logs/UnityTestGate/20260901-200231-641-results.xml` reports exactly 3 passed and 0 failed.
- Commit order: `a24b4a4` RED precedes `3645939` GREEN.

## Authentication Gates

None. The live Unity runs resolved entitlements and returned XML-backed GREEN without a login checkpoint.

## Known Stubs

None.

## Threat Flags

None. The private input seam adds no network, authentication, file-access, or schema trust boundary.

## User Setup Required

None.

## Next Phase Readiness

- Phase 1 is sealed with exact automated, build, and human evidence.
- Phase 2 can begin rule-correctness work without reopening the refactored solo ownership or baseline verification boundary.

## Self-Check: PASSED

- All three modified plan files and this summary exist.
- Commits `a24b4a4`, `3645939`, `27b57d8`, and `2c47d5f` resolve to commit objects.
- RED, lifecycle GREEN, trace GREEN, BuildReport, executable, and PID-backed ledger evidence exist at their recorded local paths.

---
*Phase: 01-executable-baseline*
*Completed: 2026-09-01*
