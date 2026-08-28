# Project Research Summary

**Project:** Project Riichi Nya  
**Domain:** Brownfield Unity riichi-mahjong rules refactor and fixed four-seat hanchan portfolio project  
**Researched:** 2026-08-28  
**Confidence:** HIGH for repository facts and phase order; MEDIUM for unimplemented rules and target architecture

## Executive Summary

Project Riichi Nya is a nine-day brownfield portfolio refactor, not a new mahjong product. The strongest deliverable is a demonstrably corrected and tested rules engine that preserves the existing 180-second solo score/boost/distance experience and also drives a fixed-basic hanchan with one human and three non-winning `Tsumogiri Dummy` seats. Experts would approach this by protecting the executable baseline, turning confirmed defects into regression tables, separating pure rule results from mode-specific progression, and proving the four-seat match headlessly before adding scene presentation.

Keep the pinned Unity 2022.3.29f1/C# 9/.NET Standard 2.1 project and its existing uGUI, TextMesh Pro, DOTween, URP, persistence, and package graph. Add no packages. Resolve the research's assembly-boundary disagreement sequentially: first remove runtime `UnityEditor` imports and introduce one broad named runtime assembly plus test assembly so current production code is test-referenceable; only after mixed UI/domain types are separated should the mahjong rules move into a nested pure assembly with **No Engine References**. This avoids combining a broad file move, type redesign, and scene migration in one risky change.

The stable completion floor is Stage 2: corrected shared rules, preserved and tested solo mode with riichi, a complete headless and presented fixed-basic hanchan, and a repeatable Windows build/release evidence package. Stage 3 remains an explicit target, not silently discarded: human chi, pon, daiminkan, ankan, kakan, open-hand evaluation, and kan wall/dora flow form one conditional phase after Stage 2 is green. The dominant risks are refactoring before characterization, incomplete wall/solver/payment fixes, UI/events acting as the state machine, distributed score mutation, and shipping partial kan behavior. Each roadmap phase therefore needs a named test/build exit gate; if the Stage 2 floor is not green, Stage 3 must not consume the release budget.

## Key Findings

### Recommended Stack

The minimal stack is the current stack. Preserve exact editor and package versions, use the already-installed Unity Test Framework/NUnit, and add only small repository-local verification seams. No engine upgrade, UI rewrite, new input system, mocking framework, CI service, coverage package, database, server, documentation site, or AI library belongs in this milestone. Detailed evidence is in [STACK.md](./STACK.md).

**Core technologies:**

- **Unity 2022.3.29f1 (`8d510ca76d2b`)**: editor, runtime, tests, and Windows build — exact repository and local-environment pin; upgrading adds unrelated serialization and package risk.
- **C# 9.0 / .NET Standard 2.1**: production and pure-domain code — current compiler/API surface, adequate for deterministic value objects and state transitions.
- **URP 14.0.11, uGUI 1.0.0, TextMesh Pro 3.0.6, vendored DOTween**: existing presentation — retain assets and scene wiring; do not reinstall the unversioned DOTween binaries.
- **Unity Test Framework 1.1.33 / NUnit extension 1.0.6**: EditMode rule tables and minimal PlayMode lifecycle smoke — already installed; no external test or assertion dependency.
- **Newtonsoft.Json 3.2.1, PlayerPrefs, ScriptableObject tile database**: existing local state/assets — preserve save compatibility and keep persistence out of the rules core.
- **Unity `BuildPipeline.BuildPlayer` plus PowerShell**: repeatable EditMode, PlayMode, and Windows x64 release gate — success requires test XML/logs, successful `BuildReport`, and zero process exit.
- **Git annotated baseline/release tags and Markdown**: before/after and AI-development evidence — use history, named regression cases, and a short demo rather than duplicate source trees or full chat logs.

**Assembly decision:**

1. Clean Player compilation first by removing editor-only runtime imports.
2. Add a broad `RiichiNya.Runtime` assembly and EditMode test assembly to characterize and fix current code without an all-at-once layer split.
3. After `_Structs.cs` and other mixed files are separated, extract `ProjectRiichiNya.Mahjong` as a pure **No Engine References** assembly referenced by the Unity-facing runtime assembly.
4. Add a PlayMode test assembly only with the first real lifecycle test. Do not create empty or speculative layer assemblies.

