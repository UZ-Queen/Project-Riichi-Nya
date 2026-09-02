# Project Riichi Nya

## What This Is

15개월 전에 중단된 Unity 리치마작 프로젝트를 다시 분석하고 개선하여, 2026 넥토리얼 게임 프로그래머 지원용 포트폴리오로 완성한다. 기존의 제한 시간·점수·이동 거리 기반 1인 조패 모드는 독립적인 플레이 경험으로 유지하고, 같은 규칙 엔진을 사용하는 4인 반장전 모드를 추가한다.

이번 작업은 기능 수를 늘리는 것보다 기존 코드의 치명적인 규칙 오류를 고치고, 자동화 테스트로 검증하며, Unity 표현 계층과 순수 C# 마작 도메인을 분리하는 데 우선순위를 둔다. AI의 제안을 그대로 채택하지 않고 코드 조사·판단·테스트를 거쳐 개선한 과정을 포트폴리오 증거로 남긴다.

## Core Value

기존 마작 로직을 정확하고 테스트 가능하며 유지보수하기 쉬운 규칙 엔진으로 개선하고, 그 엔진이 서로 다른 1인·4인 플레이 흐름에 재사용되는 과정을 검증 가능한 전후 증거로 보여준다.

## Requirements

### Validated

- ✓ Unity 2022.3.29f1과 C# 9.0을 사용하는 Windows 대상 프로젝트가 존재한다 — existing
- ✓ 제한 시간 동안 조패·화료하여 점수, 부스트, 이동 거리를 얻는 1인 게임 흐름이 존재한다 — existing
- ✓ 패, 몸통, 화료 형태, 역, 판, 부, 점수표를 다루는 자체 마작 도메인 구현이 존재한다 — existing
- ✓ uGUI, TextMesh Pro, DOTween을 사용한 메뉴·게임·화료·게임오버 UI가 존재한다 — existing
- ✓ JSON 고득점 저장, PlayerPrefs 음량 저장, ScriptableObject 패 이미지 데이터베이스가 존재한다 — existing
- ✓ 타일 데이터베이스를 생성하는 Unity Editor 도구가 존재한다 — existing
- ✓ `portfolio-baseline` 태그, 고정 시드 trace, Unity XML, Windows Player 빌드로 기존 솔로 동작을 재현하는 실행 기준선을 확립했다 — Phase 1
- ✓ 솔로 포기·메뉴 복귀·동일 프로세스 재시작과 저장 복구 안전성을 자동 검증 및 사용자 UAT로 확인했다 — Phase 1

### Active

- [ ] 139장 패산, 편향 셔플, 탐욕적 화료패 분해, 역만·쯔모 점수 처리 등 확인된 치명적 규칙 오류를 수정한다.
- [ ] 패산 생성, 화료 판정, 역·부·점수 계산에 회귀 테스트와 표 기반 EditMode 테스트를 추가한다.
- [ ] Unity UI와 게임 진행에서 순수 C# 마작 규칙을 분리하고, 대국·국·플레이어·행동·정산의 책임과 상태 전이를 명확히 한다.
- [ ] 기존 1인 모드를 유지하면서 리치 기능과 재시작·빌드 안정성을 개선한다.
- [ ] 사람 1명과 쯔모기리만 수행하는 더미 좌석 3명으로 기본 규칙 반장전을 진행하는 4인 모드를 구현한다.
- [ ] 4인 모드에서 사람 좌석은 쯔모와 론으로 화료할 수 있고, 상대 세 좌석의 버림패에 대한 론 가능 여부를 판정한다.
- [ ] 4인 모드에서 25,000점 시작, 동장·남장, 친 연장, 연장봉, 리치봉, 남4국 종료를 처리한다.
- [ ] 단계 2의 정확성과 테스트가 확보된 뒤 사람 좌석의 치·퐁·대명깡·암깡·가깡과 열린 손패의 화료·역·부 계산을 구현한다.
- [ ] 실행 가능한 Windows 빌드, README, 자동화 테스트 결과, 개선 전후 자료, 짧은 플레이 시연 영상을 준비한다.
- [ ] 대표 개선 사례 3~5개의 문제 정의, AI 제안, 채택·거절 판단, 구현, 검증 결과를 간결한 AI 개발 기록으로 남긴다.

### Out of Scope

