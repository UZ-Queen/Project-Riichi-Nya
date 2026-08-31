# Phase 1: Executable Baseline — 실행 기준선과 검증 경로 - Context

**Gathered:** 2026-08-31
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

### 솔로 모드 책임과 명명
- **D-14:** 현재 `MahjongGameManager`는 범용 마작 매니저가 아니라 3분 솔로 스코어링 모드의 생명주기·진행·타이머·점수·종료 정책을 관리한다. 구조 개선 단계에서 `SoloScoringGameManager`로 개명하고, 추후 4인전은 별도 `FourPlayerGameManager`가 맡는다. 공유 규칙과 라운드 상태는 `MahjongRound` 계층에 남긴다.
- **D-15:** 현재 `GameUIManager`를 새 관리자로 감싸지 않고 `SoloScoringUIController`로 개명·확장한다. 기존 게임 패널 전환과 DOTween 표현을 유지하면서 점수·거리·라운드·승리·게임오버·포기 확인과 `PlayerHandView` 등 솔로 모드의 출력 View 참조를 이쪽으로 옮긴다. 로비·모드 선택 UI는 계속 `UiManager`가 담당한다.
- **D-16:** `SoloScoringGameManager`는 게임 입력 경계인 `PlayerHandController`와 출력 경계인 `SoloScoringUIController`를 직접 참조한다. 입력 의도는 GameManager로 올라가고, 처리 결과는 UIController의 명시적 표시 메서드로 내려간다. 양쪽이 같은 `PlayerHand` 이벤트를 중복 구독하거나 범용 이벤트 버스를 추가하지 않는다.
- **D-17:** 모드별 GameManager와 UIController는 재사용하지 않는다. 4인전은 자체 GameManager/UIController를 가지며, `PlayerHandView` 같은 실제 공통 표현 컴포넌트와 `MahjongRound` 도메인만 필요가 확인된 범위에서 공유한다.

### 씬과 실행 생명주기
- **D-18:** 단일 `SampleScene` 구조를 유지한다. 모드 Root는 씬에 직렬화해 두되 기본적으로 비활성화하고, 사용자가 모드를 선택할 때 해당 Root만 활성화한다. 메인 메뉴로 복귀할 때 Root를 비활성화하여 다른 모드의 입력 구독이나 `Update`가 동시에 실행되지 않게 한다.
- **D-19:** 컴포넌트 생명주기와 한 번의 3분 실행 생명주기를 분리한다. `OnEnable`/`OnDisable`은 `PlayerHandController`와 UI 확인·취소 이벤트의 구독/해제를 대칭으로 담당하고, `StartNewGame`은 타이머·점수·거리·라운드·손패·하천·UI 등 한 실행의 상태만 초기화한다. 게임오버 결과 화면을 보는 동안에는 모드 Root를 유지한다.

### 입력과 PlayerHand 분리
- **D-20:** `PlayerCallType.Forfeit`는 마작 선언이 아니라 솔로 실행을 끝내려는 세션 의도이므로 구조 개선 단계에서 제거하고 별도 `ForfeitRequested` 이벤트로 분리한다. 버림·리치·쯔모 등 마작 행동 이벤트와 포기 요청을 같은 enum 경로에 추가하지 않는다.
- **D-21:** Phase 1 중간에 현재 `PlayerHand`의 입력과 표시 책임을 별도 작업으로 분리한다. `PlayerHandController`는 키 입력·선택 인덱스·버림/선언/포기 의도를, `PlayerHandView`는 손패 생성·선택 강조·쯔모패 표시를 담당한다. 4인 좌석·상대 손패·후로 영역·가변 손패 일반화는 이 작업에 포함하지 않는다.
- **D-22:** `PlayerHandController`는 `Esc`를 게임 입력 차단 검사보다 먼저 처리하고 요청을 발생시킨 즉시 그 프레임의 처리를 반환한다. 확인창이 열린 동안 손패 이동·버림·쯔모 입력은 받지 않지만 두 번째 `Esc`는 취소 요청으로 계속 동작한다.

