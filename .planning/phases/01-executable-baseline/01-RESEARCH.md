# Phase 1: Executable Baseline - Research

**Researched:** 2026-08-29
**Domain:** Unity 2022.3 실행 기준선, 결정론적 솔로 라운드, Player 빌드, EditMode 배치 테스트, 재시작 수명주기
**Confidence:** MEDIUM

<user_constraints>
## User Constraints (from CONTEXT.md)

### Locked Decisions

#### 대표 재현 시나리오
- **D-01:** 별도 리플레이 UI나 범용 행동 기록 시스템을 만들지 않고, 고정 시드와 행동을 테스트 코드에 둔 작은 EditMode 자동화 테스트로 재현한다.
- **D-02:** 대표 행동은 쯔모기리를 반복하는 것이며, 첫 국 유국 후 다음 국의 첫 쯔모까지 실행한다.
- **D-03:** 성공 시에는 시드·행동 수·핵심 상태 요약·PASS를 짧게 출력하고, 실패 시에는 최초 불일치 행동 번호와 예상·실제 상태를 출력한다.
- **D-04:** 이 시나리오는 결정론적 실행과 국 전환을 검증할 뿐 역·판·부·점수 정확성을 주장하지 않는다. 순수 도메인 경계 정리와 일반형·치또이츠·국사무쌍·역·판·부·지불액의 표 기반 정확성 테스트는 Phase 2에서 수행한다.

#### 포기·종료·재시작
- **D-05:** 실제 솔로 게임은 시작할 때마다 새 난수를 사용하고 자동화 테스트만 고정 시드를 사용한다.
- **D-06:** 포기 시 기존 결과 화면을 재사용하되 종료 사유를 `포기`로 표시하고 현재 거리는 보여준다. 완료하지 않은 게임이므로 고득점은 갱신하지 않는다.
- **D-07:** 결과 화면에서 메인 메뉴로 돌아간 뒤 기존 시작 버튼으로 새 솔로 게임을 시작한다. 새 게임은 타이머·점수·거리·라운드·손패·하천·UI와 이벤트 구독을 이전 실행과 분리해 초기화한다.
- **D-08:** 게임 중 `Esc`를 누르면 포기 확인창을 표시한다. 확인창이 열려 있는 동안 게임 입력은 차단하지만 180초 타이머는 계속 진행한다.
- **D-09:** 확인창에서 취소하거나 다시 `Esc`를 누르면 게임 입력을 복구하고, 확인하면 포기 결과 화면으로 이동한다. 확인 중 타이머가 끝나면 확인창을 닫고 정상 시간 종료를 우선 처리한다.

#### 검증과 증거 보존
- **D-10:** Git에는 사람이 읽을 수 있는 짧은 Markdown 검증 요약 하나만 보존한다. Unity Test Framework XML, batchmode 로그, Windows 빌드 출력과 `BuildReport`는 프로젝트 로컬 출력 폴더에 실행 산출물로 생성하고 기본적으로 커밋하지 않는다.
- **D-11:** 검증 요약은 `portfolio-baseline`의 테스트 부재·빌드 문제·재시작 증상과 Phase 1 완료 후 결과를 한 문서에서 전후 비교한다. 정확한 실행 명령, Unity 버전, 대상 커밋, 테스트 수, 재현 시드와 행동 요약, 빌드·실행 결과를 포함한다.
- **D-12:** Codex 실행 에이전트가 EditMode 배치 테스트, Windows 빌드와 Player GUI 기본 동작 확인을 수행한다. 화면 판독이나 제어가 불확실하면 추측으로 PASS 처리하지 않고 사용자 확인을 요청한다.
- **D-13:** Windows Player 기본 동작 확인 범위는 빌드 → 실행 → 솔로 시작 → `Esc` 포기 확인 → 포기 결과 → 메뉴 복귀 → 같은 프로세스에서 재시작까지다. 실제 180초 시간 종료는 Phase 1 완료 조건에 포함하지 않는다.

### the agent's Discretion
- 테스트 assembly와 폴더 구성, 고정 시드 값, 상태 요약의 정확한 직렬화 형식과 비교 방식은 기존 Unity 2022.3.29f1/Test Framework 제약 안에서 planner가 최소 변경으로 정한다.
- 원본 XML·로그·빌드 출력의 프로젝트 로컬 생성 경로와 검증 Markdown 파일명은 기존 `.gitignore`와 Phase 7 증거 수집 흐름을 방해하지 않도록 planner가 정한다.
- 포기 종료 사유를 표현하는 최소 도메인 값과 확인창의 기존 UI 통합 위치는 현재 `PlayerCallType.Forfeit` 및 게임 상태 흐름을 재사용하는 방향으로 planner가 정한다.

### Deferred Ideas (OUT OF SCOPE)
- 실제 플레이 테스트에서 필요성이 확인되면 포기 확인창을 타이머가 계속 흐르는 `Esc` 홀드 입력으로 교체할 수 있다.
- 게임 중 설정 UI가 실제 범위에 들어올 때 포기 동작을 같은 공간에 통합할지 다시 논의한다. Phase 1에서는 설정 화면이나 범용 pause manager를 미리 만들지 않는다.
</user_constraints>

<phase_requirements>
## Phase Requirements

| ID | Description | Research Support |
|----|-------------|------------------|
| BASE-01 | 개발자는 기존 소스 기준 커밋 `b18320e`를 `portfolio-baseline` annotated tag로 재현할 수 있다. | 현재 커밋 객체와 태그 상태를 확인했고, annotated tag 검증 명령을 정의한다. |
| BASE-02 | 개발자는 고정된 시드와 행동 기록으로 대표 솔로 패 진행을 반복 재현할 수 있다. | `MahjongRound.NewRound(int, out MahjongPlayer)`, `GenerateYama`, `DiscardTile(13)`, `OnNewRoundStart`를 이용한 bounded trace를 정의한다. |
| BASE-03 | Windows Player 빌드는 런타임 코드의 `UnityEditor` 참조 없이 컴파일되고 실행된다. | 런타임 두 파일의 실제 `UnityEditor` import를 확인했고, Editor 폴더/Player 빌드 분리를 적용한다. |
| BASE-04 | 개발자는 Unity Test Framework에서 프로젝트 규칙 EditMode 테스트를 배치 실행하고 결과 파일을 얻을 수 있다. | 설치된 Test Framework 1.1.33과 NUnit 1.0.6, EditMode assembly 및 CLI 결과 경로를 확인했다. |
| BASE-05 | 플레이어는 같은 실행 세션에서 솔로 게임을 시작하고 종료 또는 포기한 뒤 다시 시작할 수 있다. | `StartNewGame`, `HandleGameOver`, UI back flow, Timer/Score/UI/event reset의 실제 중복 지점을 추적했다. |
</phase_requirements>

