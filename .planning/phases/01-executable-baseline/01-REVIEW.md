---
phase: 01-executable-baseline
reviewed: 2026-09-02T02:51:56Z
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
  warning: 2
  info: 0
  total: 4
status: issues_found
---

# Phase 01: Code Review Report

**Reviewed:** 2026-09-02T02:51:56Z
**Depth:** standard
**Files Reviewed:** 17
**Status:** issues_found

## Summary

Phase 01의 gap closure로 이전의 timeout 기록 표시 순서와 재시작 선택 강조 문제는 해소됐다. 그러나 현재 범위에는 사용자 저장 데이터 손실 위험 1건, 실제 입력이 아무 동작도 하지 않는 기능 결함 1건, 손패 렌더링 방어 실패와 씬 패널 오배선 2건이 남아 있다. 검토 범위에서 네트워크·인증·명령 실행 관련 보안 취약점은 발견하지 못했다.

## Narrative Findings (AI reviewer)

## Critical Issues

### CR-01: EditMode 테스트가 실제 플레이어 저장 파일을 덮어쓴다

**Classification:** BLOCKER
**File:** `Assets/Editor/Tests/SoloSessionLifecycleTests.cs:52-97, 241-297, 684-699`
**Issue:** 세 persistence 테스트가 `Application.persistentDataPath/yaml.json`을 직접 읽고 테스트 값으로 덮어쓴 뒤, 원본을 메모리의 `byte[]`로만 보관한다. Editor와 Windows Player는 같은 company/product persistent-data 위치를 사용할 수 있으므로 테스트 중 Editor 종료, Unity crash, 강제 중단이 발생하면 `finally`가 실행되지 않아 사용자의 실제 최고 기록이 테스트 값으로 영구 교체되거나 삭제된다. 테스트가 통과하는 경우만 복원된다는 구조라 데이터 손실 위험이 있다.

**Fix:** 테스트 시작 전에 원본을 동일 디렉터리의 명시적 backup 파일로 원자적으로 이동하거나 복사하고, `[SetUp]`에서 이전 실행의 stale backup을 먼저 복구한 뒤 `[TearDown]`에서 복원한다. 더 안전한 방법은 `SettingsManager`가 테스트 전용 임시 경로를 받도록 저장 경로 seam을 두고 모든 테스트 저장을 `Temp` 아래로 격리하는 것이다.

```csharp
private const string BackupSuffix = ".phase1-test-backup";

[SetUp]
public void RecoverInterruptedSaveTest()
{
    string savePath = Path.Combine(Application.persistentDataPath, "yaml.json");
    string backupPath = savePath + BackupSuffix;
    if (File.Exists(backupPath))
    {
        File.Copy(backupPath, savePath, true);
        File.Delete(backupPath);
    }
}
```

### CR-02: 리치 키 입력과 manager route가 모두 무동작이다

**Classification:** BLOCKER
**File:** `Assets/Scripts/UI-Kozeki/PlayerHandController.cs:79-82`; `Assets/Scripts/SoloScoringGameManager.cs:368-372, 391-393`
**Issue:** 리치 키를 누르면 사용되지 않는 `callRiichiNya` 필드만 `true`가 된다. 이 필드는 읽히지 않으므로 `OnPlayerCall(PlayerCallType.Riichi)`가 발행되지 않으며, 설령 외부에서 해당 event를 발행해도 `RiichiHandler`가 비어 있어 규칙 상태가 전혀 바뀌지 않는다. 화면은 리치 가능 상태를 표시하지만 실제 사용자 입력은 조용히 무시된다.

**Fix:** 리치를 현재 범위에서 지원한다면 controller가 리치 intent를 발행하고 manager가 기존 `MahjongRound`의 리치 상태 전이 API로 처리하도록 연결한 뒤 한 개의 입력→도메인 회귀 테스트를 추가한다. 아직 지원하지 않는 기능이라면 리치 표시와 key route를 제거하거나 명시적으로 비활성화해 동작 가능한 기능처럼 노출하지 않는다.

```csharp
if (Input.GetKeyDown(InputPreset.riichi))
{
    OnPlayerCall(PlayerCallType.Riichi);
}
```

## Warnings

### WR-01: 잘못된 손패 길이를 진단한 뒤 배열 범위를 벗어난다

**Classification:** WARNING
**File:** `Assets/Scripts/UI-Kozeki/PlayerHandView.cs:56-81`
**Issue:** `FillHand`는 14장 이상 입력을 발견하면 오류를 기록하지만 곧바로 `tilesInHand[index]`에 계속 접근하여 `IndexOutOfRangeException`을 발생시킨다. 반대로 13장보다 적으면 남은 타일에 이전 손패 이미지가 남는다. `null`도 검사하지 않아 호출 경계가 잘못되면 부분 갱신 또는 예외로 끝난다.

**Fix:** mutation 전에 `tiles != null && tiles.Count == tilesInHand.Length`를 한 번 검증하고, 실패하면 명확한 오류를 남긴 뒤 return하여 기존 화면을 원자적으로 보존한다.

```csharp
if (tiles == null || tiles.Count != tilesInHand.Length)
{
    Debug.LogError($"손패는 정확히 {tilesInHand.Length}장이어야 합니다.");
    return;
}
```

### WR-02: Score 패널이 활성화되지 않으며 다른 패널의 CanvasGroup에 연결돼 있다

**Classification:** WARNING
**File:** `Assets/Scripts/UI-Kozeki/SoloScoringUIController.cs:120-137`; `Assets/Scenes/SampleScene.unity:2421-2424, 9837-9844`
**Issue:** `Initialize`는 RoundInfo, PlayerHand, Distance, Time만 활성화하고 `GameUIState.Score`를 한 번도 활성화하지 않는다. 또한 씬의 Score entry는 rect `1739634553`과 Round Info의 CanvasGroup `71275690`을 짝지었고, Score 자체 CanvasGroup `1739634555`는 사용하지 않는다. 따라서 점수 텍스트를 갱신해도 기본 세션에서 패널이 보이지 않으며, 외부에서 Score를 활성화하면 RoundInfo의 alpha/raycast 상태를 잘못 변경한다.

**Fix:** 솔로 HUD에 점수를 표시할 의도라면 `Initialize`에서 `ActivePanel(GameUIState.Score)`를 호출하고, Unity Inspector에서 Score entry의 group을 `{fileID: 1739634555}`로 다시 저장한다. 각 panel entry의 rect와 group이 같은 GameObject 소유인지 검사하는 scene contract assertion도 추가한다.

---

_Reviewed: 2026-09-02T02:51:56Z_
_Reviewer: the agent (gsd-code-reviewer)_
_Depth: standard_
