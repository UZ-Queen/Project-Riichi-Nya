# Testing Patterns

**Analysis Date:** 2026-08-28

## Test Framework

**Runner:**
- Unity Test Framework 1.1.33 is installed through `com.unity.test-framework` in `Packages/manifest.json`; NUnit support is supplied transitively by `com.unity.ext.nunit` 1.0.6 in `Packages/packages-lock.json`.
- Config: package versions live in `Packages/manifest.json` and `Packages/packages-lock.json`; `ProjectSettings/ProjectSettings.asset` has `playModeTestRunnerEnabled: 0`, and no repository test settings asset or test `.asmdef` is present.

**Assertion Library:**
- NUnit via Unity Test Framework (`Assert`, constraints, and test attributes); the dependency is present in `Packages/packages-lock.json`, but no first-party test currently imports NUnit under `Assets/`.

**Run Commands:**
```powershell
& "<path-to-Unity.exe>" -batchmode -projectPath "$PWD" -runTests -testPlatform EditMode -testResults ".\TestResults\editmode.xml" -quit
& "<path-to-Unity.exe>" -batchmode -projectPath "$PWD" -runTests -testPlatform PlayMode -testResults ".\TestResults\playmode.xml" -quit
# Coverage command: not available; com.unity.testtools.codecoverage is absent from Packages/manifest.json.
```
- No checked-in PowerShell, batch, CI, or package script wraps these commands; the Unity editor version required by the project is recorded in `ProjectSettings/ProjectVersion.txt`.

## Test File Organization

**Location:**
- Not detected: there is no `Assets/Tests/`, `Tests/`, EditMode directory, PlayMode directory, or first-party test assembly in the repository; production code is under `Assets/Scripts/` and editor tooling under `Assets/Editor/`.
- Add pure domain tests under `Assets/Tests/EditMode/` and Unity lifecycle/scene tests under `Assets/Tests/PlayMode/`; each directory needs a test `.asmdef` because current production code is only in Unity's default `Assembly-CSharp` assembly (`Assembly-CSharp.csproj`).

**Naming:**
- No existing test naming convention is established. Use `{TypeName}Tests.cs`, mirroring source names such as `Assets/Scripts/AL-1S/MahjongUtilities.cs`, `Assets/Scripts/Timer.cs`, and `Assets/Scripts/Configs/SettingsManager.cs`.

**Structure:**
```text
Assets/
└── Tests/
    ├── EditMode/       # Pure mahjong rules, parsing, scoring, and persistence contracts
    └── PlayMode/       # MonoBehaviour lifecycle, events, time, audio, and UI wiring
```
- The structure above is required guidance for the first tests; it is not currently present beside `Assets/Scripts/`.

## Test Structure

**Suite Organization:**
```csharp
// No suite exists yet. Seed EditMode coverage with the public pure API in
// Assets/Scripts/AL-1S/MahjongTileAndBlock.cs.
using NUnit.Framework;

public class MahjongTileTests
{
    [Test]
    public void StringToTile_ValidCode_ReturnsExpectedTile()
    {
        MahjongTile tile = MahjongTile.StringToTile("5m");

        Assert.That(tile.TileID, Is.EqualTo(5));
    }
}
```

**Patterns:**
- Setup pattern: not established. Prefer direct construction for pure value objects from `Assets/Scripts/AL-1S/MahjongTileAndBlock.cs` and `Assets/Scripts/AL-1S/_Structs.cs`; create `GameObject` instances only for `MonoBehaviour` tests such as `Assets/Scripts/Timer.cs`.
- Teardown pattern: not established. PlayMode tests that instantiate objects from `Assets/Scripts/MahjongGameManager.cs`, `Assets/Scripts/UI-Kozeki/GameUIManager.cs`, or `Assets/Scripts/SoundArchive/AudioManager.cs` must destroy them and reset singleton/static state after each test.
- Assertion pattern: not established. Use NUnit constraint assertions and assert observable domain results/events rather than private fields in `Assets/Scripts/AL-1S/MahjongUtilities.cs` and `Assets/Scripts/ScoreManagerDistance.cs`.

