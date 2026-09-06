# Phase 2 계획 변경 반영 및 전체 플랜 검토

- 작업: `260906-mov` / `gsd-quick --validate`
- 검토일: 2026-09-06
- 시작 HEAD: `4c4a015a826912de5d50c7f64f1ce3c4ae30c707`
- 입력: [P2-CR-01~04 요청서](phase2-plan-change-requests.md)
- 결과: **네 요청의 문서 반영 완료. 8개 플랜 구조·정합성 검사 통과. P2-OPEN-01은 미결로 보존.**
- 실행 상태: Phase 2 **0/8**, 구현·Unity 테스트·빌드·Player 관찰·execute-phase 미실행.

## 요청 ID별 반영 위치

| ID | 반영한 계약 | 주 반영 위치 | 연결한 검증 |
|---|---|---|---|
| **P2-CR-01** | 기본 ==/!=/typed/object Equals/hash/CompareTo는 TileID. 원본 적색 속성은 점수·그림용으로 보존. 선택 위치를 아는 제거는 인덱스 사용. 패산 재현은 적색 위치까지 비교. | [CONTEXT D-06](../../.planning/phases/02-shared-rules-core/02-CONTEXT.md), [02-01 T1/T2](../../.planning/phases/02-shared-rules-core/02-01-PLAN.md), [02-02 T1](../../.planning/phases/02-shared-rules-core/02-02-PLAN.md), [02-03 T1](../../.planning/phases/02-shared-rules-core/02-03-PLAN.md) | ==/Equals 실제 불일치 RED, default/invalid 법칙, 일반5/적5 인덱스 제거 양방향, 적5 머리·커쯔·순쯔·치또이츠·대기, 136/종류별4/적5각1, 적색 위치 교환 검출, resolver 원본 필드 비변이. |
| **P2-CR-02** | 02-04는 부·기본점·지불·좌석 delta·최선 후보 계산. 실제 솔로 지급 결함의 같은 테스트 RED/GREEN은 02-05가 함께 소유. | [02-04 T2/완료 조건](../../.planning/phases/02-shared-rules-core/02-04-PLAN.md), [02-05 T1](../../.planning/phases/02-shared-rules-core/02-05-PLAN.md) | SOL-PAY-01: 비리치 Pinfu+MenzenTsumo 2판20부, legacy zaRon 1300 대 올바른 쯔모 700+400+400=1500. 실제 player delta 및 거리 service 금액1500/1회. XML·테스트 이름·양수 개수·커밋을 RED/GREEN 양쪽에 연결. |
| **P2-CR-03** | 현재 쯔모패를 제외한 남은 패. 1→0 마지막 일반 패 취득 시 화료 기회를 보존하고, 0의 최종 타패 후 유국. solo 허용 마지막 패와 future four-seat live-wall 마지막 패는 caller가 구분. | [CONTEXT D-14/D-20](../../.planning/phases/02-shared-rules-core/02-CONTEXT.md), [02-03 T1](../../.planning/phases/02-shared-rules-core/02-03-PLAN.md), [02-05 T2/실행 전제](../../.planning/phases/02-shared-rules-core/02-05-PLAN.md) | 1→0 직후 종료0회; 마지막 패 Haitei 화료/score1회 또는 최종 타패/추가 draw0회/유국·다음 국1회. 양수 Haitei 없음, wall 잔존 solo, synthetic 4인 caller, 타패 후 tenpai/noten·친 진행, 음수/중복 종료/stale Haitei 거부. **18 초기화는 P2-OPEN-01.** |
| **P2-CR-04** | 우라 공개는 유효한 화료 성립 AND 화료자 리치 상태. 표시패·우라 유래 강조·결과 정보에 같은 조건. 비공개 입력 차단과 다음 국/비리치 결과 초기화. 0판도 공개 자격 유지. | [CONTEXT D-09](../../.planning/phases/02-shared-rules-core/02-CONTEXT.md), [02-03 T1](../../.planning/phases/02-shared-rules-core/02-03-PLAN.md), [02-05 T1](../../.planning/phases/02-shared-rules-core/02-05-PLAN.md), [02-07 T1/T2](../../.planning/phases/02-shared-rules-core/02-07-PLAN.md), [02-08 T1/T2](../../.planning/phases/02-shared-rules-core/02-08-PLAN.md) | raw hidden ura가 존재하는 리치 화료 전/비리치 화료/리치 유국·오화료의 비공개, 유효 리치 화료 우라 양수/0 공개, stale-clear. 자동 표시 경계 검사와 같은 PID의 PLAYER-OBS-01~05 직접 관찰을 분리. |