## Summary

Phase 1은 새 게임 모드나 리플레이 시스템을 만드는 일이 아니라, 현재 단일 씬의 실제 진입점을 고정하고 관찰 가능한 seeded trace·Player build·같은 프로세스 재시작을 검증하는 기준선이다. 현재 도메인에는 이미 `MahjongRound.NewRound(int seed, out MahjongPlayer)`, `GenerateYama()`, `DiscardTile(int)`와 `OnTsumoTile`/`OnNewRoundStart` 이벤트가 있어, private 상태를 조작하지 않는 작은 EditMode trace를 추가할 수 있다. `[VERIFIED: Assets/Scripts/AL-1S/MahjongRound.cs:248-283,389-414]`

가장 큰 계획 리스크는 기능 부재보다 수명주기와 빌드 경계다. `StartNewGame`은 실게임에서 새 `System.Random()`을 만들고, `PlayerHand`·`UiScoreDistanceInfo`·`UiRemainingTimeIndicator`의 구독 및 `GameUIManager.Initialize`가 반복 실행 시 누적되거나 토글 상태를 뒤집을 수 있다. 또한 `Assets/Scripts/AL-1S/MahjongTileAndBlock.cs`와 `Assets/Scripts/UI-Kozeki/UiScoreDistanceInfo.cs`가 런타임 파일에서 `UnityEditor` namespace를 import한다. `[VERIFIED: Assets/Scripts/MahjongGameManager.cs:41-74; Assets/Scripts/UI-Kozeki/PlayerHand.cs:45-49; Assets/Scripts/UI-Kozeki/UiScoreDistanceInfo.cs:48-75; Assets/Scripts/UI-Kozeki/UiRemainingTimeIndicator.cs:16-27; Assets/Scripts/AL-1S/MahjongTileAndBlock.cs:1-10]`

**Primary recommendation (corrected after Unity assembly-boundary verification):** 기존 `Assembly-CSharp`를 Phase 1에서 분리하지 말고, 두 test fixture를 `Assets/Editor/Tests`에 두어 predefined `Assembly-CSharp-Editor`로 컴파일한다. Phase 1은 새 `.asmdef`/`.asmref`를 추가하지 않고 `b18320e`의 기존 descriptor path/blob을 그대로 보존한다. `Assets/Editor`의 Windows build entry point과 명시적 session reset/forfeit reason 경계를 추가한 뒤 `Temp/phase1` 및 `Builds/phase1`에 raw 결과를 남기고 한 개의 사람이 읽는 Markdown 요약만 커밋한다. 이 경로는 asmdef assembly가 predefined `Assembly-CSharp`를 참조할 수 없는 Unity 2022.3 제약을 지킨다.

## Architectural Responsibility Map

| Capability | Primary Tier | Secondary Tier | Rationale |
|------------|--------------|----------------|-----------|
| seeded tsumo-giri trace와 국 전환 | API / Backend (runtime domain) | Browser / Client (Unity test runner) | 라운드·패산·행동 전이는 `MahjongRound`가 소유하고 EditMode runner는 결과만 관찰한다. `[VERIFIED: Assets/Scripts/AL-1S/MahjongRound.cs:150-166,248-283]` |
| `Esc` 확인·취소·포기 | Browser / Client (Unity UI/input) | API / Backend (`MahjongGameManager`) | `PlayerHand`가 `PlayerCallType`을 발행하고 manager가 `GameState`와 종료 사유를 판정해야 하며, UI가 직접 라운드를 종료하면 안 된다. `[VERIFIED: Assets/Scripts/UI-Kozeki/PlayerHand.cs:154-185,209-224; Assets/Scripts/MahjongGameManager.cs:260-300]` |
| 같은 프로세스 session reset | API / Backend (`MahjongGameManager`, Timer, Score) | Browser / Client (panels/presenter) | manager가 round와 서비스 수명주기를 재구성하고 UI는 명시된 상태를 표시한다. `[VERIFIED: Assets/Scripts/MahjongGameManager.cs:78-141; Assets/Scripts/ScoreManagerDistance.cs:60-66]` |
| Windows Player compile/build | Unity Editor build tooling | Windows Player | Editor API와 build report는 `Assets/Editor`가 소유하고 Player에는 runtime assembly만 들어간다. `[CITED: https://docs.unity3d.com/Manual/SpecialFolders.html]` |
| 테스트·증거 수집 | Unity Editor/CLI | Git/local filesystem | Test Framework XML, log, BuildReport는 local output이고 Git에는 요약만 보존한다. `[VERIFIED: .planning/phases/01-executable-baseline/01-CONTEXT.md:29-33; .gitignore:5-11,58-62]` |

## Project Constraints (from AGENTS.md)

- 영구 삭제·재귀 삭제·force delete·bulk move/rename·Git history rewrite는 명시적 승인과 명확한 target 없이는 실행하지 않는다. 삭제가 필요하면 Recycle Bin 또는 복구 가능한 위치를 우선한다. `[VERIFIED: AGENTS.md project instructions loaded this session]`
- text는 UTF-8로 읽고, 편집 시 기존 encoding/line ending을 보존한다. `[VERIFIED: AGENTS.md project instructions loaded this session]`
- 새 C# identifier는 English, Allman braces, four-space indentation을 사용한다. 새/변경 public member의 XML Summary와 comment는 Korean convention을 따른다. `[VERIFIED: AGENTS.md project instructions loaded this session]`
- Unity lifecycle method는 class 상단에 모으고, 이벤트 구독은 deterministic unsubscribe와 짝지운다. `[VERIFIED: AGENTS.md project instructions loaded this session]`
- Unity 2022.3.29f1 LTS, 기존 asset/UI/DOTween과 global-namespace 구조를 유지하며, 엔진 업그레이드·전면 교체·비교용 중복 source tree를 만들지 않는다. `[VERIFIED: AGENTS.md:18-22; AGENTS.md:32-43]`
- 계획/실행은 현재 작업 범위를 넘는 새 패키지, AI, alternate rules, 설정 UI를 끌어오지 않는다. `[VERIFIED: AGENTS.md:19-22; .planning/phases/01-executable-baseline/01-CONTEXT.md:92-97]`

## Standard Stack

### Core

