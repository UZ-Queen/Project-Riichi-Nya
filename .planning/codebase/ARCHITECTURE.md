<!-- refreshed: 2026-08-28 -->
# Architecture

**Analysis Date:** 2026-08-28

## System Overview

```text
┌────────────────────────────────────────────────────────────┐
│              Unity scene and serialized composition root       │
│                 `Assets/Scenes/SampleScene.unity`              │
├──────────────────┬──────────────────┬───────────────────────┤
│ Menu/UI routing  │  Game UI/input   │  Timer / score / audio │
│ `UiManager.cs`   │ `UI-Kozeki/`    │ `Assets/Scripts/`     │
└────────┬─────────┴────────┬─────────┴────────────┬──────────┘
         │                  │                      │
         └──────────────────▼─────────────────────┘
┌─────────────────────────────────────────────────────────────┐
│                    Application coordinator                    │
│             `Assets/Scripts/MahjongGameManager.cs`           │
└──────────────────────────────┬──────────────────────────────┘
                             │ events and method calls
                             ▼
┌────────────────────────────────────────────────────────────┐
│                     Mahjong domain model                      │
│ `Assets/Scripts/AL-1S/MahjongRound.cs` and solver/value types │
└────────────────────────────┬────────────────────────────────┘
                           │
             ┌─────────────┴─────────────┐
             ▼                           ▼
┌─────────────────────────┐  ┌──────────────────────────────┐
│ ScriptableObject tile DB│  │ JSON / PlayerPrefs persistence│
│ `ScriptableObjects/`    │  │ `Configs/SettingsManager.cs` │
└─────────────────────────┘  └──────────────────────────────┘
```

## Component Responsibilities

| Component | Responsibility | File |
|-----------|----------------|------|
| Scene composition root | Instantiates the only build scene and wires managers, UI presenters, timer, score service, prefabs, and the tile database through serialized references. | `Assets/Scenes/SampleScene.unity` |
| Menu UI coordinator | Owns menu panel history and starts gameplay from Unity button callbacks. | `Assets/Scripts/UI-Kozeki/UiManager.cs` |
| Game application coordinator | Starts and ends games, owns the current round/player, gates input by `GameState`, bridges domain events to UI, and persists the high score. | `Assets/Scripts/MahjongGameManager.cs` |
| Round aggregate | Owns wall/dead-wall state, hand/river/player state, turn progression, win/exhaustive-draw resolution, scoring deltas, and next-round creation. | `Assets/Scripts/AL-1S/MahjongRound.cs` |
| Hand solver and scoring model | Enumerates winning arrangements, derives yaku/han/fu, and builds score tables. | `Assets/Scripts/AL-1S/MahjongUtilities.cs`, `Assets/Scripts/AL-1S/MahjongYaku.cs`, `Assets/Scripts/AL-1S/MahjongWinInfo.cs` |
| Game UI coordinator | Maps `GameUIState` values to serialized panels and performs DOTween transitions. | `Assets/Scripts/UI-Kozeki/GameUIManager.cs`, `Assets/Scripts/UI-Kozeki/UiTransition.cs` |
| Player input presenter | Creates the visual hand, handles legacy `Input` polling/DAS-ARR navigation, and emits discard/call events. | `Assets/Scripts/UI-Kozeki/PlayerHand.cs`, `Assets/Scripts/DASandARR.cs` |
| Distance score service | Converts mahjong score into boost/distance and publishes rank/distance events behind `IScoreDistanceService`. | `Assets/Scripts/ScoreManagerDistance.cs`, `Assets/Scripts/IScoreDistanceService.cs` |
| Tile asset lookup | Maps domain tile codes to sprites stored in a generated ScriptableObject. | `Assets/Scripts/MahjongTileDatabase.cs`, `Assets/ScriptableObjects/MahjongTileDatabase.asset` |
| Persistence | Loads and saves high score, sound, input, and statistics DTOs as JSON under `Application.persistentDataPath`. | `Assets/Scripts/Configs/SettingsManager.cs`, `Assets/Scripts/Configs/Settings.cs` |
| Audio subsystem | Provides global music/SFX playback, a named clip archive, and scene-change music switching. | `Assets/Scripts/SoundArchive/AudioManager.cs`, `Assets/Scripts/SoundArchive/SoundArchive.cs`, `Assets/Scripts/SoundArchive/MusicManager.cs` |
| Editor asset generator | Regenerates the tile database from tile sprites through a Unity `Tools` menu. | `Assets/Editor/MahjongTileDataGenerator.cs` |