### Expected Features

The project uses one fixed rules contract rather than configurable variants: 136 physical tiles with three red fives replacing normal fives, four 25,000-point seats, East/South hanchan, dealer repeats, honba, riichi sticks, exhaustive-draw noten payments, no bankruptcy end, and no abortive draws or west entry. Detailed behavior is in [FEATURES.md](./FEATURES.md).

**Must have — stable Stage 2 completion floor:**

- Correct wall inventory, unbiased deterministic shuffle, exhaustive decomposition and highest-payment selection, consistent equality/hash behavior, yaku/fu/yakuman/dora legality, and payer-specific ron/tsumo payments.
- Preserved 180-second solo loop, score/boost/distance/high-score behavior, clean finish/forfeit/restart, and Windows Player operation.
- Complete solo riichi cycle: legal discard candidates, 1,000-point deposit, hand commitment, ippatsu, ura-dora, and correct hand-end reset/carry behavior.
- Four real seat states with hands, rivers, winds, scores, live/dead wall, turn owner, and deterministic dummy behavior: draw then discard the exact drawn tile, with no strategic choice or declaration.
- Human tsumo and ron, a mandatory response window after each dummy discard, own-river/temporary/riichi-pass furiten, and no dummy win/riichi/call/kan behavior.
- Central four-seat settlement for dealer/non-dealer payments, honba, riichi pool, exhaustive-draw 3,000-point distribution, dealer continuation, East/South progression, and final South 4 termination.
- Minimal hanchan HUD/result presentation, mode/restart integration, table-driven EditMode tests, focused PlayMode smoke, repeatable Windows build, README, demo, and three to five evidence-backed AI-development cases.

**Should have — explicit Stage 3 target after the floor is green:**

- Human chi and pon with exact legal combinations, response priority, explicit pass, post-call discard without a normal draw, kuikae restriction, and typed open meld state.
- All human kan types: daiminkan, ankan, and kakan, including declaration/completion boundaries, rinshan draw, live-wall tail transfer, a constant 14-tile dead wall, four-kan ceiling, and indicator activation.
- Open-hand solving, closed-only yaku removal, kuisagari, open/closed meld fu, rinshan, kan-dora/kan-ura, ippatsu cancellation, and riichi-after-ankan restrictions.
- Compact legal-action rejection reasons and detailed settlement output where they help debugging and portfolio explanation; these are secondary to correctness.

**Defer beyond this milestone:**

- Strategic AI or external AI integration, dummy declarations/wins, four-human or network play, multiple ron, configurable rules/presets, abortive draws, west entry, bankruptcy, oka/uma, pao, nagashi mangan, local yaku, match resume, cloud services, and online rankings.
- UI/art replacement, generalized plugin/DI/event-bus architecture, performance caching without a profile, broad CI/coverage infrastructure, and durable replay storage.

### Architecture Approach

Use an incremental strangler migration around the existing `SampleScene.unity` composition root. `MahjongGameManager` becomes a mode host; `SoloGameSession` preserves timed arcade progression, while `HanchanGameSession` drives a separate `HanchanMatch`/`FourPlayerRound`. Both consume immutable results from the same pure tile/wall/hand/yaku/fu/payment core. Commands enter through a single validated action boundary; transitions, snapshots, prompts, settlements, and terminal results come out. Unity events remain presentation notifications only, never the authority for legality or next-state selection. Detailed boundaries are in [ARCHITECTURE.md](./ARCHITECTURE.md).

**Major components:**

1. **Unity composition and presenters** — retain scene assets, serialized references, input, uGUI/TMP/DOTween, audio, settings, and high-score persistence; render snapshots and submit prompt-scoped actions only.
2. **`SoloGameSession` / `SoloRound`** — own the timer-driven hand loop and adapt shared payment results to existing boost/distance rewards without importing four-seat counters.
3. **`HanchanGameSession` and action sources** — coordinate asynchronous human input and immediate deterministic dummy actions, cancel stale prompts, and publish read-only presentation state.
4. **`FourPlayerRound`** — sole per-hand state owner: wall, four round-player states, turn/phase, legal actions, response windows, and terminal `RoundResult`.
5. **`HanchanMatch`** — sole persistent match authority: four score accounts, dealer/round wind, honba, riichi pool, settlement application, round advance, and match end.
6. **Pure rule pipeline** — stable tile/meld values, seeded wall, exhaustive hand solver, explicit `WinContext`, yaku/fu/dora/yakuman evaluation, payment tables, and pure settlement transfer calculation.
7. **EditMode and PlayMode verification** — rule/property/transition/ledger tables below the scene; one small scene lifecycle suite above it.

