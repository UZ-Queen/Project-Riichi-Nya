# Codebase Structure

**Analysis Date:** 2026-08-28

## Directory Layout

```
Project-Riichi-Nya/
├── Assets/                         # Unity-owned source assets and project code
│   ├── Editor/                     # Editor-only tile database generator
│   ├── Fonts/                      # TextMesh Pro font assets and materials
│   ├── Localizations/              # Korean JSON localization data (currently unused by a completed manager)
│   ├── Materials/                  # Shader Graphs, materials, and visual-effect assets
│   ├── Plugins/Demigiant/          # Vendored DOTween, DOTween Pro, and DemiLib assets
│   ├── Prefaps/                    # Project prefabs; directory name is intentionally preserved as-is
│   ├── Resources/                  # Unity Resources-loaded package settings
│   ├── Scenes/                     # Runtime scenes; one build scene exists
│   ├── ScriptableObjects/          # Generated tile database asset
│   ├── Scripts/                    # First-party runtime C# code
│   │   ├── AL-1S/                  # Mahjong domain model, rules, solver, and logging
│   │   ├── Configs/                # Save/config DTOs and JSON persistence
│   │   ├── SoundArchive/           # Audio playback, music, and clip archive
│   │   └── UI-Kozeki/              # Menu/game UI presenters and transitions
│   ├── Settings/                   # URP renderer, quality, and volume profile assets
│   ├── Sprites/                    # Mahjong tiles and other image assets
│   └── TextMesh Pro/               # Imported TMP resources, shaders, fonts, and documentation
├── Packages/                       # Unity Package Manager manifest and lockfile
├── ProjectSettings/                # Unity project, build, render, input, physics, and editor settings
├── Library/                        # Generated Unity import/cache data; ignored
├── Logs/                           # Generated Unity logs; ignored
├── Temp/                           # Generated Unity temporary data; ignored
├── UserSettings/                   # Machine/user-specific Unity settings; ignored
├── .planning/codebase/             # Generated GSD codebase reference documents
├── .vscode/                        # Checked-in VS Code workspace settings
├── .VSCodeCounter/                 # Checked-in historical code-count reports
├── Packages/manifest.json          # Declared Unity package dependencies
├── ProjectSettings/ProjectVersion.txt # Required Unity editor version
└── README.md                       # One-line project description
```

## Directory Purposes

**`Assets/Scripts/`:**
- Purpose: Hold all first-party runtime code compiled by Unity into the default `Assembly-CSharp` assembly.
- Contains: Application managers, the score-distance service seam, timer/input helpers, tile database schema, and four responsibility-based subdirectories.
- Key files: `Assets/Scripts/MahjongGameManager.cs`, `Assets/Scripts/ScoreManagerDistance.cs`, `Assets/Scripts/Timer.cs`, `Assets/Scripts/MahjongTileDatabase.cs`

**`Assets/Scripts/AL-1S/`:**
- Purpose: Hold the mahjong domain model and rule engine; place new tile/round/win/yaku rules here.
- Contains: Tile/block value types, player/round aggregates, hand decomposition, yaku detection, scoring DTOs, shared domain helpers, and `MyLogger`.
- Key files: `Assets/Scripts/AL-1S/MahjongTileAndBlock.cs`, `Assets/Scripts/AL-1S/MahjongRound.cs`, `Assets/Scripts/AL-1S/MahjongUtilities.cs`, `Assets/Scripts/AL-1S/MahjongWinInfo.cs`, `Assets/Scripts/AL-1S/MahjongYaku.cs`

**`Assets/Scripts/UI-Kozeki/`:**
- Purpose: Hold uGUI/TextMesh Pro presenters, interaction handlers, and UI animation helpers; place new screens or game HUD views here.
- Contains: Menu and gameplay panel coordinators, player hand/tile presenters, win/score/round/time components, and DOTween transition extensions.
- Key files: `Assets/Scripts/UI-Kozeki/UiManager.cs`, `Assets/Scripts/UI-Kozeki/GameUIManager.cs`, `Assets/Scripts/UI-Kozeki/PlayerHand.cs`, `Assets/Scripts/UI-Kozeki/UiTransition.cs`

**`Assets/Scripts/Configs/`:**
- Purpose: Hold serializable user-data DTOs, input action names, and local persistence logic.
- Contains: `PetitGameSaveData`, sound/input/statistics settings, string action identifiers, and Newtonsoft JSON load/save/delete operations.
- Key files: `Assets/Scripts/Configs/Settings.cs`, `Assets/Scripts/Configs/SettingsManager.cs`, `Assets/Scripts/Configs/InputLists.cs`

