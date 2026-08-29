# Phase 1: Executable Baseline - Pattern Map

**Mapped:** 2026-08-29
**Files analyzed:** 14 new/modified files
**Analogs found:** 12 / 14

## Scope Guard

Phase 1 should reuse the current single-scene application, `MahjongRound` public events, Unity Test Framework, and the existing panel maps. Do not add a replay system, generic pause manager, alternate rules layer, dependency-injection framework, or Phase 2 scoring fixtures.

`Timer`, `ScoreManagerDistance`, and `MahjongRound` already expose the required reset/seeded-run operations. Treat them as reusable dependencies unless implementation proves a missing reset contract:

- `Timer.StartTimer(float)` resets remaining time, running state, and pause state (`Assets/Scripts/Timer.cs:13-18`).
- `ScoreManagerDistance.Initialize()` resets distance, accumulated score, boost, and game-over state (`Assets/Scripts/ScoreManagerDistance.cs:60-66`).
- `MahjongRound.NewRound(int, out MahjongPlayer)` and `GenerateYama()` provide the test-only seeded entry point (`Assets/Scripts/AL-1S/MahjongRound.cs:389-414,195-243`).

## File Classification

| New/Modified File | Role | Data Flow | Closest Analog | Match Quality |
|---|---|---|---|---|
| `Assets/Editor/Phase1Build.cs` | utility | batch, file-I/O | `Assets/Editor/MahjongTileDataGenerator.cs` | role-match |
| `Assets/Editor/Tests/MahjongRoundTraceTests.cs` | test | event-driven, batch | `Assets/Editor/MahjongTileDataGenerator.cs` for predefined Editor placement; exercises `Assets/Scripts/AL-1S/MahjongRound.cs` | role-match |
| `Assets/Editor/Tests/SoloSessionLifecycleTests.cs` | test | event-driven, batch | same predefined Editor placement and repository NUnit shape | role-match |
| `Assets/Scripts/GameEndReason.cs` | model | event-driven | `Assets/Scripts/AL-1S/_Enums_Ecchi_Nun_Mari.cs` | role-match |
| `Assets/Scripts/MahjongGameManager.cs` | controller | event-driven, request-response | same file's session/event coordinator | exact |
| `Assets/Scripts/UI-Kozeki/PlayerHand.cs` | component | event-driven | same file's input-to-intent path | exact |
| `Assets/Scripts/UI-Kozeki/GameUIManager.cs` | controller | event-driven | same file's enum-to-panel map | exact |
| `Assets/Scripts/UI-Kozeki/UiManager.cs` | controller | event-driven | same file's menu history/start flow | exact |
| `Assets/Scripts/UI-Kozeki/UiGameOver.cs` | component | transform | same file's result presenter | exact |
| `Assets/Scripts/UI-Kozeki/UiScoreDistanceInfo.cs` | component | event-driven | same file plus `MahjongGameManager.OnEnable/OnDisable` | exact |
| `Assets/Scripts/UI-Kozeki/UiRemainingTimeIndicator.cs` | component | event-driven | same file plus `MahjongGameManager.OnEnable/OnDisable` | exact |
| `Assets/Scripts/AL-1S/MahjongTileAndBlock.cs` | model | transform | same file; import cleanup only | exact |
| `Assets/Scenes/SampleScene.unity` | config | event-driven | existing panel and serialized-reference entries in the same scene | exact |
| `.planning/phases/01-executable-baseline/01-BASELINE.md` | config | batch, file-I/O | `.planning/codebase/TESTING.md` | role-match |

Unity will create corresponding `.meta` files for new assets. Preserve and commit them with their assets; do not hand-maintain generated `.csproj` or `.sln` files.

## Pattern Assignments

### `Assets/Editor/Phase1Build.cs` (utility, batch/file-I-O)

**Analog:** `Assets/Editor/MahjongTileDataGenerator.cs` (role match; there is no existing `BuildPipeline` entry point)

**Editor-only import and entry-point pattern** (`Assets/Editor/MahjongTileDataGenerator.cs:1-12`):

