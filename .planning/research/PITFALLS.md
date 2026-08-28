# Domain Pitfalls

**Domain:** Brownfield Unity riichi-mahjong rules refactor and fixed hanchan extension
**Project:** Project Riichi Nya
**Researched:** 2026-08-28
**Overall confidence:** HIGH for repository-specific failure modes; MEDIUM for rule interpretation details inherited from the cited WRC/EMA primary sources and the project-fixed contract

## Phase Vocabulary

The roadmap should preserve this dependency order even if phase numbers or names change:

1. **Executable Baseline** — Player compilation, deterministic entry points, lifecycle smoke, and characterization tests.
2. **Correct Shared Rules Core** — wall/shuffle, value semantics, decomposition, yaku/fu, and payments.
3. **Solo Preservation and Riichi** — move the existing timed mode onto the shared results without importing hanchan progression.
4. **Headless Fixed Hanchan** — four seats, legal transitions, tsumogiri dummies, human tsumo/ron, furiten, settlement, and East/South progression.
5. **Hanchan Unity Integration** — human action adapter, four-seat presentation, scene wiring, and restart smoke.
6. **Human Calls and Kans (conditional)** — chii, pon, daiminkan, ankan, kakan, open-hand scoring, and kan wall flow.
7. **Portfolio Release Gate** — full tests, Windows Player build, smoke, documentation, and evidence.

Phase 6 is not part of the stable completion line. It starts only after Phase 4 is green headlessly and Phase 5 has not destabilized the solo mode.

## Critical Pitfalls

### Pitfall 1: Refactoring Before Characterizing the Executable Solo Path

**Confidence:** HIGH

**What goes wrong:** `Assets/Scripts/MahjongGameManager.cs`, `Assets/Scripts/AL-1S/MahjongRound.cs`, and the presenters under `Assets/Scripts/UI-Kozeki/` are structurally changed before current lifecycle and output contracts are recorded. Later failures cannot be classified as an intended rule correction, an accidental solo regression, or a broken serialized reference.

**Why it happens:** There are no first-party tests, and the confirmed wall, decomposition, yakuman, and tsumo-payment bugs make developers reluctant to record existing behavior. That confuses characterization with preserving known-wrong rules.

**Warning signs:**
- The first test is added after production classes have moved or been renamed.
- One commit combines an `.asmdef`, type moves, solver replacement, and scene changes.
- Tests assert only newly corrected scores but do not exercise start, finish/forfeit, and second start.
- A failure is explained as “probably an old bug” without a baseline trace or seed.

**Consequences:** The distinctive timed score/boost/distance mode can be lost while the new hanchan still appears to work; serialization and event regressions become expensive to localize under the nine-day deadline.

**Prevention:** In Phase 1, create the agreed `portfolio-baseline` tag before source changes, record fixed seeds, and add characterization around observable solo behavior. Explicitly mark the confirmed rule defects as expected changes rather than freezing their wrong values. Keep `Assets/Scenes/SampleScene.unity` runnable after each migration step, and extract behind the current API before deleting it.

**Smallest runnable verification:** One PlayMode scenario loads `Assets/Scenes/SampleScene.unity`, starts the solo mode, performs or simulates a legal discard, ends by forfeit or timer, starts again, and asserts required panels and each observed event occur once. One EditMode fixture records a fixed-seed wall/hand trace while separately asserting that known defects are scheduled for correction.

**Phase:** Phase 1 — Executable Baseline; rerun at every later phase gate.

### Pitfall 2: Treating Wall Size, Red Fives, Dead Wall, and Shuffle as Separate Fixes

**Confidence:** HIGH for the current defects; MEDIUM for external rule details, resolved by the project-fixed contract in `.planning/research/FEATURES.md`

**What goes wrong:** A local fix changes `Assets/Scripts/AL-1S/MahjongRound.cs:195` from 139 to 136 tiles but leaves five copies of a suited five, a biased `Utilities.ShuffleArray`, nondeterministic fallback randomness, or inconsistent live/dead-wall accounting. Every later riichi, exhaustive-draw, dora, and kan test then rests on a corrupt wall.

**Why it happens:** `GenerateYama` currently builds three copies of every base tile and appends `GetAllTiles(true)`, while `Assets/Scripts/AL-1S/Utilities.cs:44` excludes the final index from `Random.Next`. Wall creation, shuffle, dead-wall extraction, and dealing are performed as one mutable procedure.

