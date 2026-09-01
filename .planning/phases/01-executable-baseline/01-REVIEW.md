---
phase: 01-executable-baseline
reviewed: 2026-09-01T12:38:41Z
depth: standard
files_reviewed: 17
files_reviewed_list:
  - Assets/Editor/Phase1Build.cs
  - Assets/Editor/Tests/MahjongRoundTraceTests.cs
  - Assets/Editor/Tests/SoloSessionLifecycleTests.cs
  - Assets/Scenes/SampleScene.unity
  - Assets/Scripts/AL-1S/_Enums_Ecchi_Nun_Mari.cs
  - Assets/Scripts/GameEndReason.cs
  - Assets/Scripts/SoloScoringGameManager.cs
  - Assets/Scripts/SoloScoringGameManager.cs.meta
  - Assets/Scripts/UI-Kozeki/MahjongTileGameObject.cs
  - Assets/Scripts/UI-Kozeki/PlayerHandController.cs
  - Assets/Scripts/UI-Kozeki/PlayerHandController.cs.meta
  - Assets/Scripts/UI-Kozeki/PlayerHandView.cs
  - Assets/Scripts/UI-Kozeki/PlayerHandView.cs.meta
  - Assets/Scripts/UI-Kozeki/SoloScoringUIController.cs
  - Assets/Scripts/UI-Kozeki/SoloScoringUIController.cs.meta
  - Assets/Scripts/UI-Kozeki/UiGameOver.cs
  - Assets/Scripts/UI-Kozeki/UiManager.cs
findings:
  critical: 2
  warning: 4
  info: 0
  total: 6
status: issues_found
---

# Phase 01: Code Review Report

**Reviewed:** 2026-09-01T12:38:41Z  
**Depth:** standard  
**Files Reviewed:** 17  
**Status:** issues_found

## Summary

The Phase 01 implementation has two ship-blocking state/presentation defects and four robustness or test-reliability defects. The forfeit idempotency and direct event unsubscription paths are present, but the fresh-session UI state and new-record result display are not internally consistent. No security vulnerability was found in the reviewed scope.

## Narrative Findings (AI reviewer)

## Critical Issues

### CR-01: New timeout record is rendered as the old record

**Classification:** BLOCKER  
**File:** `Assets/Scripts/SoloScoringGameManager.cs:243-252`  
**Issue:** `FinalizeGame` sends `saveData.highScore` to `ShowGameOver` before applying a newly earned timeout record. When `yourScore` exceeds the stored value, the result screen displays the previous record even though the higher value is saved immediately afterward. The UI and persisted state therefore disagree on the run that sets a record.

**Fix:** Update the timeout record before rendering and saving, while preserving the no-save forfeit policy.

```csharp
PetitGameSaveData saveData = SettingsManager.Load();
if (reason == GameEndReason.TimeExpired && yourScore > saveData.highScore)
{
    saveData.highScore = yourScore;
}

soloUIController?.ShowGameOver(yourScore, saveData.highScore, reason);
if (reason == GameEndReason.TimeExpired)
{
    SettingsManager.Save(saveData);
}
```

Add a lifecycle regression where `yourScore` is greater than the stored record and assert both the rendered record and saved value.

### CR-02: Fresh sessions retain the previous tile highlight

**Classification:** BLOCKER  
**File:** `Assets/Scripts/UI-Kozeki/PlayerHandController.cs:159-164`  
**Issue:** `HandleGameStart` resets `currentIndex` to 6 but never pushes that value to `PlayerHandView`. `MahjongTileGameObject.isSelected` survives root disable/enable, and `FillHand` only changes tile images and dora state. If a player moves the selection and then forfeits or times out, the next session reports index 6 internally while the old tile remains visibly raised. This violates the claimed fresh-session boundary and can make the next discard target differ from the highlighted tile.

**Fix:** Synchronize the view whenever the controller resets the selection.

