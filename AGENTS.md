<!-- GSD:project-start source:PROJECT.md -->

## Project

**Project Riichi Nya**

15개월 전에 중단된 Unity 리치마작 프로젝트를 다시 분석하고 개선하여, 2026 넥토리얼 게임 프로그래머 지원용 포트폴리오로 완성한다. 기존의 제한 시간·점수·이동 거리 기반 1인 조패 모드는 독립적인 플레이 경험으로 유지하고, 같은 규칙 엔진을 사용하는 4인 반장전 모드를 추가한다.

이번 작업은 기능 수를 늘리는 것보다 기존 코드의 치명적인 규칙 오류를 고치고, 자동화 테스트로 검증하며, Unity 표현 계층과 순수 C# 마작 도메인을 분리하는 데 우선순위를 둔다. AI의 제안을 그대로 채택하지 않고 코드 조사·판단·테스트를 거쳐 개선한 과정을 포트폴리오 증거로 남긴다.

**Core Value:** 기존 마작 로직을 정확하고 테스트 가능하며 유지보수하기 쉬운 규칙 엔진으로 개선하고, 그 엔진이 서로 다른 1인·4인 플레이 흐름에 재사용되는 과정을 검증 가능한 전후 증거로 보여준다.

### Constraints

- **Timeline**: 작업 기한은 9일이다 — 단계 2의 정확한 완료와 포트폴리오 전달물을 우선하고, 단계 3은 하위 단계가 검증된 뒤 진행한다.
- **Quality gate**: 치명적 오류와 규칙 로직에는 자동화 테스트가 필요하다 — 기능 구현 속도를 위해 정확성과 회귀 방지를 생략하지 않는다.
- **Architecture**: 객체지향은 책임과 상태 전이를 명확히 하는 수단이다 — 클래스 수, 계층 수, 패턴 사용 자체를 목표로 삼지 않는다.
- **Compatibility**: Unity 2022.3.29f1 LTS와 기존 에셋·UI·DOTween 기반을 유지한다 — 9일 동안 엔진 업그레이드나 전면 교체를 하지 않는다.
- **Rules**: 반장전은 하나의 고정 기본 규칙을 구현한다 — 미래 규칙은 현재 모델을 방해하지 않는 범위에서만 고려하고 미리 구현하지 않는다.
- **Opponent behavior**: 상대 세 좌석은 쯔모기리만 수행하고 어떤 선언도 하지 않는다 — 이를 완전한 전략 AI로 표현하지 않는다.
- **Portfolio evidence**: 모든 주요 개선은 기존 증상, 근본 원인, 선택 이유, 테스트 또는 빌드 결과로 설명 가능해야 한다.
- **Source preservation**: 현재 소스 기준점과 변경 이력을 Git으로 보존한다 — 비교용 중복 소스 트리를 추가하지 않는다.

<!-- GSD:project-end -->

<!-- GSD:stack-start source:codebase/STACK.md -->

## Technology Stack

## Languages

- C# 9.0 - Gameplay, UI, persistence, audio, domain logic, and editor tooling live under `Assets/Scripts/` and `Assets/Editor/`; Unity's generated `Assembly-CSharp.csproj` sets `<LangVersion>9.0</LangVersion>`.
- Unity YAML - Scenes, prefabs, materials, render-pipeline assets, and project configuration are serialized in `Assets/**/*.asset`, `Assets/**/*.prefab`, `Assets/Scenes/SampleScene.unity`, and `ProjectSettings/*.asset`.
- JSON - Unity Package Manager manifests use `Packages/manifest.json` and `Packages/packages-lock.json`; Korean localization data is stored in `Assets/Localizations/ko.json`; runtime save data is serialized as JSON by `Assets/Scripts/Configs/SettingsManager.cs`.
- ShaderLab/Cg - Text rendering shaders and include files are vendored in `Assets/TextMesh Pro/Shaders/`; project shader graphs are stored in `Assets/Materials/ooh Shiny.shadergraph` and `Assets/Materials/ooh Shiny 2.shadergraph`.

## Runtime

- Unity Editor 2022.3.29f1 LTS, revision `8d510ca76d2b`, pinned by `ProjectSettings/ProjectVersion.txt`.
- Unity managed runtime targeting .NET Standard 2.1; `ProjectSettings/ProjectSettings.asset` sets `apiCompatibilityLevel: 6`, and the generated `Assembly-CSharp.csproj` defines `NET_STANDARD_2_1`.
- Mono is the observed Standalone editor/player backend: `ProjectSettings/ProjectSettings.asset` has no explicit `scriptingBackend` override, while generated `Assembly-CSharp.csproj` defines `ENABLE_MONO` and `PLATFORM_STANDALONE_WIN`.
- Unity Package Manager bundled with Unity 2022.3.29f1 - Registry and built-in package dependencies are declared in `Packages/manifest.json`.
- Lockfile: present at `Packages/packages-lock.json`; use it as the authoritative resolved-version source.

