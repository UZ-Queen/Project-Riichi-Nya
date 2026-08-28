# Architecture Patterns

**Domain:** Brownfield Unity riichi-mahjong refactor and four-seat hanchan extension
**Project:** Project Riichi Nya
**Researched:** 2026-08-28
**Overall confidence:** HIGH for current-state findings; MEDIUM for target design until exercised by migration tests

## Recommended Architecture

Evolve the project in place. Keep `Assets/Scenes/SampleScene.unity` as the composition root and keep the existing solo presentation, timer, distance, audio, and persistence assets. Extract one pure C# mahjong assembly below the existing `Assets/Scripts/AL-1S/` ownership area, then make the current solo flow and the new four-player flow call that assembly through explicit commands and results.

```text
`Assets/Scenes/SampleScene.unity` — composition root, serialized references
                              |
                 `MahjongGameManager` — mode host only
                    /                         \
         `SoloGameSession`             `HanchanGameSession`
       timer/distance loop          match/round/action loop
                    \                         /
                     `ProjectRiichiNya.Mahjong`
               pure C# assembly, no UnityEngine reference
       ┌─────────────┬─────────────┬───────────────┐
       │ Rules       │ State       │ Transitions   │
       │ tile/wall   │ match       │ prompt/action │
       │ win/yaku/fu │ round/player│ result/settle │
       │ payment     │             │               │
       └─────────────┴─────────────┴───────────────┘
                              |
                EditMode NUnit table/transition tests
```