**`Assets/Scripts/SoundArchive/`:**
- Purpose: Hold the complete audio subsystem; keep audio clip lookup and playback changes together here.
- Contains: Singleton audio-source management, scene-triggered music selection, and name-to-clip archive data.
- Key files: `Assets/Scripts/SoundArchive/AudioManager.cs`, `Assets/Scripts/SoundArchive/MusicManager.cs`, `Assets/Scripts/SoundArchive/SoundArchive.cs`

**`Assets/Editor/`:**
- Purpose: Hold code that may reference `UnityEditor` and must never enter a player assembly.
- Contains: The custom `EditorWindow` that constructs the tile sprite database.
- Key files: `Assets/Editor/MahjongTileDataGenerator.cs`

**`Assets/Scenes/`:**
- Purpose: Hold Unity scene composition roots.
- Contains: The complete single-scene menu/game runtime, serialized manager wiring, canvases, lighting, event system, and prefab instances.
- Key files: `Assets/Scenes/SampleScene.unity`, `Assets/Scenes/SampleScene.unity.meta`

**`Assets/Prefaps/`:**
- Purpose: Hold reusable serialized GameObject/UI templates; preserve the existing misspelled directory name to avoid splitting assets across two roots.
- Contains: Mahjong tile presentation prefabs, the reusable yaku row, and the reusable UI/button prefab.
- Key files: `Assets/Prefaps/MahjongTile PreFap.prefab`, `Assets/Prefaps/Tile for 2D sprite.prefab`, `Assets/Prefaps/UI Yaku Preset(리치 1판).prefab`, `Assets/Prefaps/재포장된 라자.prefab`

**`Assets/ScriptableObjects/`:**
- Purpose: Hold project-authored serialized data assets used at runtime.
- Contains: The generated tile-code-to-sprite database referenced by the scene's game manager.
- Key files: `Assets/ScriptableObjects/MahjongTileDatabase.asset`, `Assets/Scripts/MahjongTileDatabase.cs`

**`Assets/Sprites/` and `Assets/Materials/`:**
- Purpose: Hold first-party visual inputs consumed by prefabs, UI, and rendering.
- Contains: Tile images/code-named PNGs under `Assets/Sprites/MahjongTiles/`, other UI/reference images under `Assets/Sprites/`, and Shader Graph/material assets under `Assets/Materials/`.
- Key files: `Assets/Sprites/MahjongTiles/1m.png`, `Assets/Sprites/MahjongTiles/BGs/Front.png`, `Assets/Materials/ooh Shiny.shadergraph`

**`Assets/Plugins/` and `Assets/TextMesh Pro/`:**
- Purpose: Hold vendored third-party Unity assets; treat these as upstream/imported code rather than first-party extension points.
- Contains: DOTween/DOTween Pro/DemiLib binaries and source modules under `Assets/Plugins/Demigiant/`, plus imported TMP resources under `Assets/TextMesh Pro/`.
- Key files: `Assets/Plugins/Demigiant/DOTween/DOTween.dll`, `Assets/Plugins/Demigiant/DOTweenPro/DOTweenPro.dll`, `Assets/TextMesh Pro/Resources/TMP Settings.asset`

**`Packages/`:**
- Purpose: Declare and lock Unity Package Manager dependencies.
- Contains: Direct package versions and their resolved transitive dependency graph.
- Key files: `Packages/manifest.json`, `Packages/packages-lock.json`

**`ProjectSettings/`:**
- Purpose: Define Unity editor/runtime behavior for the project; keep settings changes under version control with matching asset references.
- Contains: Unity version, build scenes, URP/quality/graphics settings, legacy input map, physics, audio, tags/layers, and package manager settings.
- Key files: `ProjectSettings/ProjectVersion.txt`, `ProjectSettings/EditorBuildSettings.asset`, `ProjectSettings/GraphicsSettings.asset`, `ProjectSettings/InputManager.asset`, `ProjectSettings/QualitySettings.asset`

## Key File Locations

**Entry Points:**
- `Assets/Scenes/SampleScene.unity`: Sole enabled build scene and runtime composition root.
- `Assets/Scripts/UI-Kozeki/UiManager.cs`: Menu button callbacks and transition into gameplay.
- `Assets/Scripts/MahjongGameManager.cs`: Game lifecycle/application entry point.
- `Assets/Scripts/UI-Kozeki/PlayerHand.cs`: Per-frame gameplay input entry point.
- `Assets/Editor/MahjongTileDataGenerator.cs`: Unity Editor `Tools` menu entry point.

