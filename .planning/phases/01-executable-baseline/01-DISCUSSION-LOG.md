# Phase 1: Executable Baseline — 실행 기준선과 검증 경로 - Discussion Log

> **Audit trail only.** Do not use as input to planning, research, or execution agents.
> Decisions are captured in CONTEXT.md — this log preserves the alternatives considered.

**Date:** 2026-08-29, updated 2026-08-31
**Phase:** 1-Executable Baseline — 실행 기준선과 검증 경로
**Areas discussed:** 대표 재현 시나리오, 포기·종료·재시작, 검증 증거, 솔로 모드 책임과 명명, 모드 생명주기, 입력과 UI 전파, PlayerHand 분리, 포기 모달 상태와 입력, 모달 표현, 구현 순서와 검증

---

## 2026-08-29 초기 논의 요약

### 대표 재현 시나리오

| Option | Description | Selected |
|--------|-------------|----------|
| 첫 배패 + 고정 횟수 타패 | 가장 짧지만 국 종료와 다음 국 전환은 검증하지 않는다. | |
| 첫 국 유국 후 다음 국 첫 쯔모 | 전체 국 생명주기를 180초 대기 없이 검증한다. | ✓ |
| 180초 솔로 게임 전체 종료 | 실제 흐름과 가깝지만 느리고 시간 의존적이다. | |

| Option | Description | Selected |
|--------|-------------|----------|
| 작은 EditMode 자동화 테스트 | 고정 시드와 쯔모기리 행동을 테스트 코드에 둔다. | ✓ |
| 별도 행동 기록 시스템 | 포트폴리오 기록은 늘지만 관리 대상이 추가된다. | |
| Unity 화면 자동 재생 | 별도 리플레이 기능이 되어 Phase 1에 과하다. | |

**User's choice:** 고정 시드 쯔모기리로 첫 국을 유국 처리하고 다음 국 첫 쯔모까지 작은 EditMode 테스트로 재현한다.
**Notes:** 이 테스트는 결정론적 실행과 국 전환만 검증하며 역·판·부·점수 정확성은 Phase 2에 둔다. 성공 로그는 짧게, 실패 로그는 최초 불일치를 자세히 남긴다.

### 포기·종료·재시작

| Option | Description | Selected |
|--------|-------------|----------|
| 기존 결과 화면 + 포기 사유 | 현재 결과 흐름을 재사용하면서 종료 원인을 구분한다. | ✓ |
| 결과 없이 메인 메뉴 | 짧지만 현재 결과를 확인할 수 없다. | |
| 별도 포기 결과 화면 | 구분은 명확하지만 UI 상태가 증가한다. | |

| Option | Description | Selected |
|--------|-------------|----------|
| 현재 거리 표시, 고득점 미갱신 | 미완료 게임의 결과와 공식 기록을 구분한다. | ✓ |
| 포기도 고득점 갱신 | 당시 코드 동작을 그대로 유지한다. | |
| 포기 결과 무효 | 현재 거리도 표시하지 않는다. | |

| Option | Description | Selected |
|--------|-------------|----------|
| 메뉴 복귀 후 기존 시작 버튼 | 기존 메뉴 흐름과 초기화 경계를 재사용한다. | ✓ |
| 결과 화면에서 즉시 재시작 | 빠르지만 새 버튼과 경로가 필요하다. | |
| 자동 재시작 | 메뉴 선택과 결과 확인 기회가 없다. | |

**User's choice:** 포기는 기존 결과 화면에서 구분하고 고득점을 갱신하지 않으며, 메뉴를 거쳐 실제 게임은 새 난수로 재시작한다.
**Notes:** 재시작 때 타이머·점수·거리·라운드·손패·하천·UI·이벤트 구독을 모두 초기화한다. 고정 시드는 자동화 테스트에서만 사용한다.

### 검증 증거와 입력 안전장치

| Option | Description | Selected |
|--------|-------------|----------|
| 짧은 Markdown 요약만 커밋 | 사람이 읽기 쉽고 raw 출력 잡음을 피한다. | ✓ |
| XML·빌드 로그도 커밋 | 원본은 남지만 경로·시간·대용량 diff가 누적된다. | |
| 결과를 커밋하지 않음 | 저장소는 깔끔하지만 당시 증거가 없다. | |