## Pattern Overview

**Overall:** Scene-centric Unity monolith with an event-driven application coordinator and a mostly plain-C# mahjong domain model, composed in `Assets/Scenes/SampleScene.unity`.

**Key Characteristics:**
- Use the scene as the composition root: serialized fields in `Assets/Scripts/MahjongGameManager.cs`, `Assets/Scripts/UI-Kozeki/GameUIManager.cs`, and `Assets/Scripts/UI-Kozeki/UiManager.cs` are wired in `Assets/Scenes/SampleScene.unity`.
- Keep rules and scoring in the POCO/struct domain types under `Assets/Scripts/AL-1S/`; `MahjongRound` exposes events rather than directly referencing UI types.
- Route the primary runtime flow through `MahjongGameManager` in `Assets/Scripts/MahjongGameManager.cs`; UI sends intent upward, the round mutates domain state, and events send results back to presenters.
- Use interface injection only at the existing score-service seam in `Assets/Scripts/IScoreDistanceService.cs` and `Assets/Scripts/IScoreDistanceConsumer.cs`; other dependencies remain concrete scene references.
- Treat `Assets/Scenes/SampleScene.unity` as a single-scene application: it is the sole enabled scene in `ProjectSettings/EditorBuildSettings.asset`.

## Layers

**Composition and lifecycle layer:**
- Purpose: Assemble components and invoke Unity lifecycle methods and serialized button callbacks.
- Location: `Assets/Scenes/SampleScene.unity`, `ProjectSettings/EditorBuildSettings.asset`
- Contains: The `Managers` hierarchy, canvases, `EventSystem`, camera/light, instantiated UI prefabs, and serialized component references.
- Depends on: Runtime `MonoBehaviour` classes under `Assets/Scripts/` and assets under `Assets/Prefaps/`, `Assets/ScriptableObjects/`, `Assets/Materials/`, and `Assets/Sprites/`.
- Used by: Unity player startup configured by `ProjectSettings/EditorBuildSettings.asset`.

**Application orchestration layer:**
- Purpose: Coordinate game lifecycle, state transitions, timing, score-distance progression, and UI/domain communication.
- Location: `Assets/Scripts/MahjongGameManager.cs`, `Assets/Scripts/ScoreManagerDistance.cs`, `Assets/Scripts/Timer.cs`
- Contains: `MonoBehaviour` coordinators, the `GameState` gate, game lifecycle events, and the injected score-distance service.
- Depends on: The domain layer in `Assets/Scripts/AL-1S/`, presenters in `Assets/Scripts/UI-Kozeki/`, persistence in `Assets/Scripts/Configs/`, and scene references in `Assets/Scenes/SampleScene.unity`.
- Used by: `Assets/Scripts/UI-Kozeki/UiManager.cs`, `Assets/Scripts/UI-Kozeki/PlayerHand.cs`, and `Assets/Scripts/UI-Kozeki/GameUIManager.cs`.

**Domain layer:**
- Purpose: Represent tiles, blocks, hands, players, rounds, win decompositions, yaku, fu, han, and score tables.
- Location: `Assets/Scripts/AL-1S/`
- Contains: `MahjongTile`, `MahjongBlock`, `MahjongRound`, `MahjongPlayer`, `MahjongWin`, `MahjongWinInfo`, `MahjongUtility`, `MahjongYakuSolver`, enums, and round/event DTOs.
- Depends on: Core .NET collections/LINQ plus limited Unity types/logging in `Assets/Scripts/AL-1S/MahjongUtilities.cs`, `Assets/Scripts/AL-1S/_Structs.cs`, and `Assets/Scripts/AL-1S/MyLogger.cs`.
- Used by: `Assets/Scripts/MahjongGameManager.cs`, all gameplay presenters under `Assets/Scripts/UI-Kozeki/`, and `Assets/Editor/MahjongTileDataGenerator.cs`.

