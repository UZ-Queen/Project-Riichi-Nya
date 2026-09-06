# Phase 2: Shared Rules Core — 정확한 공유 규칙 코어 - Pattern Map

**Mapped:** 2026-09-04
**Files analyzed:** 15개 신설/수정 후보
**Analogs found:** 15 / 15

## Symbol Status

- **Existing:** `MahjongTile`, `MahjongBlock`, `MahjongRound`, `MahjongPlayer`, `MahjongUtility`, `MahjongWin`, `MahjongHandInfo`, `MahjongWinInfo`, `Yaku`, `YakuInfo`, `MahjongYakuSolver`, `SoloScoringGameManager`, `PlayerHandView`, `MahjongTileGameObject`, `PetitGameSaveData`, `SettingsManager`, `Phase1Build`, `MahjongRoundTraceTests`, `SoloSessionLifecycleTests`.
- **Proposed by research, not present:** `MahjongWall`, `MahjongHandDecomposer`, `Phase2RegressionTests`, `Phase2ConformanceTests`, `02-EVIDENCE.md`, 그리고 정확한 이름이 아직 정해지지 않은 win context/payment result/rules-version 값.
- 제안 이름은 책임 경계를 설명하는 작업명이다. planner는 실제 중복이 없으면 `MahjongWall.cs`와 `MahjongHandDecomposer.cs`만 분리하고, 나머지 값 타입은 기존 `MahjongWinInfo.cs`에 두는 최소 파일 구성을 우선한다.

## File Classification

| New/Modified File | Status | Role | Data Flow | Closest Analog | Match Quality |
|---|---|---|---|---|---|
| `Assets/Scripts/AL-1S/MahjongTileAndBlock.cs` | modify | model/value object | transform | self | exact |
| `Assets/Scripts/AL-1S/MahjongWall.cs` | proposed new | service/utility | batch transform | `MahjongRound.GenerateYama` + `Utilities.ShuffleArray` | flow-match |
| `Assets/Scripts/AL-1S/MahjongHandDecomposer.cs` | proposed new | service/utility | transform | `MahjongUtility.CheckWinnable` | role-match; algorithm replaced |
| `Assets/Scripts/AL-1S/MahjongUtilities.cs` | modify | utility/facade | request-response transform | self | exact |
| `Assets/Scripts/AL-1S/MahjongYaku.cs` | modify | model/service catalog | transform | self | exact |
| `Assets/Scripts/AL-1S/MahjongWinInfo.cs` | modify | model/service result | transform | self | exact |
| `Assets/Scripts/AL-1S/MahjongRound.cs` | modify | model/aggregate adapter | event-driven | self | exact |
| `Assets/Scripts/SoloScoringGameManager.cs` | modify | controller/coordinator | event-driven request-response | self | exact |
| `Assets/Scripts/UI-Kozeki/MahjongTileGameObject.cs` | modify | component/view | event-driven transform | self | exact |
| `Assets/Scripts/UI-Kozeki/PlayerHandView.cs` | modify | component/view | event-driven transform | self | exact |
| `Assets/Scripts/Configs/Settings.cs` | modify | model/DTO | file-I/O serialization | self | exact |
| `Assets/Scripts/Configs/SettingsManager.cs` | modify | service/persistence adapter | file-I/O | self | exact |
| `Assets/Editor/Tests/Phase2RegressionTests.cs` | proposed new | test | request-response/batch | `MahjongRoundTraceTests.cs` | role-match |
| `Assets/Editor/Tests/Phase2ConformanceTests.cs` | proposed new | test | table-driven transform | `MahjongRoundTraceTests.cs` | role-match |
| `.planning/phases/02-shared-rules-core/02-EVIDENCE.md` | proposed new | config/evidence ledger | batch/file-I/O | `01-BASELINE.md` | role-match |

새 `.cs`가 실제로 분리되면 대응 `.meta`도 함께 보존한다. 별도 `.asmdef`, rules profile JSON, package, manager, event bus는 만들지 않는다.