**Patterns to enforce:** strangler adapters around current solo APIs; command-in/transition-result-out; pure calculation followed by one explicit state application; immutable snapshots; seed plus accepted action trace for reproduction; symmetric Unity subscription ownership; stale action rejection by actor, phase, and prompt ID.

### Critical Pitfalls

1. **Refactor before an executable baseline** — tag `portfolio-baseline`, capture deterministic behavior, pass solo start→finish/forfeit→restart smoke, and prove a Windows build before structural moves.
2. **Partial correctness fixes** — treat wall inventory/red fives/dead wall/shuffle as one invariant set; fix equality, exhaustive decomposition, candidate deduplication, yaku/fu/yakuman/dora, and final payer transfers as connected table-tested contracts.
3. **Mutable mode sharing and hidden state ownership** — share pure rule results, not the current mutable `MahjongRound`; keep solo progression separate and permit only `HanchanMatch` to mutate match scores/counters.
4. **Events, UI, or dummy recursion becoming the state machine** — use explicit phases and one validated submission path; always stop after a dummy discard for the human `Ron`/`Pass` window and reject stale, wrong-seat, duplicate, and post-terminal actions.
5. **Riichi/furiten reduced to UI flags** — store commitment, deposit, ippatsu, river, temporary furiten, and riichi-pass furiten in per-round player state and test their exact reset boundaries.
6. **Calls/kans added as buttons** — Stage 3 must include typed melds, altered hand cardinality, response priority, open scoring, live/dead-wall invariants, rinshan, indicators, and all three kan transitions; never ship a partially wired kan control.
7. **Editor success mistaken for release success** — repeat EditMode, PlayMode, `BuildReport`-checked Windows build, and built-player smoke after every structural phase; preserve `.meta` GUIDs and audit lifecycle subscriptions after scene/script changes.

## Implications for Roadmap

The roadmap should use seven phases. Phases 1–5 plus Phase 7 constitute the Stage 2 stable completion floor. Phase 6 is the explicit Stage 3 target and starts only at its quality gate; it is not allowed to displace the release phase.

### Phase 1: Executable Baseline and Verification Seam

**Rationale:** Existing code has no first-party tests, known restart faults, runtime editor imports, and one serialized composition root. Behavior must be classifiable before correction or migration.  
**Delivers:** `portfolio-baseline` tag, fixed seed/trace capture, clean Player compilation, one broad runtime/test assembly seam, characterization cases, solo start→end→restart PlayMode smoke, batch Windows x64 build method/wrapper, and initial build evidence.  
**Addresses:** solo preservation, restart stability, Windows build, reproducible demonstrations.  
**Avoids:** refactor-before-characterization, assembly/build surprises, duplicate subscriptions, and lost serialized references.  
**Exit gate:** the unchanged solo experience runs twice in the scene and in a built Player; known rule defects are recorded as intended corrections rather than frozen expectations.

### Phase 2: Correct Shared Rules Core

**Rationale:** Riichi, hanchan, furiten, and calls all inherit wall, hand, and payment defects; they cannot be built safely above the current behavior.  
**Delivers:** consistent tile identity/hash semantics; pure seeded 136-tile wall and dead-wall model; unbiased Fisher-Yates shuffle; exhaustive standard/chiitoitsu/kokushi solving; best legal interpretation; explicit win context; yaku/yakuman/dora/fu/payment results; table-driven regression coverage. After mixed types are separated, extract the pure no-engine rules assembly behind adapters.  
**Addresses:** all confirmed critical rule failures and shared-result equivalence for both modes.  
**Avoids:** fixing only the 136 count, losing decompositions through equality/deduplication, dora-only wins, flattened yakuman, and applying ron totals to tsumo.  
**Exit gate:** named wall, equality, ambiguous-decomposition, yaku/fu, dealer/non-dealer ron/tsumo, dora-only rejection, and multiple-yakuman cases are green; solo still builds and launches after each extraction step.

