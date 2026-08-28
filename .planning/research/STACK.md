# Technology Stack

**Project:** Project Riichi Nya
**Researched:** 2026-08-28
**Scope:** Brownfield Unity portfolio refactor with a 9-day delivery window
**Overall confidence:** HIGH for repository state; MEDIUM for forward recommendations because they have not yet been exercised against the refactored project

## Recommendation

Keep Unity `2022.3.29f1`, C# 9.0, .NET Standard 2.1, the current rendering/UI stack, and every currently resolved package version unchanged. Add only the assembly and automation seams required to make production code referenceable from tests and to turn tests plus a Windows player build into repeatable release gates.

Do not split the project into domain, application, presentation, and infrastructure assemblies during this milestone. The current source has cross-layer types (for example, `Assets/Scripts/AL-1S/_Structs.cs` contains both mahjong and UI data) and editor-only imports in runtime files. A one-shot layered assembly migration would consume the 9-day correctness budget. One broad named runtime assembly is the minimum viable boundary; deeper separation should follow proven source boundaries, not precede them.

## Recommended Stack

### Core Framework

| Technology | Version | Purpose | Why | Confidence |
|------------|---------|---------|-----|------------|
| Unity Editor | `2022.3.29f1` revision `8d510ca76d2b` | Editor, runtime, asset import, tests, and Windows build | Exact version is pinned by `ProjectSettings/ProjectVersion.txt`, installed locally, and required by the project constraint. An engine upgrade would add serialization, package, rendering, and build risk without helping the nine-day goal. | HIGH |
| C# | `9.0` | Gameplay and tooling language | The generated project already targets C# 9.0. Keep the compiler surface stable while correcting rules and extracting testable responsibilities. | HIGH |
| .NET API compatibility | .NET Standard `2.1` | Managed runtime API surface | Already configured by the project. Unity recommends the .NET Standard profile for portability, smaller API surface, and earlier compile-time failures. | HIGH |
| Universal Render Pipeline | `14.0.11` | Existing rendering pipeline | Active project assets already depend on this exact version. Keep it; rendering migration has no correctness or portfolio payoff in this milestone. | HIGH |
| Unity UI / TextMesh Pro | uGUI `1.0.0`; TMP `3.0.6` | Existing menus, HUD, tile and score presentation | These are the implemented presentation stack. Retain the current UI rather than replacing it with UI Toolkit or another framework. | HIGH |
| DOTween / DOTween Pro | Vendored; upstream version not recoverable from repository metadata | Existing UI transitions and feedback | Preserve the checked-in binaries and `Assets/Resources/DOTweenSettings.asset`. Do not reinstall or upgrade an asset whose exact provenance/version is not recorded during a deadline refactor. | MEDIUM |

### Database and Local State

| Technology | Version | Purpose | Why | Confidence |
|------------|---------|---------|-----|------------|
| Newtonsoft.Json for Unity | `3.2.1` | Existing local JSON save data | Already pinned and used by `SettingsManager`; changing serializer risks save compatibility and adds no portfolio value. | HIGH |
| ScriptableObject tile database | Unity `2022.3.29f1` native | Tile-code-to-sprite mapping | Existing editor generator and asset are sufficient. Keep asset generation in `Assets/Editor/`; no database service is needed. | HIGH |
| Unity PlayerPrefs | Unity `2022.3.29f1` native | Existing volume settings | Appropriate for the small local preferences already stored. Do not consolidate persistence merely for architectural symmetry. | HIGH |

No SQL, server, cloud save, analytics backend, or online leaderboard belongs in this milestone.

### Infrastructure