## Pattern Assignments

### `MahjongTileAndBlock.cs` — 값 정체성과 compact fixture 입구

**Analog:** 같은 파일의 기존 value-object와 parser. 필드/직렬화 표면을 유지하면서 하나의 typed equality에 operator/hash/compare를 모은다. 기본 equality와 solver의 34종 count key는 같은 `TileID` 의미이며 원본 적색 속성을 별도로 보존한다.

**Imports and global-namespace pattern** (`Assets/Scripts/AL-1S/MahjongTileAndBlock.cs:3-8`):

```csharp
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
```

**Existing identity fields and current inconsistent entry points** (`:238-247`, `:270-278`, `:315-334`, `:367-370`):

```csharp
public TileType tileType;
public int number;
public int TileID => (int)this.tileType + number;
public bool isAkaDora;
public bool isDora;
public int doraCount;

public static bool operator ==(MahjongTile a, MahjongTile b)
{
    return a.TileID == b.TileID;
}

public override int GetHashCode()
{
    return Utilities.HashCombine(TileID);
}

public int CompareTo(MahjongTile other)
{
    return TileID.CompareTo(other.TileID);
}
```

수정 시 `IEquatable<MahjongTile>`의 `TileID` 비교를 단일 source로 삼는다. `isDora`/`doraCount` 변이는 제거 대상이며, 표시패 해석 결과를 tile identity에 넣지 않는다.

**Fixture parser pattern** (`:405-463`, `:466-482`): invalid 입력은 `NullTile()` sentinel을 반환하고 compact strings는 `StringToTiles`로 만든다. Phase 2 테스트도 이 공개 입구를 재사용하되 evaluator 입구에서 tile count와 sentinel을 다시 검증한다.

---

### `MahjongWall.cs`와 `MahjongRound.cs` — 결정론적 패산 생성과 솔로 adapter

**Analog:** `MahjongRound.NewRound/GenerateYama`의 PRNG 주입과 `MahjongTile.GetAllTiles`; 현재 139장 조립과 shuffle 구현은 복사하지 않는 결함 표면이다.

**Seed ownership pattern** (`Assets/Scripts/AL-1S/MahjongRound.cs:389-413`, `:422-426`):

```csharp
private MahjongRound(int seed, out MahjongPlayer player)
{
    prng = new System.Random(seed);
    Wind playerWind = GetRandomWind();
    MahjongRoundInfo newInfo = MahjongRoundInfo.NewRound(playerWind);
    this.currentRoundInfo = newInfo;
    player = new MahjongPlayer(playerWind);
    player.UpdateWindInfo(newInfo.RoundWind);
    this.player = player;
}

public static MahjongRound NewRound(int seed, out MahjongPlayer player)
{
    return new MahjongRound(seed, out player);
}
```

**Existing defect to replace, not copy** (`:207-212`):

```csharp
var yamatoCannon = MahjongTile.GetAllTiles().SelectMany(tile => Enumerable.Repeat(tile, 3)).ToList();
yamatoCannon.AddRange(MahjongTile.GetAllTiles(true));
yama = new LinkedList<MahjongTile>
    (Utilities.ShuffleArray(yamatoCannon.ToArray(), prng.Next()));
```

`MahjongWall`은 pure static/factory 수준이면 충분하다: 34종×4를 만들고 각 suit의 normal 5 한 장을 red 5로 치환한 뒤 02-01에서 교정하는 기존 `Utilities.ShuffleArray`의 descending Fisher–Yates `Next(i + 1)`를 재사용한다. `MahjongRound.GenerateYama()`는 결과를 받아 기존 배패/event 흐름에 넣는다. 숨겨진 opponent/graveyard/dead-wall 진행을 새로 만들지 않는다.

**Aggregate event boundary to preserve** (`:156-166`):