### Phase 3: Solo Preservation and Complete Riichi

**Rationale:** The existing mode is valuable portfolio functionality and must prove the new core can replace legacy rule ownership without forcing hanchan state into the arcade loop. Riichi must be completed before hanchan ron/furiten integration.  
**Delivers:** `SoloGameSession`/adapter progression using shared rule results; preserved timer, score, boost, distance, high score, forfeit and restart; valid riichi-discard selection, deposit, hand lock, ippatsu, ura-dora, and reset/carry behavior.  
**Addresses:** all Stage 1 features and the shared-engine differentiator.  
**Avoids:** `isSolo`/`isHanchan` branches in shared mutable rounds, duplicate scoring paths, button-only riichi, and lifecycle leaks.  
**Exit gate:** same-hand evaluation matches the shared core, only the solo adapter changes boost/distance, and full riichi plus second-run PlayMode and Windows build checks pass.

### Phase 4: Headless Fixed-Basic Hanchan

**Rationale:** Four-seat legality and settlement are easier to make deterministic and testable without UI timing. This phase establishes the complete Stage 2 domain contract.  
**Delivers:** four real seats and round-player states; explicit phases/prompts/actions; three exact-drawn-tile dummy policies; human discard/tsumo/ron/pass; response windows; complete furiten; exhaustive draw; central settlement ledger; riichi pool/honba/dealer continuation; East/South progression and fixed South 4 finish.  
**Addresses:** all non-presentation Stage 2 features, including dummy tenpai participation and truthful non-AI behavior.  
**Avoids:** event-driven legality, skipped ron windows, dummy wins, score mutation in several classes, and inconsistent end rules.  
**Exit gate:** transition matrix, stale/wrong-seat/no-op rejection, one-seat-rotation dummy trace, scripted ron, all noten distributions, ledger conservation, dealer repeat/rotation, and South 4 repeat-then-finish pass in EditMode.

### Phase 5: Hanchan Unity Integration

**Rationale:** Presentation should consume a proven state machine, not define it. Integrating after Phase 4 confines Unity-specific failures to adapters and scene wiring.  
**Delivers:** human and tsumogiri action sources, prompt cancellation, mode selection, minimal four-seat hands/rivers/turn/score/wind/wall/dora HUD, settlement/result flow, and truthful `Tsumogiri Dummy` labels.  
**Addresses:** user-observable Stage 2 hanchan, minimal HUD, settlement explanation, and two-mode presentation.  
**Avoids:** UI visibility as legality, late callbacks, hidden-information leaks, duplicate subscriptions, and art-scope expansion.  
**Exit gate:** scene PlayMode smoke covers solo and hanchan start/end/restart, exactly-once callbacks, prompt cancellation, and representative panel updates; the Windows build demonstrates a complete fixed-basic hanchan path.

### Phase 6: Human Calls, Open Hands, and All Kans — Conditional Stage 3 Target

**Rationale:** This is the user's target beyond the stable floor, but every feature changes the proven hand, wall, turn, scoring, and riichi contracts. It begins only after Phase 4 is fully green and Phase 5 has not destabilized the solo or hanchan build.  
**Delivers:** human chi, pon, daiminkan, ankan, and kakan; legal combination selection and pass; typed melds; post-call discard; kuikae; open-hand solver/yaku/fu; kan declaration/completion; rinshan, live-wall tail transfer, constant dead wall, indicator activation, rinshan scoring, kan-dora/kan-ura, ippatsu and riichi-after-ankan rules.  
**Addresses:** the complete Stage 3 ambition rather than a token call button.  
**Avoids:** concealed-hand assumptions, double draws, invalid 15-tile states, fifth kan, early wall exhaustion, open-hand over-scoring, and partial UI claims.  
**Entry/exit gate:** enter only with all Stage 2 domain/integration/build gates green and enough remaining time for a complete vertical slice. Exit only when chi/pon/all three kan types, open scoring, wall conservation, indicators, and scene actions pass. If that cannot be achieved, omit the incomplete interaction from the release and document Stage 3 as unshipped target scope.

### Phase 7: Portfolio Release and Evidence