## Frameworks

- Unity 2022.3.29f1 - Component lifecycle, scene management, asset serialization, audio, input, and 2D gameplay runtime; version is pinned in `ProjectSettings/ProjectVersion.txt`.
- Universal Render Pipeline 14.0.11 - Active scriptable render pipeline configured by `ProjectSettings/GraphicsSettings.asset`, with quality-specific assets in `Assets/Settings/URP-Performant.asset`, `Assets/Settings/URP-Balanced.asset`, and `Assets/Settings/URP-HighFidelity.asset`.
- Unity UI (`com.unity.ugui`) 1.0.0 - Canvas-based game UI used by scripts such as `Assets/Scripts/UI-Kozeki/UiManager.cs` and `Assets/Scripts/UI-Kozeki/UiGameOver.cs`.
- TextMesh Pro 3.0.6 - Runtime text rendering used throughout `Assets/Scripts/UI-Kozeki/` and declared in `Packages/manifest.json`.
- Unity Visual Scripting 1.9.4 - Runtime extension methods/types imported by game and UI scripts such as `Assets/Scripts/AL-1S/MahjongRound.cs` and `Assets/Scripts/UI-Kozeki/GameUIManager.cs`.
- Unity 2D Feature Set 2.0.0 - Resolves Sprite, Tilemap, PSD Importer, SpriteShape, Aseprite, and 2D Animation packages through `Packages/packages-lock.json`; tilemap types are used in `Assets/Scripts/AL-1S/_Structs.cs`.
- Unity Test Framework 1.1.33 with NUnit extension 1.0.6 - Installed through `Packages/manifest.json` and `Packages/packages-lock.json`; no project-authored test files or test assemblies are present under `Assets/`.
- Unity Editor build pipeline - The only enabled player scene is `Assets/Scenes/SampleScene.unity`, configured in `ProjectSettings/EditorBuildSettings.asset`; no project-authored `BuildPipeline` automation is present.
- Burst 1.8.13 - Resolved transitively by URP and Unity 2D packages in `Packages/packages-lock.json`; Standalone Windows AOT settings are stored in `ProjectSettings/BurstAotSettings_StandaloneWindows.json`.
- Rider Editor 3.0.28, Visual Studio Editor 2.0.22, and VS Code Editor 1.2.5 - IDE integrations declared in `Packages/manifest.json`.
- Unity Version Control integration 2.7.1 (`com.unity.collab-proxy`) - Editor package declared in `Packages/manifest.json`; version-control serialization is set to visible meta files in `ProjectSettings/VersionControlSettings.asset`.

## Key Dependencies

- DOTween / DOTween Pro - Vendored animation/tween binaries and Unity modules in `Assets/Plugins/Demigiant/`; runtime UI animation calls use `DG.Tweening` in `Assets/Scripts/UI-Kozeki/GameUIManager.cs`, `Assets/Scripts/UI-Kozeki/UiManager.cs`, `Assets/Scripts/UI-Kozeki/UiTransition.cs`, `Assets/Scripts/UI-Kozeki/UiHoverShift.cs`, and `Assets/Scripts/UI-Kozeki/UiScoreDistanceInfo.cs`. The exact upstream release is not recorded; DLL metadata reports only assembly version 1.0.0.0.
- Newtonsoft.Json for Unity 3.2.1 - Serializes and deserializes `PetitGameSaveData` in `Assets/Scripts/Configs/SettingsManager.cs`; package version is declared in `Packages/manifest.json`.
- TextMesh Pro 3.0.6 - Provides `TextMeshProUGUI` UI text types used across `Assets/Scripts/UI-Kozeki/`; package version is declared in `Packages/manifest.json`.
- Universal RP 14.0.11 - Required by the active render pipeline reference in `ProjectSettings/GraphicsSettings.asset` and the assets under `Assets/Settings/`.
- Unity Package Registry (`https://packages.unity.com`) - Default development dependency source configured in `ProjectSettings/PackageManagerSettings.asset`; no additional scoped registries are configured.
- Local Unity asset database - Runtime tile definitions are stored as the ScriptableObject `Assets/ScriptableObjects/MahjongTileDatabase.asset`, generated by `Assets/Editor/MahjongTileDataGenerator.cs`.
- Unity PlayerPrefs - Stores master, music, and sound-effect volume values in `Assets/Scripts/SoundArchive/AudioManager.cs`.

## Configuration

