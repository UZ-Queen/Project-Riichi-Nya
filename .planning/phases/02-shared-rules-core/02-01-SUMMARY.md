---
phase: 02-shared-rules-core
plan: 01
subsystem: shared-rules-core
tags: [unity, editmode, mahjong-wall, fisher-yates, value-equality, tdd]

requires:
  - phase: 01-executable-baseline
    provides: Exact filtered Unity test gate, solo lifecycle ownership, and 4+15 regression baseline
provides:
  - Exact 136-tile shared wall with one red five in each suit
  - Seeded descending Fisher-Yates using the shared shuffle helper
  - TileID-only MahjongTile equality, hashing, and comparison with physical red preservation
  - XML-backed Regression and Conformance fixtures plus a Phase 2 evidence ledger
affects: [02-02-decomposition, 02-03-dora, 02-05-solo-adapter, phase-04-hanchan]

actuals:
  tokens: null
  tasks: 2
  commits: 3

tech-stack:
  added: []
  patterns: [single-shared-wall-factory, physical-tile-sequence-comparison, index-based-removal, fail-closed-unity-evidence]

key-files:
  created:
    - Assets/Scripts/AL-1S/MahjongWall.cs
    - Assets/Editor/Tests/Phase2RegressionTests.cs
    - Assets/Editor/Tests/Phase2ConformanceTests.cs
    - .planning/phases/02-shared-rules-core/02-EVIDENCE.md
  modified:
    - Assets/Scripts/AL-1S/MahjongTileAndBlock.cs
    - Assets/Scripts/AL-1S/Utilities.cs
    - Assets/Scripts/AL-1S/MahjongRound.cs

key-decisions:
  - "MahjongWall constructs each of 34 TileIDs four times, replacing exactly one five per suit with its red physical form, then delegates shuffling to Utilities."
  - "MahjongTile equality, hash, operators, and CompareTo use TileID only; code that selects a physical tile removes by index and seeded wall evidence compares TileID plus isAkaDora."
  - "Dora mutation was removed from wall generation; the original isAkaDora property remains while later plans own indicator resolution."

patterns-established:
  - "Physical sequence evidence: compare both TileID and isAkaDora whenever red-five position matters."
  - "Fail-closed Unity TDD: only assertion-only XML is RED and only positive selected, zero non-passed XML is GREEN."

requirements-completed: [RULE-01, RULE-02, RULE-03]

coverage:
  - id: D1
    description: "The real solo start path consumes one exact 136-tile wall with four copies of every TileID and three red fives."
    requirement: RULE-01
    verification:
      - kind: integration
        ref: "Logs/UnityTestGate/20260906-182707-245-results.xml#Phase2RegressionTests.SoloStart_CreatesExactly136TileWall"
        status: pass
      - kind: unit
        ref: "Logs/UnityTestGate/20260906-183706-810-results.xml#Phase2ConformanceTests.WallComposition_HasLiteral136TilesAndThreeRedFives"
        status: pass
    human_judgment: false
  - id: D2
    description: "The shared shuffle reaches the last legal index, preserves a permutation, and returns the same physical tile sequence for the same seed."
    requirement: RULE-02
    verification:
      - kind: unit
        ref: "Logs/UnityTestGate/20260906-182707-245-results.xml#Phase2RegressionTests.ShuffleArray_IncludesLastIndexInSelectionRange"
        status: pass
      - kind: unit
        ref: "Logs/UnityTestGate/20260906-183706-810-results.xml#Phase2ConformanceTests.SeededWall_UsesStablePhysicalTileSequence"
        status: pass
    human_judgment: false
  - id: D3
    description: "Normal and red copies share TileID equality/hash/order while discard and riichi candidate paths preserve the selected physical copy by index."
    requirement: RULE-03
    verification:
      - kind: integration
        ref: "Logs/UnityTestGate/20260906-182707-245-results.xml#Phase2RegressionTests.IndexedDiscard_PreservesTheUnselectedFive"
        status: pass
      - kind: integration
        ref: "Logs/UnityTestGate/20260906-183706-810-results.xml#Phase2ConformanceTests.RiichiCandidates_PreserveRedFiveAtTheOriginalIndex"
        status: pass
    human_judgment: false

duration: 1h 30m
completed: 2026-09-06
status: complete
---

# Phase 2 Plan 1: Shared Wall and Tile Identity Summary

**A single seeded 136-tile wall now drives the real solo start path with uniform Fisher-Yates shuffling and consistent TileID identity that preserves red-five physical instances.**

## Performance

- **Duration:** 1h 30m
- **Started:** 2026-09-06T08:15:36Z
- **Completed:** 2026-09-06T09:45:23Z
- **Tasks:** 2
- **Files modified:** 10
- **Diff-size token estimate:** 7,046 (`28,184` diff characters divided by 4, from `3c8068b` through `b8debab`); actual model token usage was not measured.

## Accomplishments