- 상대 좌석의 전략적 타패·리치·화료·후로 판단 — 단계 4의 AI 작업으로 연기하며, 단계 2·3에서는 쯔모기리만 수행한다.
- Mortal 등 외부 마작 AI 연동 — 기본 규칙 엔진과 합법 행동 경계가 검증된 이후의 장기 목표다.
- 4명의 로컬 인간 플레이 또는 네트워크 멀티플레이 — 이번 포트폴리오의 핵심 가치와 9일 일정에 필요하지 않다.
- 서입, 들통, 오라스 종료 선택, 사풍연타·사개리치·사깡산료 등 추가 종료·도중 유국 규칙 — 미래에 결과 종류와 진행 정책을 확장할 수 있게 모델링하되 현재는 구현하지 않는다.
- 여러 규칙을 선택하는 설정 화면, 플러그인 시스템, 사용되지 않는 미래용 인터페이스 — 실제 변형 요구가 생기기 전에는 추가하지 않는다.
- 데이터베이스, 서버, 온라인 순위표, 신뢰 가능한 점수 업로드 — 현재 프로젝트는 로컬 싱글플레이 포트폴리오다.
- UI·아트 전면 재제작 — 두 모드를 이해하고 시연하는 데 필요한 범위만 개선한다.

## Context

- 현재 코드는 단일 `Assets/Scenes/SampleScene.unity`를 composition root로 사용하며, 대부분의 런타임 코드가 기본 `Assembly-CSharp`에 함께 컴파일된다.
- `Assets/Scripts/MahjongGameManager.cs`가 게임 수명주기, 도메인 이벤트, UI, 점수·거리 서비스를 함께 조율한다. 4인 대국 확장을 위해 모드별 진행과 공통 규칙의 경계를 분리해야 한다.
- 핵심 규칙은 `Assets/Scripts/AL-1S/`에 있으나 `MahjongYaku.cs`, `MahjongUtilities.cs`, `MahjongRound.cs`에 큰 책임이 집중되어 있고 프로젝트 작성 테스트가 없다.
- 코드베이스 매핑에서 패산이 139장으로 생성되는 오류, 마지막 원소가 선택되지 않는 셔플, 유효한 분해를 놓치는 탐욕 알고리즘, 역만 배수와 쯔모 정산 오류가 확인되었다.
- 재시작 시 이벤트 중복과 UI 토글 문제, 런타임 파일의 `UnityEditor` 참조로 인한 Player 빌드 실패 위험도 확인되었다.
- 1인 모드의 타이머·점수·부스트·거리 시스템은 이미 만들어진 고유한 플레이 경험이므로 삭제하거나 4인 규칙에 억지로 합치지 않는다.
- 4인 모드의 상대 세 좌석은 AI가 아니라 결정론적 더미다. 자신의 차례에 패를 뽑고 그 패를 그대로 버리며, 리치·화료·후로를 선언하지 않는다.
- 단계 3의 후로는 사람 좌석만 사용한다. 상대의 버림패에 치·퐁·대명깡을 선언하고, 자신의 패로 암깡·가깡을 수행하며, 영상패와 깡도라 및 열린 손패 점수 계산을 처리한다.
- 포트폴리오 비교 기준은 현재 커밋 `b18320e`다. 소스 변경 전에 `portfolio-baseline` annotated tag를 만들고, 최종 결과는 Git diff와 사례별 테스트 결과로 비교할 계획이다.
- 별도의 `Before/After` 코드 복사본은 만들지 않는다. Git 이력과 태그를 원본 증거로 사용한다.
- AI 개발 기록은 전체 대화 덤프가 아니라 대표 사례별 문제, AI 활용, 사람의 판단, 코드 변경, 검증 결과를 정리한다.

## Constraints

- **Timeline**: 작업 기한은 9일이다 — 단계 2의 정확한 완료와 포트폴리오 전달물을 우선하고, 단계 3은 하위 단계가 검증된 뒤 진행한다.
- **Quality gate**: 치명적 오류와 규칙 로직에는 자동화 테스트가 필요하다 — 기능 구현 속도를 위해 정확성과 회귀 방지를 생략하지 않는다.
- **Architecture**: 객체지향은 책임과 상태 전이를 명확히 하는 수단이다 — 클래스 수, 계층 수, 패턴 사용 자체를 목표로 삼지 않는다.
- **Compatibility**: Unity 2022.3.29f1 LTS와 기존 에셋·UI·DOTween 기반을 유지한다 — 9일 동안 엔진 업그레이드나 전면 교체를 하지 않는다.
- **Rules**: 반장전은 하나의 고정 기본 규칙을 구현한다 — 미래 규칙은 현재 모델을 방해하지 않는 범위에서만 고려하고 미리 구현하지 않는다.
- **Opponent behavior**: 상대 세 좌석은 쯔모기리만 수행하고 어떤 선언도 하지 않는다 — 이를 완전한 전략 AI로 표현하지 않는다.
- **Portfolio evidence**: 모든 주요 개선은 기존 증상, 근본 원인, 선택 이유, 테스트 또는 빌드 결과로 설명 가능해야 한다.
- **Source preservation**: 현재 소스 기준점과 변경 이력을 Git으로 보존한다 — 비교용 중복 소스 트리를 추가하지 않는다.

