---
phase: 02
slug: shared-rules-core
status: draft
nyquist_compliant: false
wave_0_complete: false
created: 2026-09-04
---

# Phase 02 — Validation Strategy

> 정확한 공유 규칙 코어를 짧은 RED→GREEN 주기와 fail-closed Unity 증거로 검증하기 위한 계약이다.

---

## Test Infrastructure

| Property | Value |
|----------|-------|
| **Framework** | Unity Test Framework `1.1.33` + NUnit extension `1.0.6` |
| **Config file** | 없음 — `Assets/Editor/Tests/*.cs`가 predefined `Assembly-CSharp-Editor`로 컴파일되며 새 `.asmdef`/패키지를 추가하지 않는다. |
| **Quick run command** | `& 'C:\Users\user\.codex\skills\unity-test-gate\scripts\Invoke-UnityTests.ps1' -ProjectPath $PWD -TestPlatform EditMode -TestFilter Phase2RegressionTests -ExpectedGate Green` |
| **Conformance command** | `& 'C:\Users\user\.codex\skills\unity-test-gate\scripts\Invoke-UnityTests.ps1' -ProjectPath $PWD -TestPlatform EditMode -TestFilter Phase2ConformanceTests -ExpectedGate Green` |
| **Full suite command** | 같은 helper로 `Phase2RegressionTests`, `Phase2ConformanceTests`, `MahjongRoundTraceTests`, `SoloSessionLifecycleTests`를 각각 실행하고 모든 JSON이 `GREEN`, XML이 parseable, 선택된 case 수가 양수, non-passed가 0인지 확인한 뒤 `Phase1Build.BuildWindowsPlayer`를 실행한다. |
| **Estimated runtime** | filtered fixture당 약 2–5분, 네 fixture와 Windows build를 포함한 phase gate 약 15–30분 |

---

## Sampling Rate

- **After every task commit:** 해당 task가 소유한 정확한 test-name subset 또는 fixture filter를 helper로 실행한다. 알려진 결함을 처음 고정하는 커밋은 non-empty filter와 `-ExpectedGate Red`를 사용하고 assertion failure만 RED로 인정한다.
- **After every plan wave:** `Phase2RegressionTests`와 `Phase2ConformanceTests`를 분리 실행하고, XML에서 선택된 고유 case 이름과 zero non-passed를 확인한다.
- **Before `$gsd-verify-work`:** Phase 2 두 fixture와 기존 Phase 1 trace 4 + lifecycle 15를 독립 실행하고 fresh Windows BuildReport/exe를 확인한다.
- **Max feedback latency:** task gate 5분 목표, wave gate 10분 목표, phase gate 30분 이내. 최초 실측값으로 갱신한다.

---

## Per-Task Verification Map

| Task ID | Plan | Wave | Requirement | Threat Ref | Secure Behavior | Test Type | Automated Command | File Exists | Status |
|---------|------|------|-------------|------------|-----------------|-----------|-------------------|-------------|--------|
| 02-W0-01 | TBD | 0 | RULE-01~03 | T-02-01, T-02-02 | 잘못된 패 입력과 mutable hash identity가 wall/equality 계약을 우회하지 못한다. | regression + unit | helper `Phase2RegressionTests` RED, 교정 뒤 GREEN | ❌ W0 | ⬜ pending |
| 02-W0-02 | TBD | 0 | RULE-04~09 | T-02-01, T-02-03 | malformed hand/context를 거절하고 역·부·지불 결과를 표 기반 oracle과 비교한다. | conformance + unit | helper `Phase2ConformanceTests` GREEN | ❌ W0 | ⬜ pending |
| 02-W0-03 | TBD | 0 | RULE-10 | T-02-04 | 솔로 adapter가 순수 결과의 winner income을 거리 서비스에 정확히 한 번 전달하고 save migration이 high score만 초기화한다. | integration EditMode | Phase 2 filters + `SoloSessionLifecycleTests` GREEN | ❌ W0 | ⬜ pending |
| 02-W0-04 | TBD | 0 | RULE-01~10 | T-02-05 | missing/empty XML, zero discovery, license/IPC 실패를 GREEN으로 오인하지 않는다. | evidence gate | 네 filtered helper run + XML name/count audit + Windows build | ❌ W0 | ⬜ pending |

*Planner는 실제 PLAN/task가 확정되면 `TBD`와 Wave 0 식별자를 실행 task ID로 치환한다.*

*Status: ⬜ pending · ✅ green · ❌ red · ⚠️ flaky*

---

## Wave 0 Requirements

- [ ] `Assets/Editor/Tests/Phase2RegressionTests.cs` — 139장 패산, 마지막 index 제외 shuffle, identity 불일치, 탐욕 분해 누락, dora-only, tsumo-as-ron 결함을 현행 공개 API에서 재현한다.
- [ ] `Assets/Editor/Tests/Phase2ConformanceTests.cs` — 일반형·치또이츠·국사무쌍, 역·도라·판·부·역만·지불과 mode-independent 결과를 출처·확인일이 있는 표 기반 사례로 검증한다.
- [ ] `.planning/phases/02-shared-rules-core/02-EVIDENCE.md` — RED/GREEN 대상 commit, filter, 정확한 test name/count, 예상/실제 결과와 원본 XML/log 경로를 기록한다.
- [ ] 공개 official-ranked 근거가 없는 Mahjong Soul 변형 규칙은 직접 관찰 또는 복수 2차 출처와 `[inference]` 표시를 fixture에 남긴다.
- [x] 기존 Unity Test Framework, global test helper, predefined editor assembly와 Windows build helper를 재사용한다.

---

## Manual-Only Verifications

| Behavior | Requirement | Why Manual | Test Instructions |
|----------|-------------|------------|-------------------|
| 공유 resolver가 판정한 일반 도라가 실제 손패에서 반짝이고 공개되지 않은 우라도라는 화료 전 표시되지 않는다. | RULE-07, RULE-10 | Unity sprite/material 상태와 공개 시점은 EditMode 결과만으로 관찰할 수 없다. | fresh Windows Player를 실행해 도라 표시가 있는 솔로 국을 시작하고, 표시패→도라 매핑과 손패 반짝임을 확인한다. 화료 전 우라도라 표시가 없음을 같은 PID에서 기록한다. |
| 결과 화면과 거리/부스트가 교정된 winner income을 한 번만 반영한다. | RULE-09, RULE-10 | 실제 TMP/UI 표시와 scene wiring은 자동 XML/build만으로 PASS할 수 없다. | known-score fixture와 일치하는 손을 Player에서 완료하고 결과 숫자 및 거리 변화를 캡처한다. 자동 integration 결과와 별도 사람 관찰로 기록한다. |

---

## Environment Blocker

Unity helper가 라이선스/IPC/lock/XML 부재를 보고하면 상태는 BLOCKED다. assertion-failure XML만 RED로 인정하고, parseable XML과 양수 selected count가 없는 실행은 RED/GREEN 어느 쪽으로도 기록하지 않는다.

---

## Validation Sign-Off

- [ ] All tasks have `<automated>` verify or Wave 0 dependencies
- [ ] Sampling continuity: no 3 consecutive tasks without automated verify
- [ ] Wave 0 covers all MISSING references
- [x] No watch-mode flags or ad-hoc `-quit` Test Runner command
- [ ] Feedback latency < 30 minutes confirmed by measurement
- [ ] `nyquist_compliant: true` set in frontmatter

**Approval:** pending