- No `.env` or environment-variable configuration files are present at the repository root; runtime configuration is represented by serialized project assets under `ProjectSettings/` and `Assets/`.
- Player identity and defaults are stored in `ProjectSettings/ProjectSettings.asset`: product `Mahjong`, company `Game_Development_Department`, bundle version `1.0.0b`, 1280x720 default window, linear color space, incremental GC, deterministic compilation, and legacy input handling.
- Runtime settings/save data are written to `Application.persistentDataPath/yaml.json` by `Assets/Scripts/Configs/SettingsManager.cs`; despite its filename, its contents are JSON.
- Audio volumes are stored separately through `PlayerPrefs` keys in `Assets/Scripts/SoundArchive/AudioManager.cs`.
- Unity editor version: `ProjectSettings/ProjectVersion.txt`.
- Package graph: `Packages/manifest.json` and `Packages/packages-lock.json`.
- Player/build settings: `ProjectSettings/ProjectSettings.asset` and `ProjectSettings/EditorBuildSettings.asset`.
- Render settings: `ProjectSettings/GraphicsSettings.asset`, `ProjectSettings/QualitySettings.asset`, `ProjectSettings/URPProjectSettings.asset`, and `Assets/Settings/URP-*.asset`.
- Tween behavior and enabled modules: `Assets/Resources/DOTweenSettings.asset`; Safe Mode is enabled, along with audio, physics, physics2D, sprite, UI, and TextMesh Pro modules.
- Text serialization and visible `.meta` files are configured by `ProjectSettings/EditorSettings.asset` and `ProjectSettings/VersionControlSettings.asset`.
- Generated `.sln` and `.csproj` files at the repository root are ignored build artifacts per `.gitignore`; do not treat them as hand-maintained configuration.

## Platform Requirements

- Use Unity Hub/Editor 2022.3.29f1 on Windows to match `ProjectSettings/ProjectVersion.txt` and the Standalone Windows definitions generated in `Assembly-CSharp.csproj`.
- Restore packages from `Packages/manifest.json` using the resolved graph in `Packages/packages-lock.json`; the default registry is configured in `ProjectSettings/PackageManagerSettings.asset`.
- Keep asset metadata visible and serialized as text, matching `ProjectSettings/VersionControlSettings.asset` and `ProjectSettings/EditorSettings.asset`.
- Use the legacy Unity Input Manager: `ProjectSettings/ProjectSettings.asset` sets `activeInputHandler: 0`, and key bindings use `KeyCode` in `Assets/Scripts/Configs/Settings.cs` and `Assets/Scripts/InputManager.cs`.
- The observed development/player target is 64-bit Standalone Windows, evidenced by `ProjectSettings/BurstAotSettings_StandaloneWindows.json` and generated `Assembly-CSharp.csproj`; no committed build output or release packaging exists because `.gitignore` excludes `Build/` and `Builds/`.
- The player uses URP and a default 1280x720 window from `ProjectSettings/ProjectSettings.asset`; quality tiers select the corresponding render-pipeline assets through `ProjectSettings/QualitySettings.asset`.
- Android and iPhone application identifiers remain in `ProjectSettings/ProjectSettings.asset`, but no mobile-specific source integration, signing configuration, or committed deployment pipeline is present.

<!-- GSD:stack-end -->

<!-- GSD:conventions-start source:CONVENTIONS.md -->

## Conventions

## Naming Patterns

- Use PascalCase and match the principal type name for new C# files, following `Assets/Scripts/Timer.cs`, `Assets/Scripts/ScoreManagerDistance.cs`, and `Assets/Scripts/UI-Kozeki/UiWinInfo.cs`.
- Keep the established `Ui...` spelling for scene-facing UI types (`Assets/Scripts/UI-Kozeki/UiManager.cs`, `Assets/Scripts/UI-Kozeki/UiScoreInfo.cs`); reserve all-caps `UI` for concepts and helpers already named that way, such as `GameUIState` and `UITransitions` in `Assets/Scripts/UI-Kozeki/GameUIManager.cs` and `Assets/Scripts/UI-Kozeki/UiTransition.cs`.
- Do not copy legacy aggregate names such as `Assets/Scripts/AL-1S/_Structs.cs`, `Assets/Scripts/AL-1S/_Enums_Ecchi_Nun_Mari.cs`, or the file/type mismatch in `Assets/Scripts/DASandARR.cs`; add a normally named file for a new top-level type.
- Use PascalCase for public methods and constructors, as in `StartNewGame`, `Construct`, and `HandleGameOver` in `Assets/Scripts/MahjongGameManager.cs`.
- Use PascalCase for private methods too, matching the dominant `UpdatePlayerHand`, `AttachRoundEvent`, and `ChangeState` pattern in `Assets/Scripts/MahjongGameManager.cs`; keep Unity message names exactly as Unity defines them (`Awake`, `OnEnable`, `Start`, `Update`, `OnDisable`).
- Prefer verb-led names that expose intent (`FindAgariTiles`, `CheckWinnable`, `RemoveLowerYakues`) as used in `Assets/Scripts/AL-1S/MahjongUtilities.cs`.
- Use camelCase for locals and parameters, following `newDistance`, `beforeBoostRank`, and `deltaTime` in `Assets/Scripts/ScoreManagerDistance.cs`.
- Use camelCase for private fields; both bare fields (`accumulatedScore` in `Assets/Scripts/ScoreManagerDistance.cs`) and underscore-prefixed fields (`_timer` in `Assets/Scripts/UI-Kozeki/UiRemainingTimeIndicator.cs`) exist, so preserve a touched file's local convention instead of mixing styles inside one class.
- Keep identifiers in English for new or changed code. Existing character-themed identifiers such as `iroha`, `hina`, and `kyaruberos` in `Assets/Scripts/AL-1S/MahjongTileAndBlock.cs` and `Assets/Scripts/AL-1S/MahjongUtilities.cs` are legacy domain-flavored names, not a pattern to extend.
- Use PascalCase constants and static preset names only where the existing public API already exposes that form (`InputLists.MoveLeft` in `Assets/Scripts/Configs/InputLists.cs`); do not introduce new lowercase public statics like `InputPreset.left` in `Assets/Scripts/InputManager.cs`.
- Use PascalCase for classes, structs, enums, and enum members, following `MahjongRound`, `MahjongTile`, `GameState`, and `PlayerCallType` in `Assets/Scripts/AL-1S/MahjongRound.cs`, `Assets/Scripts/AL-1S/MahjongTileAndBlock.cs`, and `Assets/Scripts/AL-1S/_Enums_Ecchi_Nun_Mari.cs`.
- Prefix interfaces with `I`, following `IScoreDistanceService`, `IScoreDistanceConsumer`, and `ILocalizable` in `Assets/Scripts/IScoreDistanceService.cs`, `Assets/Scripts/IScoreDistanceConsumer.cs`, and `Assets/Scripts/ILocalizable.cs`.
- No first-party C# file declares a namespace under `Assets/Scripts/**/*.cs` or `Assets/Editor/**/*.cs`; new code must account for the current global-namespace API unless a coordinated namespace migration is explicitly scoped.