| Option | Description | Selected |
|--------|-------------|----------|
| Codex가 테스트·빌드·GUI 확인 | 자동화 가능한 부분은 실행하고 불확실한 화면은 사용자에게 확인한다. | ✓ |
| 사용자가 전체 체크리스트 수행 | 수동 부담이 증가한다. | |
| GUI까지 모두 PlayMode 자동화 | Phase 1 구현 범위가 커진다. | |

| Option | Description | Selected |
|--------|-------------|----------|
| 타이머가 계속 흐르는 포기 확인창 | 우발 종료를 막으면서 추가 사고 시간을 주지 않는다. | ✓ |
| `Esc` 길게 누르기 | 일시정지 악용은 없지만 홀드 표시와 취소 상태가 증가한다. | |
| 일시정지·설정·포기 메뉴 통합 | 미래 설정에는 적합할 수 있으나 현재 범위를 확대한다. | |

**User's choice:** 기준선과 완료 상태를 비교하는 Markdown 요약 하나를 커밋하고, 포기 확인 중에는 게임 입력만 막고 타이머는 계속 진행한다.
**Notes:** Windows Player 확인은 시작→포기 확인→포기 결과→메뉴→같은 프로세스 재시작까지다. 화면을 실제로 관찰하지 못했으면 PASS로 추정하지 않는다.

---

## 솔로 모드 책임과 명명

| Option | Description | Selected |
|--------|-------------|----------|
| 범용 `MahjongGameManager` 유지 | 솔로와 4인전의 생명주기를 한 manager가 모두 담당한다. | |
| `SoloScoringGameController` | 현재 책임을 솔로 모드로 제한하되 Controller라는 이름을 사용한다. | |
| `SoloScoringGameManager` | 현재 책임을 솔로 모드로 제한하고 프로젝트에서 직관적인 Manager 명칭을 사용한다. | ✓ |

**User's choice:** `MahjongGameManager`를 `SoloScoringGameManager`로 변경한다.
**Notes:** 사용자는 게임 생명주기와 진행을 소유하는 타입에 Controller라는 이름이 비직관적이라고 보았다. 미래 4인전은 별도 `FourPlayerGameManager`가 담당하고, 공유 규칙은 `MahjongRound` 계층에 둔다.

| Option | Description | Selected |
|--------|-------------|----------|
| 기존 `GameUIManager` 유지 | 패널 전환만 유지하고 GameManager의 다수 UI 참조도 유지한다. | |
| 새 UI 계층 추가 | `GameUIManager` 위에 별도 `SoloScoringUIController`를 추가한다. | |
| 기존 타입 개명·확장 | `GameUIManager`를 `SoloScoringUIController`로 바꾸고 솔로 출력 View를 통합한다. | ✓ |

**User's choice:** 기존 `GameUIManager`를 새 계층으로 감싸지 않고 개명·확장한다.
**Notes:** 로비·모드 선택은 기존 `UiManager`가 계속 담당한다. 모드 UI Controller는 다른 게임 모드에서 공유하지 않고 실제 leaf view만 재사용한다.

---

## 모드 생명주기

| Option | Description | Selected |
|--------|-------------|----------|
| 모든 GameManager 상시 활성 | 씬 로드부터 모든 모드가 구독한 채 `GameState`로 입력을 무시한다. | |
| 선택 모드 Root만 활성 | 단일 씬에 비활성 Root를 두고 모드 진입/복귀에 맞춰 활성화한다. | ✓ |
| 지금 모드별 씬 분리 | 솔로와 미래 4인전을 각각 별도 씬으로 이동한다. | |

**User's choice:** 씬에는 모드 Root가 존재하지만 선택한 모드만 활성화한다.
**Notes:** `OnEnable`/`OnDisable`은 컴포넌트 이벤트 연결 생명주기, `StartNewGame`/`HandleGameOver`는 한 번의 3분 실행 생명주기로 분리한다. 결과 화면 동안 Root는 유지하고 메뉴 복귀 때 비활성화한다.

