# Phase 2 Rules Evidence

## Plan 02-01 Wall and Tile Identity

Production target: `aaae281` (`feat(02-01): implement shared deterministic wall`)

### Valid RED

- Commit: `a3d97ed` (`test(02-01): capture wall and identity regressions`)
- Command: `& 'C:\Users\user\.codex\skills\unity-test-gate\scripts\Invoke-UnityTests.ps1' -ProjectPath (Get-Location).Path -TestPlatform EditMode -TestFilter Phase2RegressionTests -ExpectedGate Red`
- Helper status: `RED`
- Result: 4 selected, 1 passed, 3 assertion failures, 0 skipped
- XML: `Logs/UnityTestGate/20260906-173055-458-results.xml`
- Log: `Logs/UnityTestGate/20260906-173055-458-unity.log`
- Assertion failures:
  - `Phase2RegressionTests.ShuffleArray_IncludesLastIndexInSelectionRange`
  - `Phase2RegressionTests.SoloStart_CreatesExactly136TileWall`
  - `Phase2RegressionTests.TileIdentity_UsesTileIdAcrossAllEqualityMembers`
- Existing behavior already passing: `Phase2RegressionTests.IndexedDiscard_PreservesTheUnselectedFive`

The RED shuffle assertion used a two-item input. The legacy exclusive upper bound can never select index 1 for any seed. The final test checks that at least one seed in a fixed 0–255 range selects it, preserving the same defect check without assuming a specific Mono `System.Random` sequence.

### Invalid Attempts Excluded from RED/GREEN

- `20260906-172819-301`: compile FAIL, 0 selected. NUnit 3.5 does not provide `Assert.Multiple`; no RED evidence was claimed.
- `20260906-172937-928`: helper FAIL, 4 selected. One fixture setup path raised `NullReferenceException`; mixed assertion and error results were excluded from RED.
- `20260906-182516-142`: compile FAIL, 0 selected. Removing `System.Linq` broke the existing `LinkedList<T>.First()` and `Last()` extension calls; no GREEN evidence was claimed.
- `20260906-182626-230`: helper FAIL, 4 selected, 3 passed, 1 assertion failure. A seed-specific shuffle expectation was not portable to the pinned Mono runtime and was replaced with the equivalent reachable-index contract.

### Regression GREEN

- Commit: `aaae281`
- Command: `& 'C:\Users\user\.codex\skills\unity-test-gate\scripts\Invoke-UnityTests.ps1' -ProjectPath (Get-Location).Path -TestPlatform EditMode -TestFilter Phase2RegressionTests -ExpectedGate Green`
- Helper status: `GREEN`
- Result: 4 selected, 4 passed, 0 failed, 0 skipped
- XML: `Logs/UnityTestGate/20260906-182707-245-results.xml`
- Log: `Logs/UnityTestGate/20260906-182707-245-unity.log`
- Passed names:
  - `Phase2RegressionTests.IndexedDiscard_PreservesTheUnselectedFive`
  - `Phase2RegressionTests.ShuffleArray_IncludesLastIndexInSelectionRange`
  - `Phase2RegressionTests.SoloStart_CreatesExactly136TileWall`
  - `Phase2RegressionTests.TileIdentity_UsesTileIdAcrossAllEqualityMembers`

### Conformance GREEN

- Production target commit: `aaae281`
- Command: `& 'C:\Users\user\.codex\skills\unity-test-gate\scripts\Invoke-UnityTests.ps1' -ProjectPath (Get-Location).Path -TestPlatform EditMode -TestFilter Phase2ConformanceTests -ExpectedGate Green`
- Helper status: `GREEN`
- Result: 5 selected, 5 passed, 0 failed, 0 skipped
- Unity process: PID 41376, exit 0
- XML: `Logs/UnityTestGate/20260906-183706-810-results.xml`
- Log: `Logs/UnityTestGate/20260906-183706-810-unity.log`
- Passed names:
  - `Phase2ConformanceTests.RiichiCandidates_PreserveRedFiveAtTheOriginalIndex`
  - `Phase2ConformanceTests.SeededWall_UsesStablePhysicalTileSequence`
  - `Phase2ConformanceTests.TileIdentity_ObeysEqualityAndHashLawsForRedAndInvalidValues`
  - `Phase2ConformanceTests.WallComposition_HasLiteral136TilesAndThreeRedFives`
  - `Phase2ConformanceTests.WallFactory_RejectsNullRandom`

### Phase 1 Regression Safety

The Phase 1 fixtures remain separate from the Phase 2 count.