```csharp
#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;

public class MahjongTileDataGenerator : EditorWindow
{
    [MenuItem("Tools/MahjongTileDataGenerator")]
    private static void ShowWindow()
```

Use the same `Assets/Editor` boundary and global namespace, but make the Phase 1 build entry point a static method callable with Unity `-executeMethod`. The actual build core comes from Unity's installed `BuildPipeline`, not from a new wrapper framework.

**Fixed project-local path and early-return pattern** (`Assets/Editor/MahjongTileDataGenerator.cs:28-44`):

```csharp
private static void GenerateTileDatabaseWithConfirmation()
{
    string assetPath = "Assets/ScriptableObjects/MahjongTileDatabase.asset";

    bool exists = File.Exists(assetPath);
    if (exists)
    {
        bool overwrite = EditorUtility.DisplayDialog(/* ... */);
        if (!overwrite) return;
    }

    GenerateTileDB(assetPath);
}
```

For the non-interactive build, hard-code `Assets/Scenes/SampleScene.unity` and `Builds/phase1/RiichiNya.exe`, then fail the method when `BuildReport.summary.result` is not `Succeeded`. Do not accept an arbitrary output path from shell text.

---

### `Assets/Editor/Tests/` predefined Editor test boundary (config, batch)

**Analog:** `Assets/Editor/MahjongTileDataGenerator.cs` proves that this legacy project already compiles Editor-only sources into the predefined `Assembly-CSharp-Editor` assembly.

Place both Phase 1 fixtures directly under `Assets/Editor/Tests/`. Phase 1 adds no `.asmdef` or `.asmref`, and it preserves every descriptor path/blob already present at `b18320e`: Unity asmdef assemblies cannot reference the predefined `Assembly-CSharp`, while predefined `Assembly-CSharp-Editor` is compiled after and can use the runtime types in `Assembly-CSharp`. The first test command is a compile/discovery gate, not an assumption: it must produce XML containing each of the three exact trace cases once, otherwise execution halts. The final full-suite gate similarly requires all 3 trace and 13 lifecycle cases. Do not move runtime code into a new domain assembly in Phase 1.

---

### `Assets/Editor/Tests/MahjongRoundTraceTests.cs` (test, event-driven/batch)

**Analog:** No project-authored test exists. Use the repository's documented NUnit shape and the real round API.

**Test naming/assertion pattern** (`.planning/codebase/TESTING.md:43-57`):

```csharp
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

Rename the fixture/method for the round trace and keep it synchronous. Use NUnit constraint assertions; no mocking dependency or `UnityTest` coroutine is needed.

**Seeded construction and public event pattern** (`Assets/Scripts/AL-1S/MahjongRound.cs:156-166,389-414`):

```csharp
public event Action<TsumoInfo> OnTsumoTile = delegate { };
public event Action<MahjongRound> OnNewRoundStart = delegate { };

private MahjongRound(int seed, out MahjongPlayer player)
{
    prng = new System.Random(seed);
    // ...
}

public static MahjongRound NewRound(int seed, out MahjongPlayer player)
{
    return new MahjongRound(seed, out player);
}
```

**Tsumogiri and next-round boundary** (`Assets/Scripts/AL-1S/MahjongRound.cs:248-283,476-489`):

```csharp
public void DiscardTile(int index)
{
    index = Mathf.Clamp(index, 0, 13);
    if (index == 13)
    {
        player.River.AddLast(player.tsumoTile);
    }
    // ...
    Tsumo();
}