| Library/tool | Version | Purpose | Why Standard |
|--------------|---------|---------|--------------|
| Unity Editor | `2022.3.29f1` (`8d510ca76d2b`) | scene import, EditMode runner, Windows Player build | 프로젝트가 이 Editor revision을 pin한다. `"m_EditorVersion: 2022.3.29f1"`, `"m_EditorVersionWithRevision: 2022.3.29f1 (8d510ca76d2b)"` `[VERIFIED: ProjectSettings/ProjectVersion.txt:1-2]` |
| Unity Test Framework | `1.1.33` | EditMode batch tests와 XML results | manifest와 lockfile에 동일 버전이 선언·해결되어 있고 package cache도 존재한다. `"com.unity.test-framework": "1.1.33"`; `"version": "1.1.33"` `[VERIFIED: Packages/manifest.json:1-12; Packages/packages-lock.json:232-241; Library/PackageCache local probe]` |
| NUnit extension | `1.0.6` | `Assert`, `[Test]`, constraints | Test Framework lockfile의 dependency다. `"com.unity.ext.nunit": "1.0.6"` `[VERIFIED: Packages/packages-lock.json:232-239]` |
| Git CLI | `2.53.0.windows.1` | baseline commit/tag와 evidence target commit 기록 | 현재 host에 설치되어 있고 `b18320e` commit object가 존재한다. `[VERIFIED: local command probe 2026-08-29]` |
| Unity `BuildPipeline` / `BuildReport` | Unity 2022.3 built-in | Windows build와 성공/실패 결과 확인 | Unity build API가 `BuildPlayerOptions`와 `BuildReport.summary.result`를 제공한다. `[CITED: https://docs.unity3d.com/Manual/build-script-build.html]` |

### Supporting

| Library/tool | Version | Purpose | When to Use |
|--------------|---------|---------|-------------|
| `System.Random` | runtime built-in | 테스트에서만 seed를 주는 PRNG | `MahjongRound` 생성자는 이미 `new System.Random(seed)`을 사용하므로, production random과 test seed를 분리한다. `[VERIFIED: Assets/Scripts/AL-1S/MahjongRound.cs:389-397,422-426]` |
| `BuildTarget.StandaloneWindows64` | Unity 2022.3 built-in | 64-bit Windows Player target | build entry point에서만 사용한다. `[CITED: https://docs.unity3d.com/Manual/build-script-build.html]` |
| Unity legacy Input / `KeyCode` | project setting | 기존 A/D/W/Q/R/Space/Esc 입력 보존 | `activeInputHandler: 0`이고 `PlayerHand`가 `Input.GetKeyDown`을 사용한다. `"activeInputHandler: 0"` `[VERIFIED: ProjectSettings/ProjectSettings.asset:939-940; Assets/Scripts/UI-Kozeki/PlayerHand.cs:154-185]` |

### Alternatives Considered

| Instead of | Could Use | Tradeoff |
|------------|-----------|----------|
| `Assets/Editor/Tests` predefined `Assembly-CSharp-Editor` 경로 | Phase 1에서 pure domain asmdef로 전면 이동 | Predefined Editor assembly는 현재 runtime `Assembly-CSharp`를 사용할 수 있지만, 새 asmdef test assembly에서 predefined assembly를 참조하는 방식은 유효하지 않다. Pure domain boundary는 Phase 2로 남긴다. |
| 작은 고정 action trace | replay UI / 범용 기록 시스템 | locked D-01에 어긋나고 evidence 관리 표면적만 늘린다. `[VERIFIED: 01-CONTEXT.md:16-20]` |
| `Assets/Editor` build method | 수동 Editor 창 클릭만 기록 | 정확한 실행 명령과 재현성이 약해진다. `[ASSUMED]` CLI entry point가 D-10/D-11 evidence를 단순화한다. |

**Installation:** 새 외부 패키지를 설치하지 않는다. 기존 UPM graph(`Packages/manifest.json`, `Packages/packages-lock.json`)를 그대로 사용한다. `[VERIFIED: Packages/manifest.json:1-12; Packages/packages-lock.json:232-241]`

**Version verification:** 이 bounded local pass에서는 registry publish date를 재조회하지 않았다. Phase 1은 이미 pin된 Unity/Test Framework 버전을 갱신하지 않으며, registry upgrade를 계획하지 않는다. `[VERIFIED: local-only scope; Packages/manifest.json:1-12]`

## Package Legitimacy Audit

외부 package 설치가 없는 Phase이므로 Package Legitimacy Gate는 적용되지 않는다. 기존 `com.unity.test-framework`와 transitive NUnit만 사용한다. `[VERIFIED: Packages/manifest.json:1-12; Packages/packages-lock.json:232-241]`

## Architecture Patterns

### System Architecture Diagram

```text
[Unity CLI / Editor]
        ├──> [EditMode Test Assembly (Editor only)]
        │          └──> [Assembly-CSharp: MahjongRound]
        │                    └──> seed + DiscardTile(13) trace
        └──> [Assets/Editor build method]
                   └──> BuildPipeline.BuildPlayer
                              └──> [Builds/phase1 Windows Player + BuildReport]

[Windows Player GUI]
  Menu Start -> UiManager -> MahjongGameManager -> Timer/Score/Round events -> UI presenters
                                      │
                                      └─ Esc -> ForfeitPending -> Confirm
                                               ├─ Cancel / Esc -> PlayerTurn
                                               ├─ Timer expiry -> TimeExpired GameOver (priority)
                                               └─ Confirm -> Forfeit GameOver -> result -> menu -> StartNewGame
```

The domain trace enters through the existing public round API; the GUI path enters through the existing menu/player input events; build and test tooling remain Editor/CLI boundaries. `[VERIFIED: Assets/Scripts/UI-Kozeki/UiManager.cs:155-161,218-222; Assets/Scripts/MahjongGameManager.cs:41-74,260-300; Assets/Scripts/AL-1S/MahjongRound.cs:248-283,412-414]`

### Recommended Project Structure

```text
Assets/
├── Editor/
│   ├── Phase1Build.cs                 # Windows BuildPipeline entry point
│   └── Tests/
│       ├── MahjongRoundTraceTests.cs  # predefined Assembly-CSharp-Editor
│       └── SoloSessionLifecycleTests.cs
└── Scripts/
    ├── MahjongGameManager.cs          # session/forfeit/reset integration
    ├── Timer.cs
    ├── UI-Kozeki/                     # confirmation/result/presenter integration
    └── AL-1S/                         # existing round API

Temp/phase1/                            # XML/log/BuildReport, ignored
Builds/phase1/                          # Player output, ignored
.planning/phases/01-executable-baseline/01-BASELINE.md  # one committed summary
```

`Assets/Editor` is already the project's editor-only location, and fixtures below `Assets/Editor/Tests` compile through predefined `Assembly-CSharp-Editor`. `/Builds/` and `/Temp/` are ignored by the existing `.gitignore`. `[VERIFIED: .planning/codebase/STRUCTURE.md:1-18; .planning/codebase/TESTING.md:1-24; .gitignore:5-11]` The exact summary filename is discretionary; the path above is a plan recommendation. `[ASSUMED]`