### 포기 확인 모달
- **D-23:** `SoloScoringGameManager`가 `isForfeitConfirmationOpen`에 해당하는 정책 상태와 게임 입력 차단을 소유한다. 모달이 열려 있어도 게임 상태와 180초 타이머는 계속 진행하며, `SoloScoringUIController`는 상태를 판단하지 않고 화면만 표시한다. 시간 종료가 먼저 발생하면 모달을 닫고 정상 시간 종료를 우선 처리한다.
- **D-24:** Yes/No의 방향키·WASD 이동, Enter 제출과 마우스 클릭은 씬의 기존 Unity `EventSystem`/`StandaloneInputModule`과 `Button.Navigation`을 사용한다. 모달을 열 때 안전한 기본값인 No/Cancel을 선택하고, 별도 `UIInputManager`나 매 프레임 Enter 폴링을 만들지 않는다.
- **D-25:** 확인창은 기존 게임 패널을 대체하지 않는 씬 로컬 모달 오버레이로 둔다. 손패·점수·타이머 화면 위에 표시하며 상호 배타적인 `GameUIState` 패널 맵에는 넣지 않는다. Phase 1에서는 프리팹이나 범용 모달 스택을 만들지 않고, 다른 모드에서 실제 재사용할 때 프리팹 추출을 검토한다.
- **D-26:** 모달 열기 전환은 한 GameManager 메서드 안에서 상태 설정·게임 입력 차단·UI 활성화를 동기식으로 완료한다. `Esc`와 버림 입력이 같은 프레임에 들어와도 모달만 열리고 패가 버려지지 않는 불변조건을 회귀 검증에 포함한다. DOTween은 표시 애니메이션만 담당하며 입력 차단을 애니메이션 완료까지 미루지 않는다.

### 구현 순서와 재계획 경계
- **D-27:** 먼저 기존 구조에서 `Esc` 확인 → 취소/확인 → 포기 결과 흐름을 완성하고 검증한다. 이후 명명 변경, `SoloScoringUIController`로 UI 참조 이동, `PlayerHandController`/`PlayerHandView` 분리를 기능 변경과 분리된 중간 리팩토링으로 수행한 뒤 같은 검증을 다시 실행한다.
- **D-28:** 현재 브랜치 `codex/forfeit-rebuild`의 `2eebf7a`, `d88f86d`, `ad3c5e7`은 완료된 01-01 구현 경계로 보존한다. 기존 PLAN 문서는 이번 결정보다 먼저 작성되었으므로 현재 HEAD에서 재계획하되, SUMMARY가 없다는 이유로 이 커밋의 작업을 재실행하거나 되돌리지 않는다.

### the agent's Discretion
- 테스트 assembly와 폴더 구성, 고정 시드 값, 상태 요약의 정확한 직렬화 형식과 비교 방식은 기존 Unity 2022.3.29f1/Test Framework 제약 안에서 planner가 최소 변경으로 정한다.
- 원본 XML·로그·빌드 출력의 프로젝트 로컬 생성 경로와 검증 Markdown 파일명은 기존 `.gitignore`와 Phase 7 증거 수집 흐름을 방해하지 않도록 planner가 정한다.
- 포기 종료 사유를 표현하는 최소 결과 값, 구체적인 필드명, DOTween 전환 세부값과 EventSystem 선택 복구 방식은 위 책임·입력 경계를 바꾸지 않는 범위에서 planner가 정한다.
- C# 파일/타입 개명 시 Unity `.meta` GUID와 씬 직렬화 참조를 보존하는 구체적인 마이그레이션 절차는 planner가 현재 자산 상태에 맞춰 정한다.
- 같은 프레임 입력 차단 불변조건의 자동 검증을 EditMode 또는 PlayMode 중 어디에 둘지는 실제 입력 코드의 테스트 가능 경계에 맞춰 정하되, Windows Player 사람 확인만으로 대체하지 않는다.

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
- `.planning/codebase/ARCHITECTURE.md` — 단일 씬 composition root, 기존 GameManager/UIManager/PlayerHand 책임과 도메인 이벤트 경계를 정리한다.