| Technology | Version | Purpose | Why | Confidence |
|------------|---------|---------|-----|------------|
| Unity Package Manager | Bundled with `2022.3.29f1` | Dependency restoration | Treat `Packages/manifest.json` and `Packages/packages-lock.json` as the complete, pinned dependency graph. Do not regenerate or broadly update the lockfile. | HIGH |
| Unity Test Framework | `1.1.33` | EditMode and PlayMode tests | Already installed and officially documented at this exact version. It supplies the required runner without adding a framework. | HIGH |
| NUnit extension | `1.0.6` | Assertions and parameterized/table tests | Already resolved transitively by Unity Test Framework. Use NUnit constraints and `[TestCase]`; do not install another assertion or mocking library. | HIGH |
| Unity Editor build pipeline | Unity `2022.3.29f1` | Reproducible Windows x64 build | `BuildPipeline.BuildPlayer(BuildPlayerOptions)` returns a `BuildReport`; a small Editor entry point can make failure produce a non-zero batch result. | HIGH |
| PowerShell | Windows built-in / locally available | One local verification entry point | A short repository script can run EditMode tests, optional PlayMode tests, and the Windows build with explicit output paths. It avoids CI service setup while leaving a repeatable command and logs. | MEDIUM |
| Git annotated tag and GitHub Release | Git plus GitHub native release feature | Before/after baseline and downloadable portfolio build | The project already chose Git history instead of duplicate source trees. A tagged release can attach the zipped Windows build and release notes without a deployment platform. | HIGH |
| Markdown | CommonMark/GitHub Markdown | README and AI development record | Reviewable beside the code, diffable, and sufficient for a nine-day portfolio. No documentation generator is warranted. | HIGH |

### Supporting Libraries

| Library | Version | Purpose | When to Use |
|---------|---------|---------|-------------|
| `com.unity.feature.2d` | `2.0.0` | Existing 2D package feature set | Keep because the current asset/package graph already resolves it; do not expand its use speculatively. |
| `com.unity.visualscripting` | `1.9.4` | Currently referenced by runtime source imports | Keep the package version during the milestone. Remove unused `using Unity.VisualScripting` directives when encountered, but do not uninstall the package until a later clean-build audit proves it unused. |
| `com.unity.timeline` | `1.7.6` | Existing package graph | Leave pinned. Do not introduce Timeline-based orchestration unless existing assets already require it. |
| IDE integrations | Rider `3.0.28`; Visual Studio `2.0.22`; VS Code `1.2.5` | Developer editor integration | Leave the manifest unchanged; they are not runtime architecture decisions and do not justify cleanup work now. |
| Unity Version Control integration | `2.7.1` | Existing editor integration | Leave pinned even if Git is the actual workflow; package pruning is outside the correctness path. |

## Minimal Assembly Structure to Add

```text
Assets/
├── Scripts/
│   └── RiichiNya.Runtime.asmdef
├── Editor/
│   ├── MahjongTileDataGenerator.cs
│   └── PortfolioBuild.cs
└── Tests/
    ├── EditMode/
    │   └── RiichiNya.EditModeTests.asmdef
    └── PlayMode/
        └── RiichiNya.PlayModeTests.asmdef   # add with the first PlayMode test

Tools/
└── Verify-Portfolio.ps1

TestResults/                                # ignored generated evidence
Builds/Windows/                             # ignored generated player output
```

### `RiichiNya.Runtime.asmdef`

Add one assembly definition at `Assets/Scripts/` so all existing runtime scripts move together from predefined `Assembly-CSharp` into a named assembly that tests can reference. Unity documents that code in an assembly-definition assembly cannot use types from predefined assemblies; therefore adding only a test `.asmdef` while leaving production code in `Assembly-CSharp` is not a workable test boundary.

Use `Auto Referenced: true`, `Any Platform`, and normal Unity engine references. Add only the package assembly references required by compiled source, initially `Unity.TextMeshPro`, `UnityEngine.UI`, and `Unity.VisualScripting.Core`. DOTween and Newtonsoft are precompiled plug-ins and remain auto-referenced unless their existing importer settings say otherwise.

Before adding the runtime `.asmdef`, remove the unused runtime imports of `UnityEditor` from `UiScoreDistanceInfo.cs` and `UnityEditor.Rendering` from `MahjongTileAndBlock.cs`. This is not optional cleanup: editor-only namespaces in runtime code are a player-build boundary violation. Keep actual editor APIs in the existing top-level `Assets/Editor/` folder.

