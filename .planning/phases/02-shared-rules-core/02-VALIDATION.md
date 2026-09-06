---
phase: 02
slug: shared-rules-core
status: draft
nyquist_compliant: false
wave_0_complete: false
created: 2026-09-04
revised: 2026-09-06
plan_review: revised_with_open_initialization_decision
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
| **Full suite command** | Plan 02-07 Task 2의 단일 fail-closed PowerShell `<automated>` command를 그대로 실행한다. 기존 helper로 `Phase2RegressionTests`, `Phase2ConformanceTests`, `MahjongRoundTraceTests`, `SoloSessionLifecycleTests`를 각각 실행해 JSON `GREEN`, XML 존재/parse, 양수·고유 selected names, zero non-passed를 검사하고 trace=4/lifecycle=15를 exact assert한 뒤 Unity `2022.3.29f1`의 기존 `Phase1Build.BuildWindowsPlayer`와 `Phase1Build.BuildPhase2ObservationWindowsPlayer`를 각각 실행해 일반 `Builds/phase1` 및 관찰 `Builds/phase2-observation/build-report.txt`의 `Result: Succeeded`, `Errors: 0`과 `RiichiNya.exe` 존재 및 각 명령 시작 이후 작성된 report임을 assert한다. |
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
| 02-01-T1/T2 | 02-01 | 1 | RULE-01~03 | T-02-01~03 | 실제 solo start의 공유 136장 wall, TileID equality, 양방향 인덱스 제거·원본 적색 보존·적색 위치 시퀀스 검증을 수행하고 RED/GREEN은 ==/Equals 불일치를 대상으로 한다. | tracer regression + conformance | `Phase2RegressionTests` RED→GREEN, `Phase2ConformanceTests` GREEN | ❌ plan creates | ⬜ pending |
| 02-02-T1/T2 | 02-02 | 2 | RULE-04~05 | T-02-04~06 | malformed hand/meld를 거절하고 exhaustive candidates와 income/Han/Fu/stable selection을 검증한다. | regression + unit | 두 Phase 2 fixture GREEN | ❌ plan expands | ⬜ pending |
| 02-03-T1/T2 | 02-03 | 3 | RULE-06~07 | T-02-07~09 | contradictory context와 dora-only를 거절하고 fixed yaku/yakuman catalog를 source-dated rows로 검증한다. | regression + parameterized conformance | 두 Phase 2 fixture GREEN | ❌ plan expands | ⬜ pending |
| 02-04-T1/T2 | 02-04 | 4 | RULE-03/05/08/09 | T-02-10~12 | winning attribution별 fu, result equality와 income selector, four payment shapes/zero-sum deltas를 계산 코어 literal oracle과 비교한다. 실제 솔로 지급 RED/GREEN은 02-05 소유다. | regression + parameterized conformance | 두 Phase 2 fixture GREEN | ❌ plan expands | ⬜ pending |
| 02-05-T1/T2 | 02-05 | 5 | RULE-10 | T-02-13~15 | SOL-PAY-01 실제 지급 assertion RED→동일 GREEN과 player delta/거리 service 1500 한 호출을 검증한다. 남은 패 1→0 no-end, 0의 Haitei 화료 또는 타패 후 유국, 친/국풍을 검증한다. P2-OPEN-01은 초기화 구현 전 확인한다. PLAYER-OBS-01~05 setup은 개발 guard에 한정한다. | integration EditMode | SOL-PAY-01 named RED→GREEN; 두 Phase 2 fixture + Phase 1 fixtures GREEN; PLAYER-OBS-01=6판20부/12000/distance80.00/boost6 유지 | ❌ plan expands | ⬜ pending |
| 02-06-T1/T2 | 02-06 | 6 | RULE-10 | T-02-16~18 | score-version mismatch가 high score만 초기화하고 durable test scope가 exact prior save를 복구한다. | persistence integration | `Phase2ConformanceTests` + exact `SoloSessionLifecycleTests` 15 GREEN | ❌ plan creates | ⬜ pending |
| 02-07-T1/T2 | 02-07 | 7 | RULE-07/10 | T-02-19~21 | view는 shared resolver를 사용하고 raw hidden ura 존재하에서도 유효 리치 화료만 공개하며(0판 포함) 새 국·비리치 결과를 초기화한다. 자동 evidence는 네 fixture XML/count와 observation BuildReport/exe를 모두 요구한다. | presentation EditMode + evidence gate | Plan 02-07 Task 2 exact command: Phase 2 fixtures positive, trace exact 4, lifecycle exact 15, fresh normal + development Windows builds Succeeded/Errors 0/exe exists | ❌ plan expands | ⬜ pending |
| 02-08-T1/T2 | 02-08 | 8 | RULE-07/09/10 | T-02-22~23 | same-PID PLAYER-OBS-01~05 각 항목의 직접 관찰만 GUI PASS이고 final ledger가 39 edges/7 prohibitions/source coverage를 보존한다. | human checkpoint + final audit | Regression GREEN backstop + PLAYER-OBS-01 기존 5항목 + 아래 PLAYER-OBS-02~05의 명시 관찰 | ✅ prior plans | ⬜ pending |