## Code Style

- No `.editorconfig`, C# formatter configuration, or StyleCop configuration is present; formatting is manual across `Assets/Scripts/**/*.cs` and `Assets/Editor/MahjongTileDataGenerator.cs`.
- Use four-space indentation and Allman braces in new or edited blocks, matching the clean sections of `Assets/Scripts/ScoreManagerDistance.cs` and `Assets/Scripts/Configs/SettingsManager.cs`. Existing same-line braces in `Assets/Scripts/AL-1S/MahjongRound.cs` and `Assets/Scripts/SoundArchive/MusicManager.cs` should not be copied.
- Keep one statement per line and normal spacing around operators and commas. Avoid extending compressed forms such as `a.y+b.y` in `Assets/Scripts/AL-1S/Utilities.cs` or `+=StartNextRound` in `Assets/Scripts/MahjongGameManager.cs`.
- Use C# 9-compatible syntax: Unity generates `<LangVersion>9.0</LangVersion>` in `Assembly-CSharp.csproj`, while the editor version is fixed in `ProjectSettings/ProjectVersion.txt`.
- Put Unity lifecycle methods together near the top of a `MonoBehaviour` when changing a class. Lifecycle methods are currently scattered in `Assets/Scripts/MahjongGameManager.cs` and `Assets/Scripts/UI-Kozeki/PlayerHand.cs`, so preserve behavior while grouping only code already in scope.
- No repository lint command or analyzer ruleset is configured in `.vscode/settings.json`, `Assembly-CSharp.csproj`, or the project root; rely on Unity compilation and IDE diagnostics.
- Treat compiler warnings as actionable. Unity-generated `Assembly-CSharp.csproj` uses warning level 4 and suppresses only `0169` and `USG0001`; do not add broad suppressions to first-party files under `Assets/Scripts/`.
- Remove unused imports when touching a file. Several files carry unused namespaces, and runtime scripts must not import editor-only namespaces such as `UnityEditor` in `Assets/Scripts/UI-Kozeki/UiScoreDistanceInfo.cs` or `UnityEditor.Rendering` in `Assets/Scripts/AL-1S/MahjongTileAndBlock.cs`.

## Import Organization

- Not applicable: first-party code uses ordinary `using` directives and the global namespace throughout `Assets/Scripts/**/*.cs`; no alias convention is configured in `Assembly-CSharp.csproj`.

## Error Handling

- Use early returns for invalid state or unavailable input, following `ScoreManagerDistance.Update`, `GetBoost`, and `GetInstantDistance` in `Assets/Scripts/ScoreManagerDistance.cs` and panel lookup guards in `Assets/Scripts/UI-Kozeki/GameUIManager.cs`.
- At persistence boundaries, catch the narrow expected exceptions and return safe default data. `Assets/Scripts/Configs/SettingsManager.cs` catches `IOException` and `JsonException`, logs the failure, and returns `PetitGameSaveData` rather than propagating corrupt or missing save data.
- Domain parsers use explicit sentinels for invalid tile input (`MahjongTile.NullTile`) in `Assets/Scripts/AL-1S/MahjongTileAndBlock.cs`; collection-returning APIs generally return empty collections in `Assets/Scripts/AL-1S/MahjongUtilities.cs`. Preserve the established contract of the API being changed instead of mixing `null`, sentinel, and empty results.
- Throw only for violated programmer contracts that cannot be represented by an existing domain result, as `Utilities.GetRandomItem` does for a null or empty collection in `Assets/Scripts/AL-1S/Utilities.cs`.
- Pair event subscriptions with deterministic unsubscriptions, following `OnEnable`/`OnDisable` in `Assets/Scripts/SoundArchive/MusicManager.cs` and `Assets/Scripts/MahjongGameManager.cs`; use the same owner and handler on both sides.

