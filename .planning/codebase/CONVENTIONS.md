# Coding Conventions

**Analysis Date:** 2026-08-28

## Naming Patterns

**Files:**
- Use PascalCase and match the principal type name for new C# files, following `Assets/Scripts/Timer.cs`, `Assets/Scripts/ScoreManagerDistance.cs`, and `Assets/Scripts/UI-Kozeki/UiWinInfo.cs`.
- Keep the established `Ui...` spelling for scene-facing UI types (`Assets/Scripts/UI-Kozeki/UiManager.cs`, `Assets/Scripts/UI-Kozeki/UiScoreInfo.cs`); reserve all-caps `UI` for concepts and helpers already named that way, such as `GameUIState` and `UITransitions` in `Assets/Scripts/UI-Kozeki/GameUIManager.cs` and `Assets/Scripts/UI-Kozeki/UiTransition.cs`.
- Do not copy legacy aggregate names such as `Assets/Scripts/AL-1S/_Structs.cs`, `Assets/Scripts/AL-1S/_Enums_Ecchi_Nun_Mari.cs`, or the file/type mismatch in `Assets/Scripts/DASandARR.cs`; add a normally named file for a new top-level type.

**Functions:**
- Use PascalCase for public methods and constructors, as in `StartNewGame`, `Construct`, and `HandleGameOver` in `Assets/Scripts/MahjongGameManager.cs`.
- Use PascalCase for private methods too, matching the dominant `UpdatePlayerHand`, `AttachRoundEvent`, and `ChangeState` pattern in `Assets/Scripts/MahjongGameManager.cs`; keep Unity message names exactly as Unity defines them (`Awake`, `OnEnable`, `Start`, `Update`, `OnDisable`).
- Prefer verb-led names that expose intent (`FindAgariTiles`, `CheckWinnable`, `RemoveLowerYakues`) as used in `Assets/Scripts/AL-1S/MahjongUtilities.cs`.

**Variables:**
- Use camelCase for locals and parameters, following `newDistance`, `beforeBoostRank`, and `deltaTime` in `Assets/Scripts/ScoreManagerDistance.cs`.
- Use camelCase for private fields; both bare fields (`accumulatedScore` in `Assets/Scripts/ScoreManagerDistance.cs`) and underscore-prefixed fields (`_timer` in `Assets/Scripts/UI-Kozeki/UiRemainingTimeIndicator.cs`) exist, so preserve a touched file's local convention instead of mixing styles inside one class.
- Keep identifiers in English for new or changed code. Existing character-themed identifiers such as `iroha`, `hina`, and `kyaruberos` in `Assets/Scripts/AL-1S/MahjongTileAndBlock.cs` and `Assets/Scripts/AL-1S/MahjongUtilities.cs` are legacy domain-flavored names, not a pattern to extend.
- Use PascalCase constants and static preset names only where the existing public API already exposes that form (`InputLists.MoveLeft` in `Assets/Scripts/Configs/InputLists.cs`); do not introduce new lowercase public statics like `InputPreset.left` in `Assets/Scripts/InputManager.cs`.

**Types:**
- Use PascalCase for classes, structs, enums, and enum members, following `MahjongRound`, `MahjongTile`, `GameState`, and `PlayerCallType` in `Assets/Scripts/AL-1S/MahjongRound.cs`, `Assets/Scripts/AL-1S/MahjongTileAndBlock.cs`, and `Assets/Scripts/AL-1S/_Enums_Ecchi_Nun_Mari.cs`.
- Prefix interfaces with `I`, following `IScoreDistanceService`, `IScoreDistanceConsumer`, and `ILocalizable` in `Assets/Scripts/IScoreDistanceService.cs`, `Assets/Scripts/IScoreDistanceConsumer.cs`, and `Assets/Scripts/ILocalizable.cs`.
- No first-party C# file declares a namespace under `Assets/Scripts/**/*.cs` or `Assets/Editor/**/*.cs`; new code must account for the current global-namespace API unless a coordinated namespace migration is explicitly scoped.

## Code Style

**Formatting:**
- No `.editorconfig`, C# formatter configuration, or StyleCop configuration is present; formatting is manual across `Assets/Scripts/**/*.cs` and `Assets/Editor/MahjongTileDataGenerator.cs`.
- Use four-space indentation and Allman braces in new or edited blocks, matching the clean sections of `Assets/Scripts/ScoreManagerDistance.cs` and `Assets/Scripts/Configs/SettingsManager.cs`. Existing same-line braces in `Assets/Scripts/AL-1S/MahjongRound.cs` and `Assets/Scripts/SoundArchive/MusicManager.cs` should not be copied.
- Keep one statement per line and normal spacing around operators and commas. Avoid extending compressed forms such as `a.y+b.y` in `Assets/Scripts/AL-1S/Utilities.cs` or `+=StartNextRound` in `Assets/Scripts/MahjongGameManager.cs`.
- Use C# 9-compatible syntax: Unity generates `<LangVersion>9.0</LangVersion>` in `Assembly-CSharp.csproj`, while the editor version is fixed in `ProjectSettings/ProjectVersion.txt`.
- Put Unity lifecycle methods together near the top of a `MonoBehaviour` when changing a class. Lifecycle methods are currently scattered in `Assets/Scripts/MahjongGameManager.cs` and `Assets/Scripts/UI-Kozeki/PlayerHand.cs`, so preserve behavior while grouping only code already in scope.