**Rationale:** A correct Editor project is not a submission. Release proof and honest scope claims are part of the product.  
**Delivers:** final EditMode/PlayMode XML and logs, `BuildReport`-verified Windows x64 artifact, built-player solo/hanchan smoke, release tag/package, README, known limits, short demo, architecture snapshot, and three to five concise AI-assisted development cases tied to tests/diffs.  
**Addresses:** executable Windows build, before/after evidence, test results, demo, and AI-development record.  
**Avoids:** claims beyond the release commit, calling dummies AI, duplicate before/after source trees, and spending release time on infrastructure.  
**Exit gate:** the exact tagged build launches, both shipped modes complete their demonstrated paths, evidence names the seed/commit/test counts, and Stage 3 claims match what actually passed.

### Phase Ordering Rationale

- Characterization and Player build precede structural changes so failures remain attributable.
- Correct shared rules precede mode migration; preserved solo validates the shared-result seam before the larger hanchan state machine is added.
- Headless hanchan precedes UI because legality, response timing, and settlement must not depend on panels, events, or tweens.
- Stage 3 calls/kans follow the complete Stage 2 floor because they alter meld cardinality, transitions, wall accounting, indicators, scoring, and riichi simultaneously.
- Release verification is reserved as a real phase and repeated earlier as a gate; Stage 3 may not consume it.

### Research Flags

**Phases needing focused planning research or rule-contract validation:**

- **Phase 2:** enumerate the exact yaku/fu/yakuman/payment fixture matrix and validate the broad-runtime-to-pure-core assembly migration in Unity before file moves. Repository defects are known, but the final extraction sequence is unproven.
- **Phase 4:** settle the exact project South 4 repeat/end wording and encode it as one testable constant behavior. Do not add an agari-yame/rules configuration system.
- **Phase 6:** re-check authoritative rules and project overrides for call priority, kuikae, riichi-after-ankan structure/wait equivalence, kakan interruption boundary, rinshan/live-wall transfer, and kan-dora timing before implementation.

**Phases with standard or already well-documented patterns — skip additional research:**

- **Phase 1:** Unity 2022.3 assembly definitions, Test Framework command line, lifecycle pairing, and `BuildPipeline.BuildPlayer` are documented; planning should inspect compilation rather than research alternatives.
- **Phase 3:** solo preservation and adapter migration are repository-specific implementation work governed by existing behavior and tests.
- **Phase 5:** existing uGUI/TMP/DOTween presentation and action-source wiring need code/scene inspection, not ecosystem research.
- **Phase 7:** Markdown, Git tags/releases, Unity test output, build logs, and a smoke checklist are standard; avoid tooling research.

## Confidence Assessment

| Area | Confidence | Notes |
|------|------------|-------|
| Stack | HIGH | Editor/package versions and current assets are repository-pinned; the recommendation is deliberately no-upgrade/no-new-package. DOTween provenance and the exact asmdef reference list remain execution checks. |
| Features | MEDIUM | Scope and project overrides are authoritative and HIGH confidence; detailed riichi/call/kan rules are based on WRC/EMA primary sources classified MEDIUM and still need fixture-level validation. |
| Architecture | MEDIUM | Current coupling is directly evidenced and the ownership model is coherent, but action prompts, state transitions, and the two-step assembly migration have not yet compiled or run in this codebase. |
| Pitfalls | HIGH | Most failure modes map to confirmed source defects, current lifecycle/assembly constraints, or direct dependencies between requested features. |

**Overall confidence:** MEDIUM-HIGH. The stable floor, dependency order, and minimal stack are well supported; uncertainty is concentrated in unexecuted migration mechanics and Stage 3 rule interactions.

### Gaps to Address

- **Assembly migration proof:** compile after the broad runtime asmdef, then again after pure-core extraction. Add only compiler-proven references and never combine asmdef creation, broad renames, source moves, and scene edits.
- **Complete scoring fixture catalog:** Phase 2 planning must list representative closed/open, ron/tsumo, dealer/non-dealer, wait/fu, dora-only, ambiguous decomposition, and multiple-yakuman cases rather than rely on a coverage percentage.
- **South 4 terminal contract:** state precisely that South 4 dealer repeats under the fixed dealer-win/dealer-tenpai condition and the match ends when that dealer finally passes; confirm this against the user's intent before coding if any ambiguity remains.
- **Tile representation compatibility:** validate that corrected equality/hash semantics preserve tile string codes, ScriptableObject lookup, editor generator output, and serialized asset GUIDs.
- **Action-boundary behavior:** test cancellation and stale prompt rejection for delayed human input and synchronous dummy chains before trusting the proposed callback interface.
- **Stage 3 all-or-nothing release boundary:** define a time checkpoint during roadmap scheduling. The target remains chi/pon/all kan types, but no partially connected kan or open-hand path should be included in the tagged build.