---

## 입력과 UI 전파

| Option | Description | Selected |
|--------|-------------|----------|
| UIController가 입력 중계 | `PlayerHand` 이벤트를 UIController가 받아 GameManager로 다시 발행한다. | |
| 양쪽이 입력 구독 | GameManager와 UIController가 같은 `PlayerHand` 이벤트를 각각 구독한다. | |
| GameManager가 직접 입력 구독 | `PlayerHandController`가 GameManager로 의도를 보내고 GameManager가 UIController에 결과를 표시한다. | ✓ |

**User's choice:** `PlayerHand → SoloScoringGameManager → UI` 전파를 사용한다.
**Notes:** UI 갱신은 GameManager가 UIController의 명시적 메서드를 직접 호출한다. 범용 이벤트 버스는 추가하지 않는다.

| Option | Description | Selected |
|--------|-------------|----------|
| `PlayerCallType.Forfeit` 유지 | 포기를 리치·쯔모 같은 마작 선언 enum 경로로 계속 전달한다. | |
| 세션 의도로 분리 | 별도 `ForfeitRequested` 이벤트로 GameManager에 전달한다. | ✓ |

**User's choice:** `PlayerCallType.Forfeit`를 제거하고 `ForfeitRequested`로 분리한다.
**Notes:** 포기는 마작 규칙이 아니라 솔로 실행을 종료하려는 세션 제어이므로 미래 4인전의 도메인 행동과 섞지 않는다.

---

## PlayerHand 역할 분리

| Option | Description | Selected |
|--------|-------------|----------|
| Phase 1에서는 유지 | 현재 입력·선택·표시 혼합을 미래 4인전 Phase까지 그대로 둔다. | |
| 초기 기능과 동시에 분리 | ESC 확인 기능을 만드는 변경 안에서 바로 여러 책임을 나눈다. | |
| 검증 후 중간 리팩토링 | 기존 구조에서 ESC 동작을 먼저 확인하고 별도 작업으로 Controller/View를 분리한다. | ✓ |

**User's choice:** Phase 1의 별도 중간 리팩토링 단계로 `PlayerHandController`와 `PlayerHandView`를 분리한다.
**Notes:** Controller는 입력·선택 인덱스·버림/선언/포기 의도를, View는 손패 생성·강조·쯔모패 표시를 맡는다. 4인 좌석, 상대 손패, 후로 영역과 가변 손패 일반화는 추가하지 않는다.

---

## 포기 모달 상태와 입력

| Option | Description | Selected |
|--------|-------------|----------|
| UI가 모달 상태 소유 | UI가 열림 여부와 게임 입력 차단 여부를 판단한다. | |
| 전체 게임 일시정지 | 모달과 함께 타이머 및 게임 진행을 멈춘다. | |
| GameManager가 정책 상태 소유 | 게임은 계속 진행시키고 GameManager가 모달/입력 정책을 통제한다. | ✓ |

**User's choice:** 모달 열림 상태와 게임 입력 차단은 `SoloScoringGameManager`가 소유한다.
**Notes:** 모달 중에도 180초 타이머는 흐른다. 시간 종료가 먼저 발생하면 Timeout을 우선 처리한다. UIController는 Show/Hide만 수행한다.

| Option | Description | Selected |
|--------|-------------|----------|
| 입력 컴포넌트 전체 비활성 | `PlayerHandController`를 꺼서 모든 키를 차단한다. | |
| ESC 우선 + 게임 입력 플래그 | ESC는 항상 확인하고 나머지 게임 입력만 early return으로 차단한다. | ✓ |
| 범용 입력 문맥 관리자 도입 | 여러 모달을 가정한 새 전역 입력 계층을 만든다. | |

**User's choice:** `Esc`를 입력 차단 검사보다 먼저 처리하고, 게임 입력만 불리언 플래그로 차단한다.
**Notes:** 두 번째 ESC는 모달 취소로 계속 작동한다. Minecraft의 인벤토리/ESC 메뉴처럼 UI 입력이 게임 입력을 대신 점유하는 동작을 기대하지만, Phase 1에는 범용 입력 관리자를 만들지 않는다.

