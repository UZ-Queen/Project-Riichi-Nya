using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

/// <summary>
/// 공유 패산과 패 정체성의 기존 결함을 실제 공개 경로에서 재현합니다.
/// </summary>
public class Phase2RegressionTests
{
    private readonly List<GameObject> createdObjects = new List<GameObject>();

    [TearDown]
    public void TearDown()
    {
        foreach (GameObject createdObject in createdObjects)
        {
            if (createdObject != null)
            {
                UnityEngine.Object.DestroyImmediate(createdObject);
            }
        }

        SetStaticProperty(typeof(SoloScoringGameManager), "Instance", null);
    }

    [Test]
    public void SoloStart_CreatesExactly136TileWall()
    {
        SoloScoringGameManager manager = CreateManager();

        manager.StartNewGame();

        MahjongRound round = GetFieldValue<MahjongRound>(manager, "currentRound");
        List<MahjongTile> originalWall = GetFieldValue<List<MahjongTile>>(round, "originalYama");
        Assert.That(originalWall, Has.Count.EqualTo(136));
        Assert.That(originalWall.All(tile => tile != MahjongTile.NullTile()), Is.True);
        Assert.That(originalWall.GroupBy(tile => tile.TileID).Count(), Is.EqualTo(34));
        Assert.That(originalWall.GroupBy(tile => tile.TileID).All(group => group.Count() == 4), Is.True);
        Assert.That(originalWall.Count(tile => tile.isAkaDora), Is.EqualTo(3));
        Assert.That(originalWall.Count(tile => tile.TileID == 5 && !tile.isAkaDora), Is.EqualTo(3));
        Assert.That(originalWall.Count(tile => tile.TileID == 15 && !tile.isAkaDora), Is.EqualTo(3));
        Assert.That(originalWall.Count(tile => tile.TileID == 25 && !tile.isAkaDora), Is.EqualTo(3));

        MahjongTile[] first = MahjongWall.CreateShuffled(new System.Random(1557));
        MahjongTile[] second = MahjongWall.CreateShuffled(new System.Random(1557));
        Assert.That(ToPhysicalSequence(second), Is.EqualTo(ToPhysicalSequence(first)));

        MahjongTile[] redPositionChanged = (MahjongTile[])first.Clone();
        int redIndex = Array.FindIndex(redPositionChanged, tile => tile.TileID == 5 && tile.isAkaDora);
        int normalIndex = Array.FindIndex(redPositionChanged, tile => tile.TileID == 5 && !tile.isAkaDora);
        MahjongTile swap = redPositionChanged[redIndex];
        redPositionChanged[redIndex] = redPositionChanged[normalIndex];
        redPositionChanged[normalIndex] = swap;
        Assert.That(ToPhysicalSequence(redPositionChanged), Is.Not.EqualTo(ToPhysicalSequence(first)));
    }

    [Test]
    public void ShuffleArray_IncludesLastIndexInSelectionRange()
    {
        bool selectedLastIndex = Enumerable.Range(0, 256)
            .Any(seed => Utilities.ShuffleArray(new[] { 0, 1 }, seed)[0] == 1);

        Assert.That(selectedLastIndex, Is.True);
        Assert.That(Utilities.ShuffleArray(Array.Empty<int>(), 0), Is.Empty);

        int[] permutation = Enumerable.Range(0, 136).ToArray();
        Utilities.ShuffleArray(permutation, 1557);
        Assert.That(permutation.OrderBy(value => value), Is.EqualTo(Enumerable.Range(0, 136)));
    }

    [Test]
    public void TileIdentity_UsesTileIdAcrossAllEqualityMembers()
    {
        MahjongTile normalFive = MahjongTile.TileIDToTile(5);
        MahjongTile redFive = MahjongTile.TileIDToTile(5, true);

        Assert.That(normalFive == redFive, Is.True);
        Assert.That(normalFive != redFive, Is.False);
        Assert.That(normalFive.Equals(redFive), Is.True);
        Assert.That(((object)normalFive).Equals(redFive), Is.True);
        Assert.That(normalFive.GetHashCode(), Is.EqualTo(redFive.GetHashCode()));
        Assert.That(normalFive.CompareTo(redFive), Is.Zero);
        Assert.That(new HashSet<MahjongTile> { normalFive, redFive }, Has.Count.EqualTo(1));

        MahjongTile otherTile = MahjongTile.TileIDToTile(15);
        Assert.That(normalFive.Equals(otherTile), Is.False);
        Assert.That(normalFive.CompareTo(otherTile), Is.Not.Zero);

        MahjongTile defaultTile = default;
        Assert.That(defaultTile == default(MahjongTile), Is.True);
        Assert.That(defaultTile.Equals(default(MahjongTile)), Is.True);
        Assert.That(defaultTile.GetHashCode(), Is.EqualTo(default(MahjongTile).GetHashCode()));
        Assert.That(defaultTile.CompareTo(default(MahjongTile)), Is.Zero);
    }

    [Test]
    public void IndexedDiscard_PreservesTheUnselectedFive()
    {
        MahjongRound round = MahjongRound.NewRound(1557, out MahjongPlayer player);
        MahjongTile normalFive = MahjongTile.TileIDToTile(5);
        MahjongTile redFive = MahjongTile.TileIDToTile(5, true);
        round.GenerateYama();
        player.SetPlayerHand(new List<MahjongTile> { redFive, normalFive });
        player.tsumoTile = MahjongTile.StringToTile("1z");

        round.DiscardTile(1);

        Assert.That(player.River.Last.Value.isAkaDora, Is.False);
        Assert.That(player.Hand.Exists(tile => tile.TileID == 5 && tile.isAkaDora), Is.True);

        player.SetPlayerHand(new List<MahjongTile> { redFive, normalFive });
        player.tsumoTile = MahjongTile.StringToTile("1z");
        round.DiscardTile(0);

        Assert.That(player.River.Last.Value.isAkaDora, Is.True);
        Assert.That(player.Hand.Exists(tile => tile.TileID == 5 && !tile.isAkaDora), Is.True);
    }

    private static string[] ToPhysicalSequence(IEnumerable<MahjongTile> tiles)
    {
        return tiles.Select(tile => $"{tile.TileID}:{tile.isAkaDora}").ToArray();
    }

    private SoloScoringGameManager CreateManager()
    {
        GameObject managerObject = new GameObject("Phase2RegressionManager");
        createdObjects.Add(managerObject);
        managerObject.SetActive(false);

        SoloScoringGameManager manager = managerObject.AddComponent<SoloScoringGameManager>();
        ScoreManagerDistance score = managerObject.AddComponent<ScoreManagerDistance>();
        Timer timer = managerObject.AddComponent<Timer>();
        SetField(manager, "scoreManagerDistance", score);
        SetField(manager, "redstoneClock", timer);
        manager.Construct(score);
        managerObject.SetActive(true);
        return manager;
    }

    private static FieldInfo GetField(Type type, string fieldName)
    {
        return type.GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
    }

    private static T GetFieldValue<T>(object target, string fieldName)
    {
        return (T)GetField(target.GetType(), fieldName).GetValue(target);
    }

    private static void SetField(object target, string fieldName, object value)
    {
        GetField(target.GetType(), fieldName).SetValue(target, value);
    }

    private static void SetStaticProperty(Type type, string propertyName, object value)
    {
        type.GetProperty(propertyName, BindingFlags.Static | BindingFlags.Public).SetValue(null, value);
    }
}