```csharp
private void HandleGameStart()
{
    isGameOver = false;
    gameplayInputEnabled = true;
    currentIndex = 6;
    UpdateHand();
}
```

Add a regression that selects a non-default tile, cycles the real mode root, starts a new session, and asserts that only tile 6 is selected.

## Warnings

### WR-01: Oversized hand logging falls through into an array crash

**Classification:** WARNING  
**File:** `Assets/Scripts/UI-Kozeki/PlayerHandView.cs:56-75`  
**Issue:** `FillHand` detects `index >= tilesInHand.Length` and logs an error, but then continues to access `tilesInHand[index]`. A malformed 14-or-more-item hand therefore throws `IndexOutOfRangeException` immediately after the diagnostic instead of failing safely. The branch demonstrates that invalid input is expected enough to diagnose, so falling through is not a valid contract.

**Fix:** Validate once before mutation and return early (or throw a clear programmer-contract exception); do not partially update the view.

```csharp
if (tiles == null || tiles.Count != tilesInHand.Length)
{
    Debug.LogError($"손패는 {tilesInHand.Length}장이어야 합니다.");
    return;
}
```

### WR-02: Score panel is wired to the round panel's CanvasGroup

**Classification:** WARNING  
**File:** `Assets/Scenes/SampleScene.unity:2421-2424`  
**Issue:** The `GameUIState.Score` entry uses rect `1739634553` but CanvasGroup `71275690`, which belongs to `Round Info Holder`. `Score Info Holder` has its own CanvasGroup at file ID `1739634555` (`SampleScene.unity:9837-9844`). Activating or deactivating Score therefore fades/interacts with RoundInfo while moving a different rect, producing coupled panel state and incorrect transitions.

**Fix:** Set the Score entry's `group` reference to `{fileID: 1739634555}` in the Unity Inspector and save the scene. Add a scene contract assertion that every panel entry's `rect` and `group` belong to the same GameObject.

### WR-03: The restart regression does not exercise a Unity root cycle

**Classification:** WARNING  
**File:** `Assets/Editor/Tests/SoloSessionLifecycleTests.cs:367-396`  
**Issue:** `RestartAfterLobby_UsesFreshStateAndSingleHandlers` claims lobby/root-cycle coverage but directly invokes only the manager's private `OnDisable` and `OnEnable` methods on synthetic objects. It never toggles `SoloScoringModeRoot`, never runs `UiManager.OnBBagguButton`, and never checks the controller/view state. The test can pass while child lifecycle ordering, serialized hierarchy, selection reset, or another root-owned component is broken; CR-02 is one concrete defect it misses.

**Fix:** Load `SampleScene` additively, activate the serialized solo root, start a session, mutate selection/session state, call the actual lobby-return route, reactivate/start again, and assert state plus handler counts on the real components. Keep the focused synthetic subscription tests separately.

### WR-04: Trace length mismatches bypass the promised first-mismatch diagnostic

**Classification:** WARNING  
**File:** `Assets/Editor/Tests/MahjongRoundTraceTests.cs:18-23`  
**Issue:** The test asserts record-count equality before calling `FindFirstMismatch`. If deterministic behavior diverges by adding or removing a record, NUnit stops at the generic count assertion and `BuildMismatchMessage` is never used. That contradicts the trace's stated first-mismatch evidence contract and removes the exact state needed to diagnose a regression.

**Fix:** Compute and assert `firstMismatch` before any standalone count assertion; `FindFirstMismatch` already returns the common length when counts differ.

```csharp
int firstMismatch = FindFirstMismatch(expected.Records, actual.Records);
Assert.That(firstMismatch, Is.EqualTo(-1), BuildMismatchMessage(expected, actual, firstMismatch));
```

---

_Reviewed: 2026-09-01T12:38:41Z_  
_Reviewer: the agent (gsd-code-reviewer)_  
_Depth: standard_
