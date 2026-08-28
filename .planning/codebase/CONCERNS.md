# Codebase Concerns

**Analysis Date:** 2026-08-28

## Tech Debt

**Monolithic rules and scoring implementation:**
- Issue: Core hand decomposition, yaku detection, fu calculation, score calculation, and round progression are concentrated in large, tightly coupled classes. `MahjongYakuSolver` consumes a wide mutable `MahjongHandInfo` snapshot, while `MahjongRound` invokes the complete solver directly from gameplay events.
- Files: `Assets/Scripts/AL-1S/MahjongYaku.cs`, `Assets/Scripts/AL-1S/MahjongUtilities.cs`, `Assets/Scripts/AL-1S/MahjongWinInfo.cs`, `Assets/Scripts/AL-1S/MahjongRound.cs`
- Impact: Rule changes can alter recognition, score, and UI-visible results at once. The 866-line yaku file and 561-line utility file have no automated characterization tests, so regressions are difficult to localize.
- Fix approach: Keep the current public flow, but isolate pure wall construction, hand decomposition, yaku evaluation, fu calculation, and payment calculation behind small pure functions. Add table-driven tests before moving behavior.

**All runtime scripts share the default assembly:**
- Issue: No first-party `.asmdef` or `.asmref` files exist. Runtime UI, game rules, persistence, and audio compile into `Assembly-CSharp`; only `Assets/Editor/` receives Unity's folder-based editor isolation.
- Files: `Assets/Scripts/`, `Assets/Editor/MahjongTileDataGenerator.cs`
- Impact: Runtime code can accidentally acquire editor-only or optional-package dependencies, as already occurs with `UnityEditor` and `Unity.VisualScripting` imports. Any script error blocks the whole gameplay assembly.
- Fix approach: First remove invalid/unused imports. Add assemblies only where they enforce a real boundary: one runtime rules assembly, one Unity-facing runtime assembly, and dedicated EditMode/PlayMode test assemblies.

**State is split across singletons, scene references, and mutable model objects:**
- Issue: `MahjongGameManager`, `GameUIManager`, and `AudioManager` expose static instances; `MahjongRoundInfo.NextRoundOnWin` and `NextRoundOnYuguk` mutate and return the same reference; UI components subscribe directly to manager events.
- Files: `Assets/Scripts/MahjongGameManager.cs`, `Assets/Scripts/UI-Kozeki/GameUIManager.cs`, `Assets/Scripts/SoundArchive/AudioManager.cs`, `Assets/Scripts/AL-1S/_Structs.cs`
- Impact: Scene reload, game restart, or duplicate manager objects can leave stale static references, duplicate event handlers, and state that is difficult to reset deterministically.
- Fix approach: Define one explicit game-session owner. Clear static instances on destruction, pair every subscription with unsubscription, and make next-round data either intentionally mutable with a single owner or returned as a fresh value.

**Settings systems are disconnected:**
- Issue: `SettingsManager` serializes `SoundSettings` and `InputSettings` to `yaml.json`, while `AudioManager` independently loads and saves volumes through `PlayerPrefs`; `InputPreset` remains hard-coded; the configuration button is empty.
- Files: `Assets/Scripts/Configs/SettingsManager.cs`, `Assets/Scripts/Configs/Settings.cs`, `Assets/Scripts/SoundArchive/AudioManager.cs`, `Assets/Scripts/InputManager.cs`, `Assets/Scripts/UI-Kozeki/UiManager.cs`
- Impact: Saved settings are not the settings applied at runtime, and two persistence formats can disagree. The misleading `.json` content in a file named `yaml.json` also complicates support and migrations.
- Fix approach: Select one persistence path, validate loaded values, apply it once at session startup, and route audio/input UI through that same model. Rename the file only with an explicit migration from the existing path.

**Dead and placeholder code remains in production sources:**
- Issue: Empty managers and handlers, commented-out alternate implementations, unused fields, and compile symbols obscure the active path. Examples include `DBManager`, `LocalizationManager`, `InputManager`, `RiichiHandler`, `CheckRiichii`, `CheckTsumoAgari`, `UiScoreInfo.AlterScore`, and `MahjongWinInfo.UpdateDora`.
- Files: `Assets/Scripts/DBManager.cs`, `Assets/Scripts/LocalizationManager.cs`, `Assets/Scripts/InputManager.cs`, `Assets/Scripts/MahjongGameManager.cs`, `Assets/Scripts/UI-Kozeki/UiScoreInfo.cs`, `Assets/Scripts/AL-1S/MahjongWinInfo.cs`
- Impact: Serialized components can appear functional while doing nothing, and maintainers must distinguish active code from abandoned experiments.
- Fix approach: Remove unreferenced stubs. For required features, implement the smallest end-to-end path and keep unimplemented actions disabled in the UI until they work.