## Mocking

**Framework:** Not detected; only Unity Test Framework/NUnit packages appear in `Packages/manifest.json` and `Packages/packages-lock.json`.

**Patterns:**
```csharp
// No mocking pattern exists. A small hand-written fake is sufficient for the
// interface already defined in Assets/Scripts/IScoreDistanceService.cs.
private sealed class FakeScoreDistanceService : IScoreDistanceService
{
    public event System.Action<int> OnBoostRankAlters = delegate { };
    public event System.Action<float> OnDistanceChange = delegate { };
    public int BoostLevel { get; private set; }
    public float Distance { get; private set; }
    public float InterpolatedBoostValue => 0f;
    public float DistanceWithAccumulated => Distance;

    public void Initialize() { }
    public void OnGameOver() { }
    public void GetBoostAndDistance(int score) { }
    public void GetBoost(float amount) { }
    public void GetInstantDistance(float amount) { }
}
```

**What to Mock:**
- Fake interface boundaries already present in production, especially `IScoreDistanceService` consumed by `Assets/Scripts/UI-Kozeki/UiScoreDistanceInfo.cs` and `Assets/Scripts/MahjongGameManager.cs`.
- Isolate Unity global state only when required: `Time`, `Input`, `PlayerPrefs`, `Application.persistentDataPath`, and scene callbacks appear in `Assets/Scripts/Timer.cs`, `Assets/Scripts/DASandARR.cs`, `Assets/Scripts/SoundArchive/AudioManager.cs`, `Assets/Scripts/Configs/SettingsManager.cs`, and `Assets/Scripts/SoundArchive/MusicManager.cs`.

**What NOT to Mock:**
- Do not mock pure tile, hand, yaku, and score calculations in `Assets/Scripts/AL-1S/MahjongTileAndBlock.cs`, `Assets/Scripts/AL-1S/MahjongUtilities.cs`, `Assets/Scripts/AL-1S/MahjongYaku.cs`, or `Assets/Scripts/AL-1S/MahjongWinInfo.cs`; construct their real value objects and assert results.
- Do not add a third-party mocking dependency for the existing single service interface in `Assets/Scripts/IScoreDistanceService.cs`; a minimal fake keeps the test assembly compatible with `Packages/manifest.json`.

## Fixtures and Factories

**Test Data:**
```csharp
List<MahjongTile> hand = MahjongTile.StringToTiles(
    "1m1m1m2m3m4m0m6m7m8m9m9m9m");

MahjongRound round = MahjongRound.NewRound(1557, out MahjongPlayer player);
```
- Compact tile-code strings are the existing de facto data-builder pattern in `Assets/Scripts/UI-Kozeki/PlayerHand.cs`, `Assets/Scripts/AL-1S/MahjongTileAndBlock.cs`, and `Assets/Scripts/AL-1S/MahjongUtilities.cs`.
- Fixed seeds are supported by `MahjongRound.NewRound` in `Assets/Scripts/AL-1S/MahjongRound.cs` and are preferable to the unseeded `System.Random` path in `Assets/Scripts/MahjongGameManager.cs` for repeatable tests.

**Location:**
- No shared fixture directory exists. Keep one-off tile strings beside each test under the proposed `Assets/Tests/EditMode/`; introduce a shared builder only after repetition appears across tests targeting `Assets/Scripts/AL-1S/`.

## Coverage

**Requirements:** None enforced. No coverage package, threshold, report directory, CI test job, or test assembly is configured in `Packages/manifest.json`, `ProjectSettings/`, or `Assets/`.