*Status: ⬜ pending · ✅ green · ❌ red · ⚠️ flaky*

---

## Wave 0 Requirements

- [ ] Plan 02-01 owns creation of `Phase2RegressionTests.cs`, `Phase2ConformanceTests.cs`, and `02-EVIDENCE.md`; each later plan expands the same fixtures only for its owned behavior.
- [ ] Plans 02-01, 02-02, 02-03, 02-05 (SOL-PAY-01), and 02-06 each create an assertion-failure RED commit for the still-current public defect before their production correction; compile/license/missing XML is not RED.
- [ ] `Phase2ConformanceTests.cs` owns literal source-dated wall, decomposition, yaku/dora/yakuman, fu/payment, mode adapter, solo progression, save migration, and presentation rows without generating expected values from production code.
- [ ] Plan 02-06 extracts `DurableSaveTestScope` only when the second real-save fixture needs it and preserves exact Phase 1 lifecycle 15 names.
- [ ] Plan 02-07 owns the four-fixture/fresh normal + development observation-build automated phase gate and leaves human fields `NOT OBSERVED`; Plan 02-08 alone owns D-31 same-PID `PLAYER-OBS-01~05` approval and final ledger seal.
- [ ] Mahjong Soul rules without official ranked documentation use direct observation or at least two secondary sources plus non-empty `[inference]`, `CheckedOn`, and `Authority` fields.
- [x] 기존 Unity Test Framework, global test helper, predefined editor assembly와 Windows build helper를 재사용한다.

---

## Manual-Only Verifications

| Behavior | Requirement | Why Manual | Test Instructions |
|----------|-------------|------------|-------------------|
| 공유 resolver가 판정한 공개 도라·적도라가 반짝이고 우라 정보는 유효 리치 화료에만 공개된다. | RULE-07, RULE-10 | Unity sprite/material 상태와 공개 시점은 EditMode 결과만으로 관찰할 수 없다. | `Builds/phase2-observation/RiichiNya.exe --phase2-observation`을 실행해 PID를 기록하고 솔로 시작을 한 번 누른다. `PLAYER-OBS-01` hand `1m2m3m4m0m6m2p3p4p2s3s5p5p`, drawn `4s`, visible indicator `2p`, hidden ura indicator `3s`에서 3p/0m만 반짝이고 4s는 화료 전 반짝이지 않는지 같은 PID에서 기록한다. |
| 결과 화면과 거리/부스트가 교정된 winner income을 한 번만 반영한다. | RULE-09, RULE-10 | 실제 TMP/UI 표시와 scene wiring은 자동 XML/build만으로 PASS할 수 없다. | 같은 PID/상태에서 Space를 정확히 한 번 누른다. `Pinfu+Riichi+MenzenTsumo+Dora1+AkaDora1+UraDora1`, 6판20부, 비친 쯔모 6000/3000/3000, WinnerIncome 12000, distance 80.00, boost 6이 한 번 표시되는지 기록한다. |

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

**Approval:** pending (문서 정합성 검토와 미래 Unity/Player 검증 완료는 별개; P2-OPEN-01 미결)

## P2-CR-01~04 추가 검증 계약 — 2026-09-06

| 요청 | 담당 plan/task | 필수 판정 조건 |
|---|---|---|
| P2-CR-01 | 02-01 T1/T2, 02-02 T1, 02-03 T1, 02-07 T1 | TileID 기본 ==/!=/typed/object Equals/hash/CompareTo 일치; default/invalid 법칙; 실제 타패·리치 후보 인덱스 제거 양방향; 적5 머리/커쯔/순쯔/치또이츠/대기; 원본 손패/화료패/meld 적색 보존; 136/종류별4/적5각1; explicit (TileID,isAkaDora) 시퀀스와 적색만 바꾼 순서 거부; resolver 원본 필드 비변이 검사. |
| P2-CR-02 | 02-04 T1/T2 → 02-05 T1 | 02-04 계산 GREEN 뒤 02-05에서 SOL-PAY-01의 실제 player delta/service 금액·횟수 assertion RED와 동일 입력·기대값·이름 GREEN을 모두 보존. skip/ignore/filter 은폐 금지; 기존 ron 필드 의미 유지. |
| P2-CR-03 | 02-03 T1 → 02-05 T2 | 남은 패 1에서 타패+마지막 패 취득 후 0/end0회; 0에서 유효 화료는 Haitei+score1회, 타패는 extra draw0회/end1회/next1회; 양수에는 Haitei 없음; 실제 wall이 남는 solo와 synthetic four-player caller의 live-wall 마지막 패/비마지막 패 맥락 분리; 마지막 타패 후 tenpai/noten·친 진행; 음수/중복 종료/stale Haitei 거부. |
| P2-CR-04 | 02-03 T1 → 02-05 T1 → 02-07 T1 → 02-08 T1 | raw hidden ura가 존재해도 리치 화료 전/비리치 화료/리치 유국·오화료 비공개·미적용; 유효 리치 화료만 공개(ura0 포함); 이전 공개→새 국→비리치 결과 잔류 없음; 자동 presentation 검사와 실제 GUI를 별도 기록. |