## Known Bugs

**Player builds reference editor-only assemblies:**
- Symptoms: A non-Editor Unity player compilation fails because runtime scripts outside `Assets/Editor/` import `UnityEditor` namespaces without `#if UNITY_EDITOR` guards. The imports are unused by the runtime logic.
- Files: `Assets/Scripts/AL-1S/MahjongTileAndBlock.cs:9`, `Assets/Scripts/UI-Kozeki/UiScoreDistanceInfo.cs:9`
- Trigger: Build any player target from the Unity Editor.
- Workaround: Remove the unused `using UnityEditor.Rendering;` and `using UnityEditor;` directives. Do not add editor assembly references to runtime code.

**Wall shuffle is biased:**
- Symptoms: The final array element is never chosen by a swap. `Utilities.ShuffleArray` calls `prng.Next(i, array.Length - 1)`, whose upper bound is exclusive, and iterates only to `array.Length - 2`.
- Files: `Assets/Scripts/AL-1S/Utilities.cs:44`
- Trigger: Generate any wall through `MahjongRound.GenerateYama`.
- Workaround: Implement Fisher-Yates with `prng.Next(i, array.Length)` or a descending loop with an inclusive candidate range, then verify determinism by seed and permutation coverage.

**Generated wall has 139 tiles and five copies of each suited five:**
- Symptoms: `GenerateYama` creates three copies of every base tile, then appends `GetAllTiles(true)`. That second list contains all 34 base tiles plus three red fives, producing 102 + 37 = 139 tiles. Each suit receives four normal fives plus one red five.
- Files: `Assets/Scripts/AL-1S/MahjongRound.cs:211`, `Assets/Scripts/AL-1S/MahjongTileAndBlock.cs:490`
- Trigger: Start any round.
- Workaround: Construct four copies per tile and replace one normal five in each suit with its red variant; assert total count 136 and count four per `TileID`.

**Hand decomposition misses valid/scoring interpretations:**
- Symptoms: `Has4Body` tries only two greedy strategies—sequence-first and triplet-first—and `CheckWinnable` stops after the first head candidate whose greedy decomposition succeeds. Hands requiring backtracking, or a later head/decomposition that scores higher, can be rejected or undervalued.
- Files: `Assets/Scripts/AL-1S/MahjongUtilities.cs:48`, `Assets/Scripts/AL-1S/MahjongUtilities.cs:73`, `Assets/Scripts/AL-1S/MahjongUtilities.cs:182`
- Trigger: Evaluate an ambiguous hand in which removing an early sequence/triplet prevents a valid later partition, or multiple heads/decompositions yield different yaku/fu.
- Workaround: Recursively consume the lowest remaining tile, branching over pair/triplet/sequence choices, and retain every unique complete decomposition before selecting the highest payment.

**Yakuman and multiple-yakuman scoring is inconsistent:**
- Symptoms: Every yakuman is encoded as 12 han, the generic han/fu function maps 11–12 han to base score 6000, and 13+ han is capped at base score 8000. `GetBaseScoreByYakuman` exists but is unused, so combined yakuman cannot reach the double/triple values represented by `UniqueName`.
- Files: `Assets/Scripts/AL-1S/MahjongYaku.cs:101`, `Assets/Scripts/AL-1S/MahjongUtilities.cs:417`, `Assets/Scripts/AL-1S/MahjongUtilities.cs:442`, `Assets/Scripts/AL-1S/MahjongWinInfo.cs:302`
- Trigger: Score a yakuman with no added han that pushes the total to 13, or score two or more simultaneous yakuman.
- Workaround: Count yakuman separately, discard ordinary han/dora when a yakuman exists, and call one payment function with the yakuman multiplier.

