---
phase: 01
slug: executable-baseline
status: ready
nyquist_compliant: true
wave_0_complete: true
created: 2026-08-29
updated: 2026-09-01
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
| **Full suite command** | 같은 helper를 filter 없이 실행해 JSON `status`, `total`, `resultsPath`와 XML의 정확한 3 trace + 13 lifecycle 이름을 검증한 뒤 `Phase1Build.BuildWindowsPlayer`를 별도 batch process로 실행한다. Test Runner와 build 모두 `-quit`를 전달하지 않는다. |
| **Estimated runtime** | Unity 라이선스가 정상일 때 측정 예정; 현재 LicensingClient IPC timeout으로 측정 불가 |

---

## Sampling Rate

- **After every task commit:** 해당 plan이 소유한 정확한 `SoloSessionLifecycleTests` 2-case subset을 helper JSON/XML로 확인한다.
- **After plans 01-02 through 01-04:** helper `status: GREEN`, `total: 2`, XML의 두 이름이 각각 한 번 발견되는지 확인한다. 구조/정책 assertion은 이 두 기존 case 안에서 이동시키며 최종 named expansion은 01-05가 소유한다.
- **At plan 01-05:** 전체 EditMode XML에서 3 trace + 13 lifecycle, 총 16개 정확한 이름이 각각 한 번 발견되는지 확인한 뒤 StandaloneWindows64 build와 visible Player PID gate를 실행한다.
- **Before `$gsd-verify-work`:** EditMode suite와 Player build가 성공하고 D-13 Windows Player 기본 동작이 관찰되어야 한다.
- **Max feedback latency:** Unity 라이선스 복구 후 첫 성공 실행에서 측정해 기록한다.

---

## Per-Task Verification Map

| Task ID | Plan | Wave | Requirement | Threat Ref | Secure Behavior | Test Type | Automated Command | File Exists | Status |
|---------|------|------|-------------|------------|-----------------|-----------|-------------------|-------------|--------|
| V-01 | 01-02-T1/T2 | 2 | BASE-05 | T-01-20~22 | committed view GUID, enum removal, separate forfeit intent, and same-frame return preserve the two approved lifecycle paths | filtered EditMode | helper GREEN + exact two XML names once | ✅ exists | ⬜ pending |
| V-02 | 01-03-T1/T2 | 3 | BASE-05 | T-01-23~26 | UI rename, modal policy ordering, native navigation, and overlay serialization remain covered without early final expansion | filtered EditMode | helper GREEN + exact two XML names once | ✅ exists | ⬜ pending |
| V-03 | 01-04-T1/T2/T3 | 4 | BASE-05 | T-01-27~29 | manager rename, zero legacy references after facade removal, and root subscription symmetry preserve the same two paths | static scan + filtered EditMode | `rg` after Task 2 migration; helper GREEN + exact two XML names once | ✅ exists | ⬜ pending |
| V-04 | 01-05-T1 | 5 | BASE-01~05 | T-01-30~33 | final evidence cannot infer success from exit/artifact presence | full EditMode + build + process | helper GREEN + exact 16 XML names once; successful BuildReport/exe; visible launched window; recorded live PID | ✅ exists | ⬜ pending |
| V-05 | 01-05-T2/T3 | 5 | BASE-05 | T-01-31~33 | GUI PASS requires the same observed live process and explicit human approval | Windows Player + ledger | blocking eight-step same-PID checkpoint, then fail-closed ledger verification | ✅ exists | ⬜ pending |

*Status: ⬜ pending · ✅ green · ❌ red · ⚠️ flaky*

---

## Wave 0 Requirements

- [x] 01-01 created `MahjongRoundTraceTests.cs`, `SoloSessionLifecycleTests.cs`, `Phase1Build.cs`, and the XML-backed helper contract in predefined `Assembly-CSharp-Editor` without new descriptors.
- [x] 01-01 completed the initial two lifecycle cases and fail-closed walking-skeleton GUI evidence; 01-02 depends on its committed SUMMARY instead of replaying it per D-28.
- [x] Plans 01-02 through 01-04 retain an automated helper gate after every task and own the fixture whenever TDD assertions change.
- [x] Plan 01-05 exclusively owns the named expansion from two to thirteen lifecycle cases, exact combined 16-case discovery, final build/visible-window/live-PID gate, and the blocking human GUI checkpoint per D-27.
- [ ] If the helper classifies licensing or IPC as BLOCKED, use the skill's visible Unity Hub checkpoint and rerun the identical command; never classify missing XML as RED or GREEN.

---

## Manual-Only Verifications

| Behavior | Requirement | Why Manual | Test Instructions |
|----------|-------------|------------|-------------------|
| 같은 Windows Player 프로세스에서 솔로 시작 → `Esc` 확인 → 취소/재확인 → 포기 결과 → 메뉴 → 재시작 | BASE-05 | 실제 keyboard/UI 표시와 scene serialization은 자동 XML/build만으로 관찰할 수 없다. | 01-05-T1이 exact 16-case XML, build, visible window, live `Temp/phase1/player.pid`를 먼저 증명한다. 이어 01-05-T2가 그 PID로 D-13 여덟 단계를 blocking-human 확인하고, 01-05-T3가 실제 응답을 ledger에 fail-closed 기록한다. |

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