**Warning signs:**
- Tests assert only `Count == 136`.
- Red fives are added instead of replacing one normal five per suit.
- Production code creates `new System.Random()` or calls `UnityEngine.Random` inside the core.
- A kan decrements a rinshan list but does not move the live-wall tail into the dead wall.
- A failed run cannot be reproduced from a logged seed.

**Consequences:** Impossible tile multiplicities, biased draws, wrong exhaustive-draw timing, mismatched dora indicators, and non-reproducible portfolio demonstrations.

**Prevention:** Make wall construction one pure operation with an explicit seed and one 136-tile physical inventory. Assert four physical copies per `TileID`, with three normal fives plus one red five for each suit under the project override. Model the 14-tile dead wall, four replacement tiles, active indicator count, and live-wall tail transfer together. Use a correct Fisher-Yates shuffle and never silently generate a seed inside rule code.

**Smallest runnable verification:** For seed `1557`, assert 136 unique physical positions, 34 identities × four copies, exactly three red fives, a 14-tile dead wall, and no overlap between live/dead collections. Creating twice with the same seed must produce the same ordered tile trace. A small-array shuffle test across a seed table must prove the original last element can move and every result remains a permutation.

**Phase:** Phase 2 — Correct Shared Rules Core; wall invariants must be green before hand or hanchan work.

### Pitfall 3: Replacing the Greedy Solver Without Fixing Equality, Deduplication, and Best-Result Selection

**Confidence:** HIGH

**What goes wrong:** `Assets/Scripts/AL-1S/MahjongUtilities.cs:48` is changed to recurse, but valid decompositions are still lost or merged incorrectly by inconsistent equality/hash contracts in `Assets/Scripts/AL-1S/MahjongTileAndBlock.cs:316` and `Assets/Scripts/AL-1S/MahjongWinInfo.cs:441`. Alternatively, all decompositions are found but the winner is selected by han/fu ordering rather than actual legal payment.

**Why it happens:** The current solver stops after the first successful head and tries only sequence-first/triplet-first strategies. `MahjongTile.operator ==` compares only `TileID` while `Equals` uses all struct fields; `MahjongWinInfo` equality ignores `winTile` but its hash includes it.

**Warning signs:**
- The recursive solver returns a Boolean or a single decomposition.
- A `HashSet` count changes when candidates are inserted in a different order.
- Red/dora metadata changes `Equals` but not `==`, or vice versa.
- The result chooser compares only `Han` then `Fu`.
- Open-meld support adds more special-case branches to the 14-concealed-tile solver.

**Consequences:** Legal wins are rejected, waits and riichi choices are incomplete, fu/yaku depend on collection behavior, and the displayed result is not the highest-paying legal interpretation.

**Prevention:** Define tile identity and per-instance scoring metadata separately, implement one consistent `IEquatable<T>`/operator/hash contract, and give decompositions a stable structural key. Enumerate all pair/triplet/sequence branches from the lowest remaining tile, include chiitoitsu/kokushi paths, evaluate every complete interpretation in its win context, and select by final payment under the fixed rules.

**Smallest runnable verification:** The hand `1122334455667m` completed by `7m` must expose both seven-pairs and standard four-meld interpretations where legal. An equality contract test must show equal values have equal hashes in `HashSet` and unequal win-tile interpretations remain distinguishable when they change wait/fu. Repeat candidate insertion in reverse order and assert the same selected payment.

**Phase:** Phase 2 — Correct Shared Rules Core.

### Pitfall 4: Keeping Yaku, Fu, Yakuman, Dora, and Payments as One Implicit Calculation

**Confidence:** HIGH for repository defects; MEDIUM for rule tables, governed by `.planning/research/FEATURES.md`

**What goes wrong:** A recognized hand receives incorrect legality or money because yaku detection, dora, han limits, yakuman multiples, fu, ron/tsumo rounding, and score application remain entangled across `Assets/Scripts/AL-1S/MahjongYaku.cs`, `MahjongWinInfo.cs`, `MahjongUtilities.cs`, and `MahjongRound.cs:491`. The current solo path already awards ron totals for tsumo.

**Why it happens:** The current model encodes yakuman as han, has an unused yakuman base-score path, and exposes a broad mutable hand-info snapshot. UI and gameplay can independently choose which score-table field to display or apply.