- Replaced the 139-tile solo wall with one shared 136-tile factory containing four of each TileID and exactly three red fives.
- Corrected the shared shuffle to descending Fisher-Yates and aligned typed/object equality, operators, hashing, and ordering on TileID.
- Verified physical red preservation through actual discard and riichi candidate paths, then preserved separate 4-case Regression, 5-case Conformance, 4-case trace, and 15-case lifecycle XML evidence.

## Task Commits

1. **Task 1 RED: Capture wall and identity regressions** - `a3d97ed` (test)
2. **Task 1 GREEN: Implement shared deterministic wall** - `aaae281` (feat)
3. **Task 2: Verify wall and identity conformance** - `b8debab` (test)

## Files Created/Modified

- `Assets/Scripts/AL-1S/MahjongWall.cs` - Builds and shuffles a new exact 136-tile wall.
- `Assets/Scripts/AL-1S/Utilities.cs` - Uses descending Fisher-Yates with the full `0..i` selection range.
- `Assets/Scripts/AL-1S/MahjongTileAndBlock.cs` - Implements TileID-only typed/object equality and stable hashing.
- `Assets/Scripts/AL-1S/MahjongRound.cs` - Routes solo wall generation through `MahjongWall` and removes known riichi candidates by index.
- `Assets/Editor/Tests/Phase2RegressionTests.cs` - Preserves the legacy assertion RED and final integration GREEN.
- `Assets/Editor/Tests/Phase2ConformanceTests.cs` - Holds literal wall, seed, identity, null-input, and riichi-index contracts.
- `.planning/phases/02-shared-rules-core/02-EVIDENCE.md` - Records exact commands, names, counts, commits, raw paths, and the collection caller audit.

## Decisions Made

- Reused `Utilities.ShuffleArray` through a `Random` overload so `MahjongWall` owns composition only and no second shuffle implementation exists.
- Kept red-five state outside basic equality. Selection paths use indices, while evidence that cares about physical order compares `(TileID, isAkaDora)` explicitly.
- Stopped mutating `isDora` and `doraCount` during wall creation. Later Phase 2 plans own the shared indicator resolver and presentation migration.

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 3 - Blocking] Corrected test and compile compatibility issues**
- **Found during:** Task 1 RED/GREEN gates
- **Issue:** NUnit 3.5 lacked `Assert.Multiple`, the first indexed-discard harness had no initialized wall, and removing `System.Linq` broke existing `LinkedList<T>.First()`/`Last()` extension calls.
- **Fix:** Used ordinary assertions, initialized the round before exercising discard, and retained the required LINQ import.
- **Files modified:** `Assets/Editor/Tests/Phase2RegressionTests.cs`, `Assets/Scripts/AL-1S/MahjongRound.cs`
- **Verification:** Valid assertion-only RED at `20260906-173055-458`; final 4/4 GREEN at `20260906-182707-245`.
- **Committed in:** `a3d97ed`, `aaae281`

**2. [Rule 1 - Bug] Removed a runtime-specific shuffle seed assumption**
- **Found during:** Task 1 GREEN gate
- **Issue:** The first test assumed Mono `Random(0).Next(2)` chose index 1, which is not part of the required contract.
- **Fix:** The same two-item regression now proves index 1 is reachable across a fixed 0–255 seed range. The legacy exclusive bound still fails this test for every seed.
- **Files modified:** `Assets/Editor/Tests/Phase2RegressionTests.cs`
- **Verification:** Legacy RED remains semantically valid; final Regression is 4/4 GREEN.
- **Committed in:** `aaae281`

---

**Total deviations:** 2 auto-fixed (1 blocking compatibility issue, 1 test bug)
**Impact on plan:** Both changes preserve the planned behavior and fail-closed evidence contract without expanding production scope.

## Issues Encountered

- Invalid runs `20260906-172819-301`, `20260906-172937-928`, `20260906-182516-142`, and `20260906-182626-230` are recorded separately in `02-EVIDENCE.md`; none are presented as RED or GREEN.
- Git index writes required the existing narrow outside-sandbox commit route. No lock was deleted and no unrelated file was staged.

## TDD Gate Compliance

- RED commit `a3d97ed` precedes GREEN commit `aaae281`.
- RED XML selected four tests and contains three intended assertion failures with no error-labelled case.
- GREEN XML selected the same four names and contains four passes with zero failed/skipped cases.
- Conformance independently selected and passed five unique cases.

## Authentication Gates

None. Unity licensing completed without login or IPC recovery.

## Known Stubs

None added by this plan.

## User Setup Required

None.

## Next Phase Readiness

- Plan 02-01 is complete and provides the wall and identity base required by 02-02.
- Plan 02-02 has not started and remains pending an explicit user request under the requested one-plan execution boundary.

## Self-Check: PASSED

- All ten plan files and this summary exist.
- Commits `a3d97ed`, `aaae281`, and `b8debab` resolve in repository history.
- Valid RED and all four final GREEN XML files exist at the recorded paths.

---
*Phase: 02-shared-rules-core*
*Completed: 2026-09-06*