**Tsumo settlement uses ron totals:**
- Symptoms: `CheckTsumoWin` constructs a tsumo win, but `HandlePlayerWin` awards `oyaRon` or `zaRon`. `UiWinInfo` displays the same ron amount. Rounded tsumo payments can differ from the ron total.
- Files: `Assets/Scripts/AL-1S/MahjongRound.cs:401`, `Assets/Scripts/AL-1S/MahjongRound.cs:435`, `Assets/Scripts/UI-Kozeki/UiWinInfo.cs:86`, `Assets/Scripts/AL-1S/MahjongWinInfo.cs:338`
- Trigger: Win by tsumo, especially a hand whose independently rounded payments do not sum to the ron value.
- Workaround: For dealer tsumo use `oyaTsumo * 3`; for non-dealer tsumo use `zaTsumoToOya + zaTsumoToZa * 2`, and display the payment breakdown or the same computed total.

**Restarting a game duplicates subscriptions and toggles required panels off:**
- Symptoms: Every `StartNewGame` calls `UiScoreDistanceInfo.Construct` again without first unsubscribing, so active UI receives duplicate score events. `GameUIManager.Initialize` uses `TogglePanel` for `RoundInfo` and `PlayerHand`; on a second game those active panels are hidden, while `GameOver` and back panels are not explicitly cleared.
- Files: `Assets/Scripts/MahjongGameManager.cs:50`, `Assets/Scripts/UI-Kozeki/UiScoreDistanceInfo.cs:43`, `Assets/Scripts/UI-Kozeki/GameUIManager.cs:67`
- Trigger: Finish or forfeit a game, then start another game in the same scene.
- Workaround: Make initialization idempotent: unsubscribe before replacing a service, explicitly set each panel's desired state, and reset all game-over UI before session start.

**Player hand overflow logs and then crashes:**
- Symptoms: `PlayerHand.FillHand` detects `index >= tilesInHand.Length` but does not stop; it immediately accesses `tilesInHand[index]`, causing `IndexOutOfRangeException`. Hands shorter than 13 leave stale visuals enabled.
- Files: `Assets/Scripts/UI-Kozeki/PlayerHand.cs:88`
- Trigger: Pass any list with more than 13 tiles, or a malformed round state; pass fewer than 13 to observe stale slots.
- Workaround: Reject any count other than 13 before mutation, or render exactly the accepted range and explicitly clear unused slots.

**Game-over state in `PlayerHand` is immediately undone:**
- Symptoms: `PlayerHand.Start` subscribes both `HandleGameOver` and `HandleGameStart` to `OnGameOver`. The first sets `_isGameOver = true`; the second immediately resets it to false. `_isGameOver` is also not consulted by `Update`.
- Files: `Assets/Scripts/UI-Kozeki/PlayerHand.cs:47`
- Trigger: Let the timer expire or choose forfeit.
- Workaround: Subscribe `HandleGameStart` to `MahjongGameManager.OnGameStart`, unsubscribe both on destruction/disable, and guard input with the game-over/session state if the field remains.

**Timer publishes a negative second at expiry:**
- Symptoms: `Timer.Update` calls `CheckTimerTick`, which can emit `OnTimeTick(-1)`, before clamping `RemainingTime` to zero and invoking `OnTimerFinished`. The UI can briefly format a negative value.
- Files: `Assets/Scripts/Timer.cs:35`, `Assets/Scripts/UI-Kozeki/UiRemainingTimeIndicator.cs:29`
- Trigger: Allow a timer to cross from a positive fraction below one second to a negative value in one frame.
- Workaround: Clamp before deriving/emitting seconds and emit the final zero tick once.

**Audio singleton initialization continues after destroying a duplicate:**
- Symptoms: The duplicate branch calls `Destroy(gameObject)` but continues to obtain components, locate an `AudioListener`, create three child objects, and load preferences. The primary object is also parented under a scene `AudioListener` after `DontDestroyOnLoad`, undermining persistent-root ownership.
- Files: `Assets/Scripts/SoundArchive/AudioManager.cs:28`
- Trigger: Load a scene containing a second `AudioManager`, or change scenes while the listener belongs to the outgoing scene.
- Workaround: Return immediately after destroying a duplicate. Keep the persistent manager as a root object and follow the active listener without reparenting the manager.

## Security Considerations