## Logging

- Use `Debug.LogWarning` or `Debug.LogError` for diagnostics that must remain visible, following load failures in `Assets/Scripts/Configs/SettingsManager.cs` and missing clips in `Assets/Scripts/SoundArchive/SoundArchive.cs`.
- Do not rely on `MyLogger` for essential diagnostics: `Assets/Scripts/AL-1S/MyLogger.cs` defines and immediately undefines `HIMARI`, so its methods compile to no-ops in the current configuration.
- Keep routine success or per-frame logging out of hot paths. `Assets/Scripts/UI-Kozeki/UiScoreDistanceInfo.cs` updates every frame, while logs are reserved for missing service/setup states.
- Include the failing operation and useful context in warnings/errors, as the save path and exception are included by `Assets/Scripts/Configs/SettingsManager.cs`.

## Comments

- Write comments and XML summaries in Korean for new or changed first-party code, consistent with domain explanations in `Assets/Scripts/AL-1S/MahjongUtilities.cs` and lifecycle intent in `Assets/Scripts/Timer.cs`.
- Explain rules, invariants, and Unity/editor constraints rather than restating code. Good examples include the 13/14-tile preconditions in `Assets/Scripts/AL-1S/MahjongUtilities.cs` and database generation behavior in `Assets/Editor/MahjongTileDataGenerator.cs`.
- Do not retain large commented-out implementations or empty Unity template methods when touching a file; legacy examples exist in `Assets/Scripts/Configs/SettingsManager.cs`, `Assets/Scripts/UI-Kozeki/UiManager.cs`, `Assets/Scripts/InputManager.cs`, and `Assets/Scripts/LocalizationManager.cs`.
- Not applicable; use C# XML documentation (`/// <summary>`) for public types and behavior-changing public members. Existing usage is concentrated in `Assets/Scripts/AL-1S/MahjongRound.cs`, `Assets/Scripts/AL-1S/MahjongUtilities.cs`, and `Assets/Scripts/AL-1S/_Structs.cs`.
- Keep `<param>` and `<returns>` text meaningful; avoid empty tags like those currently present in parts of `Assets/Scripts/MahjongGameManager.cs` and `Assets/Scripts/UI-Kozeki/UiManager.cs`.

## Function Design

- Pass domain values directly and use `out` only where the API creates multiple related results, as in `MahjongRound.NewRound(int, out MahjongPlayer)` in `Assets/Scripts/AL-1S/MahjongRound.cs`.
- Inject cross-component services through explicit `Construct(...)` methods when a scene reference is not appropriate, following `IScoreDistanceConsumer` in `Assets/Scripts/IScoreDistanceConsumer.cs`, `Assets/Scripts/MahjongGameManager.cs`, and `Assets/Scripts/UI-Kozeki/UiScoreDistanceInfo.cs`.
- Avoid adding boolean switches when an enum or named method already describes the operation, following `PlayerCallType` dispatch in `Assets/Scripts/MahjongGameManager.cs`.
- Prefer early-returned empty collections for “no matches” in calculation APIs, following `FindAgariTiles` and `CheckWinnable` in `Assets/Scripts/AL-1S/MahjongUtilities.cs`.
- Preserve value-object sentinels such as `MahjongTile.NullTile()` for invalid tile parsing in `Assets/Scripts/AL-1S/MahjongTileAndBlock.cs`; callers compare against this sentinel in `Assets/Scripts/AL-1S/MahjongRound.cs`.
- Expose read-only state through properties with private setters or expression-bodied getters, following `Distance`, `BoostLevel`, and `DistanceWithAccumulated` in `Assets/Scripts/ScoreManagerDistance.cs`.

## Module Design