## Sources

### Project and Repository Evidence — HIGH confidence

- `.planning/PROJECT.md` — project value, active requirements, nine-day deadline, Stage 2 floor, Stage 3 target, exclusions, baseline tag, and portfolio evidence contract.
- `.planning/config.json` — enabled research, plan check, verifier, Nyquist validation, UI safety, code review, and local planning workflow.
- `.planning/codebase/STACK.md`, `.planning/codebase/ARCHITECTURE.md`, `.planning/codebase/TESTING.md`, `.planning/codebase/CONCERNS.md` — current dependencies, scene composition root, missing first-party tests/asmdefs, and confirmed defects.
- `ProjectSettings/ProjectVersion.txt`, `Packages/manifest.json`, `Packages/packages-lock.json` — Unity/C#/package pins and installed test tooling.
- `Assets/Scripts/MahjongGameManager.cs`, `Assets/Scripts/AL-1S/MahjongRound.cs`, `MahjongUtilities.cs`, `MahjongYaku.cs`, `MahjongWinInfo.cs`, `MahjongTileAndBlock.cs`, `Utilities.cs`, `_Structs.cs` — current lifecycle, wall, solver, scoring, equality, and mixed-layer ownership.
- `Assets/Scripts/UI-Kozeki/PlayerHand.cs`, `GameUIManager.cs`, `UiScoreDistanceInfo.cs`, `Assets/Scripts/ScoreManagerDistance.cs`, `Assets/Scripts/Configs/SettingsManager.cs`, and `Assets/Scenes/SampleScene.unity` — input, subscription, presentation, solo service, persistence, and serialized-wiring seams.
- [STACK.md](./STACK.md), [FEATURES.md](./FEATURES.md), [ARCHITECTURE.md](./ARCHITECTURE.md), [PITFALLS.md](./PITFALLS.md) — the four detailed research inputs synthesized here.

### Authoritative Primary Sources — MEDIUM confidence through the research seam

- [World Riichi Championship rules index](https://www.worldriichi.org/wrc-rules) — current official rules and clarification index.
- [World Riichi Championship Rules 2022](https://www.worldriichi.org/s/WRC_Rules_2022_20220708_site.pdf) — detailed riichi, furiten, calls, kan, dead-wall, hanchan, scoring, and yakuman reference used under the project's explicit overrides.
- [European Mahjong Association Riichi Rules 2025](https://mahjong-europe.org/portal/images/docs/Riichi-rules-2025-EN.pdf) — modern cross-check for exhaustive draw, noten payment, dead wall, calls, and progression.
- [WRC Yaku and Fu reference](https://www.worldriichi.org/s/WRC_Yaku_1_5_3_black.pdf) — open/closed han and fu tables.
- [Unity 2022.3 assembly definitions](https://docs.unity3d.com/2022.3/Documentation/Manual/ScriptCompilationAssemblyDefinitionFiles.html) and [Assembly Definition properties](https://docs.unity3d.com/2022.3/Documentation/Manual/class-AssemblyDefinitionImporter.html) — predefined/named assembly behavior and **No Engine References**.
- [Unity Test Framework 1.1.33 EditMode/PlayMode guidance](https://docs.unity3d.com/Packages/com.unity.test-framework@1.1/manual/edit-mode-vs-play-mode-tests.html) and [command-line test reference](https://docs.unity3d.com/Packages/com.unity.test-framework@1.1/manual/reference-command-line.html) — test assembly boundaries and repeatable runner output.
- [Unity 2022.3 `BuildPipeline.BuildPlayer`](https://docs.unity3d.com/2022.3/Documentation/ScriptReference/BuildPipeline.BuildPlayer.html) — `BuildReport`-based Player verification.
- [GitHub Docs: About releases](https://docs.github.com/en/repositories/releasing-projects-on-github/about-releases) — tag-based release notes and binary attachments.

---
*Research completed: 2026-08-28*  
*Ready for roadmap: yes*