**Configuration:**
- `Packages/manifest.json`: Direct Unity package declarations.
- `Packages/packages-lock.json`: Resolved package graph.
- `ProjectSettings/ProjectVersion.txt`: Unity `2022.3.29f1` editor pin.
- `ProjectSettings/EditorBuildSettings.asset`: Enabled scene list containing `Assets/Scenes/SampleScene.unity`.
- `ProjectSettings/GraphicsSettings.asset`: Global graphics/render-pipeline binding.
- `ProjectSettings/QualitySettings.asset`: Quality-level definitions and URP quality asset selection.
- `Assets/Settings/`: URP renderer/data assets and scene volume profile.
- `Assets/Resources/DOTweenSettings.asset`: DOTween runtime/editor configuration discoverable through Unity Resources.

**Core Logic:**
- `Assets/Scripts/MahjongGameManager.cs`: UI/domain/service orchestration and game state.
- `Assets/Scripts/AL-1S/MahjongRound.cs`: Round/player/wall lifecycle and score events.
- `Assets/Scripts/AL-1S/MahjongTileAndBlock.cs`: Tile/block value model and text/ID conversion.
- `Assets/Scripts/AL-1S/MahjongUtilities.cs`: Hand decomposition, wait discovery, fu/han/base-score helpers.
- `Assets/Scripts/AL-1S/MahjongYaku.cs`: Yaku registry and detector functions.
- `Assets/Scripts/AL-1S/MahjongWinInfo.cs`: Win-derived facts and final score table.
- `Assets/Scripts/ScoreManagerDistance.cs`: Score-to-distance minigame progression.

**Testing:**
- Not detected under `Assets/`; no first-party test files, `Assets/Tests/` directory, `.asmdef`, or `.asmref` files exist.
- `Packages/manifest.json`: Unity Test Framework is installed, but no project tests are organized around it.

## Naming Conventions

**Files:**
- Use a public type's PascalCase name for its C# filename, following `Assets/Scripts/MahjongGameManager.cs`, `Assets/Scripts/ScoreManagerDistance.cs`, and `Assets/Scripts/UI-Kozeki/PlayerHand.cs`.
- Prefix UI presenter classes/files with `Ui` and reserve `GameUIManager` for the gameplay panel coordinator, following `Assets/Scripts/UI-Kozeki/UiWinInfo.cs`, `Assets/Scripts/UI-Kozeki/UiRoundInfo.cs`, and `Assets/Scripts/UI-Kozeki/GameUIManager.cs`.
- Prefix interfaces with `I`, following `Assets/Scripts/IScoreDistanceService.cs`, `Assets/Scripts/IScoreDistanceConsumer.cs`, and `Assets/Scripts/ILocalizable.cs`.
- Keep Unity asset and `.meta` files together; moving `Assets/Prefaps/MahjongTile PreFap.prefab` requires moving `Assets/Prefaps/MahjongTile PreFap.prefab.meta` with it.
- Preserve code-named tile sprite filenames (`0m.png` through `9s.png` and `1z.png` through `7z.png`) because `Assets/Editor/MahjongTileDataGenerator.cs` derives filenames from `MahjongTile.ToString()`.
- Do not copy legacy underscore-prefixed filenames from `Assets/Scripts/AL-1S/_Structs.cs` and `Assets/Scripts/AL-1S/_Enums_Ecchi_Nun_Mari.cs` for new files; the dominant first-party pattern is PascalCase.

**Directories:**
- Use responsibility-based PascalCase names for new top-level asset folders, matching `Assets/Scenes/`, `Assets/Materials/`, `Assets/ScriptableObjects/`, and `Assets/Settings/`.
- Reuse the existing ownership/responsibility folders `Assets/Scripts/AL-1S/`, `Assets/Scripts/UI-Kozeki/`, `Assets/Scripts/Configs/`, and `Assets/Scripts/SoundArchive/` instead of adding a parallel hierarchy for the same code.
- Continue using `Assets/Prefaps/` for prefabs unless a deliberate repository-wide rename moves every asset and `.meta`; do not create a competing `Assets/Prefabs/` directory.

## Where to Add New Code

