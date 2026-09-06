---
quick: 260906-mov
decision: P2-OPEN-01
status: resolved
date: 2026-09-06
scope: documentation-only
---

# P2-OPEN-01 후속 결정 반영

사용자가 기본값 18에서 첫 쯔모를 하면 17이 된다고 명시했다. 기본 18은 배패 후 첫 쯔모를 포함한 한 국의 허용 쯔모 총량이다. 배패 13장은 차감하지 않는다. 첫 성공 쯔모 후 상태와 표시 입력은 17이며, 조기 종료가 없다면 18번째 쯔모 후 0이 된다.

0이 되는 순간에는 마지막 패의 화료/타패를 기다린다. 마지막 타패 후 같은 국의 19번째 쯔모 없이 유국한다. 새 국은 다시 18로 초기화한 뒤 첫 쯔모 후 17이 된다. 기존 Haitei·중복 종료 차단·친/국풍 진행 조건은 유지한다.

## 반영 및 검증

- D-20을 CONTEXT/RESEARCH에 동일하게 반영하고 DISCUSSION-LOG에 후속 결정을 추가했다. 과거 미결 기록과 기존 quick PLAN/PLAN-CHECK/SUMMARY/VERIFICATION은 당시 상태로 보존한다. 현재 상태는 이 후속 기록과 STATE 및 요청별 반영 보고서를 따른다.
- 02-05 Task 2의 초기값·배패 비차감·첫 표시 17·총 18회·새 국 reset 검증 조건을 명시하고, 결정 대기에 따른 `autonomous: false`를 해제했다. PATTERNS/VALIDATION/02-08 coverage와 요청서 상태 및 반영 보고서를 맞췄다.
- Canonical plan structure: quick 1개와 Phase 2 8개, **9/9 valid**, 경고 없음. 기존 Phase 2 **8개 plan / 16개 task**, 순차 의존성·task 파일 소유·RULE 10개·D-01~31 동일·edge category 39개·prohibition 7개를 확인했다.
- 문서 검사 **16개 통과**. 보호 대상 **452개 파일 SHA-256 동일**, 기존 논의 및 요청 본문 보존, `git diff --check` 통과.
- 재실행: `node output/reviews/phase2-plan-revision-260906-mov/check-plans.cjs`. 이번 결과는 `initialization-validation-results.json`이며 이전 `validation-results.json`은 보존했다.

P2-OPEN-01의 추가 사용자 결정은 필요하지 않다. 기존 연구 추론/Phase 3·4 결정과 미래 실행 증거는 계속 구분한다. Phase 2는 **0/8, 구현 미시작**이다. 멘젠쯔모 제외·Unity 실행·빌드·Player 관찰·execute-phase는 수행하지 않았다.
