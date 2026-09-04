# Phase 2: Shared Rules Core — 정확한 공유 규칙 코어 - Research

**Researched:** 2026-09-04
**Domain:** Unity 2022.3 / C# 9 리치마작 패산·화료 분해·점수·지불 코어
**Confidence:** MEDIUM

<user_constraints>
## User Constraints (from CONTEXT.md)

### Locked Decisions

### 기준 규칙과 확장 경계
- **D-01:** 고정 기본 규칙은 웹게임 작혼의 **4인 남장 랭크전 동작**을 목표로 한다. 공개 대회 PDF 하나를 랭크전 전체 규칙으로 간주하지 않는다.
- **D-02:** 규칙 근거의 우선순위는 현재 작혼 4인 남장 랭크전에서 확인되는 동작, 작혼 공식 도움말·공지, 동일한 동작을 설명하는 공식 행사/대회 자료, 신뢰할 수 있는 2차 정리 순이다. 공개 공식 문서로 확정할 수 없는 항목은 출처와 확인일을 적은 표 기반 fixture로 동결하고, 추론임을 표시한다.
- **D-03:** MVP는 하나의 고정된 개발자 작성 규칙 프로필만 사용한다. 향후 증강마작 규칙을 코드에서 교체할 수 있는 작은 프로필 경계는 허용하지만, 인게임 규칙 설정 화면, 외부 JSON/mod 프로필, 범용 규칙 해석기나 플러그인 시스템은 만들지 않는다.
- **D-04:** 작혼 4인 랭크전에 없는 로컬 역과 3인전 북빼기 도라는 제외한다.

### 패 정체성, 패산과 도라
- **D-05:** 패산은 정확히 136장이다. 각 패 종류는 네 장이며 만·통·삭의 일반 5 한 장을 적5 한 장으로 각각 대체하여 적5는 총 세 장만 존재한다.
- **D-06:** 관찰 가능한 패 값의 정체성은 `패 종류 + 적5 여부`다. 동일한 일반패 네 장을 구분하는 물리적 copy ID는 추가하지 않는다. 화료 분해와 대기 계산은 적색을 무시한 패 종류로 비교하고, 값 객체의 `==`, `Equals`, `GetHashCode`는 동일한 정체성 계약을 사용한다. — **Reversibility:** costly — 이 계약을 뒤집으면 HashSet/Dictionary 중복 처리, 직렬화와 모든 solver 비교 지점을 함께 마이그레이션해야 한다.
- **D-07:** 한 공유 패산 생성기는 같은 시드에 항상 같은 136장 순서를 반환하고, 셔플의 마지막 원소를 포함한 모든 합법 위치를 선택할 수 있어야 한다. Phase 2에서 보장하는 것은 두 모드의 **생성 순서**이며, 아직 존재하지 않는 4인 플레이 흐름과 솔로의 좌석별 쯔모열이 같다고 주장하지 않는다.
- **D-08:** 일반 도라와 우라도라는 표시패에서 화료 평가 시 계산한다. 패 값에 `isDora`나 `doraCount`를 누적해 변이시키지 않는다. 적5 여부만 패의 고유 속성으로 유지한다.
- **D-09:** 인게임 패의 도라 반짝임도 점수 코어와 같은 표시패→도라 해석 결과를 사용한다. 공개되지 않은 우라도라는 화료 전 화면에 표시하지 않는다.

### 화료 분해와 최종 해석 선택
- **D-10:** 일반형, 치또이츠, 국사무쌍을 포함하여 입력과 맥락에 맞는 모든 합법 화료 분해를 열거하고 각각 점수화한다. 실제 런타임 API는 최종 최적 결과 한 개를 반환하며 전체 후보 목록은 테스트·디버그 경계에서만 사용한다.
- **D-11:** 최적 결과는 화료자의 실제 총 수입이 큰 순서로 선택하고, 같으면 판, 다시 같으면 부를 비교한다. 이 값들까지 모두 같으면 결정론적 분해 순회에서 먼저 나온 결과를 사용한다.
- **D-12:** 동일 점수 후보를 고르기 위한 역 등장 빈도 가중치, 사람이 작성한 역 조합 순위표, 복잡한 canonical comparator는 사용하지 않는다.

### 역·부·도라와 지불 결과
- **D-13:** 순수 점수 코어는 작혼 4인 랭크전의 모든 표준 역·역만을 평가한다. 멘젠/후로 차이, 쿠이사가리, 상황역, 더블 역만과 복합 역만을 포함하고, 후로·깡 플레이가 아직 없어도 합성 맥락으로 검증할 수 있어야 한다.
- **D-14:** 리치·더블리치·일발·해저/하저·영상개화·창깡·천화/지화처럼 패 모양만으로 알 수 없는 조건은 호출자가 명시적인 화료 맥락으로 전달한다. 동시에 성립할 수 없는 상황은 서로 모순되는 여러 boolean 조합보다 배타적인 상태 값으로 표현하는 방향을 우선한다. 정확한 타입 모양은 planner가 정한다.
- **D-15:** 삼깡자·사깡자와 영상개화·창깡의 **점수 판정**은 Phase 2에 포함한다. 활성 도라/우라 표시패 목록이 주어지면 깡도라·깡우라도라도 같은 resolver로 셀 수 있다. 깡 선언, 영상패 쯔모, 라이브 월 보충, 표시패 공개 시점과 UI는 이 Phase에서 구현하지 않는다.
- **D-16:** 도라·적도라·우라도라는 판을 올리지만 독립적인 역이 아니다. 도라밖에 없는 손은 화료할 수 없으며, 최소 한 개의 비도라 역을 확인한 뒤 도라를 합산한다.
- **D-17:** 지불 계산 결과는 화료자 총수입과 좌석별 지불 delta를 함께 제공한다. 론은 방총자가 전액 지불하고, 친 쯔모는 세 자가 같은 금액을, 자 쯔모는 친 한 명과 자 두 명이 서로 다른 지분을 지불하며 각 지불액은 100점 단위로 올림한다.
- **D-18:** 본장, 리치봉 공탁·회수, 노텐벌부와 대국 종료 보상은 기본 손 점수가 아니라 모드별 정산 정책이다. 솔로는 실제 상대 잔액을 만들지 않고 화료자의 이론상 쯔모 총수입만 기존 거리·부스트 입력으로 사용하며, Phase 4의 반장전은 좌석별 delta를 실제 점수에 적용한다.

### 기존 솔로 경로의 Phase 2 통합
- **D-19:** Phase 2는 기존 솔로를 완성하거나 4인전을 함께 만들지 않는다. 현재 솔로의 패산·화료 확인·점수 호출을 새 규칙 코어로 최소 교체하고, 교정된 실제 마작 점수를 기존 거리·부스트 공식에 전달한다. 기존 오답 점수와 체감을 맞추기 위한 호환 보정은 하지 않는다.
- **D-20:** 솔로 한 국은 패산 하나와 플레이어 손패 하나만 유지한다. 숨겨진 상대 손패·강·쯔모·AI·가상 론을 만들지 않으며, 상대 패를 보관하기 위한 `graveyard`도 솔로 규칙에는 필요하지 않다. 기본 쯔모 한도는 18회이고 한도가 0이 되면 패산 잔량과 무관하게 유국 처리한다.
- **D-21:** 잘못된 쯔모 선언은 감점 후 현재 국을 끝내고 다음 국으로 진행한다. Phase 2에서는 현행 `-8,000점` 동작을 보존하며, 순수 규칙 코어는 화료 불가 결과만 반환하고 감점·국 전환은 솔로 모드가 소유한다.
- **D-22:** 3분 솔로 세션은 `동1국`에서 플레이어가 친으로 시작한다. 플레이어가 친일 때 화료하거나 유국 텐파이면 같은 국과 친을 유지하고, 친 노텐 유국이면 친과 국이 넘어간다. 플레이어가 자일 때는 플레이어 화료 또는 유국 뒤에 가상 친과 국이 진행된다. 상대 좌석의 손패 상태는 만들지 않으며 결과 입력상 항상 노텐·비화료로 취급한다.
- **D-23:** 솔로에는 본장, 리치봉, 노텐벌부를 적용하지 않는다. 친의 더 높은 화료 점수와 텐파이 유지에 따른 친 연장 자체가 전략 보상이다.
- **D-24:** 솔로의 국풍은 국 번호가 진행될 때 `동 → 남 → 서 → 북 → 동`으로 순환하고 `북4국` 다음은 `동1국`이다. 반장 종료 조건은 적용하지 않으며, 고정된 3분 타이머가 유일한 세션 종료 조건이다.

### 점수 규칙 버전과 고득점
- **D-25:** 빌드/게임 버전과 별개인 하드코딩된 **점수 규칙 버전**을 둔다. 고득점 저장값에도 그 버전을 함께 기록한다.
- **D-26:** 저장된 점수 규칙 버전이 현재와 다르면 이전 버전과 기록값을 진단 로그에 남긴 뒤 고득점만 초기화한다. 음량·입력·통계 등 다른 저장 데이터는 보존한다. 규칙 버전별 기록 이력과 여러 규칙 프로필별 최고기록은 실제 변형 모드나 공개 배포가 생길 때까지 만들지 않는다.