```csharp
public event Action OnHandUpdate = delegate { };
public event Action<TsumoInfo> OnTsumoTile = delegate { };
public event Action<MahjongRound> OnNewRoundStart = delegate { };
public event Action<MahjongRoundInfo> OnRoundInfoUpdate = delegate { };
public event Action<int> OnPlayerScoreAlters = delegate { };
public event Action<MahjongWinInfo> OnPlayerWin = delegate { };
```

`remainingTileCount`는 현재 쯔모패 제외 수량이며 1→0 마지막 패 취득 후 화료를 허용하고 0의 최종 타패에서 유국한다. 해소된 P2-OPEN-01에 따라 배패 13장은 차감하지 않고 초기 18→첫 쯔모 후 17이며, 조기 종료가 없으면 첫 쯔모 포함 총 18회에서 0이 된다. 새 국도 18→17로 reset한다. 마지막 일반 패의 Haitei는 solo caller가 전달하고 pure core는 wall/counter를 읽지 않는다. 잘못된 쯔모 `-8000`, 국/친 진행은 `MahjongRound`/solo policy에 남기고 pure scorer로 옮기지 않는다.

---

### `MahjongHandDecomposer.cs`와 `MahjongUtilities.cs` — exhaustive core + 기존 facade

**Analog:** `MahjongUtility.CheckWinnable`의 invalid-result 및 public facade 형태. 기존 greedy body search는 이름/호출 표면만 참고하고 알고리즘은 교체한다.

**Guard and empty-result pattern** (`Assets/Scripts/AL-1S/MahjongUtilities.cs:48-56`):

```csharp
static public List<MahjongWin> CheckWinnable(List<MahjongTile> copyHand, MahjongTile agariTile)
{
    List<MahjongWin> wins = new List<MahjongWin>();
    if (agariTile == MahjongTile.NullTile()) return wins;
    if (copyHand.FindAll(x => x == agariTile).Count > 4) return wins;
    copyHand.Sort();
```

**Facade-to-result pipeline** (`:89-98`):

```csharp
static public HashSet<MahjongWinInfo> CheckWinnableHashSet(
    List<MahjongTile> copyHand,
    MahjongTile agariTile,
    WindInfo windInfo)
{
    HashSet<MahjongWinInfo> info = new HashSet<MahjongWinInfo>();
    foreach (var i in CheckWinnable(copyHand, agariTile))
    {
        i.UpdateRoundWindInfo(windInfo);
        info.Add(new MahjongWinInfo(i));
    }
    return info;
}
```

`MahjongHandDecomposer`는 34종 count vector에서 가능한 head를 순서대로 고르고, 남은 최소 kind의 triplet/sequence 양쪽을 재귀 분기한다. chiitoitsu와 kokushi는 `else if`가 아닌 독립 검사다. winning tile이 어느 block을 완성했는지도 후보별로 열거한다. `MahjongUtilities.cs`는 caller migration 동안 기존 공개 facade를 새 decomposer/evaluator로 연결하며, 후보 전체는 test/debug 경계에서만 노출한다.

**Existing anti-pattern, do not copy** (`:72-85`, `:212-286`): 첫 성공 head에서 `break`, sequence-first/triplet-first 두 pass만 수행하는 구조.

---

### `MahjongYaku.cs`와 `MahjongWinInfo.cs` — 고정 catalog, context, payment result

**Analog:** 기존 `YakuInfo` readonly table, `MahjongWinInfo` 계산 pipeline, nested `ScoreTable`. 새 범용 profile interpreter 대신 compile-time fixed data를 사용한다.

**Fixed metadata table pattern** (`Assets/Scripts/AL-1S/MahjongYaku.cs:41-53`, `:80-99`, `:124-136`):