### Pattern 1: Seed + accepted-action trace

**What:** Create a round with a fixed seed, observe the initial `OnTsumoTile`, issue only `DiscardTile(13)` (tsumogiri), stop on `OnNewRoundStart`, call `GenerateYama()` on the supplied next round, and record its first `OnTsumoTile`. `[VERIFIED: Assets/Scripts/AL-1S/MahjongRound.cs:156-166,248-283,412-414]`

**When to use:** BASE-02 and only the D-01/D-02 representative path. Add a hard action bound so a broken transition fails instead of hanging the Unity runner. `[ASSUMED]`

**Example:**

```csharp
var round = MahjongRound.NewRound(seed, out MahjongPlayer player);
MahjongRound nextRound = null;
round.OnNewRoundStart += next => nextRound = next;
round.OnTsumoTile += info => trace.Add(info.tsumoTile.ToString());
round.GenerateYama();

for (int action = 0; nextRound == null && action < maxActions; action++)
{
    round.DiscardTile(13);
}

Assert.That(nextRound, Is.Not.Null);
nextRound.OnTsumoTile += info => nextFirstTsumo = info.tsumoTile.ToString();
nextRound.GenerateYama();
```

The names and values in this skeleton are existing public API/fields: `NewRound(int, out MahjongPlayer)`, `OnNewRoundStart`, `OnTsumoTile`, `DiscardTile(int)`, `GenerateYama()`, `TsumoInfo.tsumoTile`, and `DiscardTile(13)`’s tsumogiri branch. `[VERIFIED: Assets/Scripts/AL-1S/MahjongRound.cs:156-166,248-283,389-414; Assets/Scripts/AL-1S/_Structs.cs:195-210]`

Run the same trace twice for the same seed and compare compact summaries. A failure should identify first action index plus expected/actual summary; success prints seed, action count, next-round first draw, and `PASS`. Do not assert yaku/han/fu/payment values here. `[VERIFIED: 01-CONTEXT.md:16-20]`

### Pattern 2: Predefined Editor test boundary (supersedes the asmdef proposal)

Place synchronous NUnit fixtures directly under `Assets/Editor/Tests`. Unity compiles them into predefined `Assembly-CSharp-Editor`, which is ordered after and can use the existing runtime `Assembly-CSharp`. Do not add a Phase 1 `.asmdef` or `.asmref`: an asmdef-defined test assembly cannot reference the predefined runtime assembly used by this project. The first Unity command proves discovery by requiring the three exact trace cases once; later gates require the lifecycle fixture and final 3 + 13 case set. Enumerate every descriptor below `Assets` and compare both path and blob hash to `b18320e` so an alternate filename or modification cannot bypass the contract. Keep the tests synchronous `[Test]`; the trace consumes a synchronous domain API and does not need PlayMode frame waits. `[VERIFIED: Assets/Editor/MahjongTileDataGenerator.cs; Assets/Scripts/AL-1S/MahjongRound.cs:248-283; .planning/codebase/TESTING.md:1-24]`

### Pattern 3: Idempotent session lifecycle

Make `StartNewGame` establish a clean session boundary before exposing `PlayerTurn`: detach the old round, reset Timer/Score state, replace the round, attach once, explicitly set every required panel active/inactive, and attach each service only once. `GameUIManager.Initialize` currently uses `TogglePanel` for `RoundInfo` and `PlayerHand`, so second start can invert the desired state; use explicit reset rather than toggle for initialization. `[VERIFIED: Assets/Scripts/MahjongGameManager.cs:41-85,123-141; Assets/Scripts/UI-Kozeki/GameUIManager.cs:78-87; .planning/codebase/CONCERNS.md:75-79]`

Pair subscriptions and unsubscriptions in `OnEnable`/`OnDisable`, and unsubscribe an old dependency before replacing it in `Construct`. Unity documents this lifecycle pattern. `[CITED: https://docs.unity3d.com/Manual/player-loop-customizing.html; VERIFIED: Assets/Scripts/UI-Kozeki/UiScoreDistanceInfo.cs:48-75]`

### Pattern 4: Forfeit reason at the application boundary

Keep `PlayerCallType.Forfeit` as the input intent, but do not call `HandleGameOver()` immediately from the call switch. The source currently has `case PlayerCallType.Forfeit: HandleGameOver();`; route it through a pending confirmation state, then pass a reason (`Forfeit` or time expiry) to the single finalization path. `[VERIFIED: Assets/Scripts/MahjongGameManager.cs:260-283; Assets/Scripts/AL-1S/_Enums_Ecchi_Nun_Mari.cs:22-29]`

The pending state must reject discard/call input while leaving `Timer.Update` running. Confirmation must be resolved exactly once; timer expiry wins if it races the confirm action. Forfeit reuses `UiGameOver.Initialize`/result panel but skips high-score mutation and displays the reason plus current distance. `[VERIFIED: Assets/Scripts/Timer.cs:34-45; Assets/Scripts/MahjongGameManager.cs:91-112; Assets/Scripts/UI-Kozeki/UiGameOver.cs:18-29; VERIFIED: 01-CONTEXT.md:22-27]`

### Anti-Patterns to Avoid

- **Replay UI or generalized action recorder:** violates D-01 and adds a persistence contract that is not needed for a test trace. `[VERIFIED: 01-CONTEXT.md:16-20]`
- **Calling `HandleGameOver` directly for Esc:** bypasses confirmation, marks a forfeit indistinguishable from time expiry, and currently updates high score for every result. `[VERIFIED: Assets/Scripts/MahjongGameManager.cs:91-112,260-283]`
- **Using `TogglePanel` as reset:** second initialization can hide required panels; set target state explicitly. `[VERIFIED: Assets/Scripts/UI-Kozeki/GameUIManager.cs:78-87; .planning/codebase/CONCERNS.md:75-79]`
- **Adding a second state machine in UI:** `currentState` already gates manager calls; extend the one owner or use a named pending reason, not UI-local legality. `"public GameState currentState = GameState.Initializing;"` `[VERIFIED: Assets/Scripts/MahjongGameManager.cs:32-38]`
- **Leaving `UnityEditor` imports in runtime scripts:** Play Mode may look fine while Player compilation fails; Editor folder exclusion does not protect files under `Assets/Scripts`. `[VERIFIED: Assets/Scripts/AL-1S/MahjongTileAndBlock.cs:9; Assets/Scripts/UI-Kozeki/UiScoreDistanceInfo.cs:9; .planning/codebase/CONCERNS.md:39-43; CITED: https://docs.unity3d.com/Manual/SpecialFolders.html]`
- **Golden scoring assertions in this phase:** score correctness is explicitly Phase 2, so they would make the baseline depend on out-of-scope rule repairs. `[VERIFIED: 01-CONTEXT.md:16-20]`