### 회귀 테스트와 포트폴리오 증거
- **D-27:** 구형 엔진 복사본이나 런타임 이중 구현을 남기지 않는다. 먼저 현행 공개 API를 대상으로 알려진 결함을 재현하는 회귀 테스트를 추가하고, 구형 코드에서 실패하는 RED 커밋과 Unity XML/log를 보존한 뒤 엔진을 교정하여 같은 테스트를 GREEN으로 만든다. 새 API와 새 규칙은 별도의 conformance 사례로 추가한다.
- **D-28:** 최종 Phase 2 테스트는 중복된 구형/신형 전체 suite 두 개가 아니라 하나의 suite에서 `Regression`과 `Conformance` 그룹으로 구분한다. 테스트 이름이나 category filter로 두 증거를 독립 실행할 수 있어야 한다.
- **D-29:** 나중에 전후 비교가 필요하면 구형 RED 커밋 또는 별도 임시 worktree에서 회귀 그룹을 실행하고, 최신 HEAD에서는 회귀+conformance 전체를 실행한다. 정상 개발 과정에서 커밋을 반복 왕복하거나 구형 엔진을 제품 코드에 유지하지 않는다.
- **D-30:** 원본 Unity XML과 batch 로그는 기존 프로젝트 로컬 `Logs/UnityTestGate` 계열 출력에 두고 기본적으로 커밋하지 않는다. 추적되는 Phase 2 증거 ledger에는 대상 커밋, 실행 그룹·테스트명, 예상/실제 결과, 테스트 수와 원본 산출물 경로를 기록한다.
- **D-31:** 순수 규칙과 솔로 통합은 자동화 결과로 검증한다. 도라 반짝임과 결과 화면 표시는 실제 Windows Player에서 별도로 관찰하고, 직접 확인하지 않은 화면 결과를 자동화 PASS에 섞지 않는다.

### the agent's Discretion
- 새 순수 타입의 구체적인 파일 배치, 현재 거대 partial 타입에서 옮기는 순서와 public API 모양은 위 책임 경계와 Unity C# 9.0 제약 안에서 planner가 최소 변경으로 정한다.
- 화료 맥락의 enum/값 객체 구성, 결정론적 분해 순회 순서, 표 기반 fixture 파일 형식과 테스트 helper 추출 시점은 중복이 실제 생긴 뒤 가장 작은 형태로 정한다.
- 점수 규칙 버전의 문자열/정수 형식과 저장 마이그레이션 세부 구현은 `PetitGameSaveData`의 기존 필드를 보존하는 범위에서 정한다.
- 자동화 fixture의 구체적인 패 예시와 출처 조합은 작혼 목표 규칙을 충족하고 출처·확인일을 추적할 수 있는 범위에서 researcher와 planner가 정한다.

### Deferred Ideas (OUT OF SCOPE)

### Phase 3 — 솔로 리치와 도전 설정
- 솔로 리치는 유효한 텐파이 타패로 선언하고 손패를 고정하지만 1,000점 공탁, 리치봉 이월·회수는 사용하지 않는 무료 연습 리치다. 리치·일발·우라도라 점수 조건은 적용하고, 일발은 후로가 없는 솔로에서 다음 자기 쯔모까지 유지된다.
- 리치 선언 시 현재 남은 쯔모 횟수 `R`을 패산 잔량을 상한으로 `R × 4`로 한 번만 바꾼다. 계산은 솔로 규칙의 한 지점에 두어 향후 `×3` 또는 `+고정값`으로 쉽게 조정하되 지금 범용 modifier 시스템을 만들지 않는다.
- 타패 제한 시간 `없음/30초/15초/10초/5초`, 리치 가능 표시, 텐파이 타패·대기패 표시를 독립 보조 옵션으로 제공하는 아이디어를 Phase 3 discuss에서 구체화한다.
- 도전 점수는 정확한 마작 점수 뒤에 적용하며 `기본 1.0 + 선택한 난이도 보너스의 합`으로 계산한다. 설정은 세션 시작 시 고정하고 최고기록에 설정 snapshot과 솔로 점수 규칙 버전을 남긴다.
- 제한 시간별 보너스, 표시 보너스, 반올림·상한, 시간 초과 타패 동작, 현행 잘못된 쯔모 `-8,000점`에 도전 배율을 적용할지는 Phase 3 discuss에서 결정한다.

### Phase 4 — 실제 4인 남장전
- 네 좌석의 실제 손패·강·쯔모기리와 론 반응은 Phase 4에서만 만든다. Phase 2 솔로에 숨겨진 상대 상태를 미리 추가하지 않는다.
- 작혼 남장 랭크전 목표에 맞춰 서입, 음수 토비, 자동 아가리야메·텐파이야메, 종료 시 미회수 리치봉의 1위 귀속을 Phase 4에서 다룬다. 현재 `MTCH-08`~`MTCH-10`의 고정 남4국 종료·미지급 리치봉 문구는 Phase 4 계획 전에 수정해야 한다.
- 정식 텐파이 선언, 도중 유국, 복수 론, 쿠이카에와 깡도라 공개 시점 등 대국/행동 규칙은 해당 후속 Phase에서 작혼 기준으로 재확인한다. Phase 2는 호출자가 준 맥락의 점수만 계산한다.

### Phase 6 또는 MVP 이후 — 후로와 깡 플레이
- 삼깡자·사깡자·영상개화·창깡의 점수 판정은 Phase 2에 있지만, 솔로 MVP에는 어떤 깡 입력도 넣지 않는다.
- 치·퐁과 세 종류 깡의 실제 행동, 영상패·왕패·깡도라 상태 전이는 Stage 2가 안정된 뒤의 Phase 6 완전 묶음 또는 v2로 미룬다. 부분 깡 UI는 노출하지 않는다.

### 장기 아이디어
- 증강마작용 인게임 규칙 선택, 외부 JSON/mod 프로필, 아이템·퀘스트가 쯔모 기회를 늘리는 별도 1인 규칙과 규칙별 최고기록 이력은 실제 별도 모드를 계획할 때 검토한다.
</user_constraints>

<phase_requirements>
## Phase Requirements

| ID | Description | Research Support |
|----|-------------|------------------|
| RULE-01 | 두 모드가 동일한 136장·4장씩·적5 3장 패산을 사용 | 34종×4에서 일반 5 세 장을 적5로 치환하는 단일 generator와 구성 불변식 테스트 |
| RULE-02 | 동일 seed 순서와 마지막 위치를 포함한 공정 shuffle | descending Fisher–Yates와 `Random.Next(i + 1)` 경계, seed golden/동일성 테스트 |
| RULE-03 | 패와 결과의 equality/hash/comparison 계약 일치 | `MahjongTile`의 종류+적색 typed equality와 결과 값 계약 테스트 |
| RULE-04 | 일반형·치또이츠·국사무쌍의 모든 완전 분해 | 34종 count-vector 재귀 분기와 특수형 독립 검사, 모호한 손 후보 집합 테스트 |
| RULE-05 | 실제 최종 지불액 최대 해석 선택 | 후보별 payment 산출 후 총수입→판→부→열거 순서 comparator |
| RULE-06 | 최소 1역, 도라-only 거절 | 비도라 역 검증과 bonus han 합산을 두 단계로 분리 |
| RULE-07 | 멘젠/후로/상황별 역·판·도라·역만 배수 | 명시적 win context와 출처·확인일 포함 table-driven conformance catalog |
| RULE-08 | 머리·몸통·대기·론/쯔모 부와 올림 | 분해별 wait attribution, chiitoitsu/pinfu/closed-ron/open-minimum 경계 사례 |
| RULE-09 | 친·자 론/쯔모 지불자와 100점 올림 | 순수 payment 결과에 winner income와 좌석별 delta 동시 제공 |
| RULE-10 | 솔로/4인 동일 입력이 동일 기본 결과 | mode-independent evaluator 직접 비교와 솔로 adapter 단일 전달 integration test |
</phase_requirements>

## Summary

현재 코드는 고쳐 쓰기 좋은 진입점은 이미 갖고 있지만, 정확성 책임이 서로 섞여 있다. `GenerateYama()`는 34종을 3장씩 만든 뒤 일반 34종과 적5 3종을 다시 더해 139장을 만들고, 공용 `ShuffleArray`는 exclusive upper bound를 잘못 전달해 마지막 원소를 뽑지 않는다. 일반 화료 분해는 첫 머리 성공에서 중단하고 sequence-first/triplet-first 두 탐욕 경로만 시도한다. 점수 선택은 실제 수입이 아니라 판→부만 비교하며, 솔로 쯔모 화료도 론 점수를 더한다. [VERIFIED: Assets/Scripts/AL-1S/MahjongRound.cs:195-241,285-317,321-360,462-503; Assets/Scripts/AL-1S/MahjongUtilities.cs:48-87,182-287,120-128; Assets/Scripts/AL-1S/Utilities.cs:44-53]