**Presentation and input layer:**
- Purpose: Display menu/game state, animate panels, render tiles and scoring, and translate keyboard/button activity into application intents.
- Location: `Assets/Scripts/UI-Kozeki/`, `Assets/Scripts/DASandARR.cs`, `Assets/Scripts/InputManager.cs`
- Contains: Menu/game panel coordinators, hand/tile presenters, score/win/round/timer views, DOTween extensions, and legacy `UnityEngine.Input` polling.
- Depends on: `Assets/Scripts/MahjongGameManager.cs`, `Assets/Scripts/IScoreDistanceService.cs`, `Assets/Scripts/Timer.cs`, domain DTOs in `Assets/Scripts/AL-1S/`, TextMesh Pro, uGUI, and DOTween in `Assets/Plugins/Demigiant/`.
- Used by: Scene objects and button callbacks serialized in `Assets/Scenes/SampleScene.unity`.

**Persistence and asset infrastructure layer:**
- Purpose: Resolve tile sprites, persist user data, and play audio without placing those details inside mahjong rules.
- Location: `Assets/Scripts/MahjongTileDatabase.cs`, `Assets/Scripts/Configs/`, `Assets/Scripts/SoundArchive/`
- Contains: A `ScriptableObject` tile database, JSON DTO/store, PlayerPrefs-backed volume state, audio source management, and named audio clip lookup.
- Depends on: `Assets/ScriptableObjects/MahjongTileDatabase.asset`, `Assets/Sprites/MahjongTiles/`, Newtonsoft JSON from `Packages/manifest.json`, and scene audio components in `Assets/Scenes/SampleScene.unity`.
- Used by: `Assets/Scripts/UI-Kozeki/MahjongTileGameObject.cs`, `Assets/Scripts/MahjongGameManager.cs`, and `Assets/Scripts/SoundArchive/MusicManager.cs`.

**Editor tooling layer:**
- Purpose: Generate project assets without including editor-only entry points in runtime lifecycle code.
- Location: `Assets/Editor/MahjongTileDataGenerator.cs`
- Contains: An `EditorWindow`, menu registration, sprite discovery, and ScriptableObject creation/update.
- Depends on: Domain tile enumeration in `Assets/Scripts/AL-1S/MahjongTileAndBlock.cs` and asset schema in `Assets/Scripts/MahjongTileDatabase.cs`.
- Used by: Unity Editor users through `Tools/MahjongTileDataGenerator` in `Assets/Editor/MahjongTileDataGenerator.cs`.

## Data Flow

### Primary Request Path

1. A serialized UI button invokes `UiManager.OnGameStartButton`, which calls `ToInGameState` and then `MahjongGameManager.Instance.StartNewGame` (`Assets/Scripts/UI-Kozeki/UiManager.cs:218`, `Assets/Scripts/UI-Kozeki/UiManager.cs:154`).
2. `StartNewGame` initializes `GameUIManager`, constructs a `MahjongRound`, subscribes to round events, generates the wall/hand, injects `ScoreManagerDistance`, and starts the timer (`Assets/Scripts/MahjongGameManager.cs:41`).
3. `MahjongRound.GenerateYama` initializes the player, shuffles the wall, reserves dead-wall/dora tiles, deals 13 tiles, publishes round/hand updates, and immediately draws (`Assets/Scripts/AL-1S/MahjongRound.cs:195`).
4. `OnHandUpdate` and `OnTsumoTile` return through `MahjongGameManager.UpdatePlayerHand` and `LetPlayerTsumoTile`, which update the hand and call panels (`Assets/Scripts/MahjongGameManager.cs:119`, `Assets/Scripts/MahjongGameManager.cs:123`).
5. `PlayerHand.Update` polls keys and emits `OnPlayerDiscard`/`OnPlayerCall`; the manager gates the intent by `GameState` and calls `MahjongRound.DiscardTile` or `CheckTsumoWin` (`Assets/Scripts/UI-Kozeki/PlayerHand.cs:128`, `Assets/Scripts/MahjongGameManager.cs:137`, `Assets/Scripts/MahjongGameManager.cs:287`).
6. A discard swaps or removes the drawn tile, sorts the hand, and draws again; a win passes through `MahjongUtility.CheckWinnableHashSet` and `MahjongWinInfo`, then publishes score/win/next-round events (`Assets/Scripts/AL-1S/MahjongRound.cs:263`, `Assets/Scripts/AL-1S/MahjongRound.cs:462`, `Assets/Scripts/AL-1S/MahjongUtilities.cs:89`).