```csharp
public class YakuInfo
{
    public enum Condition { MenzenOnly, DecreaseHanWhenFuro, FuroOK };
    public readonly Yaku yaku;
    public int Han { get; }
    Condition condition { get; }
    public SortedSet<Yaku> lowerYakues;

    public static readonly Dictionary<Yaku, YakuInfo> YakuData =
        new Dictionary<Yaku, YakuInfo>
        {
            { Yaku.Chanta, new YakuInfo(Yaku.Chanta, 2, Condition.DecreaseHanWhenFuro) },
            { Yaku.Ittsu, new YakuInfo(Yaku.Ittsu, 2, Condition.DecreaseHanWhenFuro) },
            { Yaku.Junchan, new YakuInfo(Yaku.Junchan, 3, Condition.DecreaseHanWhenFuro, YakuSet(Yaku.Chanta)) },
        };
}
```

Planner는 catalog row가 evaluator에서 실제 호출되는지 conformance test로 교차 검증한다. 예: enum/metadata에 있는 `Ryuuiisou`는 현재 `GetYakumanYakues` (`:192-204`)에서 호출되지 않는다. `NukiDora`는 4인 profile에서 제외한다.

**Evaluation order to reshape** (`Assets/Scripts/AL-1S/MahjongWinInfo.cs:387-407`):

```csharp
Fu = MahjongUtility.GetFu(winHand, windInfo, ref info.isPinfu);
MahjongYakuSolver.Get1HanYakues(info, yakues);
MahjongYakuSolver.Get2HanYakues(info, yakues);
MahjongYakuSolver.Get3orHigherHanYakues(info, yakues);
MahjongYakuSolver.GetYakumanYakues(info, yakues);
MahjongYakuSolver.GetDora(info, yakues);
MahjongUtility.RemoveLowerYakues(yakues);
Han = MahjongUtility.GetHan(yakues, doraInfo);
int baseScore = MahjongUtility.GetBaseScoreByHanAndFu(Han, Fu);
scoreTable = new ScoreTable(baseScore);
```

새 흐름은 (1) shape/context yaku 및 yakuman, (2) non-dora yaku 없음이면 invalid, (3) dora/aka/riichi-gated ura bonus, (4) fu/base/payment 순이다. yakuman 경로는 normal yaku/dora/fu와 합산하지 않는다.

**Payment rounding primitive to reuse** (`:338-360`):

```csharp
public ScoreTable(int baseScore)
{
    this.baseScore = baseScore;
    oyaRon = RoundUp(baseScore * 6);
    oyaTsumo = RoundUp(baseScore * 2);
    zaRon = RoundUp(baseScore * 4);
    zaTsumoToOya = RoundUp(baseScore * 2);
    zaTsumoToZa = RoundUp(baseScore * 1);
}

static int RoundUp(int score)
{
    return (score + 99) / 100 * 100;
}
```

새 payment result는 각 payer share를 각각 올림하고 `WinnerIncome`과 좌석별 delta를 함께 제공한다. winner income → Han → Fu → 안정적 열거 순서로 후보를 고른다. 현재 `CompareTo`의 Han→Fu 선택 (`:441-495`)은 결과 identity/comparator의 analog이자 교체 대상이다.

Win context/result의 정확한 proposed type 이름은 planner가 정한다. boolean bag (`MahjongHandInfo.isChanKan/isRinshan/isHaitei/isHoutei`, `:188-206`)을 그대로 늘리지 말고 상호 배타 조건을 enum/value로 묶는다.

---

### `SoloScoringGameManager.cs` — 한 번만 전달하는 mode adapter

**Analog:** Phase 1에서 확정한 manager ownership, event subscription symmetry, `IScoreDistanceService` seam.

**Round replacement boundary** (`Assets/Scripts/SoloScoringGameManager.cs:149-165`):

```csharp
void StartNextRound(MahjongRound nextRound)
{
    if (sessionFinalized)
    {
        return;
    }

    currentState = GameState.Processing;
    DetachRoundEvent();
    currentRound = nextRound;
    AttachRoundEvent();
    currentRound.GenerateYama();
    currentState = GameState.PlayerTurn;
}
```

**Single score-distance delivery seam** (`:327-344`):