**Warning signs:**
- Dora alone makes a hand legally winnable.
- Multiple yakuman are flattened into ordinary han or capped at a single yakuman.
- `isTsumo`, seat wind, round wind, openness, riichi, or win source is hard-coded.
- Settlement/UI code reads `oyaRon` or `zaRon` for a tsumo result.
- Fu tests cover totals but not wait, pair, open/closed triplet, kan, or ron-completed shanpon context.

**Consequences:** Wrong availability prompts and point transfers—the central portfolio correctness claim—despite a visually complete game.

**Prevention:** Pass one explicit `WinContext`; produce separate yaku, yakuman count, dora count, fu breakdown, and payment result values. Require at least one yaku before dora, count multiple yakuman separately, and make one payment calculator the only source for UI, solo reward adaptation, and hanchan settlement. Use table-driven cases derived from the fixed rule contract.

**Smallest runnable verification:** A compact EditMode table covers dealer/non-dealer ron and tsumo with independently rounded payer amounts, one ambiguous fu hand, one open-hand reduced-yaku case, dora-only rejection, one yakuman, and two simultaneous yakuman. For every row, assert the breakdown and final transfers rather than only a display string.

**Phase:** Phase 2 — Correct Shared Rules Core; extend the same table in Phase 6 for open melds and kans.

### Pitfall 5: Sharing a Mutable Round Instead of Sharing Pure Rule Results

**Confidence:** HIGH

**What goes wrong:** Four-player support is added by giving the existing `Assets/Scripts/AL-1S/MahjongRound.cs` more players and mode flags, or the solo loop is forced to use hanchan scores, honba, dealer rotation, and noten payments. A defect in either mode then changes the other.

**Why it happens:** `MahjongRound` currently owns wall, one player, scoring, next-round creation, and domain events, while `Assets/Scripts/MahjongGameManager.cs` owns timer, UI, persistence, and score-distance orchestration. Reusing that class looks faster than separating shared calculation from mode progression.

**Warning signs:**
- Methods contain `if (isHanchan)` or `if (isSolo)` around legality/scoring.
- `ScoreManagerDistance` is referenced by hanchan domain code.
- Solo results contain seat accounts, honba, or noten transfers.
- The same mutable player/round instance survives from one hanchan hand to the next.
- A rule fix is copied into two mode-specific solver files.

**Consequences:** Solo timer/reward/save regressions, duplicated rules, hidden ownership, and a refactor larger than the deadline allows.

**Prevention:** Share tile/wall/win/yaku/fu/payment values only. Keep `SoloGameSession` and `FourPlayerRound`/`HanchanMatch` as separate progression owners, with a temporary adapter around the existing solo API. `Assets/Scripts/ScoreManagerDistance.cs` remains a solo application service consuming a standard payment/award result.

**Smallest runnable verification:** Feed the same hand and `WinContext` to the shared evaluator from both a solo fixture and a hanchan fixture and assert identical structural/yaku/fu/base-payment results. Then assert only the solo adapter changes boost/distance and only `HanchanMatch` mutates four seat scores.

**Phase:** Phase 3 — Solo Preservation and Riichi, after Phase 2 rules are green.

### Pitfall 6: Letting Events or UI Visibility Become the Four-Player State Machine

**Confidence:** HIGH for current coupling; MEDIUM for the target transition model until implemented

**What goes wrong:** Draw, discard, response, settlement, and next-round steps are chained through `MahjongRound` events, DOTween callbacks, or active panels. Late human callbacks and immediate dummy actions can then execute in the wrong turn or twice.

**Why it happens:** The current flow uses `MahjongRound.OnRoundEnds -> OnNewRoundStart` and `MahjongGameManager` event attachment, while `GameUIManager` maintains separate panel state. Four seats add asynchronous human responses to an event model already vulnerable on restart.

**Warning signs:**
- The UI decides whether an action is legal because a button/panel is active.
- More than one public method mutates round state.
- A submitted action has no actor, phase, or prompt/version identifier.
- Settlement starts from an animation completion callback.
- A dummy can recursively advance several turns without allowing cancellation or observing a terminal result.

**Consequences:** Double draws, wrong-seat discards, skipped ron windows, actions after a round ended, score application twice, and non-reproducible race-like failures on Unity's main thread.