**Client-controlled save data is trusted:**
- Risk: `highScore`, input settings, and sound settings are loaded directly from a user-writable JSON file with no schema/range validation or integrity check. This is acceptable for an offline personal score, but it cannot support trusted leaderboards, unlocks, or competitive progression.
- Files: `Assets/Scripts/Configs/SettingsManager.cs`, `Assets/Scripts/Configs/Settings.cs`, `Assets/Scripts/MahjongGameManager.cs:92`
- Current mitigation: The path is fixed under `Application.persistentDataPath`; Json.NET default concrete-type deserialization is used, and no network upload or privileged action consumes the values.
- Recommendations: Validate fields and reset invalid values on load. If scores leave the device, treat every client value as untrusted and verify results server-side rather than adding a client-side checksum as security.

**Unbounded local JSON read:**
- Risk: The entire save file is loaded into memory before parsing, so a locally replaced oversized file can cause excessive allocation or a startup hitch.
- Files: `Assets/Scripts/Configs/SettingsManager.cs:47`
- Current mitigation: The file is local and the normal writer produces a very small fixed-shape object.
- Recommendations: Check file length against a small documented maximum before reading and preserve the invalid file for diagnosis rather than silently trusting it.

**External attack surface:**
- Risk: Not detected in first-party runtime code. There are no HTTP, socket, process-launch, native interop, account, authentication, or remote-content paths under `Assets/Scripts/`.
- Files: `Assets/Scripts/`, `ProjectSettings/UnityConnectSettings.asset`
- Current mitigation: Unity Ads, Analytics, and Crash Reporting are disabled in `ProjectSettings/UnityConnectSettings.asset`.
- Recommendations: Reassess the trust boundary when networking, cloud saves, downloadable assets, or public leaderboards are introduced.

## Performance Bottlenecks

**Riichi availability performs hundreds of full hand solves per draw:**
- Problem: `TsumoInfo` calls `IsRiichiAble` after every draw. It tries 14 discards; each calls `FindAgariTiles`, which tries all 34 tile types and sorts/allocates a new hand for every candidate. This is at least 14 × 34 = 476 candidate solves per draw, plus the actual tsumo-win solve, all on Unity's main thread.
- Files: `Assets/Scripts/AL-1S/_Structs.cs:119`, `Assets/Scripts/AL-1S/MahjongRound.cs:79`, `Assets/Scripts/AL-1S/MahjongUtilities.cs:20`
- Cause: Brute-force candidate enumeration, repeated `List` copies/sorts, LINQ counts, block allocations, and full yaku/fu construction are used when only tenpai existence is initially needed.
- Improvement path: Profile first. Cache a 34-entry tile-count representation per hand, use a structural tenpai check for button availability, and build full `MahjongWinInfo` only for retained winning candidates.

**Distance catch-up is frame-rate capped:**
- Problem: `ScoreManagerDistance.GetDistance` transfers only one `distanceMinUnit` from `accumulatedScore` per frame even when the backlog contains many units. With the defaults, visible distance can advance at most 0.1 × FPS units/second; at 30 FPS this is 3 units/second while maximum boost generates 6 units/second, excluding instant score awards.
- Files: `Assets/Scripts/ScoreManagerDistance.cs:75`
- Cause: An `if` drains one quantum instead of computing all elapsed quanta.
- Improvement path: Transfer `floor(accumulatedScore / distanceMinUnit)` quanta in one update and emit one consolidated distance event.

**UI animation commands can overlap:**
- Problem: `ShowPanel`, `ActivePanel`, `DeactivePanel`, and `TogglePanel` start new DOTween sequences without killing or completing a prior sequence for the same panel. A stale completion callback can disable a panel that has just been reactivated.
- Files: `Assets/Scripts/UI-Kozeki/UiManager.cs`, `Assets/Scripts/UI-Kozeki/GameUIManager.cs`, `Assets/Scripts/UI-Kozeki/UiTransition.cs`
- Cause: Tween ownership is tracked only for one volatile delay, not per panel transition.
- Improvement path: Kill the target `RectTransform` and `CanvasGroup` tweens before starting the next transition, then set an explicit final active/position/alpha state.

**Per-frame tile transform writes:**
- Problem: Every active `MahjongTileGameObject` writes `localPosition` every frame even when selection has not changed.
- Files: `Assets/Scripts/UI-Kozeki/MahjongTileGameObject.cs:55`
- Cause: Presentation is polled in `Update` rather than applied by `SetSelected`/`ToggleSelected`.
- Improvement path: Move the transform update into the selection setters; no frame loop is required.

## Fragile Areas

