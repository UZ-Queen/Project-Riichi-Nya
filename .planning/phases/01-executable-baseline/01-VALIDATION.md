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
| **Config file** | `Assets/Tests/EditMode/ProjectRiichiNya.EditModeTests.asmdef` — Wave 0에서 생성 |
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
| W0-01 | TBD | 0 | BASE-01 | — | N/A | Git invariant | `git cat-file -t refs/tags/portfolio-baseline; git rev-parse refs/tags/portfolio-baseline^{}` | ❌ W0 | ⬜ pending |
| W0-02 | TBD | 0 | BASE-02 | T-01 | trace loop is bounded and accepts no external replay input | EditMode | Unity EditMode quick command | ❌ W0 | ⬜ pending |
| W0-03 | TBD | 0 | BASE-03 | T-02 | build output path stays under `Builds/phase1` | static scan + build | `rg -n --glob '*.cs' 'using UnityEditor(\.|;)' Assets/Scripts` then Windows build method | ❌ W0 | ⬜ pending |
| W0-04 | TBD | 0 | BASE-04 | — | N/A | batch integration | Unity EditMode quick command; verify XML exists | ❌ W0 | ⬜ pending |
| W0-05 | TBD | 0 | BASE-05 | T-03 | prior-session callbacks cannot finalize or mutate the new session | EditMode + Windows Player | automated lifecycle tests plus D-13 manual checklist | ❌ W0 | ⬜ pending |

*Status: ⬜ pending · ✅ green · ❌ red · ⚠️ flaky*

---

## Wave 0 Requirements

- [ ] `Assets/Tests/EditMode/ProjectRiichiNya.EditModeTests.asmdef` — Editor-only test assembly
- [ ] `Assets/Tests/EditMode/MahjongRoundTraceTests.cs` — BASE-02 bounded seeded trace and first-mismatch diagnostics
- [ ] lifecycle-focused EditMode tests selected by the planner — BASE-05 reset, finalization priority, and subscription regression coverage
- [ ] `Assets/Editor/Phase1Build.cs` — fixed scene list, StandaloneWindows64 target, ignored output path, and failed `BuildReport` propagation
- [ ] Unity LicensingClient activation/IPC repair or a licensed Windows Unity host

---

## Manual-Only Verifications

| Behavior | Requirement | Why Manual | Test Instructions |
|----------|-------------|------------|-------------------|
| 같은 Windows Player 프로세스에서 솔로 시작 → `Esc` 확인 → 취소/재확인 → 포기 결과 → 메뉴 → 재시작 | BASE-05 | 실제 keyboard/UI 표시와 scene serialization을 관찰해야 하며 연구 단계에서 GUI 제어가 검증되지 않았다. | Player를 실행하고 D-13 순서대로 조작한다. 확인창 동안 타이머가 진행되는지, 포기 시 고득점이 갱신되지 않는지, 재시작 후 타이머·점수·거리·라운드·손패·하천·panel·event가 이전 session과 분리되는지 기록한다. 화면 판독이 불확실하면 PASS하지 않고 사용자 확인을 요청한다. |

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