- Command: `& 'C:\Users\user\.codex\skills\unity-test-gate\scripts\Invoke-UnityTests.ps1' -ProjectPath (Get-Location).Path -TestPlatform EditMode -TestFilter MahjongRoundTraceTests -ExpectedGate Green`
- `MahjongRoundTraceTests`: helper `GREEN`, 4 selected, 4 passed, 0 failed/skipped, PID 29908, exit 0. XML `Logs/UnityTestGate/20260906-183806-165-results.xml`; log `Logs/UnityTestGate/20260906-183806-165-unity.log`.
  - `MahjongRoundTraceTests.ActionCap_ReportsFirstMismatch`
  - `MahjongRoundTraceTests.LengthDivergence_ReportsFirstMismatchState`
  - `MahjongRoundTraceTests.SameSeedAndActions_ProduceIdenticalTrace`
  - `MahjongRoundTraceTests.TraceContract_ContainsOnlyDecisionFields`
- Command: `& 'C:\Users\user\.codex\skills\unity-test-gate\scripts\Invoke-UnityTests.ps1' -ProjectPath (Get-Location).Path -TestPlatform EditMode -TestFilter SoloSessionLifecycleTests -ExpectedGate Green`
- `SoloSessionLifecycleTests`: helper `GREEN`, 15 selected, 15 passed, 0 failed/skipped, PID 36460, exit 0. XML `Logs/UnityTestGate/20260906-183826-082-results.xml`; log `Logs/UnityTestGate/20260906-183826-082-unity.log`.
  - `SoloSessionLifecycleTests.Confirmation_DoesNotPauseTimer`
  - `SoloSessionLifecycleTests.ConfirmForfeit_FinalizesOnceWithoutSavingHighScore`
  - `SoloSessionLifecycleTests.ForfeitOverlay_IsOutsidePanelMapAndSelectsCancel`
  - `SoloSessionLifecycleTests.ForfeitRequested_IsSeparateAndReturnsBeforeDiscard`
  - `SoloSessionLifecycleTests.GameOver_Forfeit_RendersReasonAndDistance`
  - `SoloSessionLifecycleTests.OpenConfirmation_SynchronouslyBlocksGameplay`
  - `SoloSessionLifecycleTests.PlayerController_Subscriptions_AreSymmetricAcrossRootCycles`
  - `SoloSessionLifecycleTests.RecoverInterruptedSaveTest_RestoresDurableBackupBeforeNextMutation`
  - `SoloSessionLifecycleTests.RestartAfterLobby_UsesFreshStateAndSingleHandlers`
  - `SoloSessionLifecycleTests.ReturnToLobby_DisablesSoloModeRoot`
  - `SoloSessionLifecycleTests.SecondEscape_CancelsAndRestoresGameplay`
  - `SoloSessionLifecycleTests.StartNewGame_Twice_DetachesAndResetsSession`
  - `SoloSessionLifecycleTests.TimeoutDuringConfirmation_WinsAndRejectsLateActions`
  - `SoloSessionLifecycleTests.TimeoutNewRecord_RendersAndPersistsUpdatedHighScore`
  - `SoloSessionLifecycleTests.UiController_Subscriptions_AreSymmetricAcrossRootCycles`
- Preserved aggregate: `4 + 15 = 19` Phase 1 cases. Phase 2 contributes a separate `4 + 5 = 9` cases.

## Tile Collection Caller Audit

- `MahjongPlayer.IsRiichiAble` knows the selected position and now removes it with `RemoveAt(i)`. Conformance observes both red-five and normal-five candidate results through `doraInfo.akadoraCount`.
- `MahjongRound.DiscardTile` already replaces the selected hand index through `MahjongUtility.SwapTiles`; Regression verifies both directions preserve the unselected physical five.
- `MahjongUtility` decomposition removes matching tile kinds from private working copies. TileID equality is intended there because decomposition is based on kind counts; these paths do not select a physical tile from the player's original hand.
- `MahjongUtility.IsKokushiMusou` uses `SequenceEqual` after sorting to compare tile kinds; TileID equality is intended.
- `MahjongBlock.Contains` and yaku checks query tile kinds; TileID equality is intended.
- No production `HashSet<MahjongTile>` or `Dictionary<MahjongTile, ...>` key usage was found. Conformance still verifies equal red, normal, and scored copies share one stable hash identity.
- Seeded wall comparisons in both Phase 2 fixtures explicitly compare `(TileID, isAkaDora)` so swapping only red and normal positions is detected.