```csharp
void UpdatePlayerScore(int delta)
{
    soloUIController?.UpdatePlayerScore(player.Score);
    if (delta > 0)
    {
        svcScoreManager?.GetBoostAndDistance(delta);
    }
}

void HandlePlayerWin(MahjongWinInfo info)
{
    soloUIController?.ShowWinInfo(info, player.IsOya);
}
```

교정된 `WinnerIncome`은 이 경로에 정확히 한 번만 넣는다. 거리/부스트 공식이나 새 score service를 만들지 않는다. Phase 1의 `AttachRoundEvent`/`DetachRoundEvent` (`:266-289`)와 `sessionFinalized` early return을 보존한다.

---

### `MahjongTileGameObject.cs`와 `PlayerHandView.cs` — resolver 결과만 표현

**Analog:** 기존 `SetDora(bool)` presentation API.

**View pattern** (`Assets/Scripts/UI-Kozeki/MahjongTileGameObject.cs:26-30`, `:42-46`):

```csharp
public void SetDora(bool isDora)
{
    this.isDora = isDora;
    uiDoraIndicator.gameObject.SetActive(isDora);
}

public void SetTile(MahjongTile tile)
{
    SetTileImage(tile);
    SetDora(tile.isDora || tile.isAkaDora);
}
```

`SetDora(bool)`는 유지하고 caller가 shared indicator resolver의 결과를 전달한다. `tile.isDora`를 다시 읽거나 UI가 indicator cycle을 자체 계산하지 않는다. `PlayerHandView.FillHand`의 기존 반복 갱신 (`Assets/Scripts/UI-Kozeki/PlayerHandView.cs:56-81`)에 resolved flag를 함께 전달하는 최소 signature 변경을 우선한다. 우라 표시패/강조/결과 입력은 유효한 화료 성립 AND 해당 화료자의 리치 상태에서만 허용한다. 비리치 화료·유국·오화료는 raw hidden ura가 있어도 입력에서 제외하고, 새 국·비리치 결과의 stale ura를 제거한다. 공개 자격은 bonus>0과 무관하다.

---

### `Settings.cs`와 `SettingsManager.cs` — high-score-only rules version migration

**Analog:** 기존 serializable DTO와 safe-default load boundary.

**DTO extension point** (`Assets/Scripts/Configs/Settings.cs:55-70`):

```csharp
[Serializable]
public class PetitGameSaveData
{
    public float highScore;
    public SoundSettings sound;
    public InputSettings input;
    public StatisticsData statistics;

    public PetitGameSaveData()
    {
        highScore = 155.7f;
        sound = new SoundSettings();
        input = new InputSettings();
        statistics = new StatisticsData();
    }
}
```

여기에 단일 hardcoded scoring-rules version 필드만 추가한다. 여러 profile history나 version registry는 만들지 않는다.

**Narrow persistence errors and safe defaults** (`Assets/Scripts/Configs/SettingsManager.cs:37-65`):

```csharp
if (!File.Exists(SaveFilePath))
{
    Debug.LogWarning($"[Load] Save file not found at {SaveFilePath}, creating new data.");
    return new PetitGameSaveData();
}

try
{
    string json = File.ReadAllText(SaveFilePath);
    var data = JsonConvert.DeserializeObject<PetitGameSaveData>(json);
    if (data == null)
    {
        return new PetitGameSaveData();
    }
    return data;
}
catch (IOException e) { /* log + fresh DTO */ }
catch (JsonException e) { /* log + fresh DTO */ }
```

version mismatch 후 DTO 전체를 교체하지 않는다. 이전 version/highScore를 `Debug.LogWarning`으로 남기고 `highScore`와 version만 갱신하여 `sound`, `input`, `statistics`를 보존한다. 기존 파일명 `yaml.json`과 `Save/Load` API는 그대로 둔다.

---

### `Phase2RegressionTests.cs`와 `Phase2ConformanceTests.cs` — predefined Editor fixture

**Analog:** `MahjongRoundTraceTests`의 작은 fixture/직접 assertion 구조와 `SoloSessionLifecycleTests`의 실제 integration/save guard.