Do not create separate `Domain`, `Application`, `Presentation`, or `Infrastructure` assemblies yet. First move misplaced UI structs out of domain files and remove package leakage as part of the functional refactor; split the runtime assembly only if the resulting dependency direction is acyclic and the split directly protects completed behavior.

### `RiichiNya.EditModeTests.asmdef`

Create an Editor-only test assembly referencing `RiichiNya.Runtime`, `UnityEngine.TestRunner`, and `UnityEditor.TestRunner`, with `nunit.framework.dll` as the test framework reference generated by Unity's Test Runner workflow. Put deterministic tile-wall, shuffle, decomposition, yaku, fu, han, and payment tables here. Exercise public production APIs with real mahjong values; use compact tile strings and fixed seeds already supported by the codebase.

Do not add Moq, NSubstitute, FluentAssertions, AutoFixture, or snapshot tooling. Use real pure-domain objects and one handwritten fake only where the existing `IScoreDistanceService` boundary makes it useful.

### `RiichiNya.PlayModeTests.asmdef`

Create this as a separate, non-Editor-only test assembly only when the first lifecycle test is implemented. Unity Test Framework 1.1.33 requires EditMode and PlayMode tests to live in separate assemblies. Limit PlayMode coverage to risks that genuinely need frames or Unity lifecycle: scene load, start/restart event subscription, a representative UI/game-state transition, and teardown of persistent singletons.

Do not reproduce the whole game as UI automation. Correctness belongs primarily in fast EditMode rule tables; PlayMode tests prove integration seams.

### Editor Assembly

Leave `Assets/Editor/` in predefined `Assembly-CSharp-Editor` for this milestone. It already references auto-referenced named runtime assemblies and contains only a small generator plus the proposed build entry point. A separate editor `.asmdef` adds no meaningful isolation within nine days.

## Build and Test Verification

### Required Local Gate

Use the exact pinned editor executable and run the following sequence from one `Tools/Verify-Portfolio.ps1` wrapper:

1. **EditMode tests:** always run; write NUnit XML and a full Unity log.
2. **PlayMode tests:** run once the PlayMode assembly contains tests; write separate XML/log outputs.
3. **Windows x64 player build:** invoke a static method in `Assets/Editor/PortfolioBuild.cs` with `-executeMethod`. Build the enabled scenes from `EditorBuildSettings` to `Builds/Windows/RiichiNya.exe` using `BuildTarget.StandaloneWindows64`.
4. **Fail hard:** if `BuildReport.summary.result` is not `Succeeded` or `summary.totalErrors` is nonzero, throw `BuildFailedException`. Unity batch mode then returns a failure code; the wrapper must stop immediately on any non-zero process exit.
5. **Manual smoke:** launch the built player and verify menu entry, one single-player draw/discard/win-or-restart path, return to menu, and clean quit. Record this short checklist and the demo video; do not attempt fragile full UI automation.

```powershell
$unity = 'C:\Program Files\Unity\Hub\Editor\2022.3.29f1\Editor\Unity.exe'

& $unity -batchmode -projectPath $PWD -runTests -testPlatform EditMode `
  -testResults 'TestResults/editmode.xml' -logFile 'TestResults/editmode.log'
if ($LASTEXITCODE -ne 0) { throw 'EditMode tests failed.' }

# Add this invocation when PlayMode tests exist.
& $unity -batchmode -projectPath $PWD -runTests -testPlatform PlayMode `
  -testResults 'TestResults/playmode.xml' -logFile 'TestResults/playmode.log'
if ($LASTEXITCODE -ne 0) { throw 'PlayMode tests failed.' }

& $unity -quit -batchmode -buildTarget Win64 -projectPath $PWD `
  -executeMethod PortfolioBuild.BuildWindows -logFile 'TestResults/build.log'