**Linting:**
- No repository lint command or analyzer ruleset is configured in `.vscode/settings.json`, `Assembly-CSharp.csproj`, or the project root; rely on Unity compilation and IDE diagnostics.
- Treat compiler warnings as actionable. Unity-generated `Assembly-CSharp.csproj` uses warning level 4 and suppresses only `0169` and `USG0001`; do not add broad suppressions to first-party files under `Assets/Scripts/`.
- Remove unused imports when touching a file. Several files carry unused namespaces, and runtime scripts must not import editor-only namespaces such as `UnityEditor` in `Assets/Scripts/UI-Kozeki/UiScoreDistanceInfo.cs` or `UnityEditor.Rendering` in `Assets/Scripts/AL-1S/MahjongTileAndBlock.cs`.

## Import Organization

**Order:**
1. Put `System` namespaces first, as demonstrated in `Assets/Scripts/MahjongGameManager.cs`.
2. Put third-party namespaces next, such as `DG.Tweening` and `TMPro` in `Assets/Scripts/UI-Kozeki/UiScoreDistanceInfo.cs`.
3. Put `UnityEngine` and its sub-namespaces last; keep `UnityEditor` imports confined to editor code such as `Assets/Editor/MahjongTileDataGenerator.cs`.

**Path Aliases:**
- Not applicable: first-party code uses ordinary `using` directives and the global namespace throughout `Assets/Scripts/**/*.cs`; no alias convention is configured in `Assembly-CSharp.csproj`.

## Error Handling

**Patterns:**
- Use early returns for invalid state or unavailable input, following `ScoreManagerDistance.Update`, `GetBoost`, and `GetInstantDistance` in `Assets/Scripts/ScoreManagerDistance.cs` and panel lookup guards in `Assets/Scripts/UI-Kozeki/GameUIManager.cs`.
- At persistence boundaries, catch the narrow expected exceptions and return safe default data. `Assets/Scripts/Configs/SettingsManager.cs` catches `IOException` and `JsonException`, logs the failure, and returns `PetitGameSaveData` rather than propagating corrupt or missing save data.
- Domain parsers use explicit sentinels for invalid tile input (`MahjongTile.NullTile`) in `Assets/Scripts/AL-1S/MahjongTileAndBlock.cs`; collection-returning APIs generally return empty collections in `Assets/Scripts/AL-1S/MahjongUtilities.cs`. Preserve the established contract of the API being changed instead of mixing `null`, sentinel, and empty results.
- Throw only for violated programmer contracts that cannot be represented by an existing domain result, as `Utilities.GetRandomItem` does for a null or empty collection in `Assets/Scripts/AL-1S/Utilities.cs`.
- Pair event subscriptions with deterministic unsubscriptions, following `OnEnable`/`OnDisable` in `Assets/Scripts/SoundArchive/MusicManager.cs` and `Assets/Scripts/MahjongGameManager.cs`; use the same owner and handler on both sides.

## Logging

**Framework:** Unity `Debug` plus the project wrapper `MyLogger` in `Assets/Scripts/AL-1S/MyLogger.cs`.

**Patterns:**
- Use `Debug.LogWarning` or `Debug.LogError` for diagnostics that must remain visible, following load failures in `Assets/Scripts/Configs/SettingsManager.cs` and missing clips in `Assets/Scripts/SoundArchive/SoundArchive.cs`.
- Do not rely on `MyLogger` for essential diagnostics: `Assets/Scripts/AL-1S/MyLogger.cs` defines and immediately undefines `HIMARI`, so its methods compile to no-ops in the current configuration.
- Keep routine success or per-frame logging out of hot paths. `Assets/Scripts/UI-Kozeki/UiScoreDistanceInfo.cs` updates every frame, while logs are reserved for missing service/setup states.
- Include the failing operation and useful context in warnings/errors, as the save path and exception are included by `Assets/Scripts/Configs/SettingsManager.cs`.

## Comments