**New Feature:**
- Primary code: Put mahjong rule/state changes in `Assets/Scripts/AL-1S/`; put cross-domain lifecycle coordination in `Assets/Scripts/MahjongGameManager.cs` or a narrowly scoped sibling under `Assets/Scripts/`; put its presenter under `Assets/Scripts/UI-Kozeki/`.
- Tests: No convention exists yet; create the first isolated rule tests under `Assets/Tests/EditMode/` and scene/lifecycle tests under `Assets/Tests/PlayMode/`, then add the required test assembly definitions alongside them without changing runtime placement under `Assets/Scripts/`.

**New Component/Module:**
- Implementation: Add a reusable UI component to `Assets/Scripts/UI-Kozeki/`, a sound component to `Assets/Scripts/SoundArchive/`, a persistence/config component to `Assets/Scripts/Configs/`, or a domain component to `Assets/Scripts/AL-1S/`; add a cross-cutting gameplay service next to `Assets/Scripts/ScoreManagerDistance.cs` and define an interface only when it has the same real injection need as `Assets/Scripts/IScoreDistanceService.cs`.
- Scene wiring: Add serialized component references and panel mappings in `Assets/Scenes/SampleScene.unity`; keep prefabricated GameObjects in `Assets/Prefaps/`.
- Editor tooling: Put any code importing `UnityEditor` under `Assets/Editor/`, following `Assets/Editor/MahjongTileDataGenerator.cs`.
- Data assets: Put runtime ScriptableObjects under `Assets/ScriptableObjects/`, source sprites under `Assets/Sprites/`, and render assets under `Assets/Materials/` or `Assets/Settings/` according to type.

**Utilities:**
- Shared helpers: Put mahjong-specific calculations/parsers in the existing partial `MahjongUtility` at `Assets/Scripts/AL-1S/MahjongUtilities.cs`; put only genuinely generic domain helpers in `Assets/Scripts/AL-1S/Utilities.cs`; put UI animation helpers in `Assets/Scripts/UI-Kozeki/UiTransition.cs`.
- Input helpers: Extend `Assets/Scripts/DASandARR.cs` for key-repeat mechanics and keep action names/default bindings in `Assets/Scripts/Configs/InputLists.cs` and `Assets/Scripts/InputManager.cs` until the input backend is intentionally replaced.

## Special Directories

**`Library/`:**
- Purpose: Unity import database, generated assemblies, package cache, and editor cache.
- Generated: Yes; ignored by `.gitignore`.
- Committed: No; recreate by opening the project with the editor version in `ProjectSettings/ProjectVersion.txt`.

**`Temp/` and `Logs/`:**
- Purpose: Unity temporary work products and editor/player logs.
- Generated: Yes; ignored by `.gitignore`.
- Committed: No; never place source or durable diagnostics in `Temp/` or `Logs/`.

**`UserSettings/`:**
- Purpose: Machine/user-specific Unity editor state.
- Generated: Yes; ignored by `.gitignore`.
- Committed: No; shared project settings belong in `ProjectSettings/` instead.

**`Assets/Plugins/Demigiant/`:**
- Purpose: Vendored DOTween, DOTween Pro, and DemiLib distribution.
- Generated: No; imported third-party content.
- Committed: Yes; extend animation behavior from `Assets/Scripts/UI-Kozeki/UiTransition.cs` rather than editing vendor files under `Assets/Plugins/Demigiant/`.

**`Assets/TextMesh Pro/`:**
- Purpose: Imported TextMesh Pro resources, shaders, default fonts, sprites, and documentation.
- Generated: No; package-imported content.
- Committed: Yes; project-specific font assets belong in `Assets/Fonts/`.

**`Assets/ScriptableObjects/`:**
- Purpose: Runtime project data created by editor tooling.
- Generated: Partly; `Assets/ScriptableObjects/MahjongTileDatabase.asset` is regenerated by `Assets/Editor/MahjongTileDataGenerator.cs`.
- Committed: Yes; preserve its `.meta` GUID because `Assets/Scenes/SampleScene.unity` references it.

**`.VSCodeCounter/`:**
- Purpose: Historical VS Code Counter output, separate from runtime/project assets.
- Generated: Yes, by an external editor extension.
- Committed: Yes; do not place current source or GSD output in `.VSCodeCounter/`.

**`.planning/codebase/`:**
- Purpose: GSD-generated maps consumed by planning/execution workflows.
- Generated: Yes, by codebase mapping.
- Committed: Repository policy currently treats `.planning/` as workflow metadata; only mapper-owned documents such as `.planning/codebase/ARCHITECTURE.md` and `.planning/codebase/STRUCTURE.md` belong here.

---

*Structure analysis: 2026-08-28*