## Don't Hand-Roll

| Problem | Don't Build | Use Instead | Why |
|---------|-------------|-------------|-----|
| EditMode execution/XML | custom NUnit/console runner | installed Unity Test Framework CLI | package and `-runTests` path already exist. `[VERIFIED: Packages/manifest.json:10; .planning/codebase/TESTING.md:1-24; CITED: https://docs.unity3d.com/Manual/test-framework/run-tests-from-command-line.html]` |
| Windows Player build | shell-copying Library artifacts or manual binary assembly | `BuildPipeline.BuildPlayer` + `BuildReport.summary.result` | Unity owns scene/import/Player compilation. `[CITED: https://docs.unity3d.com/Manual/build-script-build.html]` |
| Deterministic shuffle | new replay/random framework | existing `MahjongRound` seed and `Utilities.ShuffleArray` seam | D-01 requires a test-only fixed seed and current API already injects seed. `[VERIFIED: Assets/Scripts/AL-1S/MahjongRound.cs:189-212,389-397; Assets/Scripts/AL-1S/Utilities.cs:44-53]` |
| Event cleanup | reflection-based delegate clearing | symmetric `OnEnable`/`OnDisable` and old-service unsubscribe | delegate ownership is visible and testable. `[VERIFIED: Assets/Scripts/MahjongGameManager.cs:123-141; CITED: https://docs.unity3d.com/Manual/player-loop-customizing.html]` |
| Evidence format | committing raw XML/log/build folders | one Markdown summary plus ignored local outputs | locked D-10/D-11 defines the durable boundary. `[VERIFIED: 01-CONTEXT.md:29-33; .gitignore:5-11,58-62]` |

**Key insight:** Phase 1 needs one observable trace and one authoritative lifecycle owner. A new replay layer, DI framework, mocking package, or rules abstraction would duplicate responsibilities that already have public events and increase the chance that the baseline itself changes gameplay. `[ASSUMED]`

## Common Pitfalls

### Pitfall 1: Seeded test accidentally uses production seed path

**What goes wrong:** `StartNewGame` currently sets `prng = new System.Random()`; its fixed branch is disabled by the adjacent `#define IROHA` / `#undef IROHA` pair. `[VERIFIED: Assets/Scripts/MahjongGameManager.cs:1-2,41-52]`

**How to avoid:** Test `MahjongRound.NewRound(fixedSeed, out player)` directly; keep production `StartNewGame` fresh-random per D-05. Do not add a seed selector UI. `[VERIFIED: 01-CONTEXT.md:22-23; Assets/Scripts/AL-1S/MahjongRound.cs:389-414]`

**Warning signs:** trace output has no printed seed, or test invokes `StartNewGame` and expects repeatability. `[VERIFIED: 01-CONTEXT.md:16-20]`

### Pitfall 2: Trace loop hangs at exhaustive draw

**What goes wrong:** `DiscardTile(13)` synchronously calls `Tsumo()`, and an empty wall calls `HandlePlayerYuguk()` then raises `OnNewRoundStart`; a malformed transition can otherwise leave the test looping forever. `[VERIFIED: Assets/Scripts/AL-1S/MahjongRound.cs:248-283,504-510]`

**How to avoid:** stop on `OnNewRoundStart`, add a generous fixed action cap, and print first mismatch/action index. Do not assert an exact action count that would couple Phase 1 to Phase 2 wall correction. `[VERIFIED: 01-CONTEXT.md:16-20]` `[ASSUMED]`

### Pitfall 3: Next-round event observed but next round never dealt

**What goes wrong:** `OnRoundEnds` raises `OnNewRoundStart(newRound)`; `MahjongRound.NextRound` returns a round but does not call `GenerateYama`. `[VERIFIED: Assets/Scripts/AL-1S/MahjongRound.cs:435-452,476-489]`

**How to avoid:** trace callback captures the supplied round, subscribes to its first `OnTsumoTile`, then calls `GenerateYama()` exactly once. Manager’s `StartNextRound` performs the same attach/generate sequence. `[VERIFIED: Assets/Scripts/MahjongGameManager.cs:78-88]`

### Pitfall 4: Forfeit path updates high score or fires twice

**What goes wrong:** current `CallHandler` sends `Forfeit` directly to `HandleGameOver`, whose common path always loads/saves high score; timer and UI handlers also remain subscribed across some paths. `[VERIFIED: Assets/Scripts/MahjongGameManager.cs:91-112,260-283; Assets/Scripts/UI-Kozeki/UiRemainingTimeIndicator.cs:16-27]`

**How to avoid:** model one finalization method with an explicit reason and idempotent guard; forfeit skips high-score write, timeout has priority, and confirm/cancel callbacks become inert after finalization. `[VERIFIED: 01-CONTEXT.md:22-27; ASSUMED: explicit reason enum/value is the smallest maintainable boundary]`

### Pitfall 5: `PlayerHand` game-over flag is reset immediately

**What goes wrong:** `PlayerHand.Start` subscribes both `HandleGameOver` and `HandleGameStart` to `OnGameOver`; one sets `_isGameOver = true`, the next sets it false, and `Update` does not consult the flag. `[VERIFIED: Assets/Scripts/UI-Kozeki/PlayerHand.cs:23-49,135-188; .planning/codebase/CONCERNS.md:87-91]`

**How to avoid:** subscribe `HandleGameStart` to `OnGameStart`, unsubscribe both symmetrically, and gate local input while manager is not in `PlayerTurn`. `[VERIFIED: Assets/Scripts/MahjongGameManager.cs:14-17,216-231; CITED: https://docs.unity3d.com/Manual/player-loop-customizing.html]`

### Pitfall 6: UI/service subscriptions duplicate on second start

**What goes wrong:** `UiScoreDistanceInfo.Construct` adds handlers without removing the old service first; `UiRemainingTimeIndicator.Construct` adds timer handlers on every start and only removes them when timer finishes. `[VERIFIED: Assets/Scripts/UI-Kozeki/UiScoreDistanceInfo.cs:48-75; Assets/Scripts/UI-Kozeki/UiRemainingTimeIndicator.cs:16-27]`

**How to avoid:** detach before replacement, make `Construct` idempotent, and test Construct twice plus disable-before-Construct. `[VERIFIED: .planning/codebase/CONCERNS.md:165-169]`

### Pitfall 7: Runtime Editor dependency survives because only grep was run