- First-party runtime scripts compile into the default `Assembly-CSharp` assembly because no `.asmdef` exists under `Assets/Scripts/`; editor tooling is separated structurally under `Assets/Editor/MahjongTileDataGenerator.cs`.
- Use `[SerializeField] private` for inspector wiring while keeping runtime mutation private, following `Assets/Scripts/MahjongGameManager.cs`, `Assets/Scripts/ScoreManagerDistance.cs`, and `Assets/Scripts/UI-Kozeki/MahjongTileGameObject.cs`.
- Use `ScriptableObject` for authored asset data (`Assets/Scripts/MahjongTileDatabase.cs`) and plain C# classes/static functions for domain calculations (`Assets/Scripts/AL-1S/MahjongUtilities.cs`).
- Existing scene coordinators expose singleton entry points (`MahjongGameManager.Instance`, `GameUIManager.Instance`, and `AudioManager.instance`) in `Assets/Scripts/MahjongGameManager.cs`, `Assets/Scripts/UI-Kozeki/GameUIManager.cs`, and `Assets/Scripts/SoundArchive/AudioManager.cs`; reuse the relevant existing owner rather than adding another global manager.
- Partial types group related behavior inside large files (`MahjongGameManager` in `Assets/Scripts/MahjongGameManager.cs`, `MahjongUtility` in `Assets/Scripts/AL-1S/MahjongUtilities.cs`, and `MahjongTile` in `Assets/Scripts/AL-1S/MahjongTileAndBlock.cs`). Add to the owning type only when the behavior belongs to that existing responsibility.
- Not applicable: C# code uses the global namespace and Unity's generated assemblies; there are no barrel/index files under `Assets/Scripts/`.

<!-- GSD:conventions-end -->

<!-- GSD:architecture-start source:ARCHITECTURE.md -->

## Architecture

## System Overview

