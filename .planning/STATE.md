---
gsd_state_version: 1.0
milestone: v1.0
current_phase: 02
current_phase_name: shared-rules-core
status: planned
stopped_at: Phase 2 plans revised; P2-OPEN-01 pending before initialization implementation
last_updated: "2026-09-06T07:41:24.338Z"
last_activity: 2026-09-06
last_activity_desc: Applied P2-CR-01 through P2-CR-04; validated all 8 plans; no implementation
state_head: 09f262d
progress:
  total_phases: 7
  completed_phases: 1
  total_plans: 15
  completed_plans: 7
milestone_name: milestone
---

# Project State

## Project Reference

See: .planning/PROJECT.md (updated 2026-09-02)

**Core value:** 기존 마작 로직을 정확하고 테스트 가능한 공유 규칙 엔진으로 개선하고, 독립 솔로와 고정 4인 반장전에서 재사용되는 과정을 증거로 보여준다.
**Current focus:** Phase 2 — Shared Rules Core — 정확한 공유 규칙 코어

## Current Position

Phase: 02 (shared-rules-core) — PLANNED; P2-OPEN-01 pending for 02-05 initialization
Plan: Not started
Status: Plans revised and reviewed; implementation not started
Last activity: 2026-09-06 — Completed quick task 260906-mov: P2-CR-01~04 계획 반영 및 전체 8개 검토; 18 초기화 의미 OPEN

Progress: 7/15 phase plans completed (Phase 1: 7/7; Phase 2: 0/8)

## Performance Metrics

**Velocity:**

- Total plans completed: 7
- Average duration: -
- Total execution time: 0.0 hours

**By Phase:**

| Phase | Plans | Total | Avg/Plan |
|-------|-------|-------|----------|
| 01 | 7 | - | - |

**Recent Trend:**

- Last 5 plans: -
- Trend: -

*Updated after each plan completion*
**Per-Plan Metrics:**

| Plan | Duration | Tasks | Files |
|------|----------|-------|-------|
| Phase 01 P01 | 2d 20h 45m | 4 tasks | 18 files |
| Phase 01 P02 | 17min | 2 tasks | 9 files |
| Phase 01 P03 | 15min | 2 tasks | 7 files |
| Phase 01 P04 | 15min | 3 tasks | 7 files |
| Phase 01 P05 | 1h 35min | 3 tasks | 3 files |
| Phase 01 P06 | 7h 40min | 3 tasks | 6 files |
| Phase 01 P07 | 53min | 2 tasks | 3 files |

## Accumulated Context

### Decisions

Decisions are logged in PROJECT.md Key Decisions table.

- Stage 2의 정확한 고정 기본 반장전은 필수 릴리스 완료선이다.
- 솔로는 180초 점수·부스트·거리의 독립 플레이 경험으로 유지한다.
- Phase 6의 `CALL-01`~`CALL-18`은 완전한 Stage 3 묶음으로만 공개하며, gate 실패 시 전체를 v2로 이동한다.
- 전략 AI, 추가 종료 규칙, 전면 UI 재제작과 새 패키지는 이번 milestone에서 제외한다.
- [Phase 1]: `SoloScoringGameManager` owns 솔로 시작, 포기 모달 정책, 입력 게이트, 종료, 저장, 재시작 정리를 담당한다.
- [Phase 1]: `ForfeitRequested`는 `PlayerCallType`과 분리된 입력 이벤트이며, scene-local overlay가 확인 UI를 표시한다.
- [Phase 1]: `PlayerHandController`는 입력·선택을, `PlayerHandView`는 패 표시를 담당한다.
- [Phase 1]: Windows builds preserve a durable report before explicit Editor exit and restore the canonical ignored Temp evidence path.
- [Phase 1]: New Korean lifecycle labels reuse the existing 경기천년 2K TMP atlas and shared material.
- [Phase 01]: PlayerHandView stays on the existing hand GameObject and reuses the current tile hierarchy.
- [Phase 01]: PlayerHandController preserves the original script GUID and uses direct idempotent manager subscriptions.
- [Phase 01]: SoloScoringUIController preserves the former GameUIManager asset GUID and owns solo presentation references.
- [Phase 01]: The forfeit overlay uses existing scene Buttons and EventSystem with Cancel selected by default.
- [Phase 01]: PlayerHandController blocks ordinary input explicitly while Escape remains available for modal cancel.
- [Phase 01]: SoloScoringGameManager preserves the former manager script GUID and removes the bounded legacy facade after caller migration.
- [Phase 01]: UiManager activates one inactive-by-default SoloScoringModeRoot before session start and disables it only on lobby return.
- [Phase 01]: The solo manager is scene-local rather than DontDestroyOnLoad so root disable owns subscription teardown.
- [Phase 01]: PlayerHandController uses one private HandleEscape seam to make the real same-frame Escape branch assertion-testable without a generalized input abstraction.
- [Phase 01]: Final Phase 1 project evidence is the explicit aggregate 3 + 13 = 16 from independently filtered trace and lifecycle XML results.
- [Phase 01]: GUI PASS is recorded only from explicit approval of all eight same-PID steps on Player PID 20000.
- [Phase 01]: Trace value and length divergence use the existing first-mismatch diagnostic path.
- [Phase 01]: TimeExpired updates the record before rendering and saving while Forfeit remains no-save.
- [Phase 01]: Session start propagates index 6 through the existing UpdateHand route without a reset API.
- [Phase 01]: The 4+14 automated gate does not replace the pending D-13 visible Player observation.
- [Phase 01]: Lifecycle persistence tests use a same-directory durable backup or originally-absent marker and recover stale state before mutation.
- [Phase 01]: Real and temporary save recovery paths share fixture-private helpers while production SettingsManager remains unchanged.
- [Phase 01]: Exact 4+15 automated save-safety evidence remained separate from D-13, which was explicitly approved through the completed 20/20 UAT.