void OnRoundEnds(bool playerWon, bool playerTenpai)
{
    MahjongRound newRound = NextRound(playerWon, playerTenpai && player.Seat == Wind.Ton);
    OnNewRoundStart(newRound);
}
```

Run the same bounded trace twice for one fixed seed. Stop when `OnNewRoundStart` supplies the next round, subscribe to that round's first `OnTsumoTile`, then call `GenerateYama()` once. The assertion contract is deterministic transition/state summary only: seed, accepted action count, compact hand/river state, and next-round first draw. Do not assert yaku, han, fu, payment, or the current exact wall-length bug as correct.

On failure, the assertion message must include the first differing action index and expected/actual summaries. On success, emit one short line with seed, action count, state summary, and `PASS`.

---

### `Assets/Scripts/GameEndReason.cs` (model, event-driven)

**Analog:** `Assets/Scripts/AL-1S/_Enums_Ecchi_Nun_Mari.cs:22-29`

```csharp
public enum PlayerCallType
{
    Riichi, Tsumo, Ron, Chii, Pon, Kan, Nukidora, Forfeit
}

public enum GameState
{
    Initializing, PlayerTurn, GameOver, Processing, MOLLU,
}
```

Copy the simple global enum style, but follow the project convention that a new top-level type gets a normally named file. The minimum values are the two finalization cases actually required now (for example `TimeExpired` and `Forfeit`); do not add future disconnect/abort/replay reasons.

---

### Existing `GameState.Processing` plus private pending-forfeit discriminator (model, event-driven)

**Analog:** `Assets/Scripts/AL-1S/_Enums_Ecchi_Nun_Mari.cs:27-29` already defines `GameState.Processing`, and `MahjongGameManager` already uses it for next-round transition, discard, and tsumo processing.

Keep `Processing` as the shared non-input state, preserve `PlayerCallType.Forfeit` as the existing input intent, and add one private manager-owned pending-forfeit flag to disambiguate the confirmation from round transition/discard/tsumo work. A PlayerTurn Esc sets the flag and enters Processing; a second Esc cancels only while the flag is true. Esc received during other Processing work is ignored. Keep timer ownership in `MahjongGameManager`; no enum edit or second UI-owned state machine is needed.

---

### `Assets/Scripts/MahjongGameManager.cs` (controller, event-driven/request-response)

**Analog:** The file's existing central lifecycle and event boundary.

**Serialized composition-root pattern** (`Assets/Scripts/MahjongGameManager.cs:18-38`):

```csharp
[SerializeField] private ScoreManagerDistance scoreManagerDistance;
[SerializeField] private Timer redstoneClock;
[SerializeField] private PlayerHand playerHand;
[SerializeField] private UiScoreDistanceInfo uiScoreDistanceInfo;
[SerializeField] private UiRemainingTimeIndicator uiRemainingTime;
[SerializeField] private UiGameOver uiGameOver;

public GameState currentState = GameState.Initializing;
System.Random prng;
MahjongRound currentRound;
MahjongPlayer player;
```

Keep session ownership here. Wire only the confirmation panel reference/callbacks needed by this coordinator; do not add another manager.

**Production-random start and existing reset calls** (`Assets/Scripts/MahjongGameManager.cs:41-74`):

```csharp
public void StartNewGame()
{
    OnGameStart();
    currentState = GameState.Initializing;
    GameUIManager.Instance.Initialize();

    prng = new System.Random();
    currentRound = MahjongRound.NewRound(prng.Next(), out player);
    AttachRoundEvent();
    currentRound.GenerateYama();

    Construct(scoreManagerDistance);
    svcScoreManager.Initialize();
    uiScoreDistanceInfo.Construct(svcScoreManager);
    redstoneClock.StartTimer(180);
    redstoneClock.OnTimerFinished += HandleGameOver;
    uiRemainingTime?.Construct(redstoneClock);
    currentState = GameState.PlayerTurn;
}
```

Turn this into an idempotent session boundary: if an old round/timer subscription exists, detach it before replacement; then reset score/timer/UI, replace the round, attach once, and expose `PlayerTurn` last. Preserve fresh production randomness; tests call `MahjongRound.NewRound(fixedSeed, ...)` directly.

**Symmetric subscription pattern** (`Assets/Scripts/MahjongGameManager.cs:216-231`):

```csharp
void OnEnable()
{
    if (playerHand != null)
    {
        playerHand.OnPlayerDiscard += PlayerDiscardTile;
        playerHand.OnPlayerCall += CallHandler;
    }
}

