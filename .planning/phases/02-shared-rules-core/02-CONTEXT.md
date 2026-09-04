# Phase 2: Shared Rules Core — 정확한 공유 규칙 코어 - Context

**Gathered:** 2026-09-04
**Status:** Ready for planning

<domain>
## Phase Boundary

Phase 2는 현재 솔로 코드에 섞여 있는 패산·패 정체성·화료 분해·역·도라·판·부·지불 계산을 하나의 결정론적이고 자동 검증 가능한 순수 규칙 코어로 교정하고, 기존 솔로 실행 경로가 그 결과를 사용하도록 최소 통합한다. 점수 코어는 후속 모드가 전달할 수 있는 멘젠/후로 및 상황 맥락까지 평가하지만, 리치 선언과 솔로 난이도 설정은 Phase 3, 실제 네 좌석 대국 상태와 반장 진행은 Phase 4, Unity 반장 UI는 Phase 5, 플레이 가능한 후로·깡은 Phase 6 또는 MVP 이후 범위다.

</domain>

<decisions>
## Implementation Decisions

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

</decisions>

<canonical_refs>
## Canonical References

**Downstream agents MUST read these before planning or implementing.**

### 프로젝트 범위와 요구사항
- `.planning/PROJECT.md` — 핵심 가치, 9일 제약, 솔로/4인 모드 분리와 소스 보존 원칙.
- `.planning/ROADMAP.md` § Phase 2 — Phase 2 목표·성공 기준과 Phase 3~6 책임 경계. 논의 후 생긴 후속 단계 변경점은 이 CONTEXT의 Deferred Ideas를 우선 확인해야 한다.
- `.planning/REQUIREMENTS.md` § Shared Rules Core — `RULE-01`~`RULE-10`의 현재 추적 ID. `SOLO-06`~`SOLO-09`, `MTCH-08`~`MTCH-10`과 깡 범위에는 이번 논의와 충돌하는 오래된 문구가 있으므로 후속 Phase 계획 전에 재조정이 필요하다.

### 기준선과 검증 계약
- `.planning/phases/01-executable-baseline/01-CONTEXT.md` — 솔로 책임, 씬 생명주기, 입력/UI 경계와 fail-closed 증거 원칙.
- `.planning/phases/01-executable-baseline/01-BASELINE.md` — `portfolio-baseline`과 기존 증상 비교 기준.
- `.planning/phases/01-executable-baseline/01-VALIDATION.md` — 현재 Unity 테스트 gate와 증거 수량 계약.
- `.planning/phases/01-executable-baseline/01-07-SUMMARY.md` — Phase 1 최종 커밋·자동화·저장 안전성 상태.

### 코드베이스 조사
- `.planning/codebase/ARCHITECTURE.md` — 현재 솔로 coordinator, `MahjongRound`, 규칙 파일과 UI 연결 구조.
- `.planning/codebase/CONCERNS.md` — 139장 패산, 편향 셔플, 탐욕 분해, 역·부·쯔모 정산과 저장 위험 목록.
- `.planning/codebase/TESTING.md` — Unity Test Framework, EditMode fixture와 배치 실행 기준.

### 작혼 규칙 근거
- `https://mahjongsoul.yo-star.com/tournament/rules.pdf` — 작혼 공식 대회 자료. 대회 운영 규칙이므로 랭크전 전체 규칙의 단독 기준으로 사용하지 않는다.
- `https://note.com/qar13ktk20/n/nf9c7cf0eb025?hl=en` — 공개 공식 규칙집 부재를 전제로 작혼 4인 동작을 정리한 2차 자료. 공식 자료가 비어 있는 항목의 조사 출발점일 뿐 단독 권위 자료가 아니다.
- `https://riichi.wiki/All_last` — 작혼을 포함한 온라인 클라이언트의 오라스·연장 규칙 비교용 2차 자료. Phase 4 요구사항 재조정 시 재검증한다.

</canonical_refs>

<code_context>
## Existing Code Insights