if ($LASTEXITCODE -ne 0) { throw 'Windows build failed.' }
```

Do not pass `-ignorecompilererrors`. Do not treat the presence of an `.exe` as success; the `BuildReport`, process exit code, and build log are the gate. Keep `TestResults/` and `Builds/` ignored, but preserve final XML/log summaries as screenshots or concise Markdown evidence and attach the zipped final build to the release.

### Portfolio Documentation Tooling

Use repository Markdown and generated evidence only:

| Artifact | Tool | Required contents |
|----------|------|-------------------|
| `README.md` | Markdown | Project premise, controls, two-mode scope, architecture snapshot, exact Unity version, how to run tests/build, known limits, demo video link, downloadable release link. |
| AI development record | Markdown | Three to five cases: symptom, root cause, AI suggestion, accepted/rejected reasoning, code change, and test/build evidence. Do not publish full chat transcripts. |
| Before/after evidence | Git tag/diff plus test cases | Reference `portfolio-baseline`; show focused diffs and named regression cases rather than duplicate source folders. |
| Test evidence | Unity Test Framework NUnit XML plus short summarized table | Record command, editor version, pass/fail count, and timestamp. Scenario coverage matters more than a coverage percentage. |
| Build evidence | `BuildReport` summary, batch log, zipped Windows player | Record result, error count, output size, commit/tag, and smoke-test status. |
| Demo | Short captured video | Demonstrate preserved single-player flow, corrected behavior, and the completed four-player slice that actually shipped. |

Do not add DocFX, MkDocs, Docusaurus, a static-site host, diagram-generation packages, or a documentation database. Markdown, one small architecture diagram, Git history, test XML, and the release artifact are enough.

## Alternatives Considered

| Category | Recommended | Alternative | Why Not |
|----------|-------------|-------------|---------|
| Unity version | Keep `2022.3.29f1` | Upgrade to later 2022.3 patch or Unity 6 | Upgrade risk is broad and unrelated to the nine-day correctness goal. Revisit only after the portfolio release on a separate branch with full test/build comparison. |
| Runtime assemblies | One `RiichiNya.Runtime` assembly | Four-layer assembly architecture immediately | Current source boundaries are cyclic/mixed; forcing assembly purity first would create a large refactor before regression tests exist. |
| Tests | Installed Unity Test Framework `1.1.33` + NUnit `1.0.6` | Upgrade UTF or add an external .NET test project | Existing package runs both Unity-aware and pure C# tests inside the exact player compilation environment. External tests would duplicate project configuration. |
| Mocking | Real objects and handwritten fakes | Moq/NSubstitute/AutoFixture | No current interaction complexity justifies new dependencies; rule correctness is data-oriented. |
| Coverage | Named critical-rule matrix and pass evidence | Add `com.unity.testtools.codecoverage` and a percentage gate | The package is absent, a percentage can reward low-value lines, and the timeline needs known failure cases covered first. Add coverage later only if the critical matrix is complete and the metric will be maintained. |
| Build automation | One Editor build method plus PowerShell wrapper | GitHub Actions/GameCI/Jenkins | Cloud setup, licensing, cache, and runner debugging are not needed to prove a local Windows portfolio build in nine days. The batch commands remain CI-ready later. |
| UI | Keep uGUI/TMP/DOTween | UI Toolkit rewrite | Replaces working presentation without improving rule correctness or build evidence. |
| Input | Keep legacy Input Manager | New Input System | Current controls already use `KeyCode`; migration creates retesting work with no milestone value. |
| Documentation | Markdown + GitHub Release | Documentation site and hosted pipeline | Extra ownership and no additional hiring signal for this project scale. |

## Installation

No package installation or upgrade is recommended. Open the project with the pinned editor and let Unity restore the committed manifest/lock graph.

```powershell
& 'C:\Program Files\Unity\Hub\Editor\2022.3.29f1\Editor\Unity.exe' `
  -projectPath $PWD
```

