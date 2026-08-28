# Technology Stack

**Analysis Date:** 2026-08-28

## Languages

**Primary:**
- C# 9.0 - Gameplay, UI, persistence, audio, domain logic, and editor tooling live under `Assets/Scripts/` and `Assets/Editor/`; Unity's generated `Assembly-CSharp.csproj` sets `<LangVersion>9.0</LangVersion>`.

**Secondary:**
- Unity YAML - Scenes, prefabs, materials, render-pipeline assets, and project configuration are serialized in `Assets/**/*.asset`, `Assets/**/*.prefab`, `Assets/Scenes/SampleScene.unity`, and `ProjectSettings/*.asset`.
- JSON - Unity Package Manager manifests use `Packages/manifest.json` and `Packages/packages-lock.json`; Korean localization data is stored in `Assets/Localizations/ko.json`; runtime save data is serialized as JSON by `Assets/Scripts/Configs/SettingsManager.cs`.
- ShaderLab/Cg - Text rendering shaders and include files are vendored in `Assets/TextMesh Pro/Shaders/`; project shader graphs are stored in `Assets/Materials/ooh Shiny.shadergraph` and `Assets/Materials/ooh Shiny 2.shadergraph`.

## Runtime

**Environment:**
- Unity Editor 2022.3.29f1 LTS, revision `8d510ca76d2b`, pinned by `ProjectSettings/ProjectVersion.txt`.
- Unity managed runtime targeting .NET Standard 2.1; `ProjectSettings/ProjectSettings.asset` sets `apiCompatibilityLevel: 6`, and the generated `Assembly-CSharp.csproj` defines `NET_STANDARD_2_1`.
- Mono is the observed Standalone editor/player backend: `ProjectSettings/ProjectSettings.asset` has no explicit `scriptingBackend` override, while generated `Assembly-CSharp.csproj` defines `ENABLE_MONO` and `PLATFORM_STANDALONE_WIN`.

**Package Manager:**
- Unity Package Manager bundled with Unity 2022.3.29f1 - Registry and built-in package dependencies are declared in `Packages/manifest.json`.
- Lockfile: present at `Packages/packages-lock.json`; use it as the authoritative resolved-version source.

## Frameworks

**Core:**
- Unity 2022.3.29f1 - Component lifecycle, scene management, asset serialization, audio, input, and 2D gameplay runtime; version is pinned in `ProjectSettings/ProjectVersion.txt`.
- Universal Render Pipeline 14.0.11 - Active scriptable render pipeline configured by `ProjectSettings/GraphicsSettings.asset`, with quality-specific assets in `Assets/Settings/URP-Performant.asset`, `Assets/Settings/URP-Balanced.asset`, and `Assets/Settings/URP-HighFidelity.asset`.
- Unity UI (`com.unity.ugui`) 1.0.0 - Canvas-based game UI used by scripts such as `Assets/Scripts/UI-Kozeki/UiManager.cs` and `Assets/Scripts/UI-Kozeki/UiGameOver.cs`.
- TextMesh Pro 3.0.6 - Runtime text rendering used throughout `Assets/Scripts/UI-Kozeki/` and declared in `Packages/manifest.json`.
- Unity Visual Scripting 1.9.4 - Runtime extension methods/types imported by game and UI scripts such as `Assets/Scripts/AL-1S/MahjongRound.cs` and `Assets/Scripts/UI-Kozeki/GameUIManager.cs`.
- Unity 2D Feature Set 2.0.0 - Resolves Sprite, Tilemap, PSD Importer, SpriteShape, Aseprite, and 2D Animation packages through `Packages/packages-lock.json`; tilemap types are used in `Assets/Scripts/AL-1S/_Structs.cs`.

**Testing:**
- Unity Test Framework 1.1.33 with NUnit extension 1.0.6 - Installed through `Packages/manifest.json` and `Packages/packages-lock.json`; no project-authored test files or test assemblies are present under `Assets/`.

**Build/Dev:**
- Unity Editor build pipeline - The only enabled player scene is `Assets/Scenes/SampleScene.unity`, configured in `ProjectSettings/EditorBuildSettings.asset`; no project-authored `BuildPipeline` automation is present.
- Burst 1.8.13 - Resolved transitively by URP and Unity 2D packages in `Packages/packages-lock.json`; Standalone Windows AOT settings are stored in `ProjectSettings/BurstAotSettings_StandaloneWindows.json`.
- Rider Editor 3.0.28, Visual Studio Editor 2.0.22, and VS Code Editor 1.2.5 - IDE integrations declared in `Packages/manifest.json`.
- Unity Version Control integration 2.7.1 (`com.unity.collab-proxy`) - Editor package declared in `Packages/manifest.json`; version-control serialization is set to visible meta files in `ProjectSettings/VersionControlSettings.asset`.

## Key Dependencies

