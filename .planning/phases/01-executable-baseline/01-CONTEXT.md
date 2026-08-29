# Phase 1: Executable Baseline — 실행 기준선과 검증 경로 - Context

**Gathered:** 2026-08-29
**Status:** Ready for planning

<domain>
## Phase Boundary

기존 솔로 게임의 소스를 고치기 전에 재현 가능한 실행 기준선을 만들고, Unity EditMode 배치 테스트·Windows Player 빌드·같은 프로세스 내 포기 후 재시작을 검증할 수 있는 경로를 확립한다. 이 Phase는 기존 규칙 계산의 정확성을 교정하거나 포괄적인 리플레이·일시정지·설정 기능을 만드는 단계가 아니다.

</domain>

<decisions>
## Implementation Decisions

### 대표 재현 시나리오
- **D-01:** 별도 리플레이 UI나 범용 행동 기록 시스템을 만들지 않고, 고정 시드와 행동을 테스트 코드에 둔 작은 EditMode 자동화 테스트로 재현한다.
- **D-02:** 대표 행동은 쯔모기리를 반복하는 것이며, 첫 국 유국 후 다음 국의 첫 쯔모까지 실행한다.
- **D-03:** 성공 시에는 시드·행동 수·핵심 상태 요약·PASS를 짧게 출력하고, 실패 시에는 최초 불일치 행동 번호와 예상·실제 상태를 출력한다.
- **D-04:** 이 시나리오는 결정론적 실행과 국 전환을 검증할 뿐 역·판·부·점수 정확성을 주장하지 않는다. 순수 도메인 경계 정리와 일반형·치또이츠·국사무쌍·역·판·부·지불액의 표 기반 정확성 테스트는 Phase 2에서 수행한다.

### 포기·종료·재시작
- **D-05:** 실제 솔로 게임은 시작할 때마다 새 난수를 사용하고 자동화 테스트만 고정 시드를 사용한다.
- **D-06:** 포기 시 기존 결과 화면을 재사용하되 종료 사유를 `포기`로 표시하고 현재 거리는 보여준다. 완료하지 않은 게임이므로 고득점은 갱신하지 않는다.
- **D-07:** 결과 화면에서 메인 메뉴로 돌아간 뒤 기존 시작 버튼으로 새 솔로 게임을 시작한다. 새 게임은 타이머·점수·거리·라운드·손패·하천·UI와 이벤트 구독을 이전 실행과 분리해 초기화한다.
- **D-08:** 게임 중 `Esc`를 누르면 포기 확인창을 표시한다. 확인창이 열려 있는 동안 게임 입력은 차단하지만 180초 타이머는 계속 진행한다.
- **D-09:** 확인창에서 취소하거나 다시 `Esc`를 누르면 게임 입력을 복구하고, 확인하면 포기 결과 화면으로 이동한다. 확인 중 타이머가 끝나면 확인창을 닫고 정상 시간 종료를 우선 처리한다.

### 검증과 증거 보존
- **D-10:** Git에는 사람이 읽을 수 있는 짧은 Markdown 검증 요약 하나만 보존한다. Unity Test Framework XML, batchmode 로그, Windows 빌드 출력과 `BuildReport`는 프로젝트 로컬 출력 폴더에 실행 산출물로 생성하고 기본적으로 커밋하지 않는다.
- **D-11:** 검증 요약은 `portfolio-baseline`의 테스트 부재·빌드 문제·재시작 증상과 Phase 1 완료 후 결과를 한 문서에서 전후 비교한다. 정확한 실행 명령, Unity 버전, 대상 커밋, 테스트 수, 재현 시드와 행동 요약, 빌드·실행 결과를 포함한다.
- **D-12:** Codex 실행 에이전트가 EditMode 배치 테스트, Windows 빌드와 Player GUI 기본 동작 확인을 수행한다. 화면 판독이나 제어가 불확실하면 추측으로 PASS 처리하지 않고 사용자 확인을 요청한다.
- **D-13:** Windows Player 기본 동작 확인 범위는 빌드 → 실행 → 솔로 시작 → `Esc` 포기 확인 → 포기 결과 → 메뉴 복귀 → 같은 프로세스에서 재시작까지다. 실제 180초 시간 종료는 Phase 1 완료 조건에 포함하지 않는다.

### the agent's Discretion
- 테스트 assembly와 폴더 구성, 고정 시드 값, 상태 요약의 정확한 직렬화 형식과 비교 방식은 기존 Unity 2022.3.29f1/Test Framework 제약 안에서 planner가 최소 변경으로 정한다.
- 원본 XML·로그·빌드 출력의 프로젝트 로컬 생성 경로와 검증 Markdown 파일명은 기존 `.gitignore`와 Phase 7 증거 수집 흐름을 방해하지 않도록 planner가 정한다.
- 포기 종료 사유를 표현하는 최소 도메인 값과 확인창의 기존 UI 통합 위치는 현재 `PlayerCallType.Forfeit` 및 게임 상태 흐름을 재사용하는 방향으로 planner가 정한다.