### Live integration targets
- `Assets/Scripts/MahjongGameManager.cs` — 현재 솔로 실행 정책과 UI 직접 참조가 모여 있으며 `SoloScoringGameManager`로 변경할 대상이다.
- `Assets/Scripts/UI-Kozeki/GameUIManager.cs` — 현재 게임 패널 맵과 DOTween 전환을 보유하며 `SoloScoringUIController`로 변경할 대상이다.
- `Assets/Scripts/UI-Kozeki/PlayerHand.cs` — 현재 입력·선택·손패 표시와 manager singleton 결합이 섞여 있어 Controller/View 분리 대상이다.
- `Assets/Scripts/UI-Kozeki/UiManager.cs` — 로비·모드 선택·게임 시작 경계를 계속 소유한다.
- `Assets/Scenes/SampleScene.unity` — 모드 Root, EventSystem, UI 버튼과 직렬화 참조를 연결하는 단일 composition root다.

</canonical_refs>

<code_context>
## Existing Code Insights

### Reusable Assets
- `Assets/Scripts/AL-1S/MahjongRound.cs`: `MahjongRound.NewRound(int seed, out MahjongPlayer)`와 `DiscardTile(13)`이 고정 시드 쯔모기리 재현의 기존 진입점이다.
- `Assets/Scripts/UI-Kozeki/PlayerHand.cs`: 게임 중 `Esc`를 발행하고 손패를 표시하는 기존 동작은 보존하되, `PlayerCallType.Forfeit`와 singleton 결합은 새 입력 경계로 교체한다.
- `Assets/Scripts/MahjongGameManager.cs`: `CallHandler`, `HandleGameOver`, `AttachRoundEvent`와 `DetachRoundEvent`가 포기·종료·재시작의 공통 통합 지점이다.
- `Assets/Scripts/UI-Kozeki/GameUIManager.cs`와 `Assets/Scripts/UI-Kozeki/UiManager.cs`: 기존 결과 화면, 게임 패널 정리, 메뉴 복귀와 시작 버튼 흐름을 재사용할 수 있다.
- `Assets/Scripts/Timer.cs`와 `Assets/Scripts/ScoreManagerDistance.cs`: 재시작 시 초기화해야 할 시간·거리 상태의 기존 소유자다.
- 씬의 `EventSystem`/`StandaloneInputModule`과 uGUI `Button.Navigation`: 확인창의 키보드 선택과 Enter 제출을 새 입력 관리 코드 없이 처리한다.

### Established Patterns
- 단일 `Assets/Scenes/SampleScene.unity`가 composition root이며, `MahjongGameManager`가 도메인 이벤트와 UI를 조율한다.
- Unity Test Framework 1.1.33은 이미 설치되어 있지만 프로젝트 작성 테스트, 테스트 assembly와 실행 래퍼는 없다.
- 런타임 코드의 `UnityEditor` 참조는 Player 빌드에서 제거하고 Editor 전용 코드는 `Assets/Editor/`에 둔다.
- 실제 게임의 난수 흐름과 자동화 테스트의 고정 시드를 분리한다. 미래 규칙 교정에 종속되는 golden scoring 결과를 Phase 1에 고정하지 않는다.
- 현재 `GameUIManager`는 로비가 아니라 인게임 패널 맵/전환만 맡고, `UiManager`가 로비와 시작 흐름을 맡는다. 이 경계를 개명 후에도 유지한다.
- 현재 브랜치에는 01-01 구현 커밋이 있지만 `01-01-SUMMARY.md`가 없다. 재계획은 실제 HEAD를 기준으로 하며 완료 구현을 중복 생성하지 않는다.