**What goes wrong:** two imports are outside `Assets/Editor`; Unity’s Editor assembly exclusion applies to the reserved folder, not arbitrary runtime paths. `[VERIFIED: Assets/Scripts/AL-1S/MahjongTileAndBlock.cs:9; Assets/Scripts/UI-Kozeki/UiScoreDistanceInfo.cs:9; CITED: https://docs.unity3d.com/Manual/SpecialFolders.html]`

**How to avoid:** remove unused imports (or guard genuinely editor-only code), then run a StandaloneWindows64 build and inspect the Unity log/BuildReport. `[VERIFIED: .planning/codebase/CONCERNS.md:39-43; CITED: https://docs.unity3d.com/Manual/build-script-build.html]`

### Pitfall 8: Raw evidence accidentally becomes a Git artifact

**What goes wrong:** `.gitignore` ignores `/Temp/`, `/Builds/`, `/Logs/`, and common build outputs, but a new evidence folder outside those patterns will be tracked. `[VERIFIED: .gitignore:5-11,58-62]`

**How to avoid:** use fixed `Temp/phase1` and `Builds/phase1` paths, check `git status --short`, and commit only the single Markdown summary. `[VERIFIED: 01-CONTEXT.md:29-33]`

## Code Examples

### EditMode batch command

```powershell
& "C:\Program Files\Unity\Hub\Editor\2022.3.29f1\Editor\Unity.exe" `
  -batchmode -nographics -projectPath "$PWD" `
  -runTests -testPlatform EditMode `
  -testResults "$PWD\Temp\phase1\editmode.xml" `
  -quit -logFile "$PWD\Temp\phase1\editmode.log"
```

`-runTests`, `-batchmode`, `-projectPath`, `-testResults`, and `-testPlatform` are the documented command-line test options. `[CITED: https://docs.unity3d.com/Manual/test-framework/run-tests-from-command-line.html]` The project’s existing test notes use the same EditMode command shape. `[VERIFIED: .planning/codebase/TESTING.md:11-20]`

### Editor-only Windows build method shape

```csharp
var options = new BuildPlayerOptions
{
    scenes = new[] { "Assets/Scenes/SampleScene.unity" },
    locationPathName = "Builds/phase1/RiichiNya.exe",
    target = BuildTarget.StandaloneWindows64,
    options = BuildOptions.StrictMode
};

BuildReport report = BuildPipeline.BuildPlayer(options);
if (report.summary.result != BuildResult.Succeeded)
{
    throw new BuildFailedException(report.SummarizeErrors());
}
```

Place the method in `Assets/Editor`; the enabled scene is currently exactly `Assets/Scenes/SampleScene.unity`. `"path: Assets/Scenes/SampleScene.unity"` `[VERIFIED: ProjectSettings/EditorBuildSettings.asset:7-10]` The API/result check follows Unity’s documented build script. `[CITED: https://docs.unity3d.com/Manual/build-script-build.html]`

### Evidence summary minimum

```markdown
# Portfolio Baseline
- baseline tag: portfolio-baseline -> b18320e...
- target commit: <git rev-parse HEAD>
- Unity: 2022.3.29f1
- EditMode: <test count>, <xml path>, PASS/FAIL
- seeded trace: <seed>, <accepted action count>, <first exhaustive draw>, <next-round first tsumo>
- Player build: <path>, BuildReport result
- GUI smoke: start -> Esc confirm -> forfeit result -> menu -> restart
- before: no tests / Player build issue / restart symptom
- after: <observed result>
```

Keep success concise and preserve raw output paths only as links/locations; on failure include first mismatching action and expected/actual summary. `[VERIFIED: 01-CONTEXT.md:16-20,29-33]`

## State of the Art

| Old approach | Current Phase 1 approach | Impact |
|--------------|--------------------------|--------|
| no first-party test files | `Assets/Editor/Tests` predefined Editor fixtures + NUnit trace/lifecycle XML output, with no new descriptor | BASE-04 becomes repeatable while preserving the legacy assembly graph. `[VERIFIED: .planning/codebase/TESTING.md:1-24]` |
| direct Esc -> `HandleGameOver` | pending confirmation -> reasoned finalization | protects D-08/D-09 and separates forfeit from timeout. `[VERIFIED: Assets/Scripts/MahjongGameManager.cs:260-283; 01-CONTEXT.md:22-27]` |
| `TogglePanel` used as session initialization | explicit panel reset before each session | avoids second-start inversion. `[VERIFIED: Assets/Scripts/UI-Kozeki/GameUIManager.cs:78-87]` |
| runtime imports `UnityEditor` | Editor-only build code under `Assets/Editor`, clean runtime imports | Player compilation can enforce the boundary. `[VERIFIED: Assets/Scripts/AL-1S/MahjongTileAndBlock.cs:9; Assets/Scripts/UI-Kozeki/UiScoreDistanceInfo.cs:9; CITED: https://docs.unity3d.com/Manual/SpecialFolders.html]` |
| production random path used for reproduction | production fresh random, tests explicit seed/action trace | preserves play variation while making regression evidence reproducible. `[VERIFIED: Assets/Scripts/MahjongGameManager.cs:47-52; 01-CONTEXT.md:22-23]` |

**Deprecated/outdated for this phase:** replay UI, generic pause manager, strategy AI, score golden fixtures, and registry/package upgrades. `[VERIFIED: 01-CONTEXT.md:92-97; 01-CONTEXT.md:16-20; AGENTS.md:18-22]`

## Assumptions Log

| # | Claim | Section | Risk if Wrong |
|---|-------|---------|---------------|
| A1 | `[RESOLVED]` Phase 1 tests use predefined `Assembly-CSharp-Editor` under `Assets/Editor/Tests`; Phase 1 adds no asmdef/asmref, and pure domain extraction remains Phase 2. | Standard Stack / Architecture | Descriptor additions or runtime assembly migration would violate the verified Unity boundary and expand scope. |
| A2 | A small confirmation panel/label can be integrated into the existing game canvas and result flow without a new scene. `[RESOLVED]` The scene inspection found no confirmation object, confirmed game canvas fileID `1225745916`, panel states 0-7, and existing GameOver fileID `1563807079`; the plan adds one `ForfeitConfirmation` child and one GameOver reason label. | Pattern 4 | Fixed by the concrete `01-03` scene assignment; no generic modal or new scene is needed. |
| A3 | A fixed action cap is only a hang guard and should not be asserted as the correct wall length before Phase 2. `[ASSUMED]` | Pattern 1 / Pitfall 2 | A test could accidentally freeze the known 139-tile/biased-wall behavior as a permanent contract. |
| A4 | The host’s Unity licensing IPC failure is an environment blocker, not a source-code result. `[ASSUMED]` | Environment Availability | Automated EditMode/build verification cannot be marked PASS until licensing is repaired or a licensed Unity host is used. |

