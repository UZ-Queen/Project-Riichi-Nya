# Walking Skeleton — Project Riichi Nya

**Phase:** 1
**Generated:** 2026-08-29

## Capability Proven End-to-End

플레이어는 기존 단일 씬 Windows Player에서 솔로 게임을 시작하고 포기 결과를 거쳐 같은 프로세스에서 다시 시작할 수 있으며, 개발자는 같은 소스를 고정 시드 EditMode 테스트와 재현 가능한 Windows 빌드 명령으로 검증할 수 있다.

## Architectural Decisions

| Decision | Choice | Rationale |
|---|---|---|
| Runtime framework | Unity `2022.3.29f1` + C# 9 + existing `Assembly-CSharp` | 프로젝트가 이 버전과 전역 namespace 구조에 고정되어 있고, Phase 1은 assembly migration을 수행하지 않는다. |
| Composition root | `Assets/Scenes/SampleScene.unity` single scene | 기존 manager, UI, timer, score, asset wiring을 보존하는 실제 Player 진입점이다. |
| Domain path | `MahjongRound.NewRound(int, out MahjongPlayer)` + public events | 테스트 전용 고정 seed를 주입하면서 production의 새 난수 시작 경로를 바꾸지 않는다. |
| Test path | `Assets/Editor/Tests/` in predefined `Assembly-CSharp-Editor` | asmdef에서 predefined `Assembly-CSharp`를 참조하지 않고 기존 runtime assembly를 직접 사용할 수 있는 legacy Editor 경계를 선택한다. Phase 1은 `b18320e`의 모든 `Assets/**/*.asmdef`/`Assets/**/*.asmref` path/blob을 보존하고 정확한 test discovery를 XML로 증명한다. |
| Build target | `BuildPipeline.BuildPlayer` → `StandaloneWindows64` at `Builds/phase1/RiichiNya.exe` | Editor 전용 build entry point가 실제 Player compile 경계를 검증한다. |
| Local data | Existing JSON, PlayerPrefs, and ScriptableObject assets | Phase 1은 저장 구조나 데이터 계층을 교체하지 않는다. |
| Authentication | None | 네트워크·계정·외부 API가 없는 로컬 싱글플레이다. |
| Evidence boundary | One committed `01-BASELINE.md`; raw XML/log/report/build under ignored local paths | D-10과 D-11에 따라 사람이 읽는 요약과 실행 산출물을 분리한다. |

## Stack Touched in Phase 1

- [ ] Existing Unity project imports and compiles with the pinned Editor.
- [ ] `SampleScene.unity` starts the real solo route and returns to the existing main menu.
- [ ] A fixed-seed EditMode trace reaches the first exhaustive draw and the next round's first draw.
- [ ] A Windows x64 Player is produced by a documented batch command.
- [ ] The same Player process completes start → forfeit confirmation → result → menu → restart.

## Out of Scope (Deferred to Later Slices)

- Rule, yaku, han, fu, payment, wall-size, and shuffle correctness repairs (Phase 2).
- Complete solo riichi behavior and 180-second preservation evidence (Phase 3).
- Four-seat hanchan state, Unity hanchan UI, calls/kans, and portfolio release packaging (Phases 4-7).
- Replay UI, generic action recorder, generic pause/settings manager, new package, database, server, and deployment service.

## Subsequent Slice Plan

- Phase 2: 정확한 공유 규칙 코어와 표 기반 EditMode 검증.
- Phase 3: 독립 솔로 경험과 완전한 리치 보존.
- Phase 4: UI와 분리된 고정 기본 반장전 상태 머신.
- Phase 5: 기존 Unity 화면에 반장전 선택·플레이·결과 통합.
- Phase 6: Stage 2 gate 통과 시에만 사람 후로와 모든 깡 묶음.
- Phase 7: 최종 Windows 빌드, 태그, README, 시연, AI 개발 증거.