**Prevention:** Make `FourPlayerRound` the only owner of per-hand state and `HanchanMatch` the only owner of persistent scores/counters. Route decisions through one `Submit(PlayerAction)` boundary carrying actor and `PromptId`; revalidate legality on submission and return explicit transition/result values. UI and action sources consume immutable snapshots and legal-action lists.

**Smallest runnable verification:** A transition-matrix EditMode test rejects wrong-seat, stale-prompt, duplicate, unavailable, and post-ended actions without changing a snapshot hash. A legal draw → discard → response → pass → next draw trace changes phase exactly once per accepted action.

**Phase:** Phase 4 — Headless Fixed Hanchan, before any hanchan UI.

### Pitfall 7: Auto-Advancing Past the Human Ron Window or Accidentally Giving Dummies Winning Behavior

**Confidence:** HIGH

**What goes wrong:** A tsumogiri dummy draws and immediately chains the next seat's draw, so the human never receives `Ron`/`Pass`; or a shared “check win after draw” path lets a completed dummy hand tsumo/ron despite the agreed non-winning contract.

**Why it happens:** Dummies are synchronous and deterministic, making immediate recursion convenient. The repository's current `MahjongRound` assumes the sole player is the only participant, so it has no response phase or actor policy boundary.

**Warning signs:**
- A dummy turn method performs draw, discard, and next draw in one call.
- Dummy policy code calls the win evaluator or chooses among discards.
- Dummy seats have no real 13-tile hand because they are treated as animations.
- Exhaustive-draw tenpai ignores dummy hands.
- Documentation or UI calls them AI/CPU opponents.

**Consequences:** Human ron is unreachable or timing-dependent, tile counts and noten payments are wrong, dealer continuance is distorted, and the portfolio overclaims strategic AI.

**Prevention:** Give all four seats real hands, rivers, winds, and scores, but give dummy action sources exactly one legal action: discard the just-drawn physical tile. After every dummy discard, enter `AwaitingResponses`; only the human can receive legal `Ron` or `Pass`. Dummies never declare riichi, win, call, or kan, but their actual hands participate in exhaustive-draw tenpai.

**Smallest runnable verification:** A fixed action trace covers one complete seat rotation and asserts each dummy's concealed 13 tiles are unchanged after its draw/discard, the discarded tile is the exact drawn instance, and no next draw occurs until the human passes. A scripted winning discard exposes human `Ron`; a completed dummy hand still produces only tsumogiri.

**Phase:** Phase 4 — Headless Fixed Hanchan; presentation follows in Phase 5.

### Pitfall 8: Implementing Riichi and Furiten as UI Flags Instead of Per-Player Rule State

**Confidence:** HIGH for missing implementation; MEDIUM for rule details fixed by the project contract

**What goes wrong:** `Assets/Scripts/MahjongGameManager.cs:289` gains a working riichi button, but the discard commitment, 1,000-point deposit, hand lock, ippatsu, ura-dora, missed-win furiten, and reset boundaries are incomplete. Ron then becomes legal or illegal based on the latest UI prompt rather than history.

**Why it happens:** `RiichiHandler` is empty and current `MahjongHandInfo` fields hard-code riichi/open/furiten context. Furiten is only visible when discard-response windows exist, so it is easy to postpone until after ron UI is built.

**Warning signs:**
- Riichi is committed before a valid riichi discard is selected.
- Riichi availability ignores open hand, points, live-wall next-draw condition, or tenpai after discard.
- Any one safe tile blocks only itself instead of all ron waits under furiten.
- Passing a legal ron is not recorded.
- Temporary furiten is cleared by panel close or arbitrary next discard.

**Consequences:** Illegal ron, incorrect defensive behavior, wrong deposits/ura-dora, inconsistent solo and hanchan results, and expensive state migration when calls arrive.

**Prevention:** Store riichi declaration/commitment, ippatsu, river, temporary furiten, and riichi-pass furiten in `RoundPlayerState`. Follow the explicit project override in `.planning/research/FEATURES.md`: closed tenpai, valid discard, at least 1,000 points, and a future self-draw in the live wall. Recompute all waits for own-discard furiten; allow tsumo while furiten; clear temporary furiten only at the contract's next self draw/call boundary, and preserve riichi-pass furiten until hand end.

**Smallest runnable verification:** Table tests cover open-hand riichi rejection, insufficient points, last-live-wall rejection, deposit/hand lock, own-river furiten blocking all ron but not tsumo, passed ron becoming temporary furiten, temporary reset at the next allowed boundary, and a passed ron after riichi lasting until round end.