void OnDisable()
{
    if (playerHand != null)
    {
        playerHand.OnPlayerDiscard -= PlayerDiscardTile;
        playerHand.OnPlayerCall -= CallHandler;
    }
}
```

Use this same owner/handler pairing for round, timer, and confirmation callbacks.

**State gate and current forfeit integration point** (`Assets/Scripts/MahjongGameManager.cs:260-287`):

```csharp
void CallHandler(PlayerCallType callType)
{
    if (currentState != GameState.PlayerTurn) return;
    switch (callType)
    {
        // ...
        case PlayerCallType.Forfeit:
            HandleGameOver();
            break;
    }
}
```

Replace only the direct `HandleGameOver()` call with entry into the confirmation state. Confirmation, cancellation, second `Esc`, and timer expiry should all route back to this owner. The private pending-forfeit flag determines whether an Esc in Processing cancels a confirmation; Processing without that flag ignores Esc. Finalization must be idempotent; timeout wins if it races confirmation. Pass `GameEndReason` to one finalization method, skip high-score mutation for `Forfeit`, and retain current distance for the result presenter.

---

### `Assets/Scripts/UI-Kozeki/PlayerHand.cs` (component, event-driven)

**Analog:** Its existing input-to-intent event path.

**Input intent pattern** (`Assets/Scripts/UI-Kozeki/PlayerHand.cs:174-185,209-210`):

```csharp
if (Input.GetKeyDown(InputPreset.tsumoAgari))
{
    OnPlayerCall(PlayerCallType.Tsumo);
}
if (Input.GetKeyDown(KeyCode.Escape))
{
    OnPlayerCall(PlayerCallType.Forfeit);
}

public event Action<int> OnPlayerDiscard = delegate { };
public event Action<PlayerCallType> OnPlayerCall = delegate { };
```

Keep `Esc` as `PlayerCallType.Forfeit`; the component must not finalize the game or own confirmation legality. Gate all discard/call processing when the manager is not in `PlayerTurn`, while still allowing the manager's confirmation-cancel path to receive a second `Esc` through one explicit route.

**Lifecycle defect to replace, not copy** (`Assets/Scripts/UI-Kozeki/PlayerHand.cs:23-49`):

```csharp
void Start()
{
    MahjongGameManager.Instance.OnGameOver += HandleGameOver;
    MahjongGameManager.Instance.OnGameOver += HandleGameStart;
}
```

Subscribe `HandleGameStart` to `OnGameStart`, `HandleGameOver` to `OnGameOver`, and unsubscribe both symmetrically in `OnDisable`. Do not add a new input abstraction in this phase.

---

### `Assets/Scripts/UI-Kozeki/GameUIManager.cs` (controller, event-driven)

**Analog:** The existing enum-to-panel map and explicit panel APIs.

**Panel mapping pattern** (`Assets/Scripts/UI-Kozeki/GameUIManager.cs:11-20,49-68`):

```csharp
public enum GameUIState
{
    RoundInfo, Score, PlayerHand, WinInfo, RiichiTsumo, Distance, Time,
    GameOver, BBaggu
}

[SerializeField] List<GamePanelEntry> panels;
Dictionary<GameUIState, GamePanelEntry> panelMap;

void Awake()
{
    panelMap = new Dictionary<GameUIState, GamePanelEntry>();
    foreach (var i in panels)
    {
        i.originalPosition = i.rect.anchoredPosition;
        i.rect.gameObject.SetActive(false);
        panelMap[i.state] = i;
    }
}
```

Add one `ForfeitConfirmation` state to this existing map and serialize one scene panel entry. Do not create another panel registry.

**Explicit activation/deactivation pattern** (`Assets/Scripts/UI-Kozeki/GameUIManager.cs:149-171`):

```csharp
public void ActivePanel(GameUIState state)
{
    if (!panelMap.TryGetValue(state, out GamePanelEntry panel))
        return;
    panel.rect.anchoredPosition = panel.originalPosition;
    panel.group.alpha = 0f;
    panel.rect.gameObject.SetActive(true);
    panel.rect.SlideInAndFade(/* ... */);
}