### Score, Timer, and Game-Over Flow

1. Positive player score deltas are forwarded from `MahjongRound.OnPlayerScoreAlters` to `MahjongGameManager.UpdatePlayerScore`, then into `IScoreDistanceService.GetBoostAndDistance` (`Assets/Scripts/AL-1S/MahjongRound.cs:450`, `Assets/Scripts/MahjongGameManager.cs:158`).
2. `ScoreManagerDistance.Update` decays boost and accumulates distance each frame, emitting rank/distance events to `UiScoreDistanceInfo` (`Assets/Scripts/ScoreManagerDistance.cs:70`, `Assets/Scripts/UI-Kozeki/UiScoreDistanceInfo.cs:48`).
3. `Timer.Update` emits second ticks and eventually `OnTimerFinished`; `MahjongGameManager.HandleGameOver` detaches the round, freezes scoring/timing, initializes game-over UI, and loads/saves the high score (`Assets/Scripts/Timer.cs:34`, `Assets/Scripts/MahjongGameManager.cs:85`).
4. `MahjongGameManager.OnGameOver` activates game-over/back panels in `GameUIManager` and informs subscribed views (`Assets/Scripts/MahjongGameManager.cs:105`, `Assets/Scripts/UI-Kozeki/GameUIManager.cs:88`).

### Tile Asset Generation and Lookup

1. The editor menu in `Assets/Editor/MahjongTileDataGenerator.cs` enumerates domain tiles from `Assets/Scripts/AL-1S/MahjongTileAndBlock.cs` and loads matching PNGs from `Assets/Sprites/MahjongTiles/`.
2. The generator writes the serialized list into `Assets/ScriptableObjects/MahjongTileDatabase.asset` using the schema in `Assets/Scripts/MahjongTileDatabase.cs`.
3. At runtime `MahjongTileDatabase.OnEnable` builds a code-keyed dictionary; `MahjongTileGameObject.SetTileImage` resolves sprites through `MahjongGameManager.Instance.TileDB` (`Assets/Scripts/MahjongTileDatabase.cs:47`, `Assets/Scripts/UI-Kozeki/MahjongTileGameObject.cs:35`).

**State Management:**
- Game lifecycle state is the mutable `currentState` field on the persistent `MahjongGameManager` singleton in `Assets/Scripts/MahjongGameManager.cs`; use it as the input gate for player actions.
- Current mahjong state lives in the non-`MonoBehaviour` `MahjongRound`/`MahjongPlayer` object graph in `Assets/Scripts/AL-1S/MahjongRound.cs`; replace the aggregate between rounds and reattach events through the manager.
- Menu and in-game panel state are separate enum-to-panel maps in `Assets/Scripts/UI-Kozeki/UiManager.cs` and `Assets/Scripts/UI-Kozeki/GameUIManager.cs`; preserve that boundary when adding panels.
- Long-lived user data is split between JSON in `Assets/Scripts/Configs/SettingsManager.cs` and audio `PlayerPrefs` in `Assets/Scripts/SoundArchive/AudioManager.cs`.

## Key Abstractions

**`MahjongTile` and `MahjongBlock`:**
- Purpose: Value types for tile identity/flags and pair/sequence/triplet groupings.
- Examples: `Assets/Scripts/AL-1S/MahjongTileAndBlock.cs`
- Pattern: Comparable/equatable structs with static parsing/factory helpers; use these instead of raw strings or integers outside parsing and serialization boundaries.