**Phase:** Basic riichi in Phase 3; complete ron/furiten transitions before Phase 4 is accepted.

### Pitfall 9: Updating Scores and Hanchan Counters in Several Places

**Confidence:** HIGH

**What goes wrong:** Win evaluation, UI, `FourPlayerRound`, and `HanchanMatch` each adjust some combination of seat scores, honba, riichi sticks, dealer continuation, round wind, or South 4 termination. A correct payment table can still yield a broken match.

**Why it happens:** The existing solo code applies a positive score directly in `MahjongRound.HandlePlayerWin`, while hanchan introduces four-way transfers and cross-round counters not present in the current model.

**Warning signs:**
- A payment calculator mutates players.
- UI code subtracts from the discarder or awards the riichi pot.
- `RoundResult` starts the next round itself.
- Four seat deltas are asserted to sum to zero without accounting for deposit-to-pot or pot-to-winner movement.
- South 4 uses score thresholds, agari-yame, west round, or tobi despite the fixed contract.

**Consequences:** Point creation/loss, duplicate settlement, incorrect dealer repeats, endless or prematurely ended matches, and a result screen that cannot explain transfers.

**Prevention:** Return descriptive `RoundResult` and pure `PointTransfer[]`; only `HanchanMatch` applies transfers and advances counters. Treat the riichi pool as an explicit ledger account. Implement only the fixed contract: 25,000 each, East/South, dealer win or dealer-tenpai draw repeats, honba rules, noten 3,000 distribution, and end after South 4 finally passes from the dealer; no agari-yame, west entry, tobi, uma, or oka.

**Smallest runnable verification:** Synthetic result tests—without dealing tiles—cover dealer/non-dealer ron and tsumo, all tenpai-count distributions, riichi deposit/carry/award, honba increment/reset, dealer repeat/rotation, and South 4 repeat then finish. For every step, assert four seat balances plus the riichi pool conserve the initial ledger total.

**Phase:** Phase 4 — Headless Fixed Hanchan.

### Pitfall 10: Treating Calls and Kans as Extra Buttons on the Closed-Hand Loop

**Confidence:** HIGH for repository limitation; MEDIUM for detailed rule interactions inherited from the fixed contract

**What goes wrong:** Chii/pon/kan handlers are added to `PlayerCallType` and UI, but the domain still assumes 13 concealed tiles, normal draw-before-discard, one dora indicator, and no persistent melds. Partial kan support creates 15-tile hands, double draws, early exhaustive draws, or wrong open-hand yaku/fu.

**Why it happens:** The handlers in `Assets/Scripts/MahjongGameManager.cs:260` are mostly empty, and `Assets/Scripts/AL-1S/MahjongUtilities.cs` solves a complete 14-concealed-tile hand. Kan touches action priority, meld openness, live/dead wall, rinshan, dora, ippatsu, riichi restrictions, and scoring at once.

**Warning signs:**
- UI offers a generic `Kan` action without distinguishing daiminkan, ankan, and kakan.
- Open melds are reconstructed from sprites or river entries.
- A call is followed by a normal draw before discard.
- Kakan mutates an existing pon before the rob-kan decision boundary.
- Dead-wall count falls below 14, the live-wall tail is not transferred, a fifth kan is possible, or a kan is allowed after the final live draw.
- Open hands retain riichi, menzen tsumo, pinfu, or unreduced closed-only yaku values.

**Consequences:** Invalid hand cardinality, corrupt wall exhaustion, impossible dora/rinshan results, over-scored open hands, and a Phase 6 rewrite that threatens the stable hanchan.

**Prevention:** Start Phase 6 only after the Phase 4 transition model is green. Store typed melds in `RoundPlayerState` and solve only the remaining concealed portion while counting fixed melds toward four sets. Enumerate exact legal call combinations. Chii is only from the preceding seat; pon/daiminkan use the last discard; accepted chii/pon discard without a normal draw. Model kan declaration and completion separately, especially kakan. On each completed kan, consume one of four rinshan tiles, move one live-wall tail tile into the dead wall so it remains 14, activate the next kan-dora before discard, cancel ippatsu, and reject a fifth kan. Apply the fixed open-yaku/fu table from `.planning/research/FEATURES.md`.