**Imports and deterministic test pattern** (`Assets/Editor/Tests/MahjongRoundTraceTests.cs:1-25`):

```csharp
using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

public class MahjongRoundTraceTests
{
    private const int Seed = 1557;

    [Test]
    public void SameSeedAndActions_ProduceIdenticalTrace()
    {
        TraceResult expected = RunTrace(Seed, MaxActions);
        TraceResult actual = RunTrace(Seed, MaxActions);

        Assert.That(expected.ReachedNextRound, Is.True, expected.Diagnostic);
        Assert.That(actual.ReachedNextRound, Is.True, actual.Diagnostic);

        int firstMismatch = FindFirstMismatch(expected.Records, actual.Records);
        Assert.That(firstMismatch, Is.EqualTo(-1), BuildMismatchMessage(expected, actual, firstMismatch));
    }
}
```

Regression fixture는 각 소유 plan의 수정 직전 공개 API로 139-wall/last-index/Equals mismatch(02-01), greedy miss(02-02), dora-only(02-03)를 assertion RED로 고정한다. 실제 solo tsumo-as-ron SOL-PAY-01의 RED/GREEN은 호출부 교정 plan 02-05가 함께 소유하고 02-04에 실패 테스트를 남기지 않는다. Conformance fixture는 production scorer로 expected를 만들지 않고 literal rows를 사용하며 각 row에 `CaseId`, compact hand/melds, winning tile, context, expected decomposition/yaku/Han/Fu/payment, `SourceUrl`, `CheckedOn`, `Authority`, `InferenceNote`를 둔다. 별도 JSON fixture/profile parser는 만들지 않는다.

**Phase 1 persistence guard to reuse, not duplicate differently** (`Assets/Editor/Tests/SoloSessionLifecycleTests.cs:16-25`, `:87-104`, `:749-800`): live save를 같은 directory의 durable backup/absent marker로 보호하고 `[SetUp]`과 `[TearDown]`에서 같은 recovery helper를 호출한다. Phase 2 migration test도 이 helper 계약을 그대로 사용하거나 실제 중복이 생길 때만 공용 test helper로 추출한다.

`Regression`/`Conformance` 독립 실행은 fixture 이름 filter로 달성한다. 새 `.asmdef`, category-only global helper 변경, unfiltered suite count는 필요 없다.

---

### `02-EVIDENCE.md` — RED/GREEN 원본 경로 ledger

**Analog:** `.planning/phases/01-executable-baseline/01-BASELINE.md:3-22`.

```markdown
- Baseline commit: <sha>
- Target commit: <sha>
- EditMode command: <exact filtered command>
- EditMode status: PASS
- Test count: <exact unique selected count>
- Raw artifacts: Logs/UnityTestGate/<results.xml>, Logs/UnityTestGate/<unity.log>
- Human checkpoint: <APPROVED or NOT OBSERVED>
```

Phase 2 ledger는 RED와 GREEN 각각 대상 commit, filter/test names, expected/actual gate, exact count, raw XML/log path를 기록한다. 자동 test/build와 Windows Player 사람 관찰은 별도 행으로 유지한다. 원본 XML/log는 `Logs/UnityTestGate`에 두고 기본적으로 commit하지 않는다.

## Shared Patterns

### Domain validation

**Source:** `MahjongUtilities.cs:48-56`, `MahjongTileAndBlock.cs:405-463`

Public evaluator 경계에서 13/14 tile count, sentinel, kind별 최대 4장, winning tile 포함, declared meld shape/count, indicator 유효성, context 상호배제를 검증한다. 기존 domain contract처럼 invalid/non-winning은 empty/explicit invalid result로 반환하며 index 접근 전에 guard한다.

### Determinism without extra infrastructure

**Source:** `MahjongRound.cs:389-413`, `MahjongRoundTraceTests.cs:12-25`

