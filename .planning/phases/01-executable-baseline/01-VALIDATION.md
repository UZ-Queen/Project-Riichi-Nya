---
phase: 01
slug: executable-baseline
status: draft
nyquist_compliant: false
wave_0_complete: false
created: 2026-08-29
---

# Phase 01 — Validation Strategy

> Phase 1 실행 중 빠른 회귀 확인과 최종 Windows Player 검증을 위한 계약이다.

---

## Test Infrastructure

| Property | Value |
|----------|-------|
| **Framework** | Unity Test Framework `1.1.33` + NUnit extension `1.0.6` |
| **Config file** | 없음 — `Assets/Editor/Tests/*.cs`가 predefined `Assembly-CSharp-Editor`로 컴파일되며 Phase 1은 `.asmdef`/`.asmref`를 추가하지 않고 `b18320e`의 descriptor를 보존한다. |
| **Quick run command** | `& "C:\Program Files\Unity\Hub\Editor\2022.3.29f1\Editor\Unity.exe" -batchmode -nographics -projectPath "$PWD" -runTests -testPlatform EditMode -testResults "$PWD\Temp\phase1\editmode.xml" -quit -logFile "$PWD\Temp\phase1\editmode.log"` |
| **Full suite command** | 위 EditMode 명령 실행 후 `Assets/Editor/Phase1Build.cs`의 Windows build method를 batchmode로 실행하고 `BuildReport` 결과를 확인한다. |
| **Estimated runtime** | Unity 라이선스가 정상일 때 측정 예정; 현재 LicensingClient IPC timeout으로 측정 불가 |

---

## Sampling Rate

- **After every task commit:** runtime `UnityEditor` 정적 scan과 가능한 경우 EditMode quick command 실행
- **After every plan wave:** 전체 EditMode XML 생성과 StandaloneWindows64 build 실행
- **Before `$gsd-verify-work`:** EditMode suite와 Player build가 성공하고 D-13 Windows Player 기본 동작이 관찰되어야 한다.
- **Max feedback latency:** Unity 라이선스 복구 후 첫 성공 실행에서 측정해 기록한다.

---

## Per-Task Verification Map

| Task ID | Plan | Wave | Requirement | Threat Ref | Secure Behavior | Test Type | Automated Command | File Exists | Status |
|---------|------|------|-------------|------------|-----------------|-----------|-------------------|-------------|--------|
| W0-01 | 01-01-T1 | 0 | BASE-01 | T-01-01 | conflicting tag refs halt instead of moving history | Git invariant | `git cat-file -t refs/tags/portfolio-baseline; git rev-parse refs/tags/portfolio-baseline^{}` | ❌ W0 | ⬜ pending |
| W0-02 | 01-01-T1 | 0 | BASE-02 | T-01-02 | trace loop is bounded, uses only D-03 fields, and accepts no external replay input | EditMode | Unity trace command; require 3 exact trace cases and zero failed/errors | ❌ W0 | ⬜ pending |
| W0-03 | 01-01-T1/T2; 01-03-T1 | 0 | BASE-03 | T-01-03, T-01-12 | descriptor baseline, runtime imports, fixed build paths, licensing/report/exe/PID state cannot fabricate PASS | static scan + build + process | compare every descriptor path/blob to `b18320e`, runtime import scan, Windows build method, BuildReport/exe/PID checks | ❌ W0 | ⬜ pending |
| W0-04 | 01-01-T1/T2; 01-02-T1/T2/T3; 01-03-T1 | 0 | BASE-04 | — | every XML gate requires exact discovery, zero non-Passed cases, and zero run-level failed/errors | batch integration | 3 trace at 01-01-T1, 3+2 initial at 01-01-T2, lifecycle 5→9→13 in 01-02, exact full 3+13=16 at 01-03-T1 | ❌ W0 | ⬜ pending |
| W0-05 | 01-01-T2/T3/T4; 01-02-T1/T2/T3; 01-03-T1/T2/T3 | 0 | BASE-05 | T-01-05 through T-01-12 | prior-session callbacks cannot mutate the replacement session; both tracer and terminal gates require named human APPROVED plus GUI PASS after truthful failure recording | EditMode + Windows Player | two initial lifecycle cases, expansion to 13, fail-closed tracer checkpoint 01-01-T3/T4, final checkpoint 01-03-T2, and fail-closed terminal ledger 01-03-T3 | ❌ W0 | ⬜ pending |