**Smallest runnable verification:** Transition tables cover multiple chii choices, pon precedence for the sole human responder, pass, post-call discard without draw, and each kan type. After each of four completed kans, assert total physical tiles remain conserved, dead wall remains 14, live wall loses one additional tail tile, exactly one rinshan is consumed, indicator count increments, and fifth kan is rejected. Separate score fixtures verify open/closed triplet and kan fu, reduced yaku, rinshan tsumo, active kan-dora/kan-ura, and riichi-after-ankan restrictions.

**Phase:** Phase 6 — Human Calls and Kans, conditional; do not ship a partially wired kan button.

## Moderate Pitfalls

### Pitfall 11: Unity Subscription, Restart, and Serialized-Wiring Failures Survive Rule Tests

**Confidence:** HIGH

**What goes wrong:** Pure EditMode tests pass while the scene duplicates callbacks, toggles required panels off on the second game, accepts input after game over, or loses serialized references after a script/type move.

**Why it happens:** `Assets/Scripts/UI-Kozeki/PlayerHand.cs:45` subscribes both game-over and game-start handlers to `OnGameOver`; `Assets/Scripts/UI-Kozeki/GameUIManager.cs:78` uses `TogglePanel` during initialization; `UiScoreDistanceInfo.Construct` can accumulate subscriptions; and `Assets/Scenes/SampleScene.unity` is the sole composition root with many Inspector references.

**Warning signs:**
- Subscriptions occur in `Start` with no symmetric unsubscribe.
- `Construct` replaces a publisher without detaching the old one.
- Initialization calls `TogglePanel` rather than setting an explicit target state.
- Script moves omit matching `.meta` files, or serialized field/type renames are mixed with behavior changes.
- Tests instantiate isolated components but never load `SampleScene.unity` twice through a game lifecycle.

**Consequences:** The first demonstration works and the second fails, null references appear only in the build, or a scene/prefab silently loses a component reference.

**Prevention:** Give every subscription one owner and symmetric attach/detach; detach old dependencies before reconstruction; cancel pending prompts/tweens on session end; clear singleton/static references on destruction; make panel initialization idempotent. Preserve `.meta` GUIDs and validate required serialized fields in `OnValidate`/`Awake` with specific errors. Keep gameplay legality out of panel state.

**Smallest runnable verification:** One PlayMode smoke loads `Assets/Scenes/SampleScene.unity`, starts, ends/forfeits, restarts, disables/re-enables relevant views, and asserts one callback and explicit panel state per event with no unexpected logs. Repeat after every scene/prefab/script move.

**Phase:** Begin in Phase 1; expand for hanchan presentation in Phase 5.

### Pitfall 12: Editor Success Is Mistaken for a Shippable Windows Player and the 9-Day Scope Gate Is Ignored

**Confidence:** HIGH

**What goes wrong:** The Editor and EditMode suite are green, but Player compilation fails on `UnityEditor` imports in `Assets/Scripts/AL-1S/MahjongTileAndBlock.cs:9` and `Assets/Scripts/UI-Kozeki/UiScoreDistanceInfo.cs:9`, on `.asmdef` reference drift, or on serialized scene errors. Time is then spent finishing calls/UI while no reproducible release artifact or honest evidence exists.

**Why it happens:** All runtime scripts currently share the predefined assembly, no first-party tests/build wrapper exists, and the research documents propose two migration seams: a broad named runtime assembly for test referencing and a pure no-engine rules assembly below the Unity layer. Attempting both as one structural change maximizes compile and serialization risk.

**Warning signs:**
- `UnityEditor` appears anywhere outside `Assets/Editor/` without a narrow guard.
- `.asmdef` creation, pure-core extraction, namespace changes, and scene edits land together.
- Release status is based on an `.exe` existing rather than `BuildReport`, exit code, and log.
- Phase 6 starts while headless hanchan, solo restart, or a Windows build is red.
- README says “AI opponents,” “full riichi rules,” or “all kans” when the verified build contains only tsumogiri dummies or partial calls.

**Consequences:** Schedule failure, a non-running submission, hard-to-review diffs, and portfolio claims that are weaker than an honest smaller result.

