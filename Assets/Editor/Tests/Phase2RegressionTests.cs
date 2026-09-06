using System;
using System.Collections.Generic;
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
    }

    [Test]
    public void ShuffleArray_IncludesLastIndexInSelectionRange()
    {
        int[] values = { 0, 1 };

        Utilities.ShuffleArray(values, 0);

        Assert.That(values, Is.EqualTo(new[] { 1, 0 }));
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
