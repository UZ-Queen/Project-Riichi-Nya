---
phase: 01
slug: executable-baseline
status: ready
nyquist_compliant: true
wave_0_complete: true
created: 2026-08-29
updated: 2026-09-02
---

# Phase 01 — Validation Strategy

> Phase 1 실행 중 빠른 회귀 확인과 최종 Windows Player 검증을 위한 계약이다.

---

## Test Infrastructure

| Property | Value |
|----------|-------|
| **Framework** | Unity Test Framework `1.1.33` + NUnit extension `1.0.6` |
| **Config file** | 없음 — `Assets/Editor/Tests/*.cs`가 predefined `Assembly-CSharp-Editor`로 컴파일되며 Phase 1은 `.asmdef`/`.asmref`를 추가하지 않고 `b18320e`의 descriptor를 보존한다. |
| **Quick run command** | `& 'C:\Users\user\.codex\skills\unity-test-gate\scripts\Invoke-UnityTests.ps1' -ProjectPath $PWD -TestPlatform EditMode -TestFilter SoloSessionLifecycleTests -ExpectedGate Green` |
| **Final project-test commands** | 같은 helper를 `-TestFilter MahjongRoundTraceTests`와 `-TestFilter SoloSessionLifecycleTests`로 각각 실행한다. 각 JSON `status`, `total`, `resultsPath`와 XML에서 trace 4개와 lifecycle 14개의 정확한 이름을 독립 검증한 뒤 결과를 `4 + 14 = 18`로 집계하고, `Phase1Build.BuildWindowsPlayer`를 별도 batch process로 실행한다. Live testable package의 자체 테스트가 포함되므로 unfiltered total은 Phase 1 project-test count로 사용하지 않는다. Test Runner와 build 모두 `-quit`를 전달하지 않는다. |
| **Estimated runtime** | Unity 라이선스가 정상일 때 측정 예정; 현재 LicensingClient IPC timeout으로 측정 불가 |

---

## Sampling Rate

- **After every task commit:** 해당 plan이 소유한 정확한 `SoloSessionLifecycleTests` 2-case subset을 helper JSON/XML로 확인한다.
- **After plans 01-02 through 01-04:** helper `status: GREEN`, `total: 2`, XML의 두 이름이 각각 한 번 발견되는지 확인한다. 구조/정책 assertion은 이 두 기존 case 안에서 이동시키며 최종 named expansion은 01-05가 소유한다.
- **At plan 01-05:** `MahjongRoundTraceTests` filtered XML에서 정확한 3개 이름을, 별도의 `SoloSessionLifecycleTests` filtered XML에서 정확한 13개 이름을 각각 한 번 확인하고 `3 + 13 = 16` project cases로 집계한 뒤 StandaloneWindows64 build와 visible Player PID gate를 실행한다. Unfiltered suite total은 live testable package 때문에 고정값으로 판정하지 않는다.
- **At plan 01-06:** 두 fixture를 다시 분리 실행해 trace 4개와 lifecycle 14개의 고유 이름 및 zero non-passed를 확인하고 `4 + 14 = 18`로 집계한다. 이 자동 실행은 Player GUI 재관찰로 간주하지 않는다.
- **Before `$gsd-verify-work`:** EditMode suite와 Player build가 성공하고 D-13 Windows Player 기본 동작이 관찰되어야 한다.
- **Max feedback latency:** Unity 라이선스 복구 후 첫 성공 실행에서 측정해 기록한다.

---

## Per-Task Verification Map