**Mahjong tile equality and hashing:**
- Files: `Assets/Scripts/AL-1S/MahjongTileAndBlock.cs:333`, `Assets/Scripts/AL-1S/MahjongWinInfo.cs:441`
- Why fragile: `MahjongTile.operator ==` compares only `TileID`, while `Equals` uses the struct's field-wise default and `GetHashCode` uses only `TileID`. Identical tile identities with different dora flags therefore behave differently under `==`, `List.Contains`, and hash collections. `MahjongWinInfo` equality ignores `winTile`, but its hash code includes `winTile`, violating the equal-objects/equal-hash contract.
- Safe modification: Define identity semantics explicitly with `IEquatable<T>` and make `==`, `Equals`, and `GetHashCode` use the same fields. Separate tile identity from per-instance dora metadata if both comparisons are needed.
- Test coverage: No equality/hash collection tests exist for red dora, marked dora, alternate winning tiles, or duplicate win interpretations.

**Scene-serialized manager graph:**
- Files: `Assets/Scenes/SampleScene.unity`, `Assets/Scripts/MahjongGameManager.cs`, `Assets/Scripts/UI-Kozeki/GameUIManager.cs`, `Assets/Scripts/UI-Kozeki/PlayerHand.cs`, `Assets/Scripts/MahjongTileDatabase.cs`
- Why fragile: Managers dereference many serialized fields and singleton instances without validation. Execution order is assumed: UI `Start` expects `MahjongGameManager.Instance`, tile visuals expect its `TileDB`, and `AudioManager` expects an `AudioListener`.
- Safe modification: Add `OnValidate` checks for required serialized references, fail fast in `Awake` with component-specific messages, and avoid cross-singleton work before all owners are initialized.
- Test coverage: No PlayMode smoke test loads `Assets/Scenes/SampleScene.unity`, starts a game, completes/forfeits it, and starts a second game.

**Event lifecycle across enable/disable and restart:**
- Files: `Assets/Scripts/UI-Kozeki/UiScoreDistanceInfo.cs`, `Assets/Scripts/UI-Kozeki/UiRemainingTimeIndicator.cs`, `Assets/Scripts/UI-Kozeki/GameUIManager.cs`, `Assets/Scripts/UI-Kozeki/PlayerHand.cs`, `Assets/Scripts/MahjongGameManager.cs`
- Why fragile: Some handlers unsubscribe only when an event fires, some never unsubscribe, and `UiScoreDistanceInfo.OnDisable` dereferences `_scv` even if `Construct` never ran. Re-enabling or reconstructing can add duplicate delegates.
- Safe modification: Use symmetric `OnEnable`/`OnDisable` subscriptions where the publisher lifetime is known; otherwise unsubscribe the old dependency inside `Construct` before replacement. Null-check optional dependencies.
- Test coverage: No tests cover disabling UI before construction, repeated construction, scene unload, or game restart.

**Tile database initialization:**
- Files: `Assets/Scripts/MahjongTileDatabase.cs`, `Assets/ScriptableObjects/MahjongTileDatabase.asset`, `Assets/Editor/MahjongTileDataGenerator.cs`
- Why fragile: `OnEnable` calls `greatAssets.ToDictionary`; a null list or duplicate `code` throws and prevents all tile rendering. Lookup keys depend on string encoding matching `MahjongTile.ToString` exactly.
- Safe modification: Validate null/duplicate/invalid entries in the editor generator and `OnValidate`; build the runtime map with explicit duplicate diagnostics and test all 37 configured codes.
- Test coverage: No database completeness or duplicate-code test exists.

**Save failure recovery:**
- Files: `Assets/Scripts/Configs/SettingsManager.cs`
- Why fragile: Saves overwrite the destination directly, loads replace malformed data with defaults without quarantining it, and logging through `MyLogger` is compiled out by `#undef HIMARI`. A process interruption can leave no recoverable last-known-good file.
- Safe modification: Write to a sibling temporary file, flush/close, then atomically replace the destination; preserve a corrupt file with a diagnostic suffix and use active error logging.
- Test coverage: No round-trip, truncated-file, invalid-field, permission-denied, or interrupted-write tests exist.

## Scaling Limits

**Single-player 14-tile solver:**
- Current capacity: One local player's 13-tile hand plus one draw, evaluated synchronously through the 34 tile identities.
- Limit: Adding opponents, hints for many candidate hands, batch simulations, or AI search multiplies the current 476+ candidate-solve draw path and stalls the main thread.
- Scaling path: Use count-array pure functions suitable for memoization and background evaluation; keep Unity objects and UI events outside the solver.