**Prevention:** Remove editor-only runtime imports in Phase 1 and run a Windows x64 build immediately. Choose and prove one assembly migration step at a time: first make production code test-referenceable and buildable; then carve the pure no-engine core only after mixed UI/domain types such as `Assets/Scripts/AL-1S/_Structs.cs` are separated. Gate every structural step with compile, EditMode, PlayMode smoke, and Player build. If Phase 4 is not fully green by the end of the stable implementation window, cut Phase 6 entirely. Use `Tsumogiri Dummy`, list omitted rule variants, and claim only features shown by the release commit and test evidence.

**Smallest runnable verification:** Invoke Unity `2022.3.29f1` in batch mode for EditMode, the lifecycle PlayMode smoke, and `BuildPipeline.BuildPlayer` targeting Windows x64. Fail on a non-successful `BuildReport`, any compiler/build error, or non-zero process exit. Launch that exact build for a short solo start/restart and shipped hanchan path; record commit/tag, seed/action trace, test counts, and known omissions.

**Phase:** Player build begins in Phase 1 and repeats after each phase; scope/claim decision is mandatory before Phase 6 and final in Phase 7.

## Minor Pitfalls

### Pitfall 13: Optimizing or Generalizing Before the Correctness Matrix Is Green

**Confidence:** HIGH

**What goes wrong:** Time is spent on solver caching, AI interfaces, DI, rule configuration, coverage tooling, UI rewrites, or full automation while critical rule and lifecycle cases remain unverified.

**Warning signs:** New packages or factories appear without two current implementations; performance claims lack a profiler capture; coverage percentage replaces named wall/decomposition/payment/transition cases.

**Prevention:** Keep one fixed rules contract, concrete sessions, one justified human/dummy action boundary, installed Unity Test Framework/NUnit, and the existing uGUI/TMP/DOTween stack. Optimize the 476+ riichi-candidate path only after correctness and a main-thread profile prove it blocks play.

**Smallest runnable verification:** At each phase review, map every changed rule branch to one named table/transition case and reject work that does not close an active requirement or release gate.

**Phase:** All phases; enforce most strongly during Phases 2 and 6.

## Phase-Specific Warnings

| Phase Topic | Likely Pitfall | Mitigation / Exit Gate |
|-------------|----------------|------------------------|
| Executable Baseline | Tests added after refactor; second start fails; Player-only compile error | Baseline tag, fixed seed, start→end→restart PlayMode smoke, successful Windows build before structural changes |
| Correct Shared Rules Core | 136 count fixed without copy/red/dead-wall/shuffle invariants; recursive solver still loses candidates by hash | Wall property table, equality contract, ambiguous decomposition, yaku/fu/payment tables all green |
| Solo Preservation and Riichi | Hanchan state leaks into timer/distance mode; riichi is only a button | Shared result equivalence plus preserved timer/boost/distance/save and full riichi commitment tests |
| Headless Fixed Hanchan | UI/events drive legality; dummy turn skips human ron; settlement is distributed across classes | Transition matrix, fixed dummy trace, furiten table, ledger conservation, South 4 termination all pass without scene/UI |
| Hanchan Unity Integration | Serialized references, duplicate subscriptions, and late callbacks | Scene PlayMode smoke covers solo and hanchan, restart, prompt cancellation, and exactly-once presentation |
| Human Calls and Kans | Partial button-level implementation corrupts hand/wall/scoring | Begin only after base hanchan green; require complete vertical tests for calls, open scoring, all three kan types, rinshan, and indicators |
| Portfolio Release | Editor green but Player red; claims exceed demonstrated behavior | Batch test/build gate, built-player smoke, tagged evidence, explicit `Tsumogiri Dummy` and known limits |

## Confidence Assessment

| Area | Confidence | Notes |
|------|------------|-------|
| Current defects and file locations | HIGH | Directly grounded in `.planning/codebase/CONCERNS.md`, `ARCHITECTURE.md`, `TESTING.md`, and the cited source files. |
| Migration and phase-order risks | HIGH | Dependencies follow the existing solo composition root and the completed stack/features/architecture research. |
| State ownership and action boundary | MEDIUM | Strong fit for the scoped human/dummy flows, but must be proven by transition tests. |
| Riichi, furiten, calls, kan, and dead-wall details | MEDIUM | Based on official WRC/EMA sources already cited in `FEATURES.md`; project overrides are authoritative for this milestone. |
| Nine-day delivery gates | HIGH | Directly required by `.planning/PROJECT.md`; Phase 6 is explicitly subordinate to a verified base hanchan and release. |

## Sources