public void DeactivePanel(GameUIState state)
{
    if (!panelMap.TryGetValue(state, out GamePanelEntry panel))
        return;
    panel.rect.SlideOutAndFade(/* ... */)
        .OnComplete(() => panel.rect.gameObject.SetActive(false));
}
```

Use explicit target state for session initialization. The current `Initialize()` calls `TogglePanel` for `RoundInfo` and `PlayerHand` (`Assets/Scripts/UI-Kozeki/GameUIManager.cs:78-87`); that is not a reset because a second call can invert state. Hide the confirmation/result/transient panels and activate required gameplay panels deterministically.

---

### `Assets/Scripts/UI-Kozeki/UiManager.cs` (controller, event-driven)

**Analog:** Existing menu entry and history path.

**Game start path** (`Assets/Scripts/UI-Kozeki/UiManager.cs:155-161,218-222`):

```csharp
void ToInGameState()
{
    HidePanel();
    currentState = UIState.InGame;
    MahjongGameManager.Instance.StartNewGame();
}

public void OnGameStartButton()
{
    ToInGameState();
}
```

Keep this as the only start button path, including the second start in the same process.

**Back/history path** (`Assets/Scripts/UI-Kozeki/UiManager.cs:207-216`):

```csharp
public void OnBBagguButton()
{
    if (historyStack.Count == 0) return;
    if (currentState == UIState.InGame)
    {
        GameUIManager.Instance.HideAllPanels();
    }
    ShowPanel(historyStack.Pop(), false);
}
```

Adjust the result-screen return path only as needed to meet the locked `MainMenu` destination and clear stale in-game history/panels. Do not add a new navigation controller.

---

### `Assets/Scripts/UI-Kozeki/UiGameOver.cs` (component, transform)

**Analog:** Existing result DTO-to-text presenter.

**Presentation pattern** (`Assets/Scripts/UI-Kozeki/UiGameOver.cs:7-29`):

```csharp
public class UiGameOver : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI uiTotalScore;
    [SerializeField] private TextMeshProUGUI uiRecordScore;

    public void Initialize(float yourScore, float bestScore)
    {
        playerScore = yourScore;
        recordScore = bestScore;
        UpdateUI();
    }
}
```

Extend this presenter with one serialized reason label and a `GameEndReason` parameter. Forfeit displays `포기` plus current distance; timeout retains the normal result. High-score policy stays in `MahjongGameManager`, not in the view.

---

### `Assets/Scripts/UI-Kozeki/UiScoreDistanceInfo.cs` (component, event-driven)

**Analog:** Same component's `Construct`/lifecycle pattern, corrected using the manager's symmetric subscription pattern.

**Current replacement hazard** (`Assets/Scripts/UI-Kozeki/UiScoreDistanceInfo.cs:48-75`):

```csharp
public void Construct(IScoreDistanceService svc)
{
    _scv = svc;
    if (isActiveAndEnabled)
    {
        svc.OnBoostRankAlters += OnBoostRankAlters;
        svc.OnDistanceChange += UpdateDistance;
        Initialize();
    }
}

void OnDisable()
{
    _scv.OnBoostRankAlters -= OnBoostRankAlters;
    _scv.OnDistanceChange -= UpdateDistance;
}
```

Before replacing `_scv`, detach handlers from the old non-null service. Attach once when active; detach only when non-null. Remove the unused runtime `using UnityEditor;` at line 9. No editor code belongs in this component.

---

### `Assets/Scripts/UI-Kozeki/UiRemainingTimeIndicator.cs` (component, event-driven)

**Analog:** Same component's timer subscription, corrected using `MahjongGameManager.OnEnable/OnDisable` (`Assets/Scripts/MahjongGameManager.cs:216-231`).

**Current subscription pattern** (`Assets/Scripts/UI-Kozeki/UiRemainingTimeIndicator.cs:16-28`):

```csharp
public void Construct(Timer timer)
{
    _timer = timer;
    _timer.OnTimerFinished += OnTimerEnds;
    _timer.OnTimeTick += UpdateTimer;
    UpdateTimer(timer.RemainingSeconds);
}