**Fixed game model:**
- Current capacity: One `MahjongPlayer`, one river, concealed four-body/one-head plus chiitoitsu/kokushi recognition, and tsumo-only settlement.
- Limit: Open melds, four-player payments, ron, calls, kan/rinshan progression, furiten, and simultaneous player state do not fit the current single-player ownership and hard-coded `isTsumo = true` construction.
- Scaling path: Only if those modes enter scope, introduce an explicit table state with players, turn owner, melds, win source, and payment transfers before adding more call handlers.

**Audio concurrency:**
- Current capacity: Two persistent music sources and one 2D SFX source; positional SFX use `AudioSource.PlayClipAtPoint` temporary objects.
- Limit: Dense overlapping SFX creates transient GameObjects and provides no per-channel concurrency limit or reuse.
- Scaling path: Keep the current approach for sparse UI/game sounds. Add a small reusable source pool only after profiling shows allocation or voice-count problems.

## Dependencies at Risk

**Manually vendored DOTween and DOTween Pro binaries:**
- Risk: DOTween is stored as DLLs, editor binaries, generated modules, and source files under `Assets/Plugins/Demigiant/` rather than a package-manager dependency. The bundled readmes identify copyright 2014–2018, while DLL metadata reports only `1.0.0.0`, so the exact upstream release and update state are not machine-verifiable from the manifest.
- Impact: Security/compatibility updates and Unity upgrade testing are manual; partial replacement can mismatch Pro/editor/runtime modules.
- Migration plan: Record the licensed upstream release, retain the license/proof of purchase outside generated build output, and upgrade the whole vendor directory through DOTween's supported setup process in one verified change.

**Unnecessary optional-package coupling:**
- Risk: Multiple runtime files import `Unity.VisualScripting`, `Unity.Burst`, `System.Drawing`, `System.Threading`, `System.Net.Http.Headers`, or experimental UI namespaces without using them. Seven runtime files import `Unity.VisualScripting`, making an otherwise unused package appear mandatory.
- Impact: Package removal or platform changes can break compilation for code that does not use the package feature; review tools cannot easily distinguish real integration from stray imports.
- Migration plan: Remove unused imports first, then remove manifest packages only after a reference scan and player-build verification.

**Broad Unity package surface:**
- Risk: `Packages/manifest.json` includes Collab Proxy, three IDE integrations, Timeline, Visual Scripting, URP, and the full 2D feature set for a single-scene UI game. This is a maintenance and import-time risk, not evidence that any listed package is defective.
- Impact: More transitive packages increase upgrade conflict and editor import surface.
- Migration plan: Keep packages with serialized asset or code references; remove only packages proven unused by source, scenes, prefabs, and project settings, one at a time with an editor reimport and player build.

## Missing Critical Features

**Declared player calls are not implemented:**
- Problem: `RiichiHandler` is empty; Ron, Chii, Pon, Kan, Nukidora, and several other `PlayerCallType` cases do nothing. Riichi state, ippatsu, ura-dora, and call/open-hand state are hard-coded false or zero in `MahjongHandInfo`.
- Blocks: Complete riichi rules, open-hand scoring, ron, kan/rinshan, ura-dora, and reliable yaku evaluation for those flows.
- Files: `Assets/Scripts/MahjongGameManager.cs:269`, `Assets/Scripts/AL-1S/MahjongWinInfo.cs:208`, `Assets/Scripts/AL-1S/MahjongYaku.cs`

**Configuration, localization, statistics, and input rebinding are placeholders:**
- Problem: The configuration button is empty; `InputManager`, `LocalizationManager`, and `DBManager` have no behavior; `StatisticsData` is empty; UI yaku names and the han suffix are hard-coded.
- Blocks: User-configurable controls, application of serialized settings, language switching, and meaningful statistics screens.
- Files: `Assets/Scripts/UI-Kozeki/UiManager.cs:203`, `Assets/Scripts/InputManager.cs`, `Assets/Scripts/LocalizationManager.cs`, `Assets/Scripts/DBManager.cs`, `Assets/Scripts/Configs/Settings.cs`, `Assets/Scripts/UI-Kozeki/UiYakuPreset.cs`