| Task ID | Plan | Wave | Requirement | Threat Ref | Secure Behavior | Test Type | Automated Command | File Exists | Status |
|---------|------|------|-------------|------------|-----------------|-----------|-------------------|-------------|--------|
| V-01 | 01-02-T1/T2 | 2 | BASE-05 | T-01-20~22 | committed view GUID, enum removal, separate forfeit intent, and same-frame return preserve the two approved lifecycle paths | filtered EditMode | helper GREEN + exact two XML names once | ✅ exists | ⬜ pending |
| V-02 | 01-03-T1/T2 | 3 | BASE-05 | T-01-23~26 | UI rename, modal policy ordering, native navigation, and overlay serialization remain covered without early final expansion | filtered EditMode | helper GREEN + exact two XML names once | ✅ exists | ⬜ pending |
| V-03 | 01-04-T1/T2/T3 | 4 | BASE-05 | T-01-27~29 | manager rename, zero legacy references after facade removal, and root subscription symmetry preserve the same two paths | static scan + filtered EditMode | `rg` after Task 2 migration; helper GREEN + exact two XML names once | ✅ exists | ⬜ pending |
| V-04 | 01-05-T1 | 5 | BASE-01~05 | T-01-30~33 | final evidence cannot infer success from exit/artifact presence | two filtered EditMode runs + build + process | trace helper GREEN + exact 3 XML names once; lifecycle helper GREEN + exact 13 XML names once; aggregate `3 + 13 = 16`; successful BuildReport/exe; visible launched window; recorded live PID | ✅ exists | ⬜ pending |
| V-05 | 01-05-T2/T3 | 5 | BASE-05 | T-01-31~33 | GUI PASS requires the same observed live process and explicit human approval | Windows Player + ledger | blocking eight-step same-PID checkpoint, then fail-closed ledger verification | ✅ exists | ⬜ pending |
| V-06 | 01-06-T1/T2/T3 | 6 | BASE-02/04/05 | - | `LengthDivergence_ReportsFirstMismatchState` keeps first-mismatch state; `TimeoutNewRecord_RendersAndPersistsUpdatedHighScore` renders and persists one record value; `RestartAfterLobby_UsesFreshStateAndSingleHandlers` resets the visible selection | two filtered EditMode runs | trace helper GREEN + exact 4 unique passed XML cases; lifecycle helper GREEN + exact 14 unique passed XML cases; aggregate `4 + 14 = 18` | ✅ exists | ✅ green |

*Status: ⬜ pending · ✅ green · ❌ red · ⚠️ flaky*

---

## Wave 0 Requirements

- [x] 01-01 created `MahjongRoundTraceTests.cs`, `SoloSessionLifecycleTests.cs`, `Phase1Build.cs`, and the XML-backed helper contract in predefined `Assembly-CSharp-Editor` without new descriptors.
- [x] 01-01 completed the initial two lifecycle cases and fail-closed walking-skeleton GUI evidence; 01-02 depends on its committed SUMMARY instead of replaying it per D-28.
- [x] Plans 01-02 through 01-04 retain an automated helper gate after every task and own the fixture whenever TDD assertions change.
- [x] Plan 01-05 exclusively owns the named expansion from two to thirteen lifecycle cases, exact filtered 3-case trace plus filtered 13-case lifecycle discovery, final `3 + 13 = 16` aggregate evidence, build/visible-window/live-PID gate, and the blocking human GUI checkpoint per D-27.
- [x] Plan 01-06 adds one trace and one lifecycle regression, strengthens the existing restart case, and records the exact filtered `4 + 14 = 18` aggregate without adding descriptors or packages.
- [ ] If the helper classifies licensing or IPC as BLOCKED, use the skill's visible Unity Hub checkpoint and rerun the identical command; never classify missing XML as RED or GREEN.

---

## Manual-Only Verifications

| Behavior | Requirement | Why Manual | Test Instructions |
|----------|-------------|------------|-------------------|
| 같은 Windows Player 프로세스에서 비기본 타일 선택 → `Esc` 확인 → 취소/재확인 → 포기 결과 → 메뉴 → 재시작 후 tile 6만 강조 | BASE-05 | 실제 keyboard/UI 표시와 scene serialization은 자동 XML/build만으로 관찰할 수 없다. | End-of-phase verification은 fresh Player에서 D-13 same-PID 경로를 반복한다. 실제 180초 timeout은 D-13에서 제외하며, timeout record ordering은 `TimeoutNewRecord_RendersAndPersistsUpdatedHighScore`의 XML-backed 값 회귀로만 검증한다. |

---

## Environment Blocker

Unity helper가 라이선스/IPC/lock/XML 부재를 보고하면 상태는 BLOCKED다. helper가 parseable XML과 GREEN을 반환하고 build/process gate가 성공하기 전에는 BASE-03~05를 PASS로 기록하지 않는다.

---

## Validation Sign-Off

- [x] All implementation tasks have `<automated>` verification; the only non-automated task is the explicit final blocking GUI checkpoint.
- [x] Sampling continuity: every task before the checkpoint has an automated gate.
- [x] Wave 0 infrastructure exists from completed 01-01 and every changed TDD fixture is owned by its task.
- [x] No watch-mode flags or ad-hoc `-quit` Test Runner command.
- [ ] Feedback latency measured after Unity licensing recovery
- [x] `nyquist_compliant: true` set in frontmatter

**Approval:** pending