- [Phase 02 / 2026-09-06]: P2-CR-01~04를 계획에 반영했다. 기본 tile equality는 TileID, 실제 solo 지급 RED/GREEN은 02-05 소유, 마지막 패 1→0 이후 화료/최종 타패를 보장하고 우라는 유효한 리치 화료에만 공개한다. 과거 논의는 보존하며 멘젠쯔모 제외는 적용하지 않는다.

### Pending Todos

None yet.

### Blockers/Concerns

- 9일 범위에서 Phase 6이 Stage 2 안정화와 Phase 7 릴리스 증거를 침범하지 않게 조건부 gate를 지켜야 한다.
- Phase 2의 기존 8개 계획과 fixture/파일 소유를 검토했다. P2-OPEN-01: 기본 18의 첫 쯔모 포함 여부·초기화·표시 시점은 미결이며 02-05 Task 2의 해당 구현 전에 사용자 결정과 기록 갱신이 필요하다.
- Phase 4는 남4국 친 연장 후 친이 넘어갈 때 종료하는 고정 계약을 그대로 검증해야 한다.
- Phase 6 계획 전 call priority, kuikae, riichi-after-ankan, rinshan/live-wall 및 kan-dora timing을 권위 규칙과 재확인해야 한다.

### Quick Tasks Completed

| # | Description | Date | Commit | Status | Directory |
|---|-------------|------|--------|--------|-----------|
| 260906-kut | P2-CR-02 솔로 지급 회귀 검증 범위 조정 제안 기록 | 2026-09-06 | ff50213 |  | [260906-kut-phase-2-02-04-02-05](./quick/260906-kut-phase-2-02-04-02-05/) |
| 260906-ld7 | P2-CR-03 남은 패·마지막 타패·모드별 해저로월 제안 기록 | 2026-09-06 | b3a6fea |  | [260906-ld7-record-p2-cr-03-remaining-tiles-final-di](./quick/260906-ld7-record-p2-cr-03-remaining-tiles-final-di/) |
| 260906-lx5 | P2-CR-04 리치 화료 전용 우라도라 공개 제안 기록 | 2026-09-06 | ed9a7b4 |  | [260906-lx5-record-p2-cr-04-riichi-win-only-ura-disc](./quick/260906-lx5-record-p2-cr-04-riichi-win-only-ura-disc/) |
| 260906-mov | P2-CR-01~04 계획 반영 및 전체 8개 의존성·요구사항·검증 검토 | 2026-09-06 | 09f262d | Verified (docs); P2-OPEN-01 open | [260906-mov-apply-p2-cr-01-through-p2-cr-04-to-exist](./quick/260906-mov-apply-p2-cr-01-through-p2-cr-04-to-exist/) |

## Deferred Items

| Category | Item | Status | Deferred At | Milestone |
|----------|------|--------|-------------|-----------|
| Conditional | CALL-01~CALL-18 complete Stage 3 bundle | Gate pending | Phase 5 exit | v1 |

## Session Continuity

Last session: 2026-09-06T07:41:24.339Z
Stopped at: Phase 2 plans revised and reviewed; P2-OPEN-01 remains open
Resume file: .planning/phases/02-shared-rules-core/02-CONTEXT.md