## Open Questions — RESOLVED

1. **Confirmation UI target — RESOLVED:** `Assets/Scenes/SampleScene.unity` has no confirmation object. Add exactly one initially inactive `ForfeitConfirmation` child under the existing game canvas fileID `1225745916`, append it to `GameUIManager.panels`, and wire its two buttons to `MahjongGameManager.ConfirmForfeit` / `CancelForfeit`. Extend existing GameOver fileID `1563807079` with one reason label. No generic pause/settings/modal system or new scene is introduced. `[VERIFIED: Assets/Scenes/SampleScene.unity:2148-2189,7906-7976; repository-wide confirmation search 2026-08-29]`
2. **Compact trace contract — RESOLVED:** compare exactly seed; accepted action index/count; ordered hand tile codes; current drawn/discarded tile code; river count; a first-round/next-round transition marker; and the next round's first drawn tile. Report the first differing action with expected/actual records. Exclude `remainingTsumoCount`, wall length, yaku, han, fu, and payment values so D-04 defects are not frozen as correct. `[VERIFIED: Assets/Scripts/AL-1S/MahjongRound.cs:19-35,156-166,248-283; Assets/Scripts/AL-1S/_Structs.cs:195-210]`
3. **Unity execution gate — RESOLVED:** the host has Unity and Windows support, but the observed LicensingClient IPC timeout/exit 199 is a hard environment gate. Execution remains `BLOCKED` until LicensingClient is repaired/activated or the same commands run on a licensed Windows host. Missing XML, BuildReport, executable launch, or D-13 observation can never be converted to PASS. `[VERIFIED: Temp/phase1-baseline-editmode.log local output 2026-08-29; 01-CONTEXT.md D-12/D-13]`

## Environment Availability

| Dependency | Required By | Available | Version | Fallback |
|------------|-------------|-----------|---------|----------|
| Unity Editor | EditMode, Player build, GUI smoke | ✓ installed; execution blocked by licensing IPC | `2022.3.29f1_8d510ca76d2b` | Activate/repair Unity LicensingClient, or run on a licensed Windows Unity host. `[VERIFIED: local probe; Temp/phase1-baseline-editmode.log]` |
| Windows Standalone support | BASE-03 Player build | ✓ | installed under `...2022.3.29f1\Editor\Data\PlaybackEngines\WindowsStandaloneSupport` | — `[VERIFIED: local probe 2026-08-29]` |
| Unity Test Framework | BASE-04 EditMode | ✓ | `1.1.33` (cache present) | — `[VERIFIED: Packages/packages-lock.json:232-241; Library/PackageCache local probe]` |
| NUnit extension | EditMode assertions | ✓ | `1.0.6` | — `[VERIFIED: Packages/packages-lock.json:232-239]` |
| Git | BASE-01/evidence | ✓ | `2.53.0.windows.1` | — `[VERIFIED: local probe 2026-08-29]` |
| GUI control/visual confirmation | BASE-05 D-12 | not verified in this sub-agent | — | Codex execution agent runs Player; if screen read/control is uncertain, ask user and do not infer PASS. `[VERIFIED: 01-CONTEXT.md:29-33]` |

**Missing dependencies with no fallback:** Unity LicensingClient IPC is currently blocking this host’s batch execution; it must be repaired or the verification must move to a licensed host. `[VERIFIED: Temp/phase1-baseline-editmode.log local output]`

**Missing dependencies with fallback:** GUI automation is not established for this research agent; use the authorized Player smoke operator/user confirmation path. `[VERIFIED: 01-CONTEXT.md:29-33]`

## Validation Architecture

### Test Framework

| Property | Value |
|----------|-------|
| Framework | Unity Test Framework `1.1.33` + NUnit extension `1.0.6` |
| Config file | 없음 — `Assets/Editor/Tests/*.cs`가 predefined `Assembly-CSharp-Editor`로 컴파일되며 Phase 1은 `.asmdef`/`.asmref`를 추가하지 않음 |
| Quick run command | `Unity.exe -batchmode -nographics -projectPath . -runTests -testPlatform EditMode -testResults Temp/phase1/editmode.xml -quit -logFile Temp/phase1/editmode.log` |
| Full suite command | same command after all Phase 1 EditMode tests are present; inspect XML and exit code |

The reserved `Assets/Editor` path supplies the predefined Editor compilation boundary; exact XML discovery proves that NUnit/Test Framework recognized the fixtures. The test runner command writes XML to the requested path. `[CITED: https://docs.unity3d.com/Manual/test-framework/edit-mode-vs-play-mode-tests.html; https://docs.unity3d.com/Manual/test-framework/run-tests-from-command-line.html]`

### Phase Requirements -> Test Map

| Req ID | Behavior | Test Type | Automated Command | File Exists? |
|--------|----------|-----------|-------------------|-------------|
| BASE-01 | `portfolio-baseline` annotated tag resolves to `b18320ec1d9d647900d2173049819bab6bd47175` and remains a tag object | shell/manual Git check | `git cat-file -t refs/tags/portfolio-baseline; git rev-parse refs/tags/portfolio-baseline^{}` | ❌ Wave 0 / tag operation |
| BASE-02 | same seed + same tsumogiri actions produce same trace through exhaustive draw and next-round first draw | EditMode `[Test]` | Unity EditMode quick command above | ❌ `Assets/Editor/Tests/MahjongRoundTraceTests.cs` Wave 0 |
| BASE-03 | runtime source has no `UnityEditor` import and StandaloneWindows64 build succeeds | static scan + Player build | `rg -n --glob '*.cs' 'using UnityEditor(\.|;)' Assets/Scripts` (must return none); then Editor build CLI | ❌ `Assets/Editor/Phase1Build.cs` Wave 0 |
| BASE-04 | project EditMode suite runs and produces XML | batch integration | Unity EditMode quick command above | ❌ predefined Editor fixtures Wave 0 |
| BASE-05 | start -> Esc confirm -> cancel/confirm -> result/menu -> restart in one Player process with no duplicate events/stale panels | Windows Player smoke (manual, optionally PlayMode helper) | built Player + D-13 checklist | ❌ Wave 0 evidence and scene/UI changes |

BASE-05 remains manual because it requires a real Windows Player, keyboard/UI interaction, and visual state confirmation; do not replace it with a scene-only test and call the GUI path proven. `[VERIFIED: 01-CONTEXT.md:29-33]`

### Sampling Rate