**`MahjongRound`:**
- Purpose: Aggregate root for a single-player round and the event boundary used by the application layer.
- Examples: `Assets/Scripts/AL-1S/MahjongRound.cs`, `Assets/Scripts/MahjongGameManager.cs`
- Pattern: Plain C# object with private mutable collections and public domain events; subscribe/detach only in the manager's `AttachRoundEvent`/`DetachRoundEvent` methods.

**Win evaluation pipeline:**
- Purpose: Convert a 13/14-tile hand into decompositions, yaku, han/fu, and payments.
- Examples: `Assets/Scripts/AL-1S/MahjongUtilities.cs`, `Assets/Scripts/AL-1S/MahjongWinInfo.cs`, `Assets/Scripts/AL-1S/MahjongYaku.cs`
- Pattern: Static solver functions produce `MahjongWin`, then `MahjongWinInfo` derives immutable-at-construction scoring information; reuse this pipeline for new win checks.

**`IScoreDistanceService`:**
- Purpose: Decouple score producers and UI consumers from the concrete frame-updated distance model.
- Examples: `Assets/Scripts/IScoreDistanceService.cs`, `Assets/Scripts/IScoreDistanceConsumer.cs`, `Assets/Scripts/ScoreManagerDistance.cs`
- Pattern: Manual constructor-style injection through `Construct`; use the existing scene-owned `ScoreManagerDistance` rather than adding another global score singleton.

**`MahjongTileDatabase`:**
- Purpose: Bridge domain tile codes and Unity sprite assets.
- Examples: `Assets/Scripts/MahjongTileDatabase.cs`, `Assets/ScriptableObjects/MahjongTileDatabase.asset`, `Assets/Editor/MahjongTileDataGenerator.cs`
- Pattern: Serialized list as source data plus a runtime dictionary rebuilt in `OnEnable`; update it through the generator when sprite inventory changes.

**Enum-to-panel maps:**
- Purpose: Resolve menu/game UI states to `RectTransform`, `CanvasGroup`, direction, and original position.
- Examples: `Assets/Scripts/UI-Kozeki/UiManager.cs`, `Assets/Scripts/UI-Kozeki/GameUIManager.cs`, `Assets/Scripts/AL-1S/_Structs.cs`
- Pattern: Inspector-authored lists converted to dictionaries in `Awake`, then animated through shared extensions in `Assets/Scripts/UI-Kozeki/UiTransition.cs`.

## Entry Points

**Player startup:**
- Location: `Assets/Scenes/SampleScene.unity`
- Triggers: Unity loads the only enabled scene declared in `ProjectSettings/EditorBuildSettings.asset`.
- Responsibilities: Instantiate the complete menu/game UI and the `Managers` hierarchy, including `MahjongGameManager`, `UiManager`, `GameUIManager`, `ScoreManagerDistance`, `Timer`, and audio components.

**Menu interaction:**
- Location: `Assets/Scripts/UI-Kozeki/UiManager.cs`
- Triggers: Serialized Unity button calls in `Assets/Scenes/SampleScene.unity` and `Assets/Prefaps/재포장된 라자.prefab`.
- Responsibilities: Navigate menu panels, maintain back history, enter game UI, and delegate game creation to `MahjongGameManager`.

**Per-frame gameplay:**
- Location: `Assets/Scripts/UI-Kozeki/PlayerHand.cs`, `Assets/Scripts/ScoreManagerDistance.cs`, `Assets/Scripts/Timer.cs`
- Triggers: Unity `Update` on the main thread.
- Responsibilities: Poll player input, update selection visuals, decay/accumulate distance, and count down the game timer.

**Game lifecycle:**
- Location: `Assets/Scripts/MahjongGameManager.cs`
- Triggers: `UiManager.OnGameStartButton`, `PlayerHand` events, `MahjongRound` events, and `Timer.OnTimerFinished`.
- Responsibilities: Initialize services and rounds, mediate domain/UI events, gate calls by state, and finalize/persist game results.

**Editor tile-database generation:**
- Location: `Assets/Editor/MahjongTileDataGenerator.cs`
- Triggers: Unity Editor menu `Tools/MahjongTileDataGenerator`.
- Responsibilities: Load sprites from `Assets/Sprites/MahjongTiles/` and create/update `Assets/ScriptableObjects/MahjongTileDatabase.asset`.