**SOL-PAY-01:** 02-05 T1의 literal 비리치 Pinfu+MenzenTsumo 2판20부를 실제 solo 경로로 평가한다. 02-04의 완성된 계산 코어는 payer 700/400/400, WinnerIncome 1500을 반환하고, 수정 전 호출부가 사용하는 같은 han/fu의 zaRon은 1300이다. PLAYER-OBS-01의 12000/12000 사례로 이 결함을 검출했다고 기록하지 않는다. RED/GREEN 각각 XML, exact test names, positive selected count, commit, raw log를 연결한다. Phase 2 case 수는 구현 시 정하고 이동·추가한 이름/필터/기대수/최종 coverage를 함께 갱신한다. Phase 1은 계속 exact 4+15다.

**P2-OPEN-01:** 18이 첫 쯔모 포함 총량인지, 첫 쯔모 이후 남은 패 수인지 확인 전에는 초기화/표시/총량의 literal expected를 확정하지 않는다. 02-05 T2의 해당 구현은 사용자 결정과 기록 갱신을 요구한다. 이 항목이 열린 채 Phase 2 완료로 표시하지 않는다.

### PLAYER-OBS-01~05 고정 사례와 실제 화면 관찰

계획된 개발 전용 entry는 같은 PID에서 시작 버튼 1회로 01을 로드하고 F6으로 02→03→04→05를 순차 setup한다. 각 전환은 기존 round 교체/UI refresh를 사용하며 entry가 우라 표시를 직접 지워 테스트를 통과시키지 않는다. 새 사례마다 score/distance의 기존 초기화 경로를 사용한다. 관찰용 Space/Q는 메모리에서 쯔모/쯔모기리로 고정하고 사용자 저장 설정은 바꾸지 않는다. raw hidden ura는 fixture 입력으로 존재해야 하며 비공개 화면에는 나타나면 안 된다.

공통 A 손패는 `1m2m3m4m0m6m2p3p4p2s3s5p5p`, 쯔모패 `4s`, visible indicator `2p`, 비친/동장/멘젠 쯔모다. 명시하지 않은 일발·더블리치·특수 화료는 없다. 사례 01~03의 남은 패는 1로 고정하여 Haitei가 붙지 않게 한다. 이 말단 setup은 18 초기화 의미를 확정하지 않는다.

| Case ID | 고정 setup | 실제 동작 | 관찰할 결과 |
|---|---|---|---|
| PLAYER-OBS-01 | A, Riichi, raw ura `3s` | 솔로 시작 후 Space 1회 | 기존 5항목 유지: 화료 전 3p 공개 도라/0m 적도라 강조, 4s 우라 강조 없음; 화료 후 우라 표시패 3s 공개, UraDora1/6판20부/12000; distance80.00/boost6 1회. |
| PLAYER-OBS-02 | A, NoRiichi, raw ura `3s` 존재 | 01 공개 화면에서 F6 1회로 새 국, Space 1회 | 새 국에서 이전 우라 표시 제거. 비리치 화료 후도 우라 표시패/4s 우라 강조/우라 판수 정보 없음. Pinfu+MenzenTsumo+Dora1+Aka1, 4판20부/5200; 우라 미적용. |
| PLAYER-OBS-03 | A, Riichi, raw ura `6z`(대상 7z 없음) | F6 1회, Space 1회 | 화료 전 우라 비공개. 유효 리치 화료 후 우라 표시패 6z 공개, 우라 보너스 0/우라 유래 강조 없음, 5판20부/8000. 공개 자격을 양수 판수에 의존하지 않음. |
| PLAYER-OBS-04 | A, Riichi, raw ura `3s`, 남은 패 0의 마지막 일반 쯔모패 | 03 공개 화면에서 F6 1회, Q 1회 | 진입 시 이전 우라 표시 제거, Q 전 자동 화료/유국 없음. 마지막 타패 뒤 추가 쯔모 없이 유국·다음 국으로 각 1회 진행; 우라 표시패/강조/결과 정보는 끝까지 비공개. |
| PLAYER-OBS-05 | 손패 `1m2m3m4m0m6m2p3p4p2s3s5p5p`, 쯔모패 `9s`, Riichi, raw ura `8s`, visible `2p`, 남은 패 1 | F6 1회, Space 1회 | 불완전 손의 잘못된 쯔모. 우라 표시패/9s 우라 강조/결과 우라 정보 비공개, -8000 정책 후 다음 국 1회; 이전 공개 잔류 없음. 합성 invalid intent이며 리치 선언 구현이 아님. |

02-03은 맥락/역/우라 rows, 02-04는 해당 literal 지급값, 02-05는 실제 scorer 입력/전환 entry, 02-07은 공개 predicate와 표시 reset 및 개발 빌드, 02-08은 표의 모든 화면 결과를 소유한다. 01의 원래 수치는 변경하지 않는다. 자동 검증에서 raw list와 result/presentation 입력을 검사한 사실만으로 실제 화면의 공개/비공개를 PASS로 기록하지 않는다. 사례별 setup/input/outcome/PID와 사용자의 명시 관찰을 남긴다.