**Critical:**
- DOTween / DOTween Pro - Vendored animation/tween binaries and Unity modules in `Assets/Plugins/Demigiant/`; runtime UI animation calls use `DG.Tweening` in `Assets/Scripts/UI-Kozeki/GameUIManager.cs`, `Assets/Scripts/UI-Kozeki/UiManager.cs`, `Assets/Scripts/UI-Kozeki/UiTransition.cs`, `Assets/Scripts/UI-Kozeki/UiHoverShift.cs`, and `Assets/Scripts/UI-Kozeki/UiScoreDistanceInfo.cs`. The exact upstream release is not recorded; DLL metadata reports only assembly version 1.0.0.0.
- Newtonsoft.Json for Unity 3.2.1 - Serializes and deserializes `PetitGameSaveData` in `Assets/Scripts/Configs/SettingsManager.cs`; package version is declared in `Packages/manifest.json`.
- TextMesh Pro 3.0.6 - Provides `TextMeshProUGUI` UI text types used across `Assets/Scripts/UI-Kozeki/`; package version is declared in `Packages/manifest.json`.
- Universal RP 14.0.11 - Required by the active render pipeline reference in `ProjectSettings/GraphicsSettings.asset` and the assets under `Assets/Settings/`.

**Infrastructure:**
- Unity Package Registry (`https://packages.unity.com`) - Default development dependency source configured in `ProjectSettings/PackageManagerSettings.asset`; no additional scoped registries are configured.
- Local Unity asset database - Runtime tile definitions are stored as the ScriptableObject `Assets/ScriptableObjects/MahjongTileDatabase.asset`, generated by `Assets/Editor/MahjongTileDataGenerator.cs`.
- Unity PlayerPrefs - Stores master, music, and sound-effect volume values in `Assets/Scripts/SoundArchive/AudioManager.cs`.

## Configuration

**Environment:**
- No `.env` or environment-variable configuration files are present at the repository root; runtime configuration is represented by serialized project assets under `ProjectSettings/` and `Assets/`.
- Player identity and defaults are stored in `ProjectSettings/ProjectSettings.asset`: product `Mahjong`, company `Game_Development_Department`, bundle version `1.0.0b`, 1280x720 default window, linear color space, incremental GC, deterministic compilation, and legacy input handling.
- Runtime settings/save data are written to `Application.persistentDataPath/yaml.json` by `Assets/Scripts/Configs/SettingsManager.cs`; despite its filename, its contents are JSON.
- Audio volumes are stored separately through `PlayerPrefs` keys in `Assets/Scripts/SoundArchive/AudioManager.cs`.

**Build:**
- Unity editor version: `ProjectSettings/ProjectVersion.txt`.
- Package graph: `Packages/manifest.json` and `Packages/packages-lock.json`.
- Player/build settings: `ProjectSettings/ProjectSettings.asset` and `ProjectSettings/EditorBuildSettings.asset`.
- Render settings: `ProjectSettings/GraphicsSettings.asset`, `ProjectSettings/QualitySettings.asset`, `ProjectSettings/URPProjectSettings.asset`, and `Assets/Settings/URP-*.asset`.
- Tween behavior and enabled modules: `Assets/Resources/DOTweenSettings.asset`; Safe Mode is enabled, along with audio, physics, physics2D, sprite, UI, and TextMesh Pro modules.
- Text serialization and visible `.meta` files are configured by `ProjectSettings/EditorSettings.asset` and `ProjectSettings/VersionControlSettings.asset`.
- Generated `.sln` and `.csproj` files at the repository root are ignored build artifacts per `.gitignore`; do not treat them as hand-maintained configuration.

## Platform Requirements

**Development:**
- Use Unity Hub/Editor 2022.3.29f1 on Windows to match `ProjectSettings/ProjectVersion.txt` and the Standalone Windows definitions generated in `Assembly-CSharp.csproj`.
- Restore packages from `Packages/manifest.json` using the resolved graph in `Packages/packages-lock.json`; the default registry is configured in `ProjectSettings/PackageManagerSettings.asset`.
- Keep asset metadata visible and serialized as text, matching `ProjectSettings/VersionControlSettings.asset` and `ProjectSettings/EditorSettings.asset`.
- Use the legacy Unity Input Manager: `ProjectSettings/ProjectSettings.asset` sets `activeInputHandler: 0`, and key bindings use `KeyCode` in `Assets/Scripts/Configs/Settings.cs` and `Assets/Scripts/InputManager.cs`.

**Production:**
- The observed development/player target is 64-bit Standalone Windows, evidenced by `ProjectSettings/BurstAotSettings_StandaloneWindows.json` and generated `Assembly-CSharp.csproj`; no committed build output or release packaging exists because `.gitignore` excludes `Build/` and `Builds/`.
- The player uses URP and a default 1280x720 window from `ProjectSettings/ProjectSettings.asset`; quality tiers select the corresponding render-pipeline assets through `ProjectSettings/QualitySettings.asset`.
- Android and iPhone application identifiers remain in `ProjectSettings/ProjectSettings.asset`, but no mobile-specific source integration, signing configuration, or committed deployment pipeline is present.

---

*Stack analysis: 2026-08-28*