Any change to `Packages/manifest.json`, `Packages/packages-lock.json`, `ProjectSettings/ProjectVersion.txt`, scripting backend, API compatibility level, render pipeline, or input backend should be treated as scope change and require a fresh EditMode + PlayMode + Windows build run.

## Adoption Order

1. Remove editor-only imports from runtime files and make the current player compile boundary clean.
2. Add the single runtime `.asmdef` and the EditMode test assembly; restore compilation with only the package references actually required.
3. Add regression tests for the four confirmed rule failures before restructuring their implementations.
4. Add the batch Windows build method and PowerShell gate; run it continuously while lifecycle code changes.
5. Add the PlayMode assembly with the first restart/event/scene test, not as empty scaffolding.
6. Produce README, case-study Markdown, XML/log summaries, final tagged build, smoke checklist, and demo video from the verified release commit.

## Sources

### Repository Evidence — HIGH confidence

- `ProjectSettings/ProjectVersion.txt` — Unity `2022.3.29f1` and revision `8d510ca76d2b`.
- `Packages/manifest.json` — direct package versions, including Unity Test Framework `1.1.33`, URP `14.0.11`, TMP `3.0.6`, Newtonsoft.Json `3.2.1`, uGUI `1.0.0`, Visual Scripting `1.9.4`, Timeline `1.7.6`, and 2D Feature Set `2.0.0`.
- `Packages/packages-lock.json` — resolved NUnit extension `1.0.6` and the authoritative package graph.
- `.planning/codebase/STACK.md`, `.planning/codebase/ARCHITECTURE.md`, and `.planning/codebase/TESTING.md` — mapped runtime, missing first-party tests/asmdefs, one enabled scene, and editor-only imports in runtime code.

### Official Primary Documentation — MEDIUM confidence from the GSD source classifier, cross-checked against repository state

- [Unity 2022.3 assembly definitions](https://docs.unity3d.com/2022.3/Documentation/Manual/ScriptCompilationAssemblyDefinitionFiles.html) — folder scope, predefined `Assembly-CSharp`, named-assembly references, editor assemblies, and the rule that named assemblies cannot consume predefined-assembly types.
- [Unity Test Framework 1.1.33: create a test assembly](https://docs.unity3d.com/Packages/com.unity.test-framework@1.1/manual/workflow-create-test-assembly.html) — NUnit/TestRunner references, Editor target, and separate EditMode/PlayMode assemblies.
- [Unity Test Framework 1.1.33: command-line tests](https://docs.unity3d.com/Packages/com.unity.test-framework@1.1/manual/reference-command-line.html) — `-runTests`, `-testPlatform`, `-testResults`, assembly filters, and NUnit XML output.
- [Unity 2022.3 .NET overview](https://docs.unity3d.com/2022.3/Documentation/Manual/overview-of-dot-net-in-unity.html) — .NET Standard compatibility rationale.
- [Unity 2022.3 `BuildPipeline.BuildPlayer`](https://docs.unity3d.com/2022.3/Documentation/ScriptReference/BuildPipeline.BuildPlayer.html) — `BuildPlayerOptions`, `BuildReport`, result, size, and error reporting.
- [Unity 2022.3 Editor command-line arguments](https://docs.unity3d.com/2022.3/Documentation/Manual/EditorCommandLineArguments.html) — `-batchmode`, `-executeMethod`, `-buildTarget Win64`, logs, and non-zero failure behavior.
- [GitHub Docs: About releases](https://docs.github.com/en/repositories/releasing-projects-on-github/about-releases) — tag-based releases with notes and attached binary assets.

## Open Verification Items

- DOTween/DOTween Pro's upstream release cannot be established from the repository DLL assembly version. Preserve the vendored files; record this as provenance debt instead of guessing or reinstalling.
- The proposed broad runtime `.asmdef` package-reference list must be validated by Unity compilation after unused imports are removed. Add only compiler-proven references; do not predeclare a layered dependency graph.
- Local batch commands must be executed after implementation. Recommendations are source-grounded, but successful EditMode XML, PlayMode XML, and Windows `BuildReport` are the final proof.