The dependency direction is one way: Unity presentation and sessions depend on the pure mahjong assembly; the pure assembly never references `MonoBehaviour`, `GameObject`, `UnityEngine.Input`, DOTween, TMPro, `ScriptableObject`, `Application.persistentDataPath`, or scene singletons. Unity 2022.3 supports an Assembly Definition with **No Engine References**, which omits automatic `UnityEngine` and `UnityEditor` references. Use that setting as a build-time guard after the migrated files no longer use Unity types ([Unity 2022.3 Assembly Definition properties](https://docs.unity3d.com/2022.3/Documentation/Manual/class-AssemblyDefinitionImporter.html)).

Do not create a second Unity runtime assembly, dependency-injection container, factory layer, event bus, or plugin system for this milestone. The default `Assembly-CSharp` can reference the extracted assembly while the scene-facing code is still moving. Unity's predefined assemblies reference Assembly Definition assemblies by default, so this is a supported incremental seam rather than a rewrite ([Unity 2022.3 Assembly definitions](https://docs.unity3d.com/2022.3/Documentation/Manual/ScriptCompilationAssemblyDefinitionFiles.html)).

### Current-to-Target Component Boundaries

| Current owner and evidence | Problem | Target owner | Migration rule |
|---|---|---|---|
| `Assets/Scripts/MahjongGameManager.cs` | Owns game lifecycle, random seed creation, current round/player, input gating, UI calls, score-distance, timer, save data, and event attachment. | Keep `MahjongGameManager` as the scene-owned mode host. Move solo progression to `SoloGameSession` and hanchan progression to `HanchanGameSession`; keep serialized Unity references and high-score persistence in the host/presenters. | First delegate existing methods without changing scene wiring. Delete responsibilities from the manager only after the delegated path has tests and the solo scene still runs. |
| `Assets/Scripts/AL-1S/MahjongRound.cs` | One class owns player, wall/dead wall, draw/discard, win checking, score mutation, next-round creation, and Unity fallbacks such as `Mathf.Clamp` and `UnityEngine.Random.Range`. | Pure `SoloRound` and `FourPlayerRound` progression types share `Wall`, `RoundPlayerState`, evaluators, actions, and results. `HanchanMatch` alone owns cross-round progression and scores. | Keep the old `MahjongRound` as a temporary solo adapter. Extract one responsibility at a time; do not make the new four-player flow inherit from it. |
| `Assets/Scripts/AL-1S/MahjongUtilities.cs`, `MahjongYaku.cs`, `MahjongWinInfo.cs` | Decomposition, yaku, fu, han, and payments are coupled through wide mutable inputs and currently import Unity/optional packages. | Pure concrete functions/types: `HandSolver`, `WinEvaluator`, `YakuEvaluator`, `FuCalculator`, and `PaymentCalculator`. Inputs are explicit snapshots; outputs are values. | Preserve the existing public result shape behind an adapter until table tests cover old and corrected behavior. No rule-service interfaces: there is one ruleset in scope. |
| `Assets/Scripts/AL-1S/MahjongTileAndBlock.cs`, `Utilities.cs` | Tile identity and wall shuffle are shared core concerns, but the files contain editor imports and unrelated Unity vector helpers. | `MahjongTile`/`MahjongBlock` value semantics, `Wall.Create(seed)`, and Fisher-Yates shuffle live in the pure assembly. UI/vector helpers remain outside it. | Split mixed files rather than allowing a `UnityEngine` reference into the rules assembly. Keep tile string codes stable because `MahjongTileDatabase` and the editor generator use them. |
| `Assets/Scripts/AL-1S/_Structs.cs` | Mixes round/tsumo/domain data with `RectTransform`, `CanvasGroup`, `HideInInspector`, and `Vector2` UI types. `MahjongRoundInfo.NextRound...` mutates and returns the same instance. | Move `GamePanelEntry`, `PanelEntry`, and `Wind.ToVector2` to `Assets/Scripts/UI-Kozeki/`. Replace mutable round info with pure match/round snapshots and fresh transition results. | This split is prerequisite to enabling **No Engine References**. Do not preserve the mixed file as a cross-layer dumping ground. |
| `Assets/Scripts/UI-Kozeki/PlayerHand.cs` | Polls Unity input, owns visual state, and emits index/call events directly to the global manager; lifecycle subscriptions are asymmetric. | Human `IActionSource` adapter converts current input into a `PlayerAction` selected from an `ActionPrompt`. `PlayerHand` renders snapshots and never mutates domain state. | Reuse the current keyboard and tile visuals. Pair enable/disable subscriptions and reject stale prompts by `PromptId`. |
| `Assets/Scripts/UI-Kozeki/GameUIManager.cs` and other `Ui...` presenters | UI visibility partly acts as a gameplay state gate and is driven by broad manager events. | Presenters map immutable session/round snapshots to panels. Gameplay legality comes only from the active round phase and legal-action list. | Keep DOTween and panel maps. Make initialization idempotent; presentation state must not authorize a domain action. |
| `Assets/Scripts/ScoreManagerDistance.cs`, `IScoreDistanceService.cs` | Valuable solo-specific score-to-distance loop is coupled to the current manager. | Remains a solo application service. It consumes the standard payment result or solo award total but is not part of hanchan settlement or the rules assembly. | Preserve it; do not generalize distance/boost into four-player match scoring. |
| `Assets/Scripts/Configs/SettingsManager.cs` | Loads/saves local JSON using Unity paths and currently stores the solo high score. | Remains Unity infrastructure invoked at session start/end. Hanchan domain objects do not save themselves. | Do not add match persistence in this milestone unless resume is explicitly required. |

### Domain Ownership

| Component | Owns | Must not own |
|---|---|---|
| `MahjongTile`, `MahjongBlock`, `Meld` | Stable tile identity, red-five metadata, comparison/hash semantics, block/meld values. | Sprites, GameObjects, dora UI state, mutable global flags. |
| `Wall` | Seeded 136-tile order, live wall, dead wall, rinshan tiles, dora/ura indicators, remaining-draw invariants. | Turn order, player score, UI animation, random fallback outside the supplied seed. |
| `WinEvaluator` pipeline | All complete decompositions, win context, yaku, han/yakuman, fu, and a standard payment table. | Applying score changes, choosing a winner, advancing rounds, formatting UI text. |
| `RoundPlayerState` | One seat's concealed hand, drawn tile, river, melds, riichi/ippatsu/furiten facts, and seat wind for the current round. | Long-lived hanchan score and scene/input objects. Solo owns one instance; four-player owns four. |
| `FourPlayerRound` | Wall, four `RoundPlayerState` values, dealer/turn seat, current `RoundPhase`, legal actions, response priority, and terminal `RoundResult`. | East/South match advancement, honba/stick carry-over, persistent score application, UI/persistence. |
| `HanchanMatch` | Four seat accounts and scores, round wind/number, dealer seat, honba, riichi-stick pool, current round, settlement application, next-round and match-end decision. | Tile rendering, human input, AI strategy, timer/distance rules. It is the only authority allowed to change hanchan scores. |
| `SettlementCalculator` | Pure `RoundResult + match counters -> PointTransfer[]` calculation, including dealer/non-dealer ron/tsumo, honba, riichi sticks, and exhaustive-draw payments in the implemented ruleset. | Mutating match/player objects. `HanchanMatch` applies the returned transfers and verifies the ledger invariant. |
| `SoloGameSession` | Existing timed loop, creation of the next solo hand, solo penalties/rewards, forwarding standard win payments to distance/boost, and session end. | Four-seat dealer rotation, hanchan end rules, or dummy-seat decisions. |
| `HanchanGameSession` | Drives automatic draws, requests an action from the active source, submits it to the match, publishes snapshots/results, and chains deterministic dummy turns. | Deciding legality or calculating payments. |

For hanchan, represent persistent and per-round player state separately. A match seat account keeps only seat identity and score across rounds; `RoundPlayerState` is recreated for each round. This prevents `FourPlayerRound` from silently mutating match scores while still allowing the same hand/river model to serve the solo player.

### Explicit State Transitions

`FourPlayerRound.Submit` is the only public mutation entry for player decisions. Every action carries the `PromptId` and actor seat. The round rejects an action when the prompt is stale, the actor is wrong, the action is absent from the legal-action list, or the phase does not allow it.

```text
FourPlayerRound

NotStarted
    -> Dealing
    -> AwaitingDraw
    -> AwaitingDiscard        (after automatic draw)
       -> AwaitingResponses   (after discard)
          -> AwaitingDraw     (no ron/call; advance seat)
          -> AwaitingDiscard  (accepted chii/pon in later call phase)
          -> AwaitingDraw     (kan replacement path, when implemented)
          -> Ended            (ron)
       -> Ended               (tsumo)
    -> Ended                  (live wall exhausted)
```

Phase 2 enables only the transitions needed for draw, discard, human tsumo, human ron, pass, and exhaustive draw. Phase 3 adds chii/pon/kan branches without changing the action entry point. Dummy seats receive only a legal tsumogiri action and never receive declaration actions in Phase 2/3.

```text
HanchanMatch

NotStarted
    -> PlayingRound
    -> ApplyingSettlement
       -> PlayingRound        (dealer repeat or next East/South round)
       -> Finished            (configured South 4 end condition reached)
```

`RoundResult` is terminal and descriptive: tsumo, ron, exhaustive draw, or a future abortive-draw result. It records winner(s), discard source, winning tile/context, tenpai seats, and declared riichi facts. It does not advance the match. `HanchanMatch` calculates/applies transfers, updates honba and riichi sticks, decides dealer continuation, then creates the next round. This is the boundary that prevents the current `MahjongRound.OnRoundEnds -> NextRound -> OnNewRoundStart` event cascade from hiding ownership (`Assets/Scripts/AL-1S/MahjongRound.cs`).

### Minimal Action Boundary

Create `PlayerAction` and `ActionPrompt` as pure data in the mahjong assembly, and one Unity/application interface only when four-player input is wired:

```csharp
public interface IActionSource
{
    void RequestAction(ActionPrompt prompt, Action<PlayerAction> submit);
    void CancelPendingAction();
}
```

The interface has two real implementations in scope:

- `HumanActionSource` adapts the existing `PlayerHand` keyboard/buttons and may respond later.
- `TsumogiriActionSource` immediately selects the legal discard marked as the drawn tile for each dummy seat.

Both receive an immutable `ActionPrompt` containing `PromptId`, actor seat, phase, an information-safe table/hand snapshot, and the exact legal actions. Both return the same `PlayerAction`; neither receives the mutable round object. `HanchanGameSession` cancels the previous source before a new prompt and the domain rejects a late callback by `PromptId`. A future AI can consume the same prompt and return the same action, but no AI interface, search tree, feature vector, or background-thread system is implemented now. Rename the automated implementation behind `IActionSource` only when a second automated policy actually exists.

This callback-shaped boundary handles both asynchronous human input and immediate deterministic decisions without polling the domain. Do not put legality inside `IActionSource`; legal actions are enumerated by `FourPlayerRound` and verified again on submission.

## End-to-End Data Flow

### Solo Mode

1. `UiManager` invokes the existing scene host. `MahjongGameManager` creates `SoloGameSession` with an explicit seed, existing timer, existing `ScoreManagerDistance`, and presenter callbacks (`Assets/Scripts/UI-Kozeki/UiManager.cs`, `Assets/Scripts/MahjongGameManager.cs`).
2. `SoloGameSession` asks pure `Wall.Create(seed)` for a valid wall, deals one `RoundPlayerState`, and receives a snapshot. No Unity object enters the wall or hand model.
3. The session calls the shared win/riichi query APIs and publishes an `ActionPrompt`. `HumanActionSource` maps the existing hand selection and call buttons to one of its legal `PlayerAction` values (`Assets/Scripts/UI-Kozeki/PlayerHand.cs`).
4. `SoloGameSession` submits the action to `SoloRound`. `SoloRound` validates phase/actor/prompt, updates hand/river/wall, and returns a transition with the next snapshot/prompt or a terminal solo-hand result.
5. On tsumo, the shared evaluation pipeline enumerates all winning decompositions, evaluates yaku/fu, and returns the highest legal `WinResult`. `PaymentCalculator` supplies the correct tsumo payments; the solo adapter converts the standard payment result to the existing reward/penalty input rather than reimplementing scoring.
6. `MahjongGameManager`/presenters render the returned snapshot, and `ScoreManagerDistance` applies the solo boost/distance effect. The next solo hand is created without mutating hanchan state.
7. When `Timer` ends or the player forfeits, the host stops the session, cancels pending input, unsubscribes once, finalizes distance, and calls `SettingsManager` to persist the high score. Rules code never accesses the clock, JSON path, or game-over panels.

During migration, the existing `MahjongRound` events may remain as a compatibility adapter around pure results. Remove an event only after the current solo presenter is driven by the new result and a start-play-finish-restart smoke test passes.

### Four-Player Hanchan Mode

1. The mode button asks `MahjongGameManager` to create `HanchanGameSession`; the session creates `HanchanMatch` with four 25,000-point seat accounts and one fixed rules configuration.
2. `HanchanMatch.StartRound(seed)` creates `FourPlayerRound` and four fresh `RoundPlayerState` values, constructs/deals the wall, assigns dealer/seat winds, and enters `AwaitingDraw`.
3. The session advances the automatic draw and obtains an `ActionPrompt`. If the active seat is human, it requests an action from `HumanActionSource`; otherwise it requests one from that seat's `TsumogiriActionSource`.
4. The returned action goes through the same `HanchanMatch.SubmitAction -> FourPlayerRound.Submit` path. The round validates it and returns a transition. Presentation receives a read-only table snapshot; hidden hands remain hidden.
5. After every discard, the round enters `AwaitingResponses` and checks legal ron against the shared `WinEvaluator`. In Phase 2, only the human seat can receive `Ron`/`Pass`; dummy seats never declare a win or call. If the human passes or cannot ron, the session advances to the next draw.
6. On human tsumo/ron or exhaustive draw, `FourPlayerRound` returns `RoundResult` and becomes `Ended`. No further action is accepted for that round.
7. `HanchanMatch` calls `SettlementCalculator`, applies every `PointTransfer`, updates riichi sticks/honba/dealer/round wind, and either creates the next round or returns `MatchResult` after the South 4 end rule. A settlement check asserts the total of four seat deltas plus the riichi-pot delta is zero.
8. `HanchanGameSession` publishes the match/round snapshot and settlement to four-seat presenters. It requests the next action only after settlement UI completes or is acknowledged; UI timing never changes domain legality.

## Patterns to Follow

### Pattern 1: Strangler Adapter Around the Existing Solo Flow

**What:** Extract pure calculations behind the methods already called by `MahjongRound`, then replace orchestration only after equivalent/corrected results are characterized.
**When:** Every migration step from `MahjongUtilities`, `MahjongYaku`, `MahjongWinInfo`, and `MahjongRound`.
**Why:** It keeps `SampleScene.unity`, current serialized references, and the distinctive solo loop runnable while each rule defect receives a focused regression test.

### Pattern 2: Command In, Transition Result Out

**What:** Mode sessions submit `PlayerAction`; round/match objects return accepted/rejected transitions, snapshots, next prompts, or terminal results.
**When:** Human input, dummy turns, win declarations, later calls, and tests.
**Why:** It gives one legal mutation path, makes replay from a seed/action list possible, and avoids a global event bus. Existing C# events remain only at the Unity session-to-presenter edge and must have one owner with symmetric attach/detach.

### Pattern 3: Pure Calculation Followed by Explicit Application

**What:** Evaluators and `SettlementCalculator` return values; `HanchanMatch` applies score/counter changes.
**When:** Win selection, yaku/fu/payment, honba, riichi sticks, and exhaustive draw.
**Why:** The same inputs can be table-tested in EditMode, while ownership of mutable state remains visible.

### Pattern 4: Seed and Action Trace as Reproduction Inputs

**What:** Every session records the seed and accepted action sequence in memory/debug output.
**When:** Solo regression reproduction, four-seat turn tests, and portfolio evidence.
**Why:** `MahjongGameManager.StartNewGame` currently discards the active seed by using `new System.Random()` (`Assets/Scripts/MahjongGameManager.cs`). A seed plus accepted actions is sufficient to reproduce rules without saving scene objects. Durable replay files are not required for this milestone.

### EditMode Test Boundary

Create one pure runtime assembly, for example `Assets/Scripts/AL-1S/Domain/ProjectRiichiNya.Mahjong.asmdef`, with **No Engine References** enabled, and one test assembly under `Assets/Tests/EditMode/` that references it. Keep tests as ordinary NUnit `[Test]` table cases unless a frame/yield is genuinely required. Unity Test Framework 1.1.33 documents that EditMode tests target Editor and that test assemblies explicitly reference the code assembly ([Unity Test Framework 1.1.33 Edit Mode vs. Play Mode tests](https://docs.unity3d.com/Packages/com.unity.test-framework@1.1/manual/edit-mode-vs-play-mode-tests.html)); this project already pins `com.unity.test-framework` 1.1.33 in `Packages/manifest.json`.

Test the pure assembly without loading `SampleScene.unity`:

- wall count/copy/red-five/dead-wall/seed invariants;
- exhaustive decomposition and best-win selection;
- yaku, yakuman multiplier, fu, ron/tsumo payment tables;
- action legality and stale/wrong-seat rejection for every round phase;
- dummy tsumogiri traces;
- settlement conservation, dealer continuation, honba/riichi sticks, East/South and South 4 termination.

Use a small PlayMode smoke suite only for scene wiring, first start, finish/forfeit, restart, human input presentation, and four-seat panel updates. Pure rules failures must not require PlayMode.

## Anti-Patterns to Avoid

### Inheriting Four-Player Round from the Current Solo `MahjongRound`

**What:** Add seats and flags to the existing class until it supports both modes.
**Why bad:** The current class already owns wall, one player, scoring, round advancement, and event dispatch. Boolean mode branches would preserve its hidden state transitions and put the solo experience at risk.
**Instead:** Keep a temporary solo adapter; share value/calculation code while `SoloRound` and `FourPlayerRound` remain separate progression types.

### Domain Events as the State Machine

**What:** Chain `OnPlayerWin`, `OnNewRoundStart`, UI callbacks, and delayed tweens to decide what happens next.
**Why bad:** Event order and subscription lifetime become part of game legality, as current restart/subscription defects show in `MahjongGameManager`, `PlayerHand`, and `GameUIManager`.
**Instead:** Return a transition/result from the authoritative state owner. Use events only to notify Unity presenters after the transition is complete.

### Rules Depending on Unity Convenience APIs

**What:** Keep `Mathf.Clamp`, `UnityEngine.Random`, `Debug`, `RectTransform`, or ScriptableObjects in the core because the project runs in Unity.
**Why bad:** It defeats **No Engine References**, permits nondeterministic fallbacks, and makes rule tests depend on Unity-facing code.
**Instead:** Use `System`, explicit validation, supplied `System.Random` seeds, and returned error/results. Keep sprite lookup and logging adapters outside.

### Generalized Rule/AI Plugin Architecture

**What:** Add rule-provider factories, service locators, configurable rule graphs, AI plugin interfaces, or a DI container.
**Why bad:** The milestone has one fixed ruleset, one human source, and deterministic tsumogiri opponents. Those systems create unused extension points and slow a nine-day delivery.
**Instead:** Concrete rules and sessions plus the single `IActionSource` boundary justified by two current implementations.

### Presentation Holding Mutable Domain Collections

**What:** Pass `List<MahjongTile>` or the active `Round` object directly to UI/AI code.
**Why bad:** A presenter can mutate authoritative state, hidden information can leak, and late callbacks can act on the wrong turn.
**Instead:** Publish immutable/read-only snapshots and prompt-scoped legal action values.

## Suggested Migration and Build Order

1. **Characterize and stabilize the existing executable path.** Remove runtime `UnityEditor` imports, fix restart/event lifecycle and idempotent panel initialization, add one PlayMode start-finish-restart smoke test, and record deterministic seeds. This gives a trustworthy solo safety net before structural movement.
2. **Extract the pure rules assembly behind existing APIs.** Split UI types out of `_Structs.cs`; isolate tile identity, wall construction/shuffle, decomposition, yaku, fu, and payment; enable **No Engine References**; add EditMode tables for every corrected critical defect. Keep `MahjongRound` and `MahjongGameManager` calling adapters so the scene remains playable after every commit.
3. **Move the solo path onto explicit pure results.** Introduce `RoundPlayerState`, `SoloRound`, action/prompt values, and `SoloGameSession`; preserve timer, score-distance, panels, and save behavior. Remove old `MahjongRound` responsibilities only when solo equivalence and corrected payments pass.
4. **Build hanchan headlessly before its UI.** Add `FourPlayerRound`, `RoundPhase`, `RoundResult`, `SettlementCalculator`, and `HanchanMatch`. Test four 25,000-point seats, deterministic dummy action traces, human tsumo/ron, exhaustive draw, dealer continuation, honba, riichi sticks, East/South advancement, and South 4 termination entirely in EditMode.
5. **Wire the two real action sources and four-seat presentation.** Add `HumanActionSource` and `TsumogiriActionSource`, route both through the same prompt/action boundary, then add only the scene panels needed to show four hands/rivers/turn/settlement. Keep `SampleScene.unity` as the composition root and localize mode switching in `MahjongGameManager`.
6. **Extend calls only after the base hanchan loop is green.** Add human chii/pon/daiminkan/ankan/kakan actions and the corresponding phase transitions, open meld state, kan wall/dora behavior, and scoring tests. Do not change the action boundary.
7. **Finish with build and portfolio evidence.** Run EditMode, PlayMode smoke, and Windows Player build; capture seed/action reproduction for representative defects. Add no hanchan persistence, AI, alternate rules, or networking.

Each step should leave the Unity scene compilable and one mode playable or a headless test executable. Do not move all legacy files in one commit: an assembly boundary plus a broad type rename would make serialization and behavior regressions difficult to review.

## Scalability Considerations

| Concern | Current portfolio target | Later batch simulation | Later strategic AI |
|---|---|---|---|
| Rules execution | Pure synchronous C# on Unity main thread is sufficient. | Seed/action inputs allow many headless EditMode or external .NET-compatible runs after profiling. | Pass immutable `ActionPrompt` to a policy; keep mutable round ownership on the session thread. |
| Hand solving | Correct exhaustive results before optimization. | Introduce count-array caching only after benchmarks identify the solver as the bottleneck. | Reuse cached structural evaluation; do not expose Unity objects to search workers. |
| Action sources | One human and three deterministic tsumogiri sources. | Deterministic sources can run without frames/UI. | Add one AI `IActionSource` implementation; no change to legality or settlement. |
| Persistence | Solo high score/settings only; no match resume. | Store seed/action trace only if portfolio/repro needs a durable file. | Persist model/config separately from domain state only when AI work enters scope. |
| Assemblies | One pure mahjong asmdef plus existing Unity assembly and test asmdef. | Same boundary is adequate. | Split a simulation assembly only if Unity-free tooling becomes a real consumer. |

## Confidence Assessment

| Area | Confidence | Notes |
|---|---|---|
| Current component coupling | HIGH | Directly evidenced by mapped and inspected project files, especially `MahjongGameManager.cs`, `MahjongRound.cs`, `_Structs.cs`, and `PlayerHand.cs`. |
| Unity assembly/test boundary | HIGH | Project versions are pinned locally and behavior is documented in official Unity 2022.3 and Test Framework 1.1.33 documentation. |
| State/ownership model | MEDIUM | Standard deterministic game-domain design derived from active requirements; must be validated by transition tests during migration. |
| Action-source boundary | MEDIUM | Fits the real asynchronous human and immediate dummy cases; callback cancellation/stale prompt rejection needs implementation testing. |
| Migration order | HIGH | Preserves the existing composition root and solo mode, and puts tested pure rules before four-seat/UI expansion. |

## Sources

### Primary Unity Documentation

- [Unity 2022.3 Manual — Assembly definitions](https://docs.unity3d.com/2022.3/Documentation/Manual/ScriptCompilationAssemblyDefinitionFiles.html) — predefined assembly behavior, folder ownership, and explicit dependency direction.
- [Unity 2022.3 Manual — Assembly Definition properties](https://docs.unity3d.com/2022.3/Documentation/Manual/class-AssemblyDefinitionImporter.html) — **No Engine References** and assembly reference configuration.
- [Unity Test Framework 1.1.33 — Edit Mode vs. Play Mode tests](https://docs.unity3d.com/Packages/com.unity.test-framework@1.1/manual/edit-mode-vs-play-mode-tests.html) — EditMode test assembly requirements and explicit reference to tested runtime assemblies.
- [Unity Manual — Customizing the Player loop](https://docs.unity3d.com/Manual/player-loop-customizing.html) — official example pairing event subscription in `OnEnable` with unsubscription in `OnDisable`.

### Codebase Evidence

- `.planning/PROJECT.md` — active requirements, scope exclusions, nine-day constraint, and solo/four-player decisions.
- `.planning/codebase/ARCHITECTURE.md` — current scene composition root, manager/domain/event flow, and assembly constraints.
- `.planning/codebase/CONCERNS.md` — known rule defects, restart/event bugs, single-player scaling limit, and missing tests.
- `Assets/Scripts/MahjongGameManager.cs` — current session/UI/timer/persistence coordinator and state gate.
- `Assets/Scripts/AL-1S/MahjongRound.cs` — current single-player wall/player/round/win/score/advance aggregate.
- `Assets/Scripts/AL-1S/_Structs.cs` — mixed Unity UI and domain state plus mutating next-round methods.
- `Assets/Scripts/AL-1S/MahjongUtilities.cs`, `MahjongYaku.cs`, `MahjongWinInfo.cs` — solver/yaku/fu/payment pipeline to isolate and test.
- `Assets/Scripts/UI-Kozeki/PlayerHand.cs`, `GameUIManager.cs` — human input/presentation and lifecycle/restart seams.
- `Assets/Scripts/ScoreManagerDistance.cs`, `Assets/Scripts/Configs/SettingsManager.cs` — solo-only application and persistence services to preserve outside rules.
- `Packages/manifest.json`, `ProjectSettings/ProjectVersion.txt` — Unity Test Framework 1.1.33 and Unity 2022.3.29f1 pins.

## Open Questions for Phase Planning

- Confirm the exact South 4 termination rule when dealer repeats or remains first; the project states “South 4 end” but does not yet define agari-yame or score-threshold variants. Keep one fixed answer in `HanchanMatch`, not a configuration framework.
- Decide whether riichi declaration is required in the base four-player phase or follows the existing solo-riichi stabilization. The ownership model supports either, but tests and UI scope differ.
- Define response priority for Phase 3 calls and whether multiple ron is impossible by scope or must be represented. `RoundResult` may hold a winner list without implementing multiple ron now.
- Validate whether the existing tile/database string representation can remain unchanged after tile equality/hash corrections; sprite/editor asset GUIDs and codes should be preserved.

---

*Architecture research: 2026-08-28*