void OnTimerEnds()
{
    _timer.OnTimerFinished -= OnTimerEnds;
    _timer.OnTimeTick -= UpdateTimer;
}
```

Make `Construct` idempotent: detach from the old timer before replacement, attach once, and also detach in `OnDisable`. Do not depend on timer completion for cleanup because forfeit stops the session early and restart reuses the component.

---

### `Assets/Scripts/AL-1S/MahjongTileAndBlock.cs` (model, transform)

**Analog:** Same runtime model file; no behavioral change is needed.

Remove only the unused editor import currently found at `Assets/Scripts/AL-1S/MahjongTileAndBlock.cs:9`:

```csharp
using UnityEditor.Rendering;
```

Do not guard an unused import with `#if UNITY_EDITOR`; delete it. Keep all model behavior unchanged in Phase 1.

---

### `Assets/Scenes/SampleScene.unity` (config, event-driven)

**Analog:** The existing `GameUIManager` panel list and manager reference block in the same scene.

**Panel serialization pattern** (`Assets/Scenes/SampleScene.unity:2148-2189`):

```yaml
panels:
- state: 0
  rect: {fileID: 71275687}
  group: {fileID: 71275690}
  appearFromWhere: 3
  originalPosition: {x: 0, y: 0}
# ...
- state: 7
  rect: {fileID: 1563807080}
  group: {fileID: 1563807081}
  appearFromWhere: 3
  originalPosition: {x: 0, y: 0}
gameCanvas: {fileID: 1225745916}
```

**Composition-root reference pattern** (`Assets/Scenes/SampleScene.unity:8596-8610`):

```yaml
m_Script: {fileID: 11500000, guid: 83be086a716bef149853d38249179bd7, type: 3}
scoreManagerDistance: {fileID: 1754916433}
redstoneClock: {fileID: 1482123944}
playerHand: {fileID: 199402665}
uiRemainingTime: {fileID: 1082300751}
uiGameOver: {fileID: 1563807082}
```

Add one initially inactive confirmation panel under the existing game canvas, with a `CanvasGroup`, message, confirm button, and cancel button. Add it to `GameUIManager.panels`; wire buttons directly to public manager confirm/cancel callbacks. Extend the existing `GameOver` object, already mapped as state 7 and backed by `UiGameOver` (`Assets/Scenes/SampleScene.unity:7906-7976`), with the reason label. No new scene or generic modal system is needed.

---

### `.planning/phases/01-executable-baseline/01-BASELINE.md` (config, batch/file-I-O)

**Analog:** `.planning/codebase/TESTING.md`

**Command/evidence style** (`.planning/codebase/TESTING.md:14-20`):

````markdown
**Run Commands:**
```powershell
& "<path-to-Unity.exe>" -batchmode -projectPath "$PWD" -runTests -testPlatform EditMode -testResults ".\TestResults\editmode.xml" -quit
```
- No checked-in PowerShell, batch, CI, or package script wraps these commands.
````

Keep one short before/after document containing: annotated tag and baseline commit, target commit, Unity version, exact EditMode/build commands, test count/result, seed and accepted-action summary, build result/path, GUI checklist result, and raw local artifact paths. Record licensing or visual-verification failures as failures/blockers before validation; only an explicitly APPROVED checkpoint with GUI PASS may complete its plan, and every other result must exit nonzero until the same path is repaired and re-observed.

Use ignored `Temp/phase1/` for XML/log/BuildReport and `Builds/phase1/` for the Player. `.gitignore:5-10` already covers these roots; do not add another evidence-output directory or commit raw outputs.

## Shared Patterns

### One Session Owner

**Source:** `Assets/Scripts/MahjongGameManager.cs:41-114,123-141,260-287`

**Apply to:** `MahjongGameManager`, `PlayerHand`, `GameUIManager`, `UiManager`, result/confirmation UI.

