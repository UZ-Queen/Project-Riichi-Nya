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
| 02-01-T1/T2 | 02-01 | 1 | RULE-01~03 | T-02-01~03 | 실제 solo start가 공유 136장 wall과 stable tile identity를 사용하고 RED/GREEN evidence가 fail-closed다. | tracer regression + conformance | `Phase2RegressionTests` RED→GREEN, `Phase2ConformanceTests` GREEN | ❌ plan creates | ⬜ pending |
| 02-02-T1/T2 | 02-02 | 2 | RULE-04~05 | T-02-04~06 | malformed hand/meld를 거절하고 exhaustive candidates와 income/Han/Fu/stable selection을 검증한다. | regression + unit | 두 Phase 2 fixture GREEN | ❌ plan expands | ⬜ pending |
| 02-03-T1/T2 | 02-03 | 3 | RULE-06~07 | T-02-07~09 | contradictory context와 dora-only를 거절하고 fixed yaku/yakuman catalog를 source-dated rows로 검증한다. | regression + parameterized conformance | 두 Phase 2 fixture GREEN | ❌ plan expands | ⬜ pending |
| 02-04-T1/T2 | 02-04 | 4 | RULE-08~09 | T-02-10~12 | winning attribution별 fu와 four payment shapes/zero-sum deltas를 literal oracle과 비교한다. | regression + parameterized conformance | 두 Phase 2 fixture GREEN | ❌ plan expands | ⬜ pending |
| 02-05-T1/T2 | 02-05 | 5 | RULE-10 | T-02-13~15 | solo adapter가 winner income을 한 번 전달하고 18-draw/dealer/wind progression을 적용한다. | integration EditMode | 두 Phase 2 fixture + Phase 1 fixtures GREEN | ❌ plan expands | ⬜ pending |
| 02-06-T1/T2 | 02-06 | 6 | RULE-10 | T-02-16~18 | score-version mismatch가 high score만 초기화하고 durable test scope가 exact prior save를 복구한다. | persistence integration | `Phase2ConformanceTests` + exact `SoloSessionLifecycleTests` 15 GREEN | ❌ plan creates | ⬜ pending |
| 02-07-T1/T2 | 02-07 | 7 | RULE-07/10 | T-02-19~21 | view는 shared resolver 결과만 표시하고 automated evidence는 XML/count/BuildReport를 모두 요구한다. | presentation EditMode + evidence gate | 네 filtered fixtures + fresh Windows build | ❌ plan expands | ⬜ pending |
| 02-08-T1/T2 | 02-08 | 8 | RULE-07/09/10 | T-02-22~23 | same-PID explicit observation만 GUI PASS이고 final ledger가 39 edges/7 prohibitions/source coverage를 보존한다. | human checkpoint + final audit | Regression GREEN backstop + five explicit human observations | ✅ prior plans | ⬜ pending |

*Status: ⬜ pending · ✅ green · ❌ red · ⚠️ flaky*

---

## Wave 0 Requirements

- [ ] Plan 02-01 owns creation of `Phase2RegressionTests.cs`, `Phase2ConformanceTests.cs`, and `02-EVIDENCE.md`; each later plan expands the same fixtures only for its owned behavior.
- [ ] Plans 02-01, 02-02, 02-03, 02-04, and 02-06 each create an assertion-failure RED commit for the still-current public defect before their production correction; compile/license/missing XML is not RED.
- [ ] `Phase2ConformanceTests.cs` owns literal source-dated wall, decomposition, yaku/dora/yakuman, fu/payment, mode adapter, solo progression, save migration, and presentation rows without generating expected values from production code.
- [ ] Plan 02-06 extracts `DurableSaveTestScope` only when the second real-save fixture needs it and preserves exact Phase 1 lifecycle 15 names.
- [ ] Plan 02-07 owns the four-fixture/fresh-build automated phase gate and leaves human fields `NOT OBSERVED`; Plan 02-08 alone owns D-31 same-PID approval and final ledger seal.
- [ ] Mahjong Soul rules without official ranked documentation use direct observation or at least two secondary sources plus non-empty `[inference]`, `CheckedOn`, and `Authority` fields.
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

- [x] All planned tasks have an `<automated>` verify; Plan 02-08 additionally has the explicit human checkpoint.
- [x] Sampling continuity: every task carries a filtered Unity gate.
- [x] Wave 0 ownership is assigned to concrete plan/task IDs for every missing fixture/evidence/helper.
- [x] No watch-mode flags or ad-hoc `-quit` Test Runner command
- [ ] Feedback latency < 30 minutes confirmed by measurement
- [ ] `nyquist_compliant: true` set in frontmatter

**Approval:** pending