**View Coverage:**
```powershell
# Not available in the current repository.
# Add com.unity.testtools.codecoverage to Packages/manifest.json before using Unity's Coverage window or CLI coverage options.
```
- Highest-risk untested pure logic is the rule engine in `Assets/Scripts/AL-1S/MahjongUtilities.cs`, `Assets/Scripts/AL-1S/MahjongYaku.cs`, `Assets/Scripts/AL-1S/MahjongWinInfo.cs`, and `Assets/Scripts/AL-1S/MahjongTileAndBlock.cs`.
- Untested stateful paths include round progression/events in `Assets/Scripts/AL-1S/MahjongRound.cs`, score accumulation in `Assets/Scripts/ScoreManagerDistance.cs`, save recovery in `Assets/Scripts/Configs/SettingsManager.cs`, and singleton/UI wiring in `Assets/Scripts/MahjongGameManager.cs` and `Assets/Scripts/UI-Kozeki/`.

## Test Types

**Unit Tests:**
- Not present. Start with EditMode tests for tile parsing/equality, winning-hand decomposition, yaku filtering, fu/han/base-score calculations, and deterministic round construction in `Assets/Scripts/AL-1S/MahjongTileAndBlock.cs`, `Assets/Scripts/AL-1S/MahjongUtilities.cs`, `Assets/Scripts/AL-1S/MahjongYaku.cs`, and `Assets/Scripts/AL-1S/MahjongRound.cs`.
- Cover boundary contracts visible in current code: invalid tile strings return `MahjongTile.NullTile()` in `Assets/Scripts/AL-1S/MahjongTileAndBlock.cs`, non-13/14-tile hands return no wins in `Assets/Scripts/AL-1S/MahjongUtilities.cs`, and boost levels clamp in `Assets/Scripts/ScoreManagerDistance.cs`.

**Integration Tests:**
- Not present. PlayMode coverage is needed for event subscription/unsubscription and component construction across `Assets/Scripts/MahjongGameManager.cs`, `Assets/Scripts/ScoreManagerDistance.cs`, `Assets/Scripts/Timer.cs`, `Assets/Scripts/UI-Kozeki/UiScoreDistanceInfo.cs`, and `Assets/Scripts/UI-Kozeki/UiRemainingTimeIndicator.cs`.
- Persistence integration tests must isolate files created by `Assets/Scripts/Configs/SettingsManager.cs`; current `SaveFilePath` is private static state rooted at `Application.persistentDataPath`, so no isolated test seam currently exists.

**E2E Tests:**
- Not used. There is one configured scene, `Assets/Scenes/SampleScene.unity`, but no automated scene-load, input, UI transition, win, or game-over flow test under `Assets/`.

## Common Patterns

**Async Testing:**
```csharp
// No async test exists. Use UnityTest only for frame/coroutine behavior such as
// Assets/Scripts/SoundArchive/AudioManager.cs and Assets/Scripts/Timer.cs.
[UnityEngine.TestTools.UnityTest]
public System.Collections.IEnumerator Timer_Finishes_RaisesEvent()
{
    // Arrange a Timer component, start it, then yield frames until completion.
    yield return null;
}
```
- Production code has no `async`/`await`; its only coroutine is `FadeAudio` in `Assets/Scripts/SoundArchive/AudioManager.cs`. Pure calculations under `Assets/Scripts/AL-1S/` should remain synchronous EditMode tests.

**Error Testing:**
```csharp
[Test]
public void StringToTile_InvalidCode_ReturnsNullTile()
{
    MahjongTile actual = MahjongTile.StringToTile("bad");

    Assert.That(actual, Is.EqualTo(MahjongTile.NullTile()));
}
```
- No current error tests exist. Assert the documented fallback result and, where useful, Unity logs for invalid parsing in `Assets/Scripts/AL-1S/MahjongTileAndBlock.cs`, missing assets in `Assets/Scripts/MahjongTileDatabase.cs`, and load failures in `Assets/Scripts/Configs/SettingsManager.cs`.

---

*Testing analysis: 2026-08-28*