All session start, state gating, confirmation resolution, finalization reason, high-score policy, round replacement, and timer finalization route through `MahjongGameManager`. UI emits intent and renders state; it does not mutate the round directly.

### Idempotent Event Lifecycle

**Source:** `Assets/Scripts/MahjongGameManager.cs:216-231`

```csharp
void OnEnable()
{
    playerHand.OnPlayerDiscard += PlayerDiscardTile;
    playerHand.OnPlayerCall += CallHandler;
}

void OnDisable()
{
    playerHand.OnPlayerDiscard -= PlayerDiscardTile;
    playerHand.OnPlayerCall -= CallHandler;
}
```

**Apply to:** old/current round handlers, timer completion, `PlayerHand`, `UiScoreDistanceInfo`, and `UiRemainingTimeIndicator`. `Construct` must detach before replacing a dependency and attach no more than once.

### Explicit UI Reset

**Source:** `Assets/Scripts/UI-Kozeki/GameUIManager.cs:95-101,149-171`

```csharp
public void HideAllPanels()
{
    foreach (var i in panels)
    {
        DeactivePanel(i.state);
    }
}
```

At a new-session boundary, set each required panel to its desired state. Do not use `TogglePanel` as initialization. Kill or resolve any active volatile tween before exposing the second session.

### Seed Separation

**Source:** `Assets/Scripts/MahjongGameManager.cs:47-55`; `Assets/Scripts/AL-1S/MahjongRound.cs:389-414`

Production uses `new System.Random()` followed by `MahjongRound.NewRound(prng.Next(), ...)`. Only the EditMode test passes a fixed seed directly. Do not add a seed selector to runtime UI.

### Editor/Player Boundary

**Source:** `Assets/Editor/MahjongTileDataGenerator.cs:1-9`; runtime violations at `Assets/Scripts/UI-Kozeki/UiScoreDistanceInfo.cs:9` and `Assets/Scripts/AL-1S/MahjongTileAndBlock.cs:9`.

Editor APIs and `BuildPipeline` stay below `Assets/Editor`. Runtime files under `Assets/Scripts` must contain no `using UnityEditor...` imports. Verify with:

```powershell
rg -n --glob '*.cs' 'using UnityEditor(\.|;)' Assets/Scripts
```

Success is no matches, followed by an actual `StandaloneWindows64` build.

### Error Handling and Verification Truthfulness

- Runtime handlers use early return on invalid state (`MahjongGameManager.cs:161,262,296`; `GameUIManager.cs:111-112,151-153,163-165`).
- Build automation throws/fails on non-success `BuildReport`; do not log-and-continue.
- The trace has a fixed action cap and reports the first mismatch.
- Licensing IPC failure, missing XML, failed build, or uncertain GUI observation remains a blocker/failure in `01-BASELINE.md`.

## No Analog Found

| File | Role | Data Flow | Reason / Planner Source |
|---|---|---|---|
| `Assets/Editor/Tests/MahjongRoundTraceTests.cs` | test | event-driven, batch | No project-authored test exists; use the predefined Editor placement above, `.planning/codebase/TESTING.md:40-63` for NUnit shape, and `MahjongRound.cs:156-166,195-283,389-489` for the real API. |
| `Assets/Editor/Tests/SoloSessionLifecycleTests.cs` | test | event-driven, batch | No lifecycle-test analog exists; reuse the same predefined Editor boundary and verify exact discovery from XML. |

`Assets/Editor/Phase1Build.cs` has an Editor-role analog but no same-flow build analog. Use the installed Unity `BuildPipeline`/`BuildReport` pattern from `01-RESEARCH.md:309-327`, not custom process or file-copy code.

## Metadata

**Analog search scope:** `Assets/Scripts/`, `Assets/Editor/`, `Assets/Scenes/`, `.planning/codebase/`, project settings/package manifest

**Files in search scope:** 95

**Strong analog families used:** 5 (`MahjongGameManager`, `GameUIManager`/`UiManager`, `MahjongRound`, `MahjongTileDataGenerator`, repository testing guidance)

**Pattern extraction date:** 2026-08-29