### Repository and Project Sources — HIGH confidence

- `.planning/PROJECT.md` — active requirements, fixed nine-day deadline, solo preservation, tsumogiri dummy boundary, conditional calls/kans, baseline tag, and portfolio evidence contract.
- `.planning/codebase/CONCERNS.md` — confirmed 139-tile wall, biased shuffle, greedy decomposition, equality/hash mismatch, yakuman/tsumo-payment errors, restart/subscription faults, and Player-only import risk.
- `.planning/codebase/ARCHITECTURE.md` — current `SampleScene.unity` composition root, `MahjongGameManager`/`MahjongRound` ownership, event flow, singleton and assembly constraints.
- `.planning/codebase/TESTING.md` — installed Unity Test Framework 1.1.33, absent first-party tests/asmdefs, and recommended EditMode/PlayMode boundaries.
- `.planning/research/STACK.md` — pinned Unity/C# packages, local batch build/test gate, and minimal dependency policy.
- `.planning/research/FEATURES.md` — fixed basic rule contract, riichi/furiten/dummy/call/kan acceptance behavior, anti-features, and MVP ordering.
- `.planning/research/ARCHITECTURE.md` — pure rule results, separate solo/hanchan progression, action prompts, transition ownership, settlement ledger, and strangler migration order.
- `Assets/Scripts/AL-1S/MahjongRound.cs`, `MahjongUtilities.cs`, `MahjongYaku.cs`, `MahjongWinInfo.cs`, `MahjongTileAndBlock.cs`, `Utilities.cs` — current wall, solver, equality, scoring, and round implementation.
- `Assets/Scripts/MahjongGameManager.cs`, `Assets/Scripts/UI-Kozeki/PlayerHand.cs`, `GameUIManager.cs`, `UiScoreDistanceInfo.cs`, and `Assets/Scenes/SampleScene.unity` — current lifecycle, input, event, UI initialization, and serialized-wiring seams.

### Authoritative Primary Sources Already Established by Project Research — MEDIUM confidence via GSD source classifier

- [World Riichi Championship — current WRC Rules index](https://www.worldriichi.org/wrc-rules) — current official rules/clarifications download index and archived detailed rules.
- [World Riichi Championship Rules 2022](https://www.worldriichi.org/s/WRC_Rules_2022_20220708_site.pdf) — detailed official reference for furiten, riichi, calls, kan restrictions, 136 tiles, 14-tile dead wall, replacement draws, kan-dora timing, highest-scoring interpretation, and multiple yakuman.
- [European Mahjong Association Riichi Rules 2025](https://mahjong-europe.org/portal/images/docs/Riichi-rules-2025-EN.pdf) — current official cross-check for dead wall, exhaustive draw, calls, scoring, and modern rule context.
- [WRC Yaku and Fu reference](https://www.worldriichi.org/s/WRC_Yaku_1_5_3_black.pdf) — official open/closed han and fu reference used by the fixed project contract.
- [Unity 2022.3 Manual — Assembly definitions](https://docs.unity3d.com/2022.3/Documentation/Manual/ScriptCompilationAssemblyDefinitionFiles.html) and [Assembly Definition properties](https://docs.unity3d.com/2022.3/Documentation/Manual/class-AssemblyDefinitionImporter.html) — runtime/editor dependency boundaries and No Engine References.
- [Unity Test Framework 1.1.33 — Edit Mode vs. Play Mode tests](https://docs.unity3d.com/Packages/com.unity.test-framework@1.1/manual/edit-mode-vs-play-mode-tests.html) — test assembly and runtime integration boundaries.
- [Unity 2022.3 `BuildPipeline.BuildPlayer`](https://docs.unity3d.com/2022.3/Documentation/ScriptReference/BuildPipeline.BuildPlayer.html) — `BuildReport`-based Player build verification.

## What Might Still Be Missed

- The exact assembly migration sequence has not been executed. Treat the broad runtime assembly versus extracted no-engine rules assembly boundary as an early compile/build experiment, not a settled file-move script.
- Specific yaku/fu fixture coverage must be enumerated during Phase 2 planning; this document identifies failure classes, not a complete rules test catalog.
- Phase 6 should re-check the fixed contract for call priority, kuikae, riichi-after-ankan interpretation, and kakan interruption when its implementation is actually authorized. Do not generalize those variants earlier.

---

*Pitfalls research: 2026-08-28*