| Option | Description | Selected |
|--------|-------------|----------|
| 키를 직접 폴링 | 모달의 방향키·Enter도 커스텀 `Update`에서 처리한다. | |
| 새 `UIInputManager` 추가 | UI 전용 키 입력 관리자를 별도로 만든다. | |
| 기존 Unity EventSystem 사용 | `StandaloneInputModule`, `Button.Navigation`, Submit과 Button 클릭을 재사용한다. | ✓ |

**User's choice:** 기존 Unity `EventSystem`을 사용한다.
**Notes:** 모달을 열 때 No/Cancel 버튼을 기본 선택한다. 방향키·WASD와 Enter, 마우스 클릭이 같은 Button 경로로 합쳐진다.

---

## 모달 표현

| Option | Description | Selected |
|--------|-------------|----------|
| `GameUIState` 패널로 추가 | 확인창이 열릴 때 기존 게임 패널을 전환하거나 숨긴다. | |
| 범용 모달 프레임워크/프리팹 | 미래 여러 모드와 모달을 위한 추상 계층을 Phase 1에서 만든다. | |
| 씬 로컬 오버레이 | 현재 게임 화면 위에 별도 확인창을 표시한다. | ✓ |

**User's choice:** 확인창은 상호 배타적인 게임 패널 맵 밖의 씬 로컬 모달 오버레이로 둔다.
**Notes:** 손패와 타이머는 뒤에서 계속 보인다. 실제 두 번째 재사용처가 생길 때만 프리팹으로 추출한다. 기존 DOTween 표현은 재사용할 수 있다.

---

## 구현 순서와 검증

| Option | Description | Selected |
|--------|-------------|----------|
| 리팩토링 우선 | 새 구조를 먼저 만든 뒤 ESC 기능을 구현한다. | |
| 기능과 리팩토링 혼합 | 한 작업/커밋 안에서 동작과 구조를 동시에 바꾼다. | |
| 기능 검증 후 독립 리팩토링 | 기존 구조의 동작을 먼저 확보하고 같은 검증으로 리팩토링 전후를 비교한다. | ✓ |

**User's choice:** ESC 기능을 먼저 완성·검증한 다음 명명/UI/PlayerHand 구조를 별도 중간 리팩토링으로 수행한다.
**Notes:** 사용자는 작동하지 않는 코드와 작동하는 기준을 비교하며 원인을 배우고자 한다. 현재 01-01 구현 커밋은 보존하고 새 컨텍스트에서 기존 PLAN 문서를 다시 만든다.

| Option | Description | Selected |
|--------|-------------|----------|
| 애니메이션 후 입력 차단 | 확인창의 DOTween 표시가 끝난 뒤 손패 입력을 막는다. | |
| 동기식 상태 전환 | 한 GameManager 메서드에서 상태·입력·UI를 전환하고 ESC 처리 후 즉시 반환한다. | ✓ |

**User's choice:** 모달 표시와 입력 차단을 같은 동기식 전환으로 보장한다.
**Notes:** ESC와 버림이 같은 프레임에 들어와도 모달만 열리고 타패되지 않아야 하며 이를 회귀 검증에 포함한다. DOTween 완료 여부는 입력 정책에 영향을 주지 않는다.

---

## the agent's Discretion

- 포기 종료 사유를 표현하는 구체적인 결과 값과 메서드/필드명
- DOTween 표시 세부값과 EventSystem 선택 복구 방식
- Unity `.meta` GUID와 씬 직렬화 참조를 보존하는 개명 순서
- 같은 프레임 입력 차단 자동 검증의 EditMode/PlayMode 배치 위치

## Deferred Ideas

- 4인 좌석·상대 손패·후로 UI·가변 손패 크기 일반화
- 모드 리소스/로딩 복잡성이 실제로 커질 때 별도 씬 분리
- 인벤토리·설정·대화창 등 여러 모달이 생길 때 범용 입력 문맥/모달 스택
- 두 번째 모드가 같은 표현을 사용할 때 포기 확인창 프리팹 추출
- 실제 플레이 필요성이 확인될 경우 `Esc` 홀드 포기 방식
