---
phase: quick
plan: 260906-mov
type: execute
wave: 1
depends_on: []
autonomous: true
requirements: [P2-CR-01, P2-CR-02, P2-CR-03, P2-CR-04]
files_modified:
  - .planning/phases/02-shared-rules-core/02-CONTEXT.md
  - .planning/phases/02-shared-rules-core/02-01-PLAN.md
  - .planning/phases/02-shared-rules-core/02-02-PLAN.md
  - .planning/phases/02-shared-rules-core/02-03-PLAN.md
  - .planning/phases/02-shared-rules-core/02-04-PLAN.md
  - .planning/phases/02-shared-rules-core/02-05-PLAN.md
  - .planning/phases/02-shared-rules-core/02-07-PLAN.md
  - .planning/phases/02-shared-rules-core/02-08-PLAN.md
  - .planning/phases/02-shared-rules-core/02-RESEARCH.md
  - .planning/phases/02-shared-rules-core/02-PATTERNS.md
  - .planning/phases/02-shared-rules-core/02-VALIDATION.md
  - .planning/phases/02-shared-rules-core/02-DISCUSSION-LOG.md
  - .planning/STATE.md
  - output/reviews/phase2-plan-change-requests.md
  - output/reviews/phase2-plan-change-application-report.md
must_haves:
  truths:
    - "P2-CR-01~04가 기존 8개 Phase 2 플랜의 필요한 부분과 관련 문서에 일관되게 반영된다."
    - "기본값 18의 첫 쯔모 포함 여부는 OPEN으로 보존되고 의존 작업의 확인 조건이 명시된다."
    - "8개 플랜 전체의 DAG, 요구사항, task ownership, 자동/GUI 검증 조건을 검토하며 구현 PASS를 주장하지 않는다."
    - "기존 사용자 변경과 결정 이력을 보존하며 멘젠쯔모 제외, 구현, Unity 실행, execute-phase는 범위 밖이다."
  artifacts:
    - path: output/reviews/phase2-plan-change-application-report.md
      provides: 요청 ID별 반영 위치, 전체 플랜 감사, 남은 문제
    - path: .planning/phases/02-shared-rules-core/02-CONTEXT.md
      provides: D-06/D-09/D-14/D-20 정정 계약
  key_links:
    - from: output/reviews/phase2-plan-change-requests.md
      to: output/reviews/phase2-plan-change-application-report.md
      via: P2-CR-01~04 추적
    - from: .planning/phases/02-shared-rules-core/02-VALIDATION.md
      to: .planning/phases/02-shared-rules-core/02-08-PLAN.md
      via: 같은 검증 사례와 자동/사람 관찰 구분
---

<objective>
기준 HEAD 4c4a015의 기존 Phase 2 계획에 승인된 네 요청을 문서 변경으로 반영하고 전체 계획의 실행 조건을 검토한다. 새 phase plan을 만들거나 기존 8개를 재작성하지 않는다.
</objective>

<context>
@output/reviews/phase2-plan-change-requests.md
@.planning/STATE.md
@.planning/ROADMAP.md
@.planning/REQUIREMENTS.md
@.planning/phases/02-shared-rules-core/02-CONTEXT.md
</context>

<tasks>
<task type="auto">
  <name>Task 1: 기준 보존 및 네 요청의 계약·담당 플랜 정합성 수정</name>
  <files>.planning/phases/02-shared-rules-core/02-CONTEXT.md, 02-01~05-PLAN.md, 02-07~08-PLAN.md, 02-RESEARCH.md, 02-PATTERNS.md, 02-VALIDATION.md, 02-DISCUSSION-LOG.md</files>
  <action>수정 전 dirty files와 관련 문서 해시를 보존한다. TileID 기본 동등성 및 인덱스 제거/적색 보존, 실제 솔로 지급 RED/GREEN의 02-05 공동 소유, 남은 패 1→0 이후 화료/최종 타패 흐름과 호출자 해저 맥락, 유효한 리치 화료만 우라 공개 계약을 반영한다. 과거 논의는 수정하지 않고 후속 정정을 추가한다. 18 초기화 의미는 미결로 남기고 그 결정에 의존하는 구현을 임의 진행하지 않게 한다.</action>
  <verify><automated>문서 diff와 P2-CR별 cross-reference 확인; 모든 기존 Phase 2 PLAN에 gsd-tools query verify.plan-structure 실행.</automated></verify>
  <done>네 요청의 현재 계약과 검증 조건이 일치하며 원본 이력/비대상 파일이 보존된다.</done>
</task>
<task type="auto">
  <name>Task 2: 전체 8개 플랜 검증 및 요청별 결과 보고</name>
  <files>output/reviews/phase2-plan-change-application-report.md, output/reviews/phase2-plan-change-requests.md, .planning/STATE.md, quick SUMMARY/VERIFICATION</files>
  <action>8개 플랜의 순차 DAG, 선행 산출물, 수정 파일·task 책임, RULE-01~10와 D-01~31, 39 edge truths/7 prohibitions, XML/실제 Player 관찰 조건을 점검한다. 요청 목록에 적용 상태와 보고서 링크를 추가하되 제안 당시 본문을 이력으로 남긴다. Quick 문서 검증과 Phase 2 실행 준비 상태/미결 항목을 분리하여 기록하고 좁은 문서 커밋을 만든다.</action>
  <verify><automated>전체 문서 정합성 검사, git diff --check, 비대상 파일의 전후 해시 비교, staged 파일 범위 확인.</automated></verify>
  <done>요청 ID별 위치와 남은 문제가 기록되고 구현·Unity·execute-phase 없이 종료한다.</done>
</task>
</tasks>

<verification>
Quick plan check와 수정 후 verification을 현재 에이전트에서 순차 수행한다. 별도 에이전트 결과로 표현하지 않는다. 사용자 요청에 따라 single-plan quick 검사에 추가하여 전체 Phase 2 plan dependency/requirement/verification audit를 수행한다. 자동 문서 검사는 미래 Unity 테스트/빌드나 GUI 관찰의 성공을 뜻하지 않는다.
</verification>