### Integration Points
- 재현 테스트는 `MahjongRound`의 공개 생성·타패 흐름을 사용하고 private 상태를 직접 조작하지 않는다.
- 포기 확인창은 `PlayerHandController.ForfeitRequested` → `SoloScoringGameManager` → `SoloScoringUIController.ShowForfeitConfirmation()` 흐름으로 열며, 확인 전에는 `HandleGameOver`를 호출하지 않는다.
- `SoloScoringGameManager`는 모드 Root의 `OnEnable`/`OnDisable`에서 `PlayerHandController` 입력과 `SoloScoringUIController`의 Confirm/Cancel 이벤트를 대칭 구독한다.
- 확인창이 열린 동안 `PlayerHandController`의 게임 입력은 비활성화하지만 `Esc` 분기와 EventSystem은 유지한다. 모달을 열 때 Cancel 버튼을 기본 선택한다.
- 확인창은 기존 게임 화면 위의 오버레이이므로 게임 패널 맵의 상호 배타적 전환을 사용하지 않는다.
- 정상 시간 종료와 포기 종료는 결과 화면을 공유하되 종료 사유와 고득점 갱신 정책을 구분한다.
- 재시작 검증은 같은 Player 프로세스에서 두 번째 `StartNewGame`을 호출해 중복 이벤트와 잔존 상태를 탐지해야 한다.

</code_context>

<specifics>
## Specific Ideas

- 사용자는 3분 조패 모드에서 일시정지 후 패를 검토하는 흐름이 추가 사고 시간을 제공할 수 있다고 우려했다. 따라서 포기 확인 중에도 타이머를 멈추지 않는다.
- 사용자는 Minecraft의 인벤토리·ESC 메뉴처럼 모달이 열리면 게임 입력 대신 UI 입력이 활성화되는 전형적인 입력 문맥을 기대한다. 이번 Phase에서는 기존 EventSystem과 하나의 게임 입력 플래그로 그 동작만 구현한다.
- 구조 개선은 먼저 작동하는 포기 흐름을 확보한 다음 독립 리팩토링으로 수행하여, 리팩토링 전후에 같은 행동을 비교하고 실패 원인을 추적할 수 있어야 한다.
- 포트폴리오 증거는 긴 로그보다 사람이 읽을 수 있는 간단한 텍스트 결과를 선호한다. 실패는 숨기지 않고 최초 불일치 상태와 원본 로그 위치를 남긴다.
- `smoke test`는 문서에서 필요하면 `Windows Player 기본 동작 확인`처럼 뜻이 드러나는 표현으로 설명한다.

</specifics>

<deferred>
## Deferred Ideas

- 실제 플레이 테스트에서 필요성이 확인되면 포기 확인창을 타이머가 계속 흐르는 `Esc` 홀드 입력으로 교체할 수 있다.
- 게임 중 설정 UI가 실제 범위에 들어올 때 포기 동작을 같은 공간에 통합할지 다시 논의한다. Phase 1에서는 설정 화면이나 범용 pause manager를 미리 만들지 않는다.
- 4인전의 좌석별 손패·상대 손패·후로 UI·가변 손패 크기와 입력 규칙은 실제 4인전 Phase에서 설계한다. Phase 1의 PlayerHand 분리는 현재 입력/표시 책임만 나눈다.
- 4인전 리소스와 로딩 비용이 단일 씬 구조를 실제로 압박할 때 모드별 씬 분리를 검토한다.
- 인벤토리·설정·대화창 등 복수 모달이 생길 때 범용 입력 문맥이나 모달 스택을 검토한다.
- 두 번째 모드가 동일한 확인창 표현을 실제로 재사용할 때 씬 오버레이를 프리팹으로 추출한다.

</deferred>

---

*Phase: 1-Executable Baseline — 실행 기준선과 검증 경로*
*Context gathered: 2026-08-31*