네 요청의 공통 정합성은 [RESEARCH](../../.planning/phases/02-shared-rules-core/02-RESEARCH.md), [PATTERNS](../../.planning/phases/02-shared-rules-core/02-PATTERNS.md), [VALIDATION](../../.planning/phases/02-shared-rules-core/02-VALIDATION.md), [DISCUSSION-LOG 후속 정정](../../.planning/phases/02-shared-rules-core/02-DISCUSSION-LOG.md), [02-08 최종 coverage](../../.planning/phases/02-shared-rules-core/02-08-PLAN.md)에 연결했다. DISCUSSION-LOG의 과거 선택과 요청서 본문은 당시 이력으로 보존했다.

기존 PLAYER-OBS-01의 6판20부/12000/distance80.00/boost6은 유지했다. 추가 사례의 4판20부/5200과 5판20부/8000은 비리치 및 우라0 공개 조건을 검증하기 위한 별도 사례다. 멘젠쯔모 제외는 반영하지 않았다.

## 8개 플랜 전체 검토

기존 플랜 번호와 Wave 1~8을 유지한다. 의존성은 `02-01 → 02-02 → 02-03 → 02-04 → 02-05 → 02-06 → 02-07 → 02-08`이며 순환·누락된 선행 plan ID·같은 wave 쓰기 충돌이 없다. 아래의 통과는 **계획의 검증 조건이 연결되어 있다는 뜻**이며 미래 테스트의 성공이 아니다.

| 플랜 | 선행 | 요구사항·담당 | 검증 조건·남은 실행 전제 |
|---|---|---|---|
| 02-01 | Phase 1 | RULE-01/02/03. 공유 패산, 기존 셔플 helper, TileID 동등성·인덱스 제거·적색 보존. | 공개 결함 assertion RED→동일 GREEN; Conformance; Phase 1 exact 4+15. D-09의 화면 완료는 07/08에 배정. |
| 02-02 | 02-01 | RULE-04/05. 34종 exhaustive 분해와 귀속, 결정론적 후보 순회. | 일반형/치또이츠/국사무쌍, 적5 포함 분해·대기, invalid 입력, literal 후보 tie-break. 04 이전 임시 payment adapter는 04에서 제거. |
| 02-03 | 02-02 | RULE-06/07. validated context, 모든 표준역·역만, query-only 도라·리치 조건 우라. | dora-only 실제 무역 맥락 RED, 모순 context 거부, catalog/source metadata, raw hidden list와 비리치 context 필터 구분, 원본 필드 snapshot. |
| 02-04 | 02-03 | RULE-03/05/08/09. 결과 동등성·최선 후보·부·기본 지불 계산. | literal fu/payment/result/selector Conformance와 기존 Regression. 실제 solo 지급을 완료했다고 주장하지 않음. |
| 02-05 | 02-04 | RULE-10. 실제 solo 연결, 지급 RED/GREEN, 남은 패·Haitei·국 진행, 개발 전용 관찰 setup. | SOL-PAY-01 실제 경로 금액/횟수, 1→0 마지막 패 분기·reset·4인 합성 caller. **P2-OPEN-01 확인 전 초기화·표시·총량 구현은 진행 불가.** |
| 02-06 | 02-05 | RULE-10/D-25/26. 버전·고득점만 마이그레이션, durable save guard 재사용. | legacy/mismatch/same-version/중단 복구, 다른 저장값 보존, Phase 1 exact 4+15. **파일 원문 수정 없이 전체 검토.** |
| 02-07 | 02-06 | RULE-07/10. 공개 predicate와 결과/표시패/강조 reset, 네 fixture 및 개발 Player 빌드. | raw ura 존재 음성/양성/0판/전환 cases, zero non-passed XML와 positive unique names, trace4/lifecycle15, fresh BuildReport·exe. GUI는 NOT OBSERVED. |
| 02-08 | 02-07 | RULE-07/09/10. 실제 같은 PID 관찰과 증거 봉인. | 원래 01의 5항목 + 02~05 각 필수 결과와 직접 승인. 미관찰·실패·P2-OPEN-01 미결은 Phase 완료/PASS로 봉인 불가. |

새 런타임 타입·테스트 fixture·SUMMARY·EVIDENCE·개발 빌드는 아직 미래 산출물이다. 파일이 현재 없다는 사실을 구현 누락으로 오인하지 않고, 선행 producer와 task 소유가 있는지 확인했다. 같은 fixture를 여러 plan이 확장하지만 순차 의존성을 유지하여 동시에 변경하지 않는다. Phase 3 리치 선언과 실제 4인 상태를 synthetic scoring context/관찰 setup에 섞지 않았다.

## 전체 검토 중 함께 맞춘 사항

