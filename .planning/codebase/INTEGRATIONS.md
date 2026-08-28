# External Integrations

**Analysis Date:** 2026-08-28

## APIs & External Services

**Runtime services:**
- No active external API or SaaS integration is detected: `Assets/Scripts/` and `Assets/Editor/` contain no `UnityWebRequest`, `HttpClient`, socket client, service SDK, remote URL, webhook, or platform-authentication calls.
  - SDK/Client: Not applicable; installed networking modules in `Packages/manifest.json` are Unity engine capabilities, not evidence of an application integration.
  - Auth: Not applicable; no environment-variable or credential configuration is present at the repository root.

**Unity services:**
- Unity Analytics, Ads, Purchasing, Crash Reporting, Performance Reporting, and the general Unity Connect switch are all disabled in `ProjectSettings/UnityConnectSettings.asset`.
  - SDK/Client: Built-in Unity modules declared in `Packages/manifest.json`; no Unity Gaming Services initialization exists under `Assets/Scripts/`.
  - Auth: Not configured; `ProjectSettings/ProjectSettings.asset` has empty `cloudProjectId`, `organizationId`, and `projectName` fields and `cloudEnabled: 0`.

**Development package service:**
- Unity Package Registry is the sole configured package source in `ProjectSettings/PackageManagerSettings.asset`; it restores packages declared by `Packages/manifest.json`.
  - SDK/Client: Unity Package Manager bundled with Unity 2022.3.29f1, pinned by `ProjectSettings/ProjectVersion.txt`.
  - Auth: No project-scoped authentication configuration is present in `ProjectSettings/PackageManagerSettings.asset`.

## Data Storage

**Databases:**
- No external or embedded database is detected; `Assets/Scripts/DBManager.cs` is an empty MonoBehaviour stub and does not establish a connection.
  - Connection: Not applicable; no connection string or database environment variable is referenced under `Assets/Scripts/`.
  - Client: Not applicable; no ORM, SQL client, SQLite binary, or database package appears in `Packages/manifest.json` or `Assets/Plugins/`.
- Mahjong tile lookup data is a local Unity ScriptableObject, not a database service.
  - Connection: Asset reference to `Assets/ScriptableObjects/MahjongTileDatabase.asset`.
  - Client: `Assets/Scripts/MahjongTileDatabase.cs`, with editor-time generation via `Assets/Editor/MahjongTileDataGenerator.cs`.

**File Storage:**
- Local filesystem only for game save data: `Assets/Scripts/Configs/SettingsManager.cs` reads and writes `Application.persistentDataPath/yaml.json` using `System.IO.File` and Newtonsoft.Json.
- Unity PlayerPrefs stores master, music, and SFX volume settings in `Assets/Scripts/SoundArchive/AudioManager.cs`; these values are separate from the JSON save file.
- Static assets are bundled with the player under `Assets/`, including tile data in `Assets/ScriptableObjects/MahjongTileDatabase.asset`, Korean localization data in `Assets/Localizations/ko.json`, and sprites under `Assets/Sprites/`; `Assets/Resources/` currently contains only DOTween configuration.

**Caching:**
- No application-level cache is implemented in `Assets/Scripts/`; Unity's generated `Library/` package/import cache is development-only and excluded by `.gitignore`.

## Authentication & Identity

**Auth Provider:**
- None detected; there are no login flows, user tokens, identity SDKs, account models, or authenticated network clients under `Assets/Scripts/`.
  - Implementation: Not applicable; all observed game state is local through `Assets/Scripts/Configs/SettingsManager.cs` and `Assets/Scripts/SoundArchive/AudioManager.cs`.

## Monitoring & Observability

**Error Tracking:**
- No external error-tracking service is active; Unity Crash Reporting and Performance Reporting are disabled in `ProjectSettings/UnityConnectSettings.asset`.

**Logs:**
- Runtime diagnostics use Unity's local `Debug.Log`, `Debug.LogWarning`, and `Debug.LogError`, including direct calls in `Assets/Scripts/Configs/SettingsManager.cs` and `Assets/Scripts/SoundArchive/MusicManager.cs`.
- `Assets/Scripts/AL-1S/MyLogger.cs` wraps Unity logging with timestamps, but the file immediately undefines its `HIMARI` compilation symbol, so its wrapped log bodies compile out in the current source configuration.
- No remote log transport, telemetry exporter, or structured log sink is referenced under `Assets/Scripts/`.

## CI/CD & Deployment

**Hosting:**
- No hosting provider or distribution platform is configured; the project is a Unity client application with Standalone Windows settings in `ProjectSettings/BurstAotSettings_StandaloneWindows.json` and `ProjectSettings/ProjectSettings.asset`.
- No committed player build exists; `Build/`, `Builds/`, APK, AAB, `.app`, and `.unitypackage` outputs are excluded by `.gitignore`.

**CI Pipeline:**
- None detected; there is no `.github/workflows/`, `.gitlab-ci.yml`, `azure-pipelines.yml`, `Jenkinsfile`, Dockerfile, or project-authored Unity `BuildPipeline` script in the repository.
- Builds must currently be initiated from the Unity Editor using the enabled scene in `ProjectSettings/EditorBuildSettings.asset`.

## Environment Configuration

**Required env vars:**
- None detected; `Assets/Scripts/`, `Packages/manifest.json`, and project configuration under `ProjectSettings/` do not reference environment variables.
- Runtime save locations are derived from Unity's `Application.persistentDataPath` in `Assets/Scripts/Configs/SettingsManager.cs`, not from an environment variable.

**Secrets location:**
- Not applicable; no `.env` files or secret/credential configuration files are present at the repository root, and no secrets are referenced in `Assets/Scripts/`.
- Platform certificate/password fields in `ProjectSettings/ProjectSettings.asset` are empty; no signing material is committed.

## Webhooks & Callbacks

**Incoming:**
- None detected; the application exposes no server endpoint, custom URI protocol handler, deep-link callback, or webhook receiver under `Assets/Scripts/` or `ProjectSettings/ProjectSettings.asset`.

**Outgoing:**
- None detected; no HTTP requests, socket connections, browser launches, webhook posts, or remote event submissions are implemented under `Assets/Scripts/`.
- Unity services would be the only configured external event endpoints, but their switches are disabled in `ProjectSettings/UnityConnectSettings.asset`.

---

*Integration audit: 2026-08-28*