## Architectural Constraints

- **Threading:** All gameplay, input, DOTween callbacks, timer updates, and event dispatch run on Unity's main thread in `Assets/Scripts/MahjongGameManager.cs`, `Assets/Scripts/UI-Kozeki/PlayerHand.cs`, `Assets/Scripts/ScoreManagerDistance.cs`, and `Assets/Scripts/Timer.cs`; no worker-thread ownership model exists.
- **Global state:** `MahjongGameManager.Instance`, `GameUIManager.Instance`, and `AudioManager.instance` are mutable singletons in `Assets/Scripts/MahjongGameManager.cs`, `Assets/Scripts/UI-Kozeki/GameUIManager.cs`, and `Assets/Scripts/SoundArchive/AudioManager.cs`. `InputPreset` in `Assets/Scripts/InputManager.cs` is also mutable static state.
- **Circular imports:** No namespace/assembly boundary exists because project runtime scripts compile into the generated `Assembly-CSharp` project; runtime dependencies are bidirectional between `Assets/Scripts/MahjongGameManager.cs` and `Assets/Scripts/UI-Kozeki/PlayerHand.cs`, and between the two UI managers in `Assets/Scripts/UI-Kozeki/UiManager.cs` and `Assets/Scripts/UI-Kozeki/GameUIManager.cs`.
- **Assembly boundaries:** No project `.asmdef` or `.asmref` files exist under `Assets/`; runtime scripts share one default Unity assembly, while `Assets/Editor/MahjongTileDataGenerator.cs` is separated only by the special `Editor` folder.
- **Scene wiring:** Required references are not discovered from configuration; they are serialized directly on scene components in `Assets/Scenes/SampleScene.unity`. Renaming or moving assets is safe only with their `.meta` files, and a new scene must reproduce or replace this composition root.
- **Single-scene lifecycle:** `ProjectSettings/EditorBuildSettings.asset` enables only `Assets/Scenes/SampleScene.unity`, while `DontDestroyOnLoad` is still used by `Assets/Scripts/MahjongGameManager.cs` and `Assets/Scripts/SoundArchive/AudioManager.cs`.
- **Input backend:** Gameplay uses the legacy `UnityEngine.Input`/`KeyCode` path in `Assets/Scripts/UI-Kozeki/PlayerHand.cs`, `Assets/Scripts/DASandARR.cs`, and `Assets/Scripts/InputManager.cs`; no Input System action assets are present under `Assets/`.
- **Persistence:** JSON data is stored at `Application.persistentDataPath/yaml.json` by `Assets/Scripts/Configs/SettingsManager.cs`; audio settings are stored separately as `PlayerPrefs` keys in `Assets/Scripts/SoundArchive/AudioManager.cs`.
- **Build compatibility:** Runtime files `Assets/Scripts/UI-Kozeki/UiScoreDistanceInfo.cs` and `Assets/Scripts/AL-1S/MahjongTileAndBlock.cs` import `UnityEditor` namespaces even though they are outside `Assets/Editor/`; keep editor-only dependencies out of runtime scripts or guard them with `#if UNITY_EDITOR`.

## Anti-Patterns

### Bypassing the Scene Composition Root

**What happens:** A presenter fetches another manager globally with `FindObjectOfType` or a singleton, as in `GameUIManager.OnBackButton` in `Assets/Scripts/UI-Kozeki/GameUIManager.cs` and `MahjongTileGameObject.SetTileImage` in `Assets/Scripts/UI-Kozeki/MahjongTileGameObject.cs`.
**Why it's wrong:** Hidden scene-order and object-presence requirements make prefab isolation and replacement difficult, while the rest of the application already has explicit serialized or `Construct` wiring in `Assets/Scenes/SampleScene.unity` and `Assets/Scripts/IScoreDistanceConsumer.cs`.
**Do this instead:** Add required references to the owning coordinator in `Assets/Scripts/MahjongGameManager.cs` or pass the existing service through `Construct` following `Assets/Scripts/UI-Kozeki/UiScoreDistanceInfo.cs`; keep lookup at the scene boundary.

### Putting Editor Dependencies in Runtime Files