```text

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

- Use the scene as the composition root: serialized fields in `Assets/Scripts/MahjongGameManager.cs`, `Assets/Scripts/UI-Kozeki/GameUIManager.cs`, and `Assets/Scripts/UI-Kozeki/UiManager.cs` are wired in `Assets/Scenes/SampleScene.unity`.
- Keep rules and scoring in the POCO/struct domain types under `Assets/Scripts/AL-1S/`; `MahjongRound` exposes events rather than directly referencing UI types.
- Route the primary runtime flow through `MahjongGameManager` in `Assets/Scripts/MahjongGameManager.cs`; UI sends intent upward, the round mutates domain state, and events send results back to presenters.
- Use interface injection only at the existing score-service seam in `Assets/Scripts/IScoreDistanceService.cs` and `Assets/Scripts/IScoreDistanceConsumer.cs`; other dependencies remain concrete scene references.
- Treat `Assets/Scenes/SampleScene.unity` as a single-scene application: it is the sole enabled scene in `ProjectSettings/EditorBuildSettings.asset`.

## Layers

- Purpose: Assemble components and invoke Unity lifecycle methods and serialized button callbacks.
- Location: `Assets/Scenes/SampleScene.unity`, `ProjectSettings/EditorBuildSettings.asset`
- Contains: The `Managers` hierarchy, canvases, `EventSystem`, camera/light, instantiated UI prefabs, and serialized component references.
- Depends on: Runtime `MonoBehaviour` classes under `Assets/Scripts/` and assets under `Assets/Prefaps/`, `Assets/ScriptableObjects/`, `Assets/Materials/`, and `Assets/Sprites/`.
- Used by: Unity player startup configured by `ProjectSettings/EditorBuildSettings.asset`.
- Purpose: Coordinate game lifecycle, state transitions, timing, score-distance progression, and UI/domain communication.
- Location: `Assets/Scripts/MahjongGameManager.cs`, `Assets/Scripts/ScoreManagerDistance.cs`, `Assets/Scripts/Timer.cs`
- Contains: `MonoBehaviour` coordinators, the `GameState` gate, game lifecycle events, and the injected score-distance service.
- Depends on: The domain layer in `Assets/Scripts/AL-1S/`, presenters in `Assets/Scripts/UI-Kozeki/`, persistence in `Assets/Scripts/Configs/`, and scene references in `Assets/Scenes/SampleScene.unity`.
- Used by: `Assets/Scripts/UI-Kozeki/UiManager.cs`, `Assets/Scripts/UI-Kozeki/PlayerHand.cs`, and `Assets/Scripts/UI-Kozeki/GameUIManager.cs`.
- Purpose: Represent tiles, blocks, hands, players, rounds, win decompositions, yaku, fu, han, and score tables.
- Location: `Assets/Scripts/AL-1S/`
- Contains: `MahjongTile`, `MahjongBlock`, `MahjongRound`, `MahjongPlayer`, `MahjongWin`, `MahjongWinInfo`, `MahjongUtility`, `MahjongYakuSolver`, enums, and round/event DTOs.
- Depends on: Core .NET collections/LINQ plus limited Unity types/logging in `Assets/Scripts/AL-1S/MahjongUtilities.cs`, `Assets/Scripts/AL-1S/_Structs.cs`, and `Assets/Scripts/AL-1S/MyLogger.cs`.
- Used by: `Assets/Scripts/MahjongGameManager.cs`, all gameplay presenters under `Assets/Scripts/UI-Kozeki/`, and `Assets/Editor/MahjongTileDataGenerator.cs`.
- Purpose: Display menu/game state, animate panels, render tiles and scoring, and translate keyboard/button activity into application intents.
- Location: `Assets/Scripts/UI-Kozeki/`, `Assets/Scripts/DASandARR.cs`, `Assets/Scripts/InputManager.cs`
- Contains: Menu/game panel coordinators, hand/tile presenters, score/win/round/timer views, DOTween extensions, and legacy `UnityEngine.Input` polling.
- Depends on: `Assets/Scripts/MahjongGameManager.cs`, `Assets/Scripts/IScoreDistanceService.cs`, `Assets/Scripts/Timer.cs`, domain DTOs in `Assets/Scripts/AL-1S/`, TextMesh Pro, uGUI, and DOTween in `Assets/Plugins/Demigiant/`.
- Used by: Scene objects and button callbacks serialized in `Assets/Scenes/SampleScene.unity`.
- Purpose: Resolve tile sprites, persist user data, and play audio without placing those details inside mahjong rules.
- Location: `Assets/Scripts/MahjongTileDatabase.cs`, `Assets/Scripts/Configs/`, `Assets/Scripts/SoundArchive/`
- Contains: A `ScriptableObject` tile database, JSON DTO/store, PlayerPrefs-backed volume state, audio source management, and named audio clip lookup.
- Depends on: `Assets/ScriptableObjects/MahjongTileDatabase.asset`, `Assets/Sprites/MahjongTiles/`, Newtonsoft JSON from `Packages/manifest.json`, and scene audio components in `Assets/Scenes/SampleScene.unity`.
- Used by: `Assets/Scripts/UI-Kozeki/MahjongTileGameObject.cs`, `Assets/Scripts/MahjongGameManager.cs`, and `Assets/Scripts/SoundArchive/MusicManager.cs`.
- Purpose: Generate project assets without including editor-only entry points in runtime lifecycle code.
- Location: `Assets/Editor/MahjongTileDataGenerator.cs`
- Contains: An `EditorWindow`, menu registration, sprite discovery, and ScriptableObject creation/update.
- Depends on: Domain tile enumeration in `Assets/Scripts/AL-1S/MahjongTileAndBlock.cs` and asset schema in `Assets/Scripts/MahjongTileDatabase.cs`.
- Used by: Unity Editor users through `Tools/MahjongTileDataGenerator` in `Assets/Editor/MahjongTileDataGenerator.cs`.

## Data Flow

### Primary Request Path

### Score, Timer, and Game-Over Flow

### Tile Asset Generation and Lookup

- Game lifecycle state is the mutable `currentState` field on the persistent `MahjongGameManager` singleton in `Assets/Scripts/MahjongGameManager.cs`; use it as the input gate for player actions.
- Current mahjong state lives in the non-`MonoBehaviour` `MahjongRound`/`MahjongPlayer` object graph in `Assets/Scripts/AL-1S/MahjongRound.cs`; replace the aggregate between rounds and reattach events through the manager.
- Menu and in-game panel state are separate enum-to-panel maps in `Assets/Scripts/UI-Kozeki/UiManager.cs` and `Assets/Scripts/UI-Kozeki/GameUIManager.cs`; preserve that boundary when adding panels.
- Long-lived user data is split between JSON in `Assets/Scripts/Configs/SettingsManager.cs` and audio `PlayerPrefs` in `Assets/Scripts/SoundArchive/AudioManager.cs`.

## Key Abstractions

- Purpose: Value types for tile identity/flags and pair/sequence/triplet groupings.
- Examples: `Assets/Scripts/AL-1S/MahjongTileAndBlock.cs`
- Pattern: Comparable/equatable structs with static parsing/factory helpers; use these instead of raw strings or integers outside parsing and serialization boundaries.
- Purpose: Aggregate root for a single-player round and the event boundary used by the application layer.
- Examples: `Assets/Scripts/AL-1S/MahjongRound.cs`, `Assets/Scripts/MahjongGameManager.cs`
- Pattern: Plain C# object with private mutable collections and public domain events; subscribe/detach only in the manager's `AttachRoundEvent`/`DetachRoundEvent` methods.
- Purpose: Convert a 13/14-tile hand into decompositions, yaku, han/fu, and payments.
- Examples: `Assets/Scripts/AL-1S/MahjongUtilities.cs`, `Assets/Scripts/AL-1S/MahjongWinInfo.cs`, `Assets/Scripts/AL-1S/MahjongYaku.cs`
- Pattern: Static solver functions produce `MahjongWin`, then `MahjongWinInfo` derives immutable-at-construction scoring information; reuse this pipeline for new win checks.
- Purpose: Decouple score producers and UI consumers from the concrete frame-updated distance model.
- Examples: `Assets/Scripts/IScoreDistanceService.cs`, `Assets/Scripts/IScoreDistanceConsumer.cs`, `Assets/Scripts/ScoreManagerDistance.cs`
- Pattern: Manual constructor-style injection through `Construct`; use the existing scene-owned `ScoreManagerDistance` rather than adding another global score singleton.
- Purpose: Bridge domain tile codes and Unity sprite assets.
- Examples: `Assets/Scripts/MahjongTileDatabase.cs`, `Assets/ScriptableObjects/MahjongTileDatabase.asset`, `Assets/Editor/MahjongTileDataGenerator.cs`
- Pattern: Serialized list as source data plus a runtime dictionary rebuilt in `OnEnable`; update it through the generator when sprite inventory changes.
- Purpose: Resolve menu/game UI states to `RectTransform`, `CanvasGroup`, direction, and original position.
- Examples: `Assets/Scripts/UI-Kozeki/UiManager.cs`, `Assets/Scripts/UI-Kozeki/GameUIManager.cs`, `Assets/Scripts/AL-1S/_Structs.cs`
- Pattern: Inspector-authored lists converted to dictionaries in `Awake`, then animated through shared extensions in `Assets/Scripts/UI-Kozeki/UiTransition.cs`.

## Entry Points

- Location: `Assets/Scenes/SampleScene.unity`
- Triggers: Unity loads the only enabled scene declared in `ProjectSettings/EditorBuildSettings.asset`.
- Responsibilities: Instantiate the complete menu/game UI and the `Managers` hierarchy, including `MahjongGameManager`, `UiManager`, `GameUIManager`, `ScoreManagerDistance`, `Timer`, and audio components.
- Location: `Assets/Scripts/UI-Kozeki/UiManager.cs`
- Triggers: Serialized Unity button calls in `Assets/Scenes/SampleScene.unity` and `Assets/Prefaps/재포장된 라자.prefab`.
- Responsibilities: Navigate menu panels, maintain back history, enter game UI, and delegate game creation to `MahjongGameManager`.
- Location: `Assets/Scripts/UI-Kozeki/PlayerHand.cs`, `Assets/Scripts/ScoreManagerDistance.cs`, `Assets/Scripts/Timer.cs`
- Triggers: Unity `Update` on the main thread.
- Responsibilities: Poll player input, update selection visuals, decay/accumulate distance, and count down the game timer.
- Location: `Assets/Scripts/MahjongGameManager.cs`
- Triggers: `UiManager.OnGameStartButton`, `PlayerHand` events, `MahjongRound` events, and `Timer.OnTimerFinished`.
- Responsibilities: Initialize services and rounds, mediate domain/UI events, gate calls by state, and finalize/persist game results.
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

### Putting Editor Dependencies in Runtime Files

### Adding Parallel State Machines

## Error Handling

- Domain parsers and solvers return `MahjongTile.NullTile()`, empty collections, or `false` for invalid/non-winning input in `Assets/Scripts/AL-1S/MahjongTileAndBlock.cs` and `Assets/Scripts/AL-1S/MahjongUtilities.cs`.
- Scene/application handlers guard illegal state with early returns in `Assets/Scripts/MahjongGameManager.cs`, `Assets/Scripts/ScoreManagerDistance.cs`, and `Assets/Scripts/Timer.cs`.
- Missing tile sprites fall back to a serialized sprite and log through `MyLogger` in `Assets/Scripts/MahjongTileDatabase.cs`; missing audio names select a fallback clip in `Assets/Scripts/SoundArchive/SoundArchive.cs`.
- Save/load catches `IOException` and load additionally catches `JsonException`, returning fresh DTOs rather than aborting gameplay in `Assets/Scripts/Configs/SettingsManager.cs`.
- Unity event fields use empty delegate defaults to avoid null checks in `Assets/Scripts/MahjongGameManager.cs`, `Assets/Scripts/AL-1S/MahjongRound.cs`, `Assets/Scripts/ScoreManagerDistance.cs`, and `Assets/Scripts/Timer.cs`.

## Cross-Cutting Concerns

<!-- GSD:architecture-end -->

<!-- GSD:skills-start source:skills/ -->

## Project Skills

No project skills found. Add skills to any of: `.claude/skills/`, `.agents/skills/`, `.cursor/skills/`, `.github/skills/`, or `.codex/skills/` with a `SKILL.md` index file.
<!-- GSD:skills-end -->

<!-- GSD:workflow-start source:GSD defaults -->

## GSD Workflow Enforcement

Before using Edit, Write, or other file-changing tools, start work through a GSD command so planning artifacts and execution context stay in sync.

Use these entry points:

- `/gsd-quick` for small fixes, doc updates, and ad-hoc tasks
- `/gsd-debug` for investigation and bug fixing
- `/gsd-execute-phase` for planned phase work

Do not make direct repo edits outside a GSD workflow unless the user explicitly asks to bypass it.
<!-- GSD:workflow-end -->

<!-- GSD:profile-start -->

## Developer Profile

> Profile not yet configured. Run `/gsd-profile-user` to generate your developer profile.
> This section is managed by `generate-claude-profile` -- do not edit manually.
<!-- GSD:profile-end -->