## Key Decisions

| Decision | Rationale | Outcome |
|----------|-----------|---------|
| 기존 1인 모드를 별도 플레이 경험으로 유지한다 | 이미 구현된 타이머·점수·거리 시스템의 가치를 보존하고 4인 모드와 다른 재미를 제공한다 | — Pending |
| 1인·4인 모드는 규칙 엔진을 공유하고 진행 흐름은 분리한다 | 공통 규칙을 한 번 검증하면서 모드별 상태와 UI 조건문이 서로 얽히는 것을 막는다 | — Pending |
| 9일의 안정적 완료선은 단계 2이며 단계 3은 그 이후 목표다 | 유지보수성과 검증을 희생해 기능 수만 늘리지 않기 위해서다 | — Pending |
| 단계 2의 상대 세 좌석은 결정론적 쯔모기리 더미로 둔다 | AI 구현 없이 4좌석 턴·패산·하천·론 흐름을 검증할 수 있다 | — Pending |
| 단계 2에서 사람만 쯔모·론으로 화료한다 | 상대 의사결정은 제외하면서 사람의 4인 대국 상호작용을 제공한다 | — Pending |
| 반장전은 고정 기본 규칙으로 구현한다 | 하나의 정확한 규칙을 완성하고 불필요한 설정·플러그인 구조를 피한다 | — Pending |
| 단계 3은 사람의 치·퐁과 모든 종류의 깡을 포함한다 | 향후 AI 후로 판단에 앞서 합법 행동과 열린 손패 규칙을 완성한다 | — Pending |
| 특수 유국·추가 종료 규칙은 구현하지 않는다 | 상태 결과와 진행 책임만 명확히 해 미래 추가가 기존 코드를 흩뜨리지 않게 한다 | — Pending |
| AI 플레이는 단계 4로 연기한다 | 합법 행동과 대국 상태 표현이 검증되기 전에 의사결정 모델을 붙이지 않는다 | — Pending |
| `b18320e`를 `portfolio-baseline` 태그 기준으로 사용한다 | 중복 코드 없이 기존과 최종 구현을 재현 가능하게 비교한다 | ✓ Phase 1 — annotated tag와 정확한 peeled commit 검증 완료 |
| 대표 사례 중심 AI 개발 기록을 남긴다 | AI 제안보다 문제 정의, 사람의 판단, 검증 능력을 보여주기 위해서다 | — Pending |
| 솔로 생명주기·정책은 `SoloScoringGameManager`가 소유하고 입력·표현은 `PlayerHandController`/`PlayerHandView`로 분리한다 | 4인 모드 확장 전에 기존 솔로 흐름의 책임과 재시작 경계를 명확히 한다 | ✓ Phase 1 — 4+15 XML과 동일 PID UAT 통과 |
| 자동 XML·빌드와 실제 Player 관찰을 별도 증거로 유지한다 | 존재하지 않거나 관찰하지 않은 결과를 PASS로 추정하지 않는다 | ✓ Phase 1 — fail-closed 검증 및 UAT 완료 |

## Evolution

This document evolves at phase transitions and milestone boundaries.

**After each phase transition** (via `$gsd-transition`):
1. Requirements invalidated? → Move to Out of Scope with reason
2. Requirements validated? → Move to Validated with phase reference
3. New requirements emerged? → Add to Active
4. Decisions to log? → Add to Key Decisions
5. "What This Is" still accurate? → Update if drifted

**After each milestone** (via `$gsd-complete-milestone`):
1. Full review of all sections
2. Core Value check — still the right priority?
3. Audit Out of Scope — reasons still valid?
4. Update Context with current state

---
*Last updated: 2026-09-02 after Phase 1*