**When to Comment:**
- Write comments and XML summaries in Korean for new or changed first-party code, consistent with domain explanations in `Assets/Scripts/AL-1S/MahjongUtilities.cs` and lifecycle intent in `Assets/Scripts/Timer.cs`.
- Explain rules, invariants, and Unity/editor constraints rather than restating code. Good examples include the 13/14-tile preconditions in `Assets/Scripts/AL-1S/MahjongUtilities.cs` and database generation behavior in `Assets/Editor/MahjongTileDataGenerator.cs`.
- Do not retain large commented-out implementations or empty Unity template methods when touching a file; legacy examples exist in `Assets/Scripts/Configs/SettingsManager.cs`, `Assets/Scripts/UI-Kozeki/UiManager.cs`, `Assets/Scripts/InputManager.cs`, and `Assets/Scripts/LocalizationManager.cs`.

**JSDoc/TSDoc:**
- Not applicable; use C# XML documentation (`/// <summary>`) for public types and behavior-changing public members. Existing usage is concentrated in `Assets/Scripts/AL-1S/MahjongRound.cs`, `Assets/Scripts/AL-1S/MahjongUtilities.cs`, and `Assets/Scripts/AL-1S/_Structs.cs`.
- Keep `<param>` and `<returns>` text meaningful; avoid empty tags like those currently present in parts of `Assets/Scripts/MahjongGameManager.cs` and `Assets/Scripts/UI-Kozeki/UiManager.cs`.

## Function Design

**Size:** Keep Unity callbacks thin and delegate domain work to focused methods. `Assets/Scripts/MahjongGameManager.cs` routes `Update`, event handlers, and round transitions through named helpers, while pure mahjong calculations live in `Assets/Scripts/AL-1S/MahjongUtilities.cs`.

**Parameters:**
- Pass domain values directly and use `out` only where the API creates multiple related results, as in `MahjongRound.NewRound(int, out MahjongPlayer)` in `Assets/Scripts/AL-1S/MahjongRound.cs`.
- Inject cross-component services through explicit `Construct(...)` methods when a scene reference is not appropriate, following `IScoreDistanceConsumer` in `Assets/Scripts/IScoreDistanceConsumer.cs`, `Assets/Scripts/MahjongGameManager.cs`, and `Assets/Scripts/UI-Kozeki/UiScoreDistanceInfo.cs`.
- Avoid adding boolean switches when an enum or named method already describes the operation, following `PlayerCallType` dispatch in `Assets/Scripts/MahjongGameManager.cs`.

**Return Values:**
- Prefer early-returned empty collections for “no matches” in calculation APIs, following `FindAgariTiles` and `CheckWinnable` in `Assets/Scripts/AL-1S/MahjongUtilities.cs`.
- Preserve value-object sentinels such as `MahjongTile.NullTile()` for invalid tile parsing in `Assets/Scripts/AL-1S/MahjongTileAndBlock.cs`; callers compare against this sentinel in `Assets/Scripts/AL-1S/MahjongRound.cs`.
- Expose read-only state through properties with private setters or expression-bodied getters, following `Distance`, `BoostLevel`, and `DistanceWithAccumulated` in `Assets/Scripts/ScoreManagerDistance.cs`.

## Module Design

**Exports:**
- First-party runtime scripts compile into the default `Assembly-CSharp` assembly because no `.asmdef` exists under `Assets/Scripts/`; editor tooling is separated structurally under `Assets/Editor/MahjongTileDataGenerator.cs`.
- Use `[SerializeField] private` for inspector wiring while keeping runtime mutation private, following `Assets/Scripts/MahjongGameManager.cs`, `Assets/Scripts/ScoreManagerDistance.cs`, and `Assets/Scripts/UI-Kozeki/MahjongTileGameObject.cs`.
- Use `ScriptableObject` for authored asset data (`Assets/Scripts/MahjongTileDatabase.cs`) and plain C# classes/static functions for domain calculations (`Assets/Scripts/AL-1S/MahjongUtilities.cs`).
- Existing scene coordinators expose singleton entry points (`MahjongGameManager.Instance`, `GameUIManager.Instance`, and `AudioManager.instance`) in `Assets/Scripts/MahjongGameManager.cs`, `Assets/Scripts/UI-Kozeki/GameUIManager.cs`, and `Assets/Scripts/SoundArchive/AudioManager.cs`; reuse the relevant existing owner rather than adding another global manager.
- Partial types group related behavior inside large files (`MahjongGameManager` in `Assets/Scripts/MahjongGameManager.cs`, `MahjongUtility` in `Assets/Scripts/AL-1S/MahjongUtilities.cs`, and `MahjongTile` in `Assets/Scripts/AL-1S/MahjongTileAndBlock.cs`). Add to the owning type only when the behavior belongs to that existing responsibility.

**Barrel Files:**
- Not applicable: C# code uses the global namespace and Unity's generated assemblies; there are no barrel/index files under `Assets/Scripts/`.

---

*Convention analysis: 2026-08-28*