</decisions>

<canonical_refs>
## Canonical References

**Downstream agents MUST read these before planning or implementing.**

### Project scope and requirements
- `.planning/PROJECT.md` — 9일 일정, baseline tag, 규칙 정확성 우선순위와 포트폴리오 증거 원칙을 정의한다.
- `.planning/REQUIREMENTS.md` § Executable Baseline — `BASE-01`~`BASE-05`의 고정 요구사항을 정의한다.
- `.planning/ROADMAP.md` § Phase 1 — Phase 1 목표, 범위와 성공 기준을 정의한다.

### Existing codebase guidance
- `.planning/codebase/TESTING.md` — 설치된 Unity Test Framework, 현재 테스트 부재, 배치 실행 명령과 테스트 경계 제약을 정리한다.
- `.planning/codebase/CONVENTIONS.md` — C#·Unity 코드 스타일, 오류 처리, 이벤트 구독과 테스트 관례를 정리한다.
- `.planning/codebase/STRUCTURE.md` — 테스트·런타임·Editor 코드의 기존 위치와 새 코드 배치 지침을 정리한다.

</canonical_refs>

<code_context>
## Existing Code Insights

### Reusable Assets
- `Assets/Scripts/AL-1S/MahjongRound.cs`: `MahjongRound.NewRound(int seed, out MahjongPlayer)`와 `DiscardTile(13)`이 고정 시드 쯔모기리 재현의 기존 진입점이다.
- `Assets/Scripts/UI-Kozeki/PlayerHand.cs`: 게임 중 `Esc`를 `PlayerCallType.Forfeit`로 발행하는 기존 입력 경로가 있다.
- `Assets/Scripts/MahjongGameManager.cs`: `CallHandler`, `HandleGameOver`, `AttachRoundEvent`와 `DetachRoundEvent`가 포기·종료·재시작의 공통 통합 지점이다.
- `Assets/Scripts/UI-Kozeki/GameUIManager.cs`와 `Assets/Scripts/UI-Kozeki/UiManager.cs`: 기존 결과 화면, 게임 패널 정리, 메뉴 복귀와 시작 버튼 흐름을 재사용할 수 있다.
- `Assets/Scripts/Timer.cs`와 `Assets/Scripts/ScoreManagerDistance.cs`: 재시작 시 초기화해야 할 시간·거리 상태의 기존 소유자다.

### Established Patterns
- 단일 `Assets/Scenes/SampleScene.unity`가 composition root이며, `MahjongGameManager`가 도메인 이벤트와 UI를 조율한다.
- Unity Test Framework 1.1.33은 이미 설치되어 있지만 프로젝트 작성 테스트, 테스트 assembly와 실행 래퍼는 없다.
- 런타임 코드의 `UnityEditor` 참조는 Player 빌드에서 제거하고 Editor 전용 코드는 `Assets/Editor/`에 둔다.
- 실제 게임의 난수 흐름과 자동화 테스트의 고정 시드를 분리한다. 미래 규칙 교정에 종속되는 golden scoring 결과를 Phase 1에 고정하지 않는다.

### Integration Points
- 재현 테스트는 `MahjongRound`의 공개 생성·타패 흐름을 사용하고 private 상태를 직접 조작하지 않는다.
- 포기 확인창은 기존 `Esc` 입력과 `PlayerCallType.Forfeit` 처리 사이에 확인 단계를 추가하며, 확인 전에는 `HandleGameOver`를 호출하지 않는다.
- 정상 시간 종료와 포기 종료는 결과 화면을 공유하되 종료 사유와 고득점 갱신 정책을 구분한다.
- 재시작 검증은 같은 Player 프로세스에서 두 번째 `StartNewGame`을 호출해 중복 이벤트와 잔존 상태를 탐지해야 한다.

</code_context>

<specifics>
## Specific Ideas

- 사용자는 3분 조패 모드에서 일시정지 후 패를 검토하는 흐름이 추가 사고 시간을 제공할 수 있다고 우려했다. 따라서 포기 확인 중에도 타이머를 멈추지 않는다.
- 포트폴리오 증거는 긴 로그보다 사람이 읽을 수 있는 간단한 텍스트 결과를 선호한다. 실패는 숨기지 않고 최초 불일치 상태와 원본 로그 위치를 남긴다.
- `smoke test`는 문서에서 필요하면 `Windows Player 기본 동작 확인`처럼 뜻이 드러나는 표현으로 설명한다.

</specifics>

<deferred>
## Deferred Ideas

- 실제 플레이 테스트에서 필요성이 확인되면 포기 확인창을 타이머가 계속 흐르는 `Esc` 홀드 입력으로 교체할 수 있다.
- 게임 중 설정 UI가 실제 범위에 들어올 때 포기 동작을 같은 공간에 통합할지 다시 논의한다. Phase 1에서는 설정 화면이나 범용 pause manager를 미리 만들지 않는다.

</deferred>

---

*Phase: 1-Executable Baseline — 실행 기준선과 검증 경로*
*Context gathered: 2026-08-29*