**What happens:** Runtime scripts import `UnityEditor` or `UnityEditor.Rendering` in `Assets/Scripts/UI-Kozeki/UiScoreDistanceInfo.cs` and `Assets/Scripts/AL-1S/MahjongTileAndBlock.cs`.
**Why it's wrong:** Unity player assemblies cannot depend on editor-only APIs, and the project has no `.asmdef` boundary under `Assets/` to contain the dependency.
**Do this instead:** Remove unused editor imports; place actual editor extensions under `Assets/Editor/` following `Assets/Editor/MahjongTileDataGenerator.cs`, or wrap a narrowly required editor block with `#if UNITY_EDITOR`.

### Adding Parallel State Machines

**What happens:** Gameplay actions can be gated independently by UI active state instead of the authoritative `GameState` in `Assets/Scripts/MahjongGameManager.cs`, while panels already have separate display-only state in `Assets/Scripts/UI-Kozeki/GameUIManager.cs`.
**Why it's wrong:** UI visibility and game legality can diverge, especially during `Processing`, win transitions, and game over in `Assets/Scripts/MahjongGameManager.cs`.
**Do this instead:** Validate gameplay intent in `MahjongGameManager.CallHandler`/`PlayerDiscardTile` in `Assets/Scripts/MahjongGameManager.cs`; use `GameUIState` and `UIState` only to select panels in `Assets/Scripts/UI-Kozeki/`.

## Error Handling

**Strategy:** Use early-return validation for invalid domain/input conditions, return empty/null-object results for non-winning hands, log recoverable asset/state errors, and catch persistence failures at the filesystem/JSON boundary in `Assets/Scripts/AL-1S/MahjongUtilities.cs`, `Assets/Scripts/AL-1S/MahjongTileAndBlock.cs`, `Assets/Scripts/MahjongTileDatabase.cs`, and `Assets/Scripts/Configs/SettingsManager.cs`.

**Patterns:**
- Domain parsers and solvers return `MahjongTile.NullTile()`, empty collections, or `false` for invalid/non-winning input in `Assets/Scripts/AL-1S/MahjongTileAndBlock.cs` and `Assets/Scripts/AL-1S/MahjongUtilities.cs`.
- Scene/application handlers guard illegal state with early returns in `Assets/Scripts/MahjongGameManager.cs`, `Assets/Scripts/ScoreManagerDistance.cs`, and `Assets/Scripts/Timer.cs`.
- Missing tile sprites fall back to a serialized sprite and log through `MyLogger` in `Assets/Scripts/MahjongTileDatabase.cs`; missing audio names select a fallback clip in `Assets/Scripts/SoundArchive/SoundArchive.cs`.
- Save/load catches `IOException` and load additionally catches `JsonException`, returning fresh DTOs rather than aborting gameplay in `Assets/Scripts/Configs/SettingsManager.cs`.
- Unity event fields use empty delegate defaults to avoid null checks in `Assets/Scripts/MahjongGameManager.cs`, `Assets/Scripts/AL-1S/MahjongRound.cs`, `Assets/Scripts/ScoreManagerDistance.cs`, and `Assets/Scripts/Timer.cs`.

## Cross-Cutting Concerns

**Logging:** Use the `MyLogger` wrapper in `Assets/Scripts/AL-1S/MyLogger.cs` for gameplay/domain diagnostics; some infrastructure still uses `Debug.Log*` directly in `Assets/Scripts/Configs/SettingsManager.cs`, `Assets/Scripts/SoundArchive/SoundArchive.cs`, and `Assets/Scripts/SoundArchive/MusicManager.cs`.
**Validation:** Domain boundaries validate hand sizes, tile multiplicity, null tiles, and state before processing in `Assets/Scripts/AL-1S/MahjongUtilities.cs`, `Assets/Scripts/AL-1S/MahjongTileAndBlock.cs`, and `Assets/Scripts/MahjongGameManager.cs`; scene reference validity remains an Inspector responsibility in `Assets/Scenes/SampleScene.unity`.
**Authentication:** Not applicable; the local single-player project under `Assets/Scripts/` contains no identity or network-authentication layer.

---

*Architecture analysis: 2026-08-28*