| 검토 항목 | 조치 |
|---|---|
| 기존 셔플 직접 호출의 RED가 새 wall만 수정하면 남을 수 있음 | 02-01에 `Utilities.cs` 소유를 명시하고 기존 `ShuffleArray` 교정·재사용을 계획. 새 wall에 별도 셔플을 중복 작성하지 않음. |
| 02-04가 result equality와 최고 지급 선택을 담당하지만 frontmatter에서 해당 요구사항 누락 | RULE-03/05를 기존 RULE-08/09에 추가하고 VALIDATION과 정렬. |
| TileID equality만으로는 적색·도라 필드 변이를 검출할 수 없음 | 02-03 비변이 조건을 원본 필드 snapshot으로 수정. |
| 조건을 추가한 UI 검증의 실제 표시 경로 소유 | 02-07의 수정 범위에 기존 `UiRoundInfo`/`UiYakuPreset` 경로를 포함하고 공개 도라 슬롯 보존, 기존 결과 row 재사용, 숨긴 입력·stale-clear를 명시. |
| 새 관찰 entry의 일반 빌드 제외 검증과 오래된 BuildReport 오인 | 02-07에서 기존 normal 빌드 helper도 재사용하고 normal/observation report를 각각 실행 시작 시각과 대조. 02-08은 보조 launch PID를 본 관찰 PID와 분리해 제외 동작을 관찰. |
| 우라 음성/0판 GUI 사례를 자동 테스트로 대체할 위험 | VALIDATION에 PLAYER-OBS-01~05의 고정 setup/action/expected를 두고 03/04/05/07/08의 담당을 연결. 01 수치는 유지. |
| 18 초기화 미결인데 자동 실행 가능으로 보일 위험 | 02-05 `autonomous: false`와 실행 전제를 명시하고 현재 STATE의 구현 전 상태를 정정. |

## 수행한 검증과 범위

- Canonical `gsd-tools query verify.plan-structure`: Quick plan 1개 + Phase 2 plan 8개, **9/9 valid**, 구조 경고 없음.
- Phase 2: 기존 **8개 plan / 16개 task**, 순차 DAG와 task별 수정 파일 소유 확인.
- **RULE-01~10 10개**, CONTEXT/RESEARCH **D-01~31 31행 동일**, 기존 **edge category 39개 / prohibition 7개** 보존 확인.
- 추가 CR 검증 조건은 기존 39개 edge 분류 수와 따로 추적. 39는 실제 Unity 테스트 수가 아니다.
- 시작 시점과 비교하여 보호 대상 **452개 파일**의 SHA-256 동일. 런타임 소스·패키지·프로젝트 설정과 비대상 사용자 변경을 보존.
- `02-06-PLAN.md` 원문 동일, 기존 DISCUSSION-LOG 전체가 후속 정정 앞에 그대로 존재, 요청서의 CR 본문 원문 보존.
- `git diff --check` 및 commit 대상 범위 확인.
- 검토 방식: 현재 에이전트의 순차 Quick plan check와 수정 후 verification. 별도 서브에이전트의 독립 검증으로 표현하지 않는다.

검사 재실행: `node output/reviews/phase2-plan-revision-260906-mov/check-plans.cjs`. [원본 검사 결과](phase2-plan-revision-260906-mov/validation-results.json)와 수정 전 snapshot/편집 보조 스크립트는 프로젝트 로컬 검토 산출물이다. 제품 코드가 아니며 이 스크립트는 Unity를 실행하지 않는다.

## 남은 문제

1. **P2-OPEN-01 — 기본값 18의 초기화 의미.** 첫 쯔모를 포함한 총량인지, 첫 쯔모 이후 남은 패 수인지 미결이다. 현 코드는 counter 선언만 있고 배패 후 첫 Tsumo를 실행하므로 코드만으로 사용자 의도를 확정할 수 없다. 02-05 T2의 해당 구현 전에 사용자 결정과 CONTEXT/DISCUSSION-LOG/PLAN/VALIDATION 갱신이 필요하다. 이번 문서 수정은 1→0의 마지막 패 처리를 확정했으며 총량은 선택하지 않았다.
2. **기존 연구 추론과 후속 Phase 결정은 그대로 유지.** A1~A4의 작혼 세부 규칙은 기존 출처·dated inference 조건으로 남아 있다. Phase 3/4 요구사항의 과거 리치 경제·종료 규칙 문구 재조정은 이번 네 요청의 범위 밖이다. 새 규칙 결론을 만들지 않았다.
3. **실행 증거는 아직 없음.** 실제 RED/GREEN XML·정확한 Phase 2 case 수·빌드·GUI 관찰은 각 기존 플랜을 향후 승인된 범위에서 실행할 때 수집한다. 문서 검사 통과는 이 증거를 대신하지 않는다.

미반영한 승인 요청은 없다. P2-CR-03에 포함된 미결 초기화 항목은 의도적으로 OPEN으로 반영했다. 기존 계획 수정과 보고까지 완료하고 종료한다.