**Normal music selection is disconnected:**
- Problem: `MusicManager.PlayMusic` always leaves `music` null because scene-to-track selection is commented out; it returns before calling `AudioManager`. Only the debug right-bracket path plays `mainMusic`.
- Blocks: Menu/game music during normal navigation.
- Files: `Assets/Scripts/SoundArchive/MusicManager.cs`

**Reproducible game sessions are unavailable:**
- Problem: `MahjongGameManager.StartNewGame` uses `new System.Random()` and the fixed seed path is disabled with `#undef IROHA`; the active seed is not recorded in save data or logs.
- Blocks: Reproducing a reported wall, deterministic regression tests, daily challenges, and fair run verification.
- Files: `Assets/Scripts/MahjongGameManager.cs:1`, `Assets/Scripts/MahjongGameManager.cs:60`, `Assets/Scripts/AL-1S/MahjongRound.cs`

## Test Coverage Gaps

**No first-party automated tests:**
- What's not tested: No `*Test*.cs`, test assembly definitions, EditMode suites, or PlayMode suites exist even though `com.unity.test-framework` is installed.
- Files: `Assets/Scripts/`, `Packages/manifest.json`
- Risk: Rule, persistence, lifecycle, and player-build regressions can ship without a runnable signal.
- Priority: High

**Wall generation and deterministic randomness:**
- What's not tested: Total tile count, four-copy invariant, red-five replacement, dora dead wall setup, seeded determinism, and unbiased shuffle reachability.
- Files: `Assets/Scripts/AL-1S/Utilities.cs`, `Assets/Scripts/AL-1S/MahjongRound.cs`, `Assets/Scripts/AL-1S/MahjongTileAndBlock.cs`
- Risk: The current 139-tile and biased-shuffle defects directly alter game fairness and every downstream probability.
- Priority: High

**Hand recognition, yaku, fu, and payment tables:**
- What's not tested: Ambiguous decompositions, every wait type, chiitoitsu, kokushi, each yaku, mutually exclusive yaku, dora, limit hands, multiple yakuman, dealer/non-dealer ron, and rounded tsumo payments.
- Files: `Assets/Scripts/AL-1S/MahjongUtilities.cs`, `Assets/Scripts/AL-1S/MahjongYaku.cs`, `Assets/Scripts/AL-1S/MahjongWinInfo.cs`
- Risk: Incorrect availability prompts and score awards are the central gameplay result.
- Priority: High

**Equality and collection behavior:**
- What's not tested: `MahjongTile` and `MahjongWinInfo` consistency across operators, `Equals`, `GetHashCode`, `HashSet`, LINQ, red tiles, and dora metadata.
- Files: `Assets/Scripts/AL-1S/MahjongTileAndBlock.cs`, `Assets/Scripts/AL-1S/MahjongWinInfo.cs`
- Risk: Duplicate/missing candidates and inconsistent lookup behavior can vary by collection API.
- Priority: High

**Session and UI lifecycle:**
- What's not tested: First start, timer expiry, forfeit, second start, repeated panel transitions, service reconstruction, disable-before-construct, and scene reload with persistent managers.
- Files: `Assets/Scenes/SampleScene.unity`, `Assets/Scripts/MahjongGameManager.cs`, `Assets/Scripts/UI-Kozeki/GameUIManager.cs`, `Assets/Scripts/UI-Kozeki/UiScoreDistanceInfo.cs`, `Assets/Scripts/SoundArchive/AudioManager.cs`
- Risk: Restart and event bugs emerge only after a full lifecycle, not during a single happy-path run.
- Priority: High

**Save-data resilience:**
- What's not tested: Default creation, round trip, old/missing fields, null nested objects, invalid ranges, malformed JSON, oversized files, interrupted writes, and permission failures.
- Files: `Assets/Scripts/Configs/SettingsManager.cs`, `Assets/Scripts/Configs/Settings.cs`
- Risk: High scores and user settings can reset or fail silently; invalid nested data can later cause null references.
- Priority: Medium

**Player build compilation:**
- What's not tested: A headless or CI player build that excludes editor assemblies.
- Files: `Assets/Scripts/AL-1S/MahjongTileAndBlock.cs`, `Assets/Scripts/UI-Kozeki/UiScoreDistanceInfo.cs`, `ProjectSettings/EditorBuildSettings.asset`
- Risk: Editor play mode can remain green while shipping builds fail at compile time.
- Priority: High

---

*Concerns audit: 2026-08-28*