### Reusable Assets
- `Assets/Scripts/AL-1S/MahjongTileAndBlock.cs`: 문자열 패 fixture 생성과 기존 `MahjongTile` 값 타입을 재사용할 수 있다. 현재 연산자는 `TileID`만 비교하지만 `Equals`는 struct 전체 필드, hash는 `TileID`만 사용하므로 D-06 계약에 맞춘 교정이 필요하다.
- `Assets/Scripts/AL-1S/MahjongRound.cs`: `NewRound(seed, out player)`와 PRNG 주입 경계는 결정론적 패산 테스트에 재사용할 수 있다. `remainingTsumoCount`와 `graveyard`는 선언·주석만 있고 실제 제한 흐름은 연결되지 않았다.
- `Assets/Scripts/AL-1S/MahjongUtilities.cs`, `MahjongYaku.cs`, `MahjongWinInfo.cs`: 기존 분해·역·부·점수 호출 흐름과 compact tile-string fixture 진입점은 회귀 표면으로 활용하되, 탐욕 분해와 판/부 비교 중심 선택은 교정 대상이다.
- `Assets/Scripts/IScoreDistanceService.cs`, `Assets/Scripts/ScoreManagerDistance.cs`: 정확한 마작 점수 뒤의 솔로 거리·부스트 계산 경계는 유지한다.
- `Assets/Scripts/UI-Kozeki/MahjongTileGameObject.cs`, `PlayerHandView.cs`: 현재 도라 표시 진입점 `SetDora`는 공유 resolver 결과를 표현하는 데 재사용할 수 있다.
- `Assets/Scripts/Configs/SettingsManager.cs`, `Settings.cs`: `PetitGameSaveData`의 기존 안전 로드/저장 경계를 점수 규칙 버전과 고득점 마이그레이션에 사용한다.
- `Assets/Editor/Tests/MahjongRoundTraceTests.cs`, `SoloSessionLifecycleTests.cs`, `Assets/Editor/Phase1Build.cs`: Phase 1이 만든 EditMode gate, 동일 프로세스 수명주기 검증과 Windows 빌드 경로를 확장한다.

### Established Patterns
- `SoloScoringGameManager`가 솔로 세션·라운드 교체·점수 전달을 소유하고, `SoloScoringUIController`와 `PlayerHandController/View`가 입력/표현을 나누는 Phase 1 경계를 유지한다.
- 규칙 계산은 `Assets/Scripts/AL-1S/`의 plain C# 타입에 두고 Unity 화면은 결과만 소비한다. 새 범용 manager, 이벤트 버스나 외부 dependency는 추가하지 않는다.
- 자동 XML·로그·빌드와 사람이 관찰한 Player 결과는 서로 다른 증거로 기록한다.

### Integration Points
- `MahjongRound.GenerateYama()`의 139장 생성과 셔플을 공유 136장 generator로 교체하고, `SoloScoringGameManager`의 국 시작 경로가 이를 호출한다.
- `MahjongPlayer.IsTsumoAble`, `MahjongUtility.CheckWinnable*`, `GetHighestWinInfo`와 `MahjongWinInfo` 생성 경로가 새 분해·맥락·최적 결과 API로 모이는 핵심 교체 지점이다.
- `MahjongRound.HandlePlayerWin()`의 현재 쯔모/론 점수 선택과 `HandleDanbean()`의 `-8,000점` 전환을 각각 순수 지불 결과와 솔로 정책으로 분리한다.
- `SoloScoringGameManager`의 `IScoreDistanceService.GetBoostAndDistance` 호출은 교정된 화료자 총수입을 정확히 한 번 전달해야 한다.
- `PetitGameSaveData.highScore` 로드/저장 지점에서 점수 규칙 버전 불일치만 선택적으로 초기화한다.

</code_context>

<specifics>
## Specific Ideas

- 런타임은 후보 전체가 아니라 최선의 화료 결과 한 개만 사용하고, 후보 열거는 테스트·디버그에서만 볼 수 있게 한다.
- 같은 판·부·지불액인 후보를 억지로 의미 순서화하지 않고 고정 순회의 첫 결과를 선택한다.
- 패 자체를 도라로 변이하지 않아 점수와 도라 반짝임이 같은 표시패 resolver를 공유하도록 한다.
- 기존 오류를 먼저 실패 테스트로 가시화한 RED 커밋과 교정 뒤 GREEN 커밋을 포트폴리오 전후 증거로 남긴다.
- 솔로는 4인 흐름을 축소 시뮬레이션하는 모드가 아니라, 하나의 패산에서 제한된 횟수만 쯔모하는 독립 스코어링 모드다.

</specifics>

<deferred>
## Deferred Ideas

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

</deferred>

---

*Phase: 2-Shared Rules Core — 정확한 공유 규칙 코어*
*Context gathered: 2026-09-04*