동일한 전달 seed와 입력은 pinned Unity runtime에서 동일 결과를 만든다. custom RNG나 seed serialization을 만들지 않는다. golden sequence는 runtime upgrade를 넘는 영구 파일 형식으로 약속하지 않는다.

### Result identity and ordering

**Source:** `MahjongWinInfo.cs:441-495`

`Equals`, `GetHashCode`, operators/comparison이 같은 observable result fields를 사용하게 한다. 최적 후보 ordering과 value identity는 분리한다. 최적 선택은 `WinnerIncome`, Han, Fu, stable enumeration 순이고 yaku 빈도/canonical rank table은 추가하지 않는다.

### Phase 1 ownership boundary

**Source:** `SoloScoringGameManager.cs:104-165`, `:266-289`, `:327-344`

`SoloScoringGameManager`가 session/round replacement/score forwarding을 소유하고 UI는 결과만 표시한다. `PlayerHandController/View`, `SoloScoringUIController`, `IScoreDistanceService` 경계를 합치거나 새 singleton을 만들지 않는다.

### Evidence is fail-closed

**Source:** `.planning/phases/01-executable-baseline/01-VALIDATION.md:21-35`, `.planning/phases/02-shared-rules-core/02-VALIDATION.md:20-34`

각 fixture를 기존 helper로 따로 실행해 parseable XML, selected count > 0, zero non-passed를 확인한다. assertion-failure XML만 RED이며 missing XML/license/IPC/compile failure는 BLOCKED/FAIL이다. Phase 1의 trace 4 + lifecycle 15 계약은 변경하지 않는다.

## No Analog Found

없음. 다만 `MahjongWall`, exhaustive decomposer, validated win context/payment result의 **정확한 구현**은 기존 코드에 없으므로 기존 파일에서 복사할 것은 API/guard/ownership 형태뿐이다. 알고리즘은 `02-RESEARCH.md`의 Fisher–Yates, 34-kind recursion, payment 규칙을 따른다.

## Planner Guardrails

- `MahjongWall.cs`/`MahjongHandDecomposer.cs`는 책임이 실제로 분리될 때만 신설한다. context/payment/rules-version마다 별도 파일·interface를 만들지 않는다.
- `MahjongTile ==`/Equals/hash/CompareTo는 TileID 종류 기준이다. 선택한 패는 인덱스로 제거하고 원본 적색을 보존한다. 패산 시드 재현은 `(TileID, isAkaDora)`를 명시적으로 비교하며 red/normal 위치 교환도 검출한다.
- dora는 tile mutation이 아닌 indicator query이며 non-dora yaku 확인 뒤 bonus Han으로 합산한다.
- Phase 2에는 실제 four-seat table, opponent hand/river, call/kan action state, riichi declaration/difficulty UI를 넣지 않는다.
- compatibility facade가 필요해도 caller migration 한 wave 안에서만 사용하고 Phase 종료 전에 제거한다. 구형 scorer 복사본은 제품에 남기지 않는다.
- UI 자동화 PASS와 사람 Player 관찰 PASS를 합치지 않는다.

## Metadata

**Analog search scope:** `Assets/Scripts/AL-1S`, `Assets/Scripts`, `Assets/Scripts/UI-Kozeki`, `Assets/Scripts/Configs`, `Assets/Editor/Tests`, Phase 1 evidence/validation
**Primary analog files:** 8 (`MahjongTileAndBlock.cs`, `MahjongRound.cs`, `MahjongUtilities.cs`, `MahjongYaku.cs`, `MahjongWinInfo.cs`, `SoloScoringGameManager.cs`, `MahjongRoundTraceTests.cs`, `SoloSessionLifecycleTests.cs`)
**Pattern extraction date:** 2026-09-04

## 후속 정정 — 2026-09-06

P2-CR-01~04에 맞춘 해당 권고만 수정했다. 기존 untracked 파일의 analog/source snippet과 다른 내용을 보존한다. 실제 코드 구현·Unity 검증을 수행한 기록은 아니다.