*Status: ⬜ pending · ✅ green · ❌ red · ⚠️ flaky*

---

## Wave 0 Requirements

- [ ] `01-01-T1` creates `Assets/Editor/Tests/MahjongRoundTraceTests.cs` in predefined `Assembly-CSharp-Editor`; the first Unity run must compile it and discover all three exact cases. The same task enumerates every `Assets/**/*.asmdef`/`Assets/**/*.asmref` and requires path/blob equality with `b18320e`.
- [ ] `01-01-T2` creates `Assets/Editor/Tests/SoloSessionLifecycleTests.cs` with `ConfirmForfeit_FinalizesOnceWithoutSavingHighScore` and `StartNewGame_Twice_DetachesAndResetsSession`; `01-02-T1/T2/T3` extends that fixture to the final thirteen exact cases.
- [ ] `01-02-T1`, `01-02-T2`, and `01-02-T3` own lifecycle expansion from 2→5→9→13, including the non-forfeit Processing Esc assertion and Editor scene-structure proof without adding a fourteenth case.
- [ ] `01-01-T1` creates `Assets/Editor/Phase1Build.cs`; `01-01-T2` builds/launches the tracer and `01-03-T1` owns the final full-suite StandaloneWindows64 BuildReport/exe/process gate.
- [ ] `01-01-T3` owns the walking-skeleton Player checkpoint and `01-01-T4` records the actual result before requiring APPROVED plus PASS; any other result exits nonzero and blocks 01-02 until repair/retry. `01-03-T2` owns final D-13 human verification and `01-03-T3` applies the same fail-closed rule while sealing the terminal ledger.
- [ ] Unity LicensingClient activation/IPC repair or a licensed Windows Unity host

---

## Manual-Only Verifications

| Behavior | Requirement | Why Manual | Test Instructions |
|----------|-------------|------------|-------------------|
| 같은 Windows Player 프로세스에서 솔로 시작 → `Esc` 확인 → 취소/재확인 → 포기 결과 → 메뉴 → 재시작 | BASE-05 | 실제 keyboard/UI 표시와 scene serialization을 관찰해야 하며 연구 단계에서 GUI 제어가 검증되지 않았다. | `01-01-T3`에서 tracer를 먼저 확인하고, 01-01-T4가 APPROVED+PASS일 때만 expansion을 시작한다. 모든 expansion 뒤 blocking-human `01-03-T2`에서 같은 PID로 D-13 전체를 다시 조작한다. `01-03-T3`는 그 결과와 16-case/build/exe/PID live evidence를 독립적으로 재검증한다. 두 gate 모두 거절·불확실·non-PASS를 먼저 기록한 뒤 nonzero로 중단하며, 원인을 고쳐 같은 경로를 재실행해야 한다. |

---

## Environment Blocker

현재 host의 Unity batch 실행은 LicensingClient IPC timeout으로 종료되어 XML과 build 결과를 만들지 못했다. 이는 소스 실패가 아니며, 라이선스 복구 또는 licensed Windows host 실행 전에는 BASE-03~05를 PASS로 기록하지 않는다.

---

## Validation Sign-Off

- [ ] All tasks have `<automated>` verify or Wave 0 dependencies
- [ ] Sampling continuity: no 3 consecutive tasks without automated verify
- [ ] Wave 0 covers all MISSING references
- [ ] No watch-mode flags
- [ ] Feedback latency measured after Unity licensing recovery
- [ ] `nyquist_compliant: true` set in frontmatter

**Approval:** pending
