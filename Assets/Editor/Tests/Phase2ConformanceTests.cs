using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

/// <summary>
/// 공유 패산과 패 정체성의 고정 규칙을 독립적인 기대값으로 검증합니다.
/// </summary>
public class Phase2ConformanceTests
{
    [Test]
    public void WallComposition_HasLiteral136TilesAndThreeRedFives()
    {
        MahjongTile[] wall = MahjongWall.CreateShuffled(new Random(1557));

        Assert.That(wall, Has.Length.EqualTo(136));
        Assert.That(wall.Count(tile => tile.isAkaDora), Is.EqualTo(3));
        Assert.That(wall.Count(tile => tile.TileID == 5 && tile.isAkaDora), Is.EqualTo(1));
        Assert.That(wall.Count(tile => tile.TileID == 15 && tile.isAkaDora), Is.EqualTo(1));
        Assert.That(wall.Count(tile => tile.TileID == 25 && tile.isAkaDora), Is.EqualTo(1));
        Assert.That(wall.Count(tile => tile.TileID == 5 && !tile.isAkaDora), Is.EqualTo(3));
        Assert.That(wall.Count(tile => tile.TileID == 15 && !tile.isAkaDora), Is.EqualTo(3));
        Assert.That(wall.Count(tile => tile.TileID == 25 && !tile.isAkaDora), Is.EqualTo(3));
        Assert.That(wall.GroupBy(tile => tile.TileID).Count(), Is.EqualTo(34));
        Assert.That(wall.GroupBy(tile => tile.TileID).All(group => group.Count() == 4), Is.True);
        Assert.That(wall.All(tile => tile != MahjongTile.NullTile()), Is.True);
        Assert.That(wall.All(tile => !tile.isDora && tile.doraCount == 0), Is.True);
    }

    [Test]
    public void SeededWall_UsesStablePhysicalTileSequence()
    {
        MahjongTile[] first = MahjongWall.CreateShuffled(new Random(260906));
        MahjongTile[] second = MahjongWall.CreateShuffled(new Random(260906));

        Assert.That(ToPhysicalSequence(second), Is.EqualTo(ToPhysicalSequence(first)));

        MahjongTile[] changedRedPosition = (MahjongTile[])first.Clone();
        int redIndex = Array.FindIndex(changedRedPosition, tile => tile.TileID == 5 && tile.isAkaDora);
        int normalIndex = Array.FindIndex(changedRedPosition, tile => tile.TileID == 5 && !tile.isAkaDora);
        MahjongTile swap = changedRedPosition[redIndex];
        changedRedPosition[redIndex] = changedRedPosition[normalIndex];
        changedRedPosition[normalIndex] = swap;

        Assert.That(ToPhysicalSequence(changedRedPosition), Is.Not.EqualTo(ToPhysicalSequence(first)));
    }

    [Test]
    public void WallFactory_RejectsNullRandom()
    {
        Assert.Throws<ArgumentNullException>(() => MahjongWall.CreateShuffled(null));
    }

    [Test]
    public void TileIdentity_ObeysEqualityAndHashLawsForRedAndInvalidValues()
    {
        MahjongTile normalFive = MahjongTile.TileIDToTile(5);
        MahjongTile redFive = MahjongTile.TileIDToTile(5, true);
        MahjongTile scoredFive = normalFive;
        scoredFive.AddDora();

        Assert.That(normalFive, Is.EqualTo(redFive));
        Assert.That(normalFive, Is.EqualTo(scoredFive));
        Assert.That(normalFive.GetHashCode(), Is.EqualTo(redFive.GetHashCode()));
        Assert.That(normalFive.GetHashCode(), Is.EqualTo(scoredFive.GetHashCode()));
        Assert.That(normalFive.CompareTo(redFive), Is.Zero);
        Assert.That(new HashSet<MahjongTile> { normalFive, redFive, scoredFive }, Has.Count.EqualTo(1));

        MahjongTile invalid = MahjongTile.NullTile();
        MahjongTile invalidCopy = MahjongTile.NullTile();
        Assert.That(invalid == invalidCopy, Is.True);
        Assert.That(invalid != invalidCopy, Is.False);
        Assert.That(invalid.Equals(invalidCopy), Is.True);
        Assert.That(((object)invalid).Equals(invalidCopy), Is.True);
        Assert.That(invalid.GetHashCode(), Is.EqualTo(invalidCopy.GetHashCode()));
        Assert.That(invalid.CompareTo(invalidCopy), Is.Zero);
    }

    [Test]
    public void RiichiCandidates_PreserveRedFiveAtTheOriginalIndex()
    {
        MahjongRound.NewRound(1557, out MahjongPlayer player);
        List<MahjongTile> hand = MahjongTile.StringToTiles("1m1m1m2p3p4p6p7p8p9s9s9s");
        hand.Insert(0, MahjongTile.TileIDToTile(5, true));
        player.SetPlayerHand(hand);
        player.tsumoTile = MahjongTile.TileIDToTile(5);

        bool canRiichi = player.IsRiichiAble(out Dictionary<int, HashSet<MahjongWinInfo>> candidates);

        Assert.That(canRiichi, Is.True);
        Assert.That(candidates.ContainsKey(0), Is.True);
        Assert.That(candidates.ContainsKey(13), Is.True);
        Assert.That(candidates[0].All(info => info.doraInfo.akadoraCount == 0), Is.True);
        Assert.That(candidates[13].All(info => info.doraInfo.akadoraCount == 1), Is.True);
    }

    private static string[] ToPhysicalSequence(IEnumerable<MahjongTile> tiles)
    {
        return tiles.Select(tile => $"{tile.TileID}:{tile.isAkaDora}").ToArray();
    }
}