- **Per task commit:** run the EditMode quick command and runtime `UnityEditor` static scan. `[CITED: https://docs.unity3d.com/Manual/test-framework/run-tests-from-command-line.html]`
- **Per wave merge:** run full EditMode XML plus StandaloneWindows64 build and inspect `BuildReport`. `[CITED: https://docs.unity3d.com/Manual/build-script-build.html]`
- **Phase gate:** full EditMode green, build succeeds, and D-13 Player smoke is observed; licensing failure or uncertain visual output is not PASS. `[VERIFIED: 01-CONTEXT.md:29-33]`

### Wave 0 Gaps

- [ ] `Assets/Editor/Tests/MahjongRoundTraceTests.cs` — predefined `Assembly-CSharp-Editor` BASE-02 bounded seeded trace and first-mismatch output.
- [ ] `Assets/Editor/Tests/SoloSessionLifecycleTests.cs` — same predefined Editor boundary, beginning with two tracer cases and extending to thirteen.
- [ ] Enumerate all `Assets/**/*.asmdef` and `Assets/**/*.asmref`; require path/blob equality with `b18320e` so Phase 1 adds no descriptor and preserves any baseline descriptor.
- [ ] `Assets/Editor/Phase1Build.cs` — deterministic scene list, StandaloneWindows64 target, ignored output path, BuildReport failure propagation.
- [ ] `Assets/Scripts/MahjongGameManager.cs`, `PlayerHand.cs`, `UiScoreDistanceInfo.cs`, `UiRemainingTimeIndicator.cs`, `GameUIManager.cs`, `UiGameOver.cs` — minimal forfeit reason, input gate, reset and symmetric subscription edits.
- [ ] `.planning/phases/01-executable-baseline/01-BASELINE.md` — one committed before/after evidence summary.
- [ ] Unity LicensingClient activation/repair — required before executing the command on this host.

## Security Domain

### Applicable ASVS Categories

| ASVS Category | Applies | Standard Control |
|---------------|---------|-----------------|
| V2 Authentication | no | offline local game has no auth/identity boundary detected. `[VERIFIED: .planning/codebase/INTEGRATIONS.md:40-44]` |
| V3 Session Management | yes (local lifecycle) | make session finalization and restart idempotent; never accept late confirm/callback after finalization. `[VERIFIED: Assets/Scripts/MahjongGameManager.cs:91-112; .planning/codebase/CONCERNS.md:165-169]` |
| V4 Access Control | no remote access | no remote/API/account path; preserve manager state gate for local input. `[VERIFIED: .planning/codebase/INTEGRATIONS.md:5-20; Assets/Scripts/MahjongGameManager.cs:260-283]` |
| V5 Input Validation | yes | only accept fixed `PlayerCallType`/state transitions; bound test action loops; use fixed project-local output paths and reject malformed test data. `[VERIFIED: Assets/Scripts/AL-1S/_Enums_Ecchi_Nun_Mari.cs:22-29; Assets/Scripts/MahjongGameManager.cs:260-283]` |
| V6 Cryptography | no | no credentials, network, signed data, or cryptographic operation is in scope. `[VERIFIED: .planning/codebase/INTEGRATIONS.md:40-44,68-74]` |

### Known Threat Patterns for Unity local baseline

| Pattern | STRIDE | Standard Mitigation |
|---------|--------|---------------------|
| late event from prior session mutates current UI/domain | Tampering / Elevation | detach old round/service/timer handlers; guard finalization and generation/session identity. `[VERIFIED: .planning/codebase/CONCERNS.md:165-169]` |
| untrusted output path passed to Editor build method | Tampering | hard-code or normalize paths under `Builds/phase1`/`Temp/phase1`; do not accept arbitrary shell fragments. `[ASSUMED]` |
| local save high score changed by forfeit | Tampering | treat save as local/untrusted and skip high-score mutation for `Forfeit`; no network leaderboard. `[VERIFIED: Assets/Scripts/MahjongGameManager.cs:102-109; 01-CONTEXT.md:22-25; .planning/codebase/CONCERNS.md:105-111]` |
| oversized/malformed trace input stalls editor | Denial of Service | fixed seed/action list and max action bound; no user-provided replay file. `[VERIFIED: 01-CONTEXT.md:16-20]` |

## Sources

### Primary (HIGH confidence)

- `Assets/Scripts/AL-1S/MahjongRound.cs:150-166,248-283,389-452` - seeded constructor, public yama/tsumogiri flow, event boundary and next-round handoff.
- `Assets/Scripts/MahjongGameManager.cs:41-141,216-231,260-300` - StartNewGame, round subscriptions, timer finalization, UI input gate and current direct forfeit.
- `Assets/Scripts/UI-Kozeki/PlayerHand.cs:23-49,135-225` - Esc/PlayerCall events and incorrect game-over/start subscription.
- `Assets/Scripts/UI-Kozeki/GameUIManager.cs:49-172` - panel map, Initialize toggles, GameOver callback and transition API.
- `Assets/Scripts/UI-Kozeki/UiScoreDistanceInfo.cs:48-75` and `UiRemainingTimeIndicator.cs:16-27` - repeated Construct/subscription lifecycle.
- `ProjectSettings/ProjectVersion.txt:1-2`, `Packages/manifest.json:1-12`, `Packages/packages-lock.json:232-241`, `ProjectSettings/EditorBuildSettings.asset:7-10` - pinned editor, Test Framework graph and enabled scene.
- `.planning/REQUIREMENTS.md:8-14` and `.planning/phases/01-executable-baseline/01-CONTEXT.md:16-38` - BASE-01..05 and locked D-01..D-13.

### Secondary (MEDIUM confidence)

- `.planning/codebase/TESTING.md:1-24,31-80` - existing test gap, proposed test locations and batch command shape.
- `.planning/codebase/CONCERNS.md:39-43,75-97,165-181,234-281` - audited build, restart, timer and coverage risks.
- `.planning/codebase/STRUCTURE.md:1-18,75-115` and `.planning/codebase/ARCHITECTURE.md:293-303` - folder/assembly and composition-root constraints.
- Local environment probes and `Temp/phase1-baseline-editmode.log` - Unity executable/support availability and LicensingClient failure; not a source-code pass/fail.

### Tertiary (LOW confidence)

- None used. No external package or web research was performed during this bounded retry.

## Metadata

**Confidence breakdown:**
- Standard stack: HIGH for pinned local versions; MEDIUM for host execution because LicensingClient blocked the run.
- Architecture: HIGH for existing event/caller boundaries; MEDIUM for the minimum confirmation UI integration, which needs planner scene inspection.
- Pitfalls: HIGH for source-observed defects; MEDIUM for final trace serialization and GUI smoke implementation choices.

**Research date:** 2026-08-29
**Valid until:** 2026-09-28 for pinned repository facts; recheck Unity licensing and host availability immediately before execution.

## RESEARCH COMPLETE