계획은 기존 거대 partial 코드를 한 번에 다시 쓰지 말고, 회귀 RED를 먼저 남긴 뒤 정체성, 패산, 분해, 평가/지불, 솔로 어댑터 순으로 교체해야 한다. 각 단계는 순수 C#이고 새 패키지가 필요 없다. 표면 API는 런타임에 최선 결과 하나만 내고, 후보 열거는 internal 또는 테스트 경계에 둔다. [VERIFIED: .planning/phases/02-shared-rules-core/02-CONTEXT.md:22-59; AGENTS.md:15-22,165-169]

가장 큰 비기술 리스크는 작혼이 현재 4인 랭크전 전체 규칙집을 공개하지 않는다는 점이다. 공식 대회 PDF는 적5 3장과 쿠이탕을 확인하지만 랭크전 전체의 단독 근거가 될 수 없다. 따라서 일반 리치 규칙은 EMA 2025를 기본 산술 oracle로 쓰고, 작혼 차이(더블 역만, 역만 복합, 카조에, 키리아게 없음, 연풍패 머리 부 등)는 URL·확인일·권위등급·추론 여부를 가진 fixture 행으로 고정해야 한다. [CITED: https://mahjongsoul.yo-star.com/tournament/rules.pdf] [CITED: https://mahjong-europe.org/portal/images/docs/Riichi-rules-2025-EN.pdf] [ASSUMED]

**Primary recommendation:** 기존 호출 흐름을 유지한 채 `MahjongTile` 정체성과 순수 wall/decomposer/evaluator/payment 경계를 순차 도입하고, `Phase2RegressionTests`와 `Phase2ConformanceTests`의 독립 필터 RED→GREEN 증거로 각 교체를 잠근다. [VERIFIED: .planning/phases/02-shared-rules-core/02-CONTEXT.md:54-59]

## Architectural Responsibility Map

| Capability | Primary Tier | Secondary Tier | Rationale |
|------------|-------------|----------------|-----------|
| 패 값·패산·셔플 | Pure domain | Solo application adapter | 모드와 Unity 오브젝트가 없어도 결정론적으로 생성되어야 한다. [VERIFIED: AGENTS.md:15-22] |
| 완전 화료 분해 | Pure domain | — | 입력 패와 공개 멘츠만으로 후보를 열거하며 화면/씬 상태를 읽지 않는다. [VERIFIED: Assets/Scripts/AL-1S/MahjongUtilities.cs:13-98] |
| 역·도라·판·부 평가 | Pure domain | Caller-provided context | 패 모양 밖 상황은 호출자가 값으로 전달하고 evaluator가 검증한다. [VERIFIED: .planning/phases/02-shared-rules-core/02-CONTEXT.md:34-41] |
| 친·자 론/쯔모 지불 | Pure domain | Phase 4 settlement policy | Phase 2는 기본 delta만 만들며 본장·리치봉·노텐벌부를 적용하지 않는다. [VERIFIED: .planning/phases/02-shared-rules-core/02-CONTEXT.md:39-41] |
| 솔로 점수/거리 연결 | Application coordinator | Existing score service | `SoloScoringGameManager`만 총수입을 기존 `IScoreDistanceService`에 한 번 전달한다. [VERIFIED: Assets/Scripts/SoloScoringGameManager.cs:327-344] |
| 도라 반짝임 | Unity presentation | Shared dora resolver | UI는 resolver의 결과만 표시하고 패 값을 변이하지 않는다. [VERIFIED: Assets/Scripts/UI-Kozeki/MahjongTileGameObject.cs:26-46] |
| 최고기록 규칙 버전 | Persistence adapter | Rules version constant | 버전 불일치 시 `highScore`만 초기화하고 나머지 DTO 필드를 보존한다. [VERIFIED: Assets/Scripts/Configs/Settings.cs:55-70; .planning/phases/02-shared-rules-core/02-CONTEXT.md:50-52] |

## Project Constraints (from AGENTS.md)

- 9일 기한에서 Phase 2 정확성과 포트폴리오 증거를 우선하고, 치명 규칙에는 자동 테스트를 생략하지 않는다. [VERIFIED: AGENTS.md:15-16]
- Unity `2022.3.29f1`, C# 9, 기존 UI·DOTween·전역 namespace·predefined assemblies를 유지하고 엔진 업그레이드나 새 패키지를 넣지 않는다. 원문 버전은 `"m_EditorVersion: 2022.3.29f1"`, 패키지는 `"com.unity.test-framework": "1.1.33"`이다. [VERIFIED: ProjectSettings/ProjectVersion.txt:1-2; Packages/manifest.json:11]
- 클래스/계층 자체를 목표로 하지 않고 기존 책임 owner를 재사용한다. 새 범용 manager, event bus, factory, plugin/profile interpreter를 만들지 않는다. [VERIFIED: AGENTS.md:17-20,165-169]
- 새 식별자는 영어, 주석/XML summary는 한국어, C#은 4-space Allman, 변경된 public type/member summary를 갱신한다. [VERIFIED: AGENTS.md:101-121,147]
- invalid state는 early return으로 막고 Unity lifecycle method는 class 상단에 그룹화한다. [VERIFIED: AGENTS.md:105,121,132]
- 현재 Git 기준과 변경 이력을 보존하고 비교용 구형 소스 트리를 추가하지 않는다. [VERIFIED: AGENTS.md:21-22]

## Standard Stack

### Core

| Library / Runtime | Version | Purpose | Why Standard |
|-------------------|---------|---------|--------------|
| Unity Editor | `2022.3.29f1 (8d510ca76d2b)` | 컴파일, EditMode, Windows Player build | 프로젝트 고정 버전 원문: `"m_EditorVersionWithRevision: 2022.3.29f1 (8d510ca76d2b)"`. [VERIFIED: ProjectSettings/ProjectVersion.txt:1-2] |
| C# / .NET Standard | C# 9 / .NET Standard 2.1 | 순수 규칙 코어 | 현재 generated assembly 계약을 유지한다. [VERIFIED: AGENTS.md:32,40,120] |
| `System.Random` | pinned Unity runtime 내장 | seed 기반 PRNG | 새 의존성 없이 같은 runtime에서 결정론적 순서를 만든다. `Next(min,max)`의 max는 exclusive다. [CITED: https://learn.microsoft.com/en-us/dotnet/api/system.random.next] |
| `System.Collections.Generic` | BCL | 34종 count-vector, 후보/지불 값 | 현재 코드가 이미 사용하며 외부 solver가 필요 없다. [VERIFIED: Assets/Scripts/AL-1S/MahjongUtilities.cs:1-5] |

### Supporting

| Library | Version | Purpose | When to Use |
|---------|---------|---------|-------------|
| Unity Test Framework | `1.1.33` | EditMode 회귀·conformance·통합 검증 | package 원문 `"com.unity.test-framework": "1.1.33"`; category/name filtering과 NUnit XML 결과를 사용한다. [VERIFIED: Packages/manifest.json:11] [CITED: https://docs.unity3d.com/Packages/com.unity.test-framework@1.1/manual/reference-command-line.html] |
| NUnit extension | `1.0.6` | `[Test]`, parameterized/table-driven assertions | 기존 Unity Test Framework 의존 그래프를 그대로 쓴다. [VERIFIED: AGENTS.md:57] |
| Newtonsoft.Json for Unity | `3.2.1` | 기존 save DTO의 규칙 버전 필드 직렬화 | 새 persistence 계층 없이 `PetitGameSaveData` 확장에만 사용한다. 원문 `"com.unity.nuget.newtonsoft-json": "3.2.1"`. [VERIFIED: Packages/manifest.json:9; Assets/Scripts/Configs/SettingsManager.cs:13-65] |

### Alternatives Considered

| Instead of | Could Use | Tradeoff |
|------------|-----------|----------|
| 34종 count-vector 재귀 | 구형 List 탐욕 분해 유지 | 현재는 두 우선순위만 탐색하고 첫 머리 성공에서 멈춰 RULE-04를 충족할 수 없다. [VERIFIED: Assets/Scripts/AL-1S/MahjongUtilities.cs:72-85,182-287] |
| 기존 struct에 typed equality 구현 | 새 record struct로 전면 교체 | C# 9 record가 equality를 줄여주지만 serialized asset/API migration이 커진다. 현 `MahjongTile` 교정이 더 작다. [CITED: https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/expressions/equality] [VERIFIED: Assets/Scripts/MahjongTileDatabase.cs:21-36] |
| 내장 `System.Random` | 외부 RNG 패키지 | pinned runtime 내 동일 seed 계약에는 불필요하며 package legitimacy/UPM 변경만 늘어난다. [VERIFIED: .planning/phases/02-shared-rules-core/02-CONTEXT.md:22-28] |
| C# fixture table | JSON 규칙 프로필/해석기 | 외부 mod/profile 시스템은 명시적으로 금지되며 하나의 고정 규칙만 필요하다. [VERIFIED: .planning/phases/02-shared-rules-core/02-CONTEXT.md:16-20] |

**Installation:** 없음. 새 package를 설치하지 않는다. 상태 문서 원문은 `"전략 AI, 추가 종료 규칙, 전면 UI 재제작과 새 패키지는 이번 milestone에서 제외한다."`이다. [VERIFIED: .planning/STATE.md:79]

## Package Legitimacy Audit

Phase 2는 외부 package를 설치하지 않으므로 legitimacy gate 대상이 없다. 기존 Unity Test Framework와 Newtonsoft 패키지는 lockfile/manifest에 이미 고정되어 있다. [VERIFIED: Packages/manifest.json:9-11; Packages/packages-lock.json:190-253]

**Packages removed due to [SLOP] verdict:** none  
**Packages flagged as suspicious [SUS]:** none

## Architecture Patterns

### System Architecture Diagram

```text
seed ──> Shared wall factory ──> 136 ordered MahjongTile values
                                  │
concealed tiles + declared melds + winning tile + explicit win context
                                  │
                                  v
                         Exhaustive decomposer
                  ┌───────────────┼────────────────┐
                  v               v                v
              standard         chiitoitsu        kokushi
                  └───────────────┼────────────────┘
                                  v
                   yaku/fu/dora/yakuman evaluator
                                  │
                         at least one non-dora yaku?
                         ┌────────┴────────┐
                        no                yes
                        v                  v
                    not-winnable     base-score/payment
                                           │
                          all candidates ──> max winner income
                                           │
                   ┌───────────────────────┴──────────────────────┐
                   v                                              v
        SoloScoringGameManager adapter                 Phase 4 future consumer
        total income once -> distance                  seat deltas (not built now)
                   │
                   v
        existing UI + score versioned save
```

이 흐름은 data entry, 변환 단계, invalid 분기, 모드 경계를 분리한다. Phase 2에서 4인 table state는 만들지 않는다. [VERIFIED: .planning/phases/02-shared-rules-core/02-CONTEXT.md:42-48,135-151]

### Recommended Project Structure

```text
Assets/
├── Scripts/AL-1S/
│   ├── MahjongTileAndBlock.cs     # 기존 tile/block 값 계약 교정
│   ├── MahjongWall.cs             # 136장 생성과 shuffle만 소유
│   ├── MahjongUtilities.cs        # 기존 public facade를 새 분해기로 연결
│   ├── MahjongHandDecomposer.cs   # 34종 count-vector 완전 열거
│   ├── MahjongYaku.cs             # 고정 작혼 yaku catalog/evaluation
│   ├── MahjongWinInfo.cs          # context/result/payment 값
│   └── MahjongRound.cs            # 솔로 진행, 새 코어 호출 adapter
└── Editor/Tests/
    ├── Phase2RegressionTests.cs    # 현행 결함 RED → 교정 GREEN
    └── Phase2ConformanceTests.cs   # 표 기반 작혼/일반 규칙 사례
.planning/phases/02-shared-rules-core/
└── 02-EVIDENCE.md                 # 커밋, 그룹, case, count, XML/log path
```

파일명은 planner가 실제 diff 크기에 따라 더 합칠 수 있다. 중요한 것은 새 manager/interface가 아니라 계산 책임 경계다. [VERIFIED: AGENTS.md:17,165-169]

### Pattern 1: 종류 count-vector를 사용한 완전 열거

**What:** 일반형은 가능한 머리를 하나씩 빼고, 남은 가장 작은 종류에서 triplet과 sequence가 가능할 때 양쪽으로 재귀 분기한다. 첫 종류를 반드시 소비하므로 중복 순열 없이 모든 구조를 찾는다. 치또이츠와 국사무쌍은 독립 predicate로 함께 평가하며 `else if`로 배타화하지 않는다. [VERIFIED: 현재 반대 패턴은 Assets/Scripts/AL-1S/MahjongUtilities.cs:58-85,182-287]

**When to use:** 14장 은폐형과 Phase 6용 합성 공개 meld를 평가할 때 사용한다. 공개 meld 수만큼 필요한 은폐 meld 수를 줄이되 Phase 2에서는 meld 실행 상태를 만들지 않는다. [VERIFIED: .planning/phases/02-shared-rules-core/02-CONTEXT.md:34-38,149-151]

**Key invariant:** solver의 종류 비교는 적색을 무시하지만 결과의 tile equality는 적색을 포함한다. 따라서 solver 내부에서 `==`를 쓰지 말고 종류 key/count를 명시적으로 사용한다. [VERIFIED: .planning/phases/02-shared-rules-core/02-CONTEXT.md:24]

### Pattern 2: 평가 맥락과 모드 정책 분리

**What:** win context는 패 모양 밖의 조건을 값으로 전달한다. 화료 방식·리치 상태·마지막 패/깡 관련 상황처럼 동시에 성립할 수 없는 조건은 배타 값으로 묶고, 일발처럼 종속 조건은 생성 시 validation한다. [VERIFIED: .planning/phases/02-shared-rules-core/02-CONTEXT.md:35-38]

**When to use:** 리치/더블리치, 일발, 해저/하저, 영상개화, 창깡, 천화/지화, 좌석풍/장풍, 활성 도라/우라 표시패를 전달할 때 사용한다. 이 맥락은 solo와 future four-seat consumer 모두 동일하다. [VERIFIED: .planning/phases/02-shared-rules-core/02-CONTEXT.md:35-41]

### Pattern 3: non-dora yaku 먼저, bonus han 나중

**What:** 구조·상황역과 역만을 먼저 평가한다. 비도라 역이 0이면 not-winnable을 반환한다. 유효할 때만 표시패 resolver와 적5/우라를 합산한다. 역만이 있으면 일반 yaku·dora·fu 경로와 합산하지 않고 multiplier로 지불한다. [CITED: https://mahjong-europe.org/portal/images/docs/Riichi-rules-2025-EN.pdf] [VERIFIED: .planning/phases/02-shared-rules-core/02-CONTEXT.md:36-41]

### Pattern 4: 후보별 payment 후 선택

**What:** 후보마다 yaku/han/fu/yakuman multiplier와 친/자·론/쯔모 payment를 완성한 뒤 winner total income으로 비교한다. 동률만 han, fu, 안정적인 열거 순서를 사용한다. [VERIFIED: .planning/phases/02-shared-rules-core/02-CONTEXT.md:29-32]

**Why:** 동일 han/fu라도 친/자·쯔모 지분과 역만 multiplier가 실제 수입을 결정한다. 현재 `GetHighestWinInfo()`의 `Max()`는 `MahjongWinInfo.CompareTo()`가 han→fu만 비교한다. [VERIFIED: Assets/Scripts/AL-1S/MahjongUtilities.cs:120-128; Assets/Scripts/AL-1S/MahjongWinInfo.cs:441-495]

### Pattern 5: source-dated conformance table

각 fixture 행은 최소 `CaseId`, compact hand/melds, winning tile, context, expected decomposition/yaku/han/fu/payment, `SourceUrl`, `CheckedOn`, `Authority`(official/secondary/observed), `InferenceNote`를 가진다. 같은 expected 계산을 production 코드로 생성하지 말고 literal oracle로 적는다. [VERIFIED: .planning/phases/02-shared-rules-core/02-CONTEXT.md:17-18,59-64]

작혼 차이 fixture는 다음 묶음을 반드시 가진다. 공식 대회 자료로 적5 3장·쿠이탕을 고정하고, 나머지는 현재 클라이언트 관찰 또는 상호 교차된 2차 자료로 동결한다. [CITED: https://mahjongsoul.yo-star.com/tournament/rules.pdf] [ASSUMED]

| Fixture family | Required cases | Evidence status at research time |
|----------------|----------------|----------------------------------|
| Wall / identity | 34종×4, 적5 3장 치환, normal5/red5 inequality, kind-only solver grouping | Locked user decision + codebase regression. [VERIFIED: 02-CONTEXT.md:22-28] |
| Basic yaku | 모든 1/2/3/6 han 표준역, 멘젠/후로와 쿠이사가리 pair | Generic list/values are authoritative EMA baseline; 작혼 쿠이탕은 official tournament corroboration. [CITED: https://mahjong-europe.org/portal/images/docs/Riichi-rules-2025-EN.pdf] [CITED: https://mahjongsoul.yo-star.com/tournament/rules.pdf] |
| Dora | suit 9→1, wind East→South→West→North→East, dragon White→Green→Red→White, duplicate indicators, aka, ura gated by riichi, dora-only reject | Indicator cycles and ura condition from EMA; dora-only reject is locked. [CITED: https://mahjong-europe.org/portal/images/docs/Riichi-rules-2025-EN.pdf] [VERIFIED: 02-CONTEXT.md:25-28,36-38] |
| Fu | chiitoitsu 25, pinfu tsumo 20, closed ron +10, normal tsumo +2, wait/head/set fu, open no-fu ron 30, 10-unit rounding, double-wind pair | Generic arithmetic from EMA; Mahjong Soul double-wind 4-fu requires client/secondary fixture. [CITED: https://mahjong-europe.org/portal/images/docs/Riichi-rules-2025-EN.pdf] [ASSUMED] |
| Limits | mangan thresholds, no kiriage 3han60fu/4han30fu, haneman/baiman/sanbaiman, 13+ kazoe yakuman cap | Generic tiers plus Mahjong Soul-specific no-kiriage/kazoe from secondary source; needs dated inference fixture. [ASSUMED] |
| Yakuman | every standard yakuman; multiple stacking; double variants for Daisuushi, Suuankou tanki, Kokushi 13-wait, Junsei Chuuren | Secondary sources agree on four double variants, but the provided note translation says `Daisangen` once and conflicts; do not copy that typo into code. Freeze observed/cross-checked rows. [ASSUMED] |
| Payments | dealer/non-dealer ron, dealer tsumo equal shares, non-dealer tsumo split, each share round-up, total income and zero-sum deltas | Generic formula from EMA and locked D-17. [CITED: https://mahjong-europe.org/portal/images/docs/Riichi-rules-2025-EN.pdf] [VERIFIED: 02-CONTEXT.md:39-41] |

Planner가 "모든 표준역"을 추상 문구로 남기지 않도록 conformance catalog의 최소 열 목록을 다음처럼 고정한다. 일반역은 `Riichi`, `DoubleRiichi`, `Ippatsu`, `MenzenTsumo`, `Tanyao`, `Pinfu`, `Iipeikou`, dragon/seat/round `Yakuhai`, `Haitei`, `Houtei`, `RinshanKaihou`, `Chankan`, `Chanta`, `SanshokuDoujun`, `Ikkitsuukan`, `Toitoi`, `Sanankou`, `SanshokuDoukou`, `Sankantsu`, `Chiitoitsu`, `Honroutou`, `Shousangen`, `Honitsu`, `Junchan`, `Ryanpeikou`, `Chinitsu`를 포함한다. 역만은 `KokushiMusou`, `Suuankou`, `Daisangen`, `Shousuushii`, `Daisuushii`, `Tsuuiisou`, `Chinroutou`, `Ryuuiisou`, `ChuurenPoutou`, `Suukantsu`, `Tenhou`, `Chiihou`를 포함한다. 각 행은 closed/open 적법성, 쿠이사가리, 중복 성립과 상호 배제를 함께 검증하며, 작혼 고유 double multiplier는 A1이 해소될 때까지 별도 inference 행으로 둔다. [CITED: https://mahjong-europe.org/portal/images/docs/Riichi-rules-2025-EN.pdf] [ASSUMED]

### Anti-Patterns to Avoid

- **두 탐욕 pass:** sequence-first/triplet-first는 혼합 분기가 셋 이상인 손을 누락한다. 첫 남은 종류에서 가능한 branch를 모두 재귀한다. [VERIFIED: Assets/Scripts/AL-1S/MahjongUtilities.cs:182-287]
- **`MahjongTile ==`를 solver 종류 비교에 재사용:** D-06 이후 적5와 일반5는 다른 값이므로 meld/decomposition에서는 kind key를 사용한다. [VERIFIED: Assets/Scripts/AL-1S/MahjongTileAndBlock.cs:315-334; 02-CONTEXT.md:24]
- **패 값에 도라 상태 축적:** 현재 `UpdateDora`가 wall/hand tile을 변이해 같은 indicator가 중복 적용될 수 있다. 표시패 resolver를 query로 사용한다. [VERIFIED: Assets/Scripts/AL-1S/MahjongRound.cs:285-317; Assets/Scripts/AL-1S/MahjongTileAndBlock.cs:270-278]
- **모순 boolean bag:** `isHaitei`, `isHoutei`, `isRinshan`, `isChanKan`를 독립 true로 허용하지 않는다. [VERIFIED: Assets/Scripts/AL-1S/MahjongWinInfo.cs:188-206,273-288]
- **역 enum이 있으면 구현됐다고 간주:** 현재 원문 enum은 `"Tsuuiisou, Ryuuiisou, Chinroutou"`를 포함하지만 evaluator 호출 목록에는 Ryuuiisou가 없다. catalog coverage test가 enum/metadata/evaluator/fixture를 교차 검증해야 한다. [VERIFIED: Assets/Scripts/AL-1S/MahjongYaku.cs:27-35,192-204]
- **fixture expected를 production scorer로 계산:** 같은 버그를 공유해 false GREEN이 된다. expected 값은 출처에서 수기로 동결한다. [ASSUMED]
- **새 규칙 profile framework:** 하나의 고정 profile만 코드 상수/readonly data로 유지한다. 외부 JSON, plugin, settings UI는 금지다. [VERIFIED: 02-CONTEXT.md:16-20]

## Don't Hand-Roll

| Problem | Don't Build | Use Instead | Why |
|---------|-------------|-------------|-----|
| PRNG | custom RNG/seed serialization | pinned Unity의 `System.Random` | 범위 API와 seed 동작이 이미 있고 요구는 같은 runtime 결정론이다. [CITED: https://learn.microsoft.com/en-us/dotnet/api/system.random.next] |
| Shuffle | 임의 swap loop | 표준 descending Fisher–Yates | `Next(i + 1)`로 0..i 모든 위치를 포함한다. [CITED: https://learn.microsoft.com/en-us/dotnet/api/system.random.next] |
| Equality | operator, Equals, hash를 따로 정의 | `IEquatable<MahjongTile>` 한 typed equality에 모두 위임 | hash collection과 operator가 같은 정체성 계약을 가져야 한다. [CITED: https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/expressions/equality] |
| Rule profile parser | JSON/mod/plugin interpreter | compile-time fixed data table | 이번 milestone은 단일 profile이며 확장 시스템은 out of scope다. [VERIFIED: 02-CONTEXT.md:16-20] |
| Test runner command | ad-hoc `Unity.exe -quit` invocation | 기존 `Invoke-UnityTests.ps1` | XML 존재/선택 수/실패를 fail-closed 분류하고 license IPC 복구를 제한한다. [VERIFIED: C:/Users/user/.codex/skills/unity-test-gate/SKILL.md] |
| Legacy comparison | product 내 구형 scorer 복사본 | baseline/RED commit + temporary worktree | 제품 이중 구현 없이 재현 가능한 전후 증거를 남긴다. [VERIFIED: 02-CONTEXT.md:54-59] |

**Key insight:** 이 Phase의 복잡성은 library 부재가 아니라 규칙 경우의 수와 oracle 신뢰도다. 외부 패키지보다 작고 순수한 완전 열거 + 독립 literal fixture가 더 검증 가능하다. [VERIFIED: 기존 알고리즘 범위 Assets/Scripts/AL-1S/MahjongUtilities.cs:48-360] [ASSUMED]

## Runtime State Inventory

| Category | Items Found | Action Required |
|----------|-------------|------------------|
| Stored data | `PetitGameSaveData` 원문 필드: `"public float highScore; public SoundSettings sound; public InputSettings input; public StatisticsData statistics;"`; 실제 파일명 원문은 `"yaml.json"`. [VERIFIED: Assets/Scripts/Configs/Settings.cs:55-70; Assets/Scripts/Configs/SettingsManager.cs:8-12] | 현재 scoring-rule version 필드를 추가하고 load 시 version mismatch면 이전 version/value를 log한 뒤 `highScore`만 초기화한다. 기존 JSON의 missing field default도 테스트한다. 이는 code+data migration이다. |
| Live service config | 규칙 코어가 참조하는 외부 서비스/UI database config는 발견되지 않았다. 조사: 관련 runtime 파일, `.env` 부재와 project stack. [VERIFIED: AGENTS.md:68-83] | None. |
| OS-registered state | 규칙 이름/점수 버전을 포함한 Task Scheduler, service, registry 등록은 프로젝트 계약에서 발견되지 않았다. Unity Hub/Editor 설치는 도구 상태이지 마이그레이션 대상 규칙 상태가 아니다. [VERIFIED: ProjectSettings/ProjectVersion.txt:1-2] | None. |
| Secrets/env vars | 규칙/고득점과 연결된 secret 또는 env var 이름은 발견되지 않았다. save는 `Application.persistentDataPath`를 직접 사용한다. [VERIFIED: Assets/Scripts/Configs/SettingsManager.cs:8-12] | None. |
| Build artifacts | Unity `Library`/generated project와 Windows build는 source refactor 뒤 stale할 수 있으나 source of truth가 아니다. Phase 1은 final BuildReport/Player gate를 사용한다. [VERIFIED: AGENTS.md:92-96; .planning/phases/01-executable-baseline/01-VALIDATION.md] | Unity recompile, filtered EditMode XML, fresh Windows build. artifact를 마이그레이션하거나 커밋하지 않는다. |

## Common Pitfalls

### Pitfall 1: 적5 equality를 고친 뒤 solver가 5의 meld를 잃음

**What goes wrong:** normal5와 red5가 값으로 달라지면서 기존 `==` 기반 pair/triplet/sequence가 적5를 별개 종류로 취급한다. [VERIFIED: Assets/Scripts/AL-1S/MahjongUtilities.cs:143-178]

**How to avoid:** tile value equality와 solver kind equality를 이름부터 분리하고, 34종 count key를 solver에만 사용한다. identity tests와 red-containing decomposition tests를 같은 wave에 둔다. [VERIFIED: 02-CONTEXT.md:24]

### Pitfall 2: 139장 버그를 136장으로 줄였지만 적5 치환이 아닌 추가/누락

**What goes wrong:** 현재 `GetAllTiles(true)`는 34 normal + 3 red를 반환한다. 3×34에 이를 더하면 139다. [VERIFIED: Assets/Scripts/AL-1S/MahjongTileAndBlock.cs:502-521; Assets/Scripts/AL-1S/MahjongRound.cs:207-212]

**How to avoid:** 34 kind를 각각 네 장 만든 뒤 각 suit의 normal5 한 장을 red5로 치환하고, 전체 136·kind별4·red3·normal5별3을 한 테스트에서 검증한다. [VERIFIED: 02-CONTEXT.md:23]

### Pitfall 3: shuffle의 마지막 인덱스가 영구 고정

**What goes wrong:** 현재 `prng.Next(i, array.Length - 1)`의 상한은 exclusive여서 마지막 index가 선택되지 않는다. [VERIFIED: Assets/Scripts/AL-1S/Utilities.cs:44-53] [CITED: https://learn.microsoft.com/en-us/dotnet/api/system.random.next]

**How to avoid:** descending Fisher–Yates에서 `Next(i + 1)`를 사용하고, permutation/seed-repeat/last-position regression을 분리한다. `System.Random`의 특정 sequence를 Unity 버전 간 파일 포맷 계약으로 만들지는 않는다. [ASSUMED]

### Pitfall 4: 완전 분해는 했지만 화료패 귀속을 하나만 둠

**What goes wrong:** 같은 kind의 화료패가 pair, triplet, sequence 중 어디를 완성했는지에 따라 wait fu, pinfu, sanankou/closed-triplet 판정이 달라진다. 현재 `availableWaitingBlocks`는 tile 포함 여부만 수집한다. [VERIFIED: Assets/Scripts/AL-1S/MahjongWinInfo.cs:25-44]

**How to avoid:** 구조 후보마다 winning-tile attribution 후보도 결정론적으로 열거하고 각각 점수화한다. 특히 shanpon ron으로 완성된 triplet은 해당 화료에서 concealed triplet 취급을 받지 않는 fixture가 필요하다. [CITED: https://mahjong-europe.org/portal/images/docs/Riichi-rules-2025-EN.pdf]

### Pitfall 5: open-hand han 감소 metadata가 계산에 반영되지 않음

**What goes wrong:** 현재 `YakuInfo.Condition`에는 `"MenzenOnly, DecreaseHanWhenFuro, FuroOK"`가 있지만 `GetHan()`은 metadata 조건 없이 고정 Han을 합한다. [VERIFIED: Assets/Scripts/AL-1S/MahjongYaku.cs:41-46; Assets/Scripts/AL-1S/MahjongUtilities.cs:447-453]

**How to avoid:** evaluator가 meld openness를 한 번 계산하고 yaku eligibility 및 kuisagari를 같은 metadata 행에서 적용한다. 모든 감소역에 closed/open pair fixture를 둔다. [CITED: https://mahjong-europe.org/portal/images/docs/Riichi-rules-2025-EN.pdf]

### Pitfall 6: dora를 yaku로 세어 dora-only 화료 허용

**What goes wrong:** 현재 dora가 `SortedSet<Yaku>`에 추가되고 `GetHan()`이 무조건 bonus를 더한다. no-yaku gate가 없다. [VERIFIED: Assets/Scripts/AL-1S/MahjongYaku.cs:115-121,206-212,840-879; Assets/Scripts/AL-1S/MahjongWinInfo.cs:393-407]

**How to avoid:** non-bonus yaku list가 비었으면 즉시 invalid; 그 다음 dora counts를 result의 별도 breakdown에 넣는다. [VERIFIED: 02-CONTEXT.md:36-38]

### Pitfall 7: 쯔모에서 론 금액을 솔로 보상으로 사용

**What goes wrong:** `HandlePlayerWin()`은 친이면 `oyaRon`, 자이면 `zaRon`을 더한다. [VERIFIED: Assets/Scripts/AL-1S/MahjongRound.cs:491-503]

**How to avoid:** payment result의 `WinnerIncome`을 단일 source로 사용하고, 솔로 manager가 positive delta를 기존 distance service에 정확히 한 번 전달하는 integration test를 둔다. [VERIFIED: Assets/Scripts/SoloScoringGameManager.cs:327-344; 02-CONTEXT.md:39-46]

### Pitfall 8: 공식 대회 설정을 현재 랭크전 전체 규칙으로 오인

**What goes wrong:** event manual은 적5/쿠이탕 등 일부 설정만 보여주며 시점과 대회 예외가 있다. [CITED: https://mahjongsoul.yo-star.com/tournament/rules.pdf]

**How to avoid:** fixture authority를 official-ranked-observed / official-event / generic-authoritative / secondary로 구분한다. 충돌 시 높은 tier 또는 최신 직접 관찰을 우선하고 inference note를 남긴다. 특히 2021 2차 글의 `Daisangen double` 문구는 다수 자료의 `Daisuushi double`과 충돌하므로 그대로 채택하지 않는다. [ASSUMED]

### Pitfall 9: save migration이 음량/입력/통계를 초기화

**What goes wrong:** 버전 mismatch에서 DTO 전체를 새로 만들면 unrelated user settings를 잃는다. [VERIFIED: Assets/Scripts/Configs/Settings.cs:55-70]

**How to avoid:** load 후 version/highScore만 선택적으로 수정하고 기존 Phase 1 durable save fixture 안에서 legacy JSON·same-version·mismatch 세 경로를 검증한다. [VERIFIED: 02-CONTEXT.md:50-52; .planning/phases/01-executable-baseline/01-07-SUMMARY.md]

## Code Examples

Verified patterns from official sources and current code constraints:

### Correct inclusive-position Fisher–Yates

```csharp
// Source: Microsoft Random.Next docs (upper bound is exclusive)
static void Shuffle<T>(T[] values, System.Random random)
{
    for (int i = values.Length - 1; i > 0; i--)
    {
        int swapIndex = random.Next(i + 1);
        (values[i], values[swapIndex]) = (values[swapIndex], values[i]);
    }
}
```

상한에 `i + 1`을 넘겨 현재 위치 `i`도 선택 가능하게 한다. [CITED: https://learn.microsoft.com/en-us/dotnet/api/system.random.next]

### One equality definition, all entry points delegate

기존 source-of-truth 원문 필드는 `"public int TileID"`와 `"public bool isAkaDora;"`다. [VERIFIED: Assets/Scripts/AL-1S/MahjongTileAndBlock.cs:241-247,270]

```csharp
// Source: Microsoft C# equality guidance
public bool Equals(MahjongTile other)
{
    return TileID == other.TileID && isAkaDora == other.isAkaDora;
}

public override bool Equals(object obj)
{
    return obj is MahjongTile other && Equals(other);
}

public override int GetHashCode()
{
    return Utilities.HashCombine(TileID, isAkaDora);
}

public static bool operator ==(MahjongTile left, MahjongTile right)
{
    return left.Equals(right);
}

public static bool operator !=(MahjongTile left, MahjongTile right)
{
    return !left.Equals(right);
}
```

두 값이 `Equals`로 같으면 같은 hash를 내고 operator도 같은 semantics를 써야 한다. [CITED: https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/expressions/equality]

### Payment rounding primitive

현재 source-of-truth 원문은 `"return (score + 99) / 100 * 100;"`이며 이 primitive 자체는 재사용 가능하다. [VERIFIED: Assets/Scripts/AL-1S/MahjongWinInfo.cs:347-360]

```csharp
static int RoundUpToHundred(int points)
{
    return (points + 99) / 100 * 100;
}
```

각 payer share에 따로 적용한 뒤 winner income을 합산한다. [VERIFIED: 02-CONTEXT.md:39]

## State of the Art

| Old Approach | Current Approach | When Changed | Impact |
|--------------|------------------|--------------|--------|
| 패마다 mutable `isDora`/`doraCount` 누적 | indicator→dora resolver를 평가/표시에 query | Phase 2 locked decision | 중복 변이와 scoring/UI 불일치를 제거한다. [VERIFIED: 02-CONTEXT.md:25-28] |
| 두 greedy 분해 | count-vector exhaustive recursion + winning attribution | Phase 2 requirement | 요구사항 원문은 `"규칙 엔진은 일반형, 치또이츠, 국사무쌍을 포함해 가능한 모든 완전한 화료 분해를 찾는다."`, `"여러 합법 분해가 있으면 실제 최종 지불액이 가장 높은 해석을 선택한다."`이다. [VERIFIED: .planning/REQUIREMENTS.md:21-22] |
| han→fu 비교 | actual winner income→han→fu→stable order | Phase 2 locked decision | 친/자·론/쯔모와 limit hand를 포함한 실제 최선 해석을 고른다. [VERIFIED: 02-CONTEXT.md:29-32] |
| 하나의 boolean-heavy `MahjongHandInfo` | explicit validated win context + immutable result | Phase 2 discretion/decision | 모순 상황과 mode state 누출을 줄인다. [VERIFIED: Assets/Scripts/AL-1S/MahjongWinInfo.cs:125-206; 02-CONTEXT.md:35] |
| Unity exit code/파일 존재 기반 판단 | positive selected count + parseable NUnit XML + zero non-passed | Phase 1 | test infra failure를 RED/GREEN으로 오분류하지 않는다. [VERIFIED: .planning/phases/01-executable-baseline/01-VALIDATION.md] |

**Deprecated/outdated:**

- `Utilities.ShuffleArray`의 현재 마지막-index 제외 구현은 교체 대상이다. [VERIFIED: Assets/Scripts/AL-1S/Utilities.cs:44-53]
- `MahjongTile.isDora`, `doraCount`, `AddDora()`는 점수 source로 사용하지 않는다. UI도 resolver result로 전환한 뒤 제거한다. [VERIFIED: Assets/Scripts/AL-1S/MahjongTileAndBlock.cs:270-278]
- `GetHighestWinInfo(HashSet<...>)`의 han/fu `Max()` 선택은 payment-aware selector로 대체한다. [VERIFIED: Assets/Scripts/AL-1S/MahjongUtilities.cs:120-128; Assets/Scripts/AL-1S/MahjongWinInfo.cs:455-495]
- `Yaku.NukiDora`와 작혼 4인 랭크전에 없는 local yaku는 profile에서 제외한다. 현재 enum 원문에는 `"Dora, AkaDora, UraDora, NukiDora"`가 있다. [VERIFIED: Assets/Scripts/AL-1S/MahjongYaku.cs:34-37; 02-CONTEXT.md:19]

## Assumptions Log

| # | Claim | Section | Risk if Wrong |
|---|-------|---------|---------------|
| A1 | 현재 작혼 4인 랭크전은 Daisuushi·Suuankou tanki·Kokushi 13-wait·Junsei Chuuren을 double yakuman으로 계산한다. 여러 2차 자료는 일치하지만 공식 랭크전 문서가 없다. | Fixture family / Pitfalls | yakuman multiplier 오답; client observation으로 lock 필요. |
| A2 | 현재 작혼 4인 랭크전은 13+ han을 single kazoe yakuman으로 cap하고 3han60fu/4han30fu kiriage mangan을 쓰지 않는다. | Fixture family | limit payment 오답; client result fixture 필요. |
| A3 | seat wind와 round wind가 같은 pair는 4 fu다. 제공된 2차 자료는 이를 말하지만 generic ruleset variation이 있다. | Fixture family | 일부 fu/payment 경계 오답. |
| A4 | ranked profile에 Renhou/local yakuman은 없다. | Yaku catalog | 표준역 완전성 또는 제외 목록 오답. |
| A5 | pinned Unity/Mono runtime 안에서만 seeded `System.Random` sequence를 compatibility contract로 삼으면 충분하다. | Shuffle | runtime upgrade 뒤 golden order 변경 가능. |
| A6 | `Phase2RegressionTests.cs`와 `Phase2ConformanceTests.cs` 두 fixture 파일이 현재 규모에서 가장 작은 유지보수 단위다. | Project Structure | planner가 중복/파일 크기에 따라 합치거나 helper를 늦게 추출할 수 있음. |

## Open Questions

1. **작혼 현재 랭크전의 변형 규칙을 어떤 직접 관찰로 lock할 것인가?**
   - What we know: official event 자료는 적5 3장과 open tanyao를 확인한다. generic 산술은 EMA로 검증 가능하다. [CITED: https://mahjongsoul.yo-star.com/tournament/rules.pdf] [CITED: https://mahjong-europe.org/portal/images/docs/Riichi-rules-2025-EN.pdf]
   - What's unclear: double yakuman 4종, kazoe cap, kiriage, renhou 제외, double-wind pair fu는 공개 official-ranked 문서가 없다. [ASSUMED]
   - Recommendation: 구현 GREEN 전에 Mahjong Soul result 화면/공식 도움말을 사람이 확인해 각 fixture에 `CheckedOn=2026-09-04` 이후 관찰 근거를 추가한다. 관찰이 불가능하면 2차 출처 두 개 이상과 inference note를 유지하고 포트폴리오에 한계를 명시한다. [ASSUMED]

2. **점수 규칙 버전 형식은 무엇인가?**
   - What we know: build version과 별도 hardcoded version 및 mismatch highScore-only reset은 locked다. [VERIFIED: 02-CONTEXT.md:50-52]
   - What's unclear: string/int 형식은 planner discretion이다.
   - Recommendation: 비교만 필요한 현재 scope에서는 단일 정수 상수와 DTO 정수 필드가 가장 작다. 첫 교정 version을 임의 의미 버전 체계로 확장하지 않는다. [ASSUMED]

3. **기존 public API를 얼마나 유지할 것인가?**
   - What we know: RED는 현행 API에 먼저 작성하고 제품 내 이중 엔진은 금지다. [VERIFIED: 02-CONTEXT.md:54-59]
   - What's unclear: `MahjongWinInfo`/`HashSet` 반환을 즉시 바꿀지 facade로 잠시 연결할지 planner가 정한다.
   - Recommendation: 각 caller를 같은 wave에서 바꿀 수 있으면 직접 교체한다. compile continuity가 필요할 때만 `[Obsolete]` facade를 한 wave 유지하고 Phase 종료 전에 제거한다. [ASSUMED]

## Environment Availability

| Dependency | Required By | Available | Version | Fallback |
|------------|-------------|-----------|---------|----------|
| Unity Editor | compile/EditMode/build | ✓ | `2022.3.29f1` installed under Unity Hub and pinned | none; wrong editor is not accepted. [VERIFIED: ProjectSettings/ProjectVersion.txt:1-2] |
| Unity Test Gate helper | regression/conformance XML | ✓ | project-global skill script present | do not construct ad-hoc command. [VERIFIED: C:/Users/user/.codex/skills/unity-test-gate/SKILL.md] |
| PowerShell | helper execution | ✓ | Windows PowerShell 5.1 host (`10.0.19041.3996`) | none needed. [VERIFIED: environment probe 2026-09-04] |
| Git | RED/GREEN commits/evidence refs | ✓ | `2.53.0.windows.1` | none needed. [VERIFIED: environment probe 2026-09-04] |
| Node / gsd-tools | research/commit seam | ✓ | Node `24.14.1` | planning can proceed without runtime dependency in game. [VERIFIED: environment probe 2026-09-04] |
| External package/service | rules implementation | not required | — | BCL + installed packages. [VERIFIED: Standard Stack] |

**Missing dependencies with no fallback:** none.  
**Missing dependencies with fallback:** none.

## Validation Architecture

### Test Framework

| Property | Value |
|----------|-------|
| Framework | Unity Test Framework `1.1.33` + NUnit extension `1.0.6` [VERIFIED: Packages/manifest.json:11; AGENTS.md:57] |
| Config file | none — existing `Assets/Editor/Tests/*.cs` compiles in predefined `Assembly-CSharp-Editor`; do not add asmdef/package unless compilation proves necessary. [VERIFIED: .planning/phases/01-executable-baseline/01-VALIDATION.md] |
| Quick regression command | `& 'C:\Users\user\.codex\skills\unity-test-gate\scripts\Invoke-UnityTests.ps1' -ProjectPath $PWD -TestPlatform EditMode -TestFilter Phase2RegressionTests -ExpectedGate Green` [VERIFIED: unity-test-gate SKILL.md] |
| Quick conformance command | same helper with `-TestFilter Phase2ConformanceTests -ExpectedGate Green` [VERIFIED: unity-test-gate SKILL.md] |
| Existing baseline commands | same helper separately for `MahjongRoundTraceTests` and `SoloSessionLifecycleTests`; preserve exact existing 4+15 contract. [VERIFIED: .planning/phases/01-executable-baseline/01-VALIDATION.md] |
| Full suite command | four filtered project fixtures, each helper status GREEN; aggregate exact discovered names from XML, then `Phase1Build.BuildWindowsPlayer`. Do not use unfiltered total because a testable package contributes its own tests. [VERIFIED: .planning/phases/01-executable-baseline/01-VALIDATION.md] |

Unity official docs confirm `testFilter`, `testCategory`, EditMode and NUnit-format `testResults`; exit codes alone are not a common correctness signal. The installed helper exposes `TestFilter` but not `TestCategory`, so D-28 grouping should use fixture/test-name prefixes without modifying the global helper. [CITED: https://docs.unity3d.com/Packages/com.unity.test-framework@1.1/manual/reference-command-line.html] [VERIFIED: C:/Users/user/.codex/skills/unity-test-gate/scripts/Invoke-UnityTests.ps1:1-12]

### Phase Requirements → Test Map

| Req ID | Behavior | Test Type | Automated Command | File Exists? |
|--------|----------|-----------|-------------------|-------------|
| RULE-01 | 136/34×4/red3 and shared generator | unit regression+conformance | regression/conformance filters | ❌ Wave 0 |
| RULE-02 | same seed/permutation/last index reachable | unit | regression/conformance filters | ❌ Wave 0 |
| RULE-03 | normal/red equality, hash, compare laws; result equality | unit/property table | regression/conformance filters | ❌ Wave 0 |
| RULE-04 | standard ambiguity + chiitoitsu + kokushi exhaustive set | unit | conformance filter | ❌ Wave 0 |
| RULE-05 | max winner income, han/fu tie breakers, stable tie | unit | conformance filter | ❌ Wave 0 |
| RULE-06 | yaku required, dora-only rejected | unit regression | regression/conformance filters | ❌ Wave 0 |
| RULE-07 | closed/open/situational/dora/yakuman catalog coverage | parameterized conformance | conformance filter | ❌ Wave 0 |
| RULE-08 | all fu components and rounding edge cases | parameterized conformance | conformance filter | ❌ Wave 0 |
| RULE-09 | four dealer/method payment shapes, per-share rounding/deltas | parameterized unit | conformance filter | ❌ Wave 0 |
| RULE-10 | identical core result across mode adapters + solo single delivery | integration EditMode | conformance + lifecycle filters | ❌ Wave 0 |

### Sampling Rate

- **Per task commit:** 해당 fixture의 정확한 test-name subset을 helper로 GREEN 확인한다. RED commit은 non-empty filter와 `-ExpectedGate Red`를 사용하고 assertion failure만 RED로 인정한다. [VERIFIED: unity-test-gate SKILL.md]
- **Per wave merge:** `Phase2RegressionTests`와 `Phase2ConformanceTests`를 각각 실행하고 exact unique case names/zero non-passed를 XML에서 확인한다. [VERIFIED: 02-CONTEXT.md:56-59]
- **Phase gate:** Phase 2 두 fixture + 기존 trace 4 + lifecycle 15를 독립 실행하고 fresh Windows BuildReport/exe를 확인한다. GUI 도라 표시/결과 화면은 별도 same-PID Player 관찰로만 PASS 처리한다. [VERIFIED: 02-CONTEXT.md:58-59; .planning/phases/01-executable-baseline/01-VALIDATION.md]

### Required RED Sequence

1. 현행 API로 139장, last-index, identity mismatch, greedy missed decomposition, dora-only, tsumo-as-ron 결함을 assertion RED로 만든다. compile error는 RED evidence가 아니다. [VERIFIED: 02-CONTEXT.md:54-57]
2. RED commit과 XML/log path를 ledger에 기록한다.
3. 같은 tests를 교정 후 GREEN으로 만든다.
4. 새 API/새 규칙은 conformance fixture에 추가하며 존재하지 않는 API 때문에 RED commit을 오염시키지 않는다. [VERIFIED: 02-CONTEXT.md:54-59]

### Wave 0 Gaps

- [ ] `Assets/Editor/Tests/Phase2RegressionTests.cs` — known defect RED cases and category/name grouping.
- [ ] `Assets/Editor/Tests/Phase2ConformanceTests.cs` — source-dated table-driven rule catalog.
- [ ] `.planning/phases/02-shared-rules-core/02-EVIDENCE.md` — commit/group/test/count/raw path ledger.
- [ ] Mahjong Soul-specific inferred fixture rows need direct client observation or explicit secondary-source inference before final GREEN.
- Existing framework/helper/assembly setup is sufficient; no install or asmdef gap exists. [VERIFIED: .planning/phases/01-executable-baseline/01-VALIDATION.md]

## Security Domain

`workflow.security_enforcement`의 원문 값은 `"security_enforcement": true`이므로 이 절을 적용한다. [VERIFIED: .planning/config.json:47]

### Applicable ASVS Categories

| ASVS Category | Applies | Standard Control |
|---------------|---------|-----------------|
| V2 Authentication | no | 로컬 순수 규칙/Player 기능이며 인증 경계가 없다. [VERIFIED: phase boundary 02-CONTEXT.md:6-8] |
| V3 Session Management | no | 네트워크 session/token이 없다; solo lifecycle은 gameplay state로 기존 tests가 다룬다. [VERIFIED: Assets/Scripts/SoloScoringGameManager.cs:104-257] |
| V4 Access Control | no | 사용자/역할별 권한 또는 remote action이 없다. [VERIFIED: phase boundary 02-CONTEXT.md:6-8] |
| V5 Input Validation | yes | tile count 13/14, kind count ≤4, winning tile membership, meld shape/count, mutually exclusive context, indicator validity를 public evaluator 입구에서 검증한다. [VERIFIED: 현재 부분 guard Assets/Scripts/AL-1S/MahjongUtilities.cs:26-27,48-56,182-191] |
| V6 Cryptography | no | seed는 replay determinism용이지 보안 난수가 아니며 cryptography가 필요 없다. [VERIFIED: Assets/Scripts/AL-1S/MahjongRound.cs:189-212,389-413] |

### Known Threat Patterns for Unity/C# Rules Core

| Pattern | STRIDE | Standard Mitigation |
|---------|--------|---------------------|
| malformed compact tile string creates sentinel/invalid counts and later index errors | Tampering / DoS | parse boundary returns explicit invalid result; evaluator revalidates counts and never indexes before count checks. [VERIFIED: Assets/Scripts/AL-1S/MahjongTileAndBlock.cs:405-483] |
| mutable fields used in hash key change after insertion | Tampering | identity fields immutable in practice; remove scoring mutation from tile and test HashSet stability. [CITED: https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/expressions/equality] |
| contradictory context grants impossible yaku combination | Spoofing | validated exclusive state values plus constructor/factory guard; reject impossible combinations. [VERIFIED: 02-CONTEXT.md:35] |
| scoring version migration wipes unrelated save data | Tampering / data loss | highScore-only migration, diagnostic old version/value, Phase 1 durable save guard. [VERIFIED: 02-CONTEXT.md:50-52; .planning/phases/01-executable-baseline/01-07-SUMMARY.md] |
| missing/empty XML is called GREEN | Repudiation | helper status only, parseable NUnit XML, positive selected count, zero non-passed. [VERIFIED: unity-test-gate SKILL.md] |

## Sources

### Primary / authoritative for the claim (MEDIUM seam confidence)

- https://docs.unity3d.com/Packages/com.unity.test-framework@1.1/manual/reference-command-line.html — 1.1.33 command-line filters and NUnit XML, checked 2026-09-04.
- https://learn.microsoft.com/en-us/dotnet/api/system.random.next — exclusive upper bound, checked 2026-09-04.
- https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/expressions/equality — equality/hash/operator contract, checked 2026-09-04.
- https://mahjong-europe.org/portal/images/docs/Riichi-rules-2025-EN.pdf — generic riichi yaku/fu/dora/payment baseline, published 2025 and checked 2026-09-04.
- https://mahjongsoul.yo-star.com/tournament/rules.pdf — official Mahjong Soul event settings (red five 3, open tanyao); not a complete ranked oracle, checked 2026-09-04.

### In-repo primary (HIGH for current implementation)

- `Assets/Scripts/AL-1S/MahjongTileAndBlock.cs` — tile identity, parser, 34/37 tile catalog.
- `Assets/Scripts/AL-1S/Utilities.cs` — shuffle defect.
- `Assets/Scripts/AL-1S/MahjongRound.cs` — 139 wall construction, dora mutation, solo result integration.
- `Assets/Scripts/AL-1S/MahjongUtilities.cs` — greedy decomposition, han/fu, selection.
- `Assets/Scripts/AL-1S/MahjongYaku.cs` — yaku metadata/evaluator coverage gaps.
- `Assets/Scripts/AL-1S/MahjongWinInfo.cs` — boolean context, score table, equality/comparison.
- `Assets/Scripts/SoloScoringGameManager.cs` — score-distance single integration seam.
- `Assets/Scripts/Configs/Settings.cs`, `SettingsManager.cs` — save migration boundary.
- `.planning/phases/01-executable-baseline/01-VALIDATION.md` — existing 4+15 fail-closed gate.

### Secondary / inference only (LOW for Mahjong Soul-specific claims)

- https://note.com/qar13ktk20/n/nf9c7cf0eb025?hl=en — community-maintained four-player Mahjong Soul behavior catalog, published 2021, updated through 2022, checked 2026-09-04. It explicitly states no official website rulebook and contains a double-yakuman wording conflict; never use alone.
- https://riichi.wiki/Template%3AMajsoul/Yaku_table and https://riichi.wiki/Multiple_yakuman — cross-check for yaku/double-yakuman variants, checked 2026-09-04.
- https://d7.mjsdb.ovh/play/yaku/doubleyakuman — secondary game database listing four double-yakuman forms, checked 2026-09-04.

## Metadata

**Confidence breakdown:**

- Standard stack: HIGH — pinned project files and installed environment inspected.
- Current-code architecture: HIGH — source-of-truth files opened with line-level evidence.
- Generic riichi arithmetic: MEDIUM — current authoritative association rulebook, but not Mahjong Soul itself.
- Mahjong Soul-specific variants: LOW until direct client/official-ranked evidence is attached.
- Validation architecture: HIGH — existing project helper, XML contract, fixtures, and config inspected.

**Research date:** 2026-09-04  
**Valid until:** 2026-10-04 for Unity/codebase patterns; re-check Mahjong Soul client behavior immediately before locking conformance fixtures.
