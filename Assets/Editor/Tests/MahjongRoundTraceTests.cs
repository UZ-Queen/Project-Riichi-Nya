using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

public class MahjongRoundTraceTests
{
    private const int Seed = 1557;
    private const int MaxActions = 200;

    [Test]
    public void SameSeedAndActions_ProduceIdenticalTrace()
    {
        TraceResult expected = RunTrace(Seed, MaxActions);
        TraceResult actual = RunTrace(Seed, MaxActions);

        Assert.That(expected.ReachedNextRound, Is.True, expected.Diagnostic);
        Assert.That(actual.ReachedNextRound, Is.True, actual.Diagnostic);
        Assert.That(expected.Records.Count, Is.EqualTo(actual.Records.Count));

        int firstMismatch = FindFirstMismatch(expected.Records, actual.Records);
        Assert.That(firstMismatch, Is.EqualTo(-1), BuildMismatchMessage(expected, actual, firstMismatch));

        string summary = expected.Records[expected.Records.Count - 1].ToString();
        Debug.Log($"TRACE seed={Seed} acceptedActions={expected.AcceptedActions} state={summary} PASS");
    }

    [Test]
    public void ActionCap_ReportsFirstMismatch()
    {
        TraceResult result = RunTrace(Seed, 1);

        Assert.That(result.ReachedNextRound, Is.False);
        Assert.That(result.Diagnostic, Does.Contain("firstMismatchActionIndex=1"));
        Assert.That(result.Diagnostic, Does.Contain("expected=next-round-transition"));
        Assert.That(result.Diagnostic, Does.Contain("actual="));
    }

    [Test]
    public void TraceContract_ContainsOnlyDecisionFields()
    {
        string[] expectedFields =
        {
            "Seed",
            "ActionIndex",
            "AcceptedActions",
            "Hand",
            "DrawnTile",
            "DiscardedTile",
            "RiverCount",
            "RoundTransitioned",
            "NextRoundFirstTsumo"
        };

        FieldInfo[] fields = typeof(TraceRecord).GetFields(BindingFlags.Instance | BindingFlags.Public);
        string[] actualFields = new string[fields.Length];
        for (int i = 0; i < fields.Length; i++)
        {
            actualFields[i] = fields[i].Name;
        }

        Assert.That(actualFields, Is.EquivalentTo(expectedFields));
        string[] forbiddenFields = { "Score", "Wall", "RemainingTsumoCount", "Yaku", "Han", "Fu", "Payment" };
        for (int i = 0; i < actualFields.Length; i++)
        {
            Assert.That(forbiddenFields, Does.Not.Contain(actualFields[i]));
        }
    }

    private static TraceResult RunTrace(int seed, int maxActions)
    {
        TraceResult result = new TraceResult(seed);
        MahjongRound round = MahjongRound.NewRound(seed, out MahjongPlayer player);
        string drawnTile = string.Empty;
        int actionIndex = 0;

        round.OnTsumoTile += info => drawnTile = info.tsumoTile.ToString();
        round.OnNewRoundStart += nextRound =>
        {
            TraceRecord transitionRecord = CreateRecord(
                seed,
                actionIndex,
                actionIndex + 1,
                player,
                drawnTile,
                drawnTile,
                true,
                string.Empty);

            nextRound.OnTsumoTile += info => transitionRecord.NextRoundFirstTsumo = info.tsumoTile.ToString();
            nextRound.GenerateYama();
            result.Records.Add(transitionRecord);
            result.ReachedNextRound = true;
            result.AcceptedActions = actionIndex + 1;
        };

        round.GenerateYama();

        while (!result.ReachedNextRound && actionIndex < maxActions)
        {
            string handBeforeDiscard = SerializeHand(player.Hand);
            string discardedTile = player.tsumoTile.ToString();
            round.DiscardTile(13);

            if (!result.ReachedNextRound)
            {
                result.Records.Add(new TraceRecord
                {
                    Seed = seed,
                    ActionIndex = actionIndex,
                    AcceptedActions = actionIndex + 1,
                    Hand = handBeforeDiscard,
                    DrawnTile = drawnTile,
                    DiscardedTile = discardedTile,
                    RiverCount = player.River.Count,
                    RoundTransitioned = false,
                    NextRoundFirstTsumo = string.Empty
                });
                result.AcceptedActions = actionIndex + 1;
            }

            actionIndex++;
        }

        if (!result.ReachedNextRound)
        {
            TraceRecord actual = result.Records.Count == 0
                ? new TraceRecord { Seed = seed, ActionIndex = 0, AcceptedActions = 0 }
                : result.Records[result.Records.Count - 1];
            result.Diagnostic =
                $"firstMismatchActionIndex={result.AcceptedActions} " +
                "expected=next-round-transition " +
                $"actual={actual}";
        }

        return result;
    }

    private static TraceRecord CreateRecord(
        int seed,
        int actionIndex,
        int acceptedActions,
        MahjongPlayer player,
        string drawnTile,
        string discardedTile,
        bool roundTransitioned,
        string nextRoundFirstTsumo)
    {
        return new TraceRecord
        {
            Seed = seed,
            ActionIndex = actionIndex,
            AcceptedActions = acceptedActions,
            Hand = SerializeHand(player.Hand),
            DrawnTile = drawnTile,
            DiscardedTile = discardedTile,
            RiverCount = player.River.Count,
            RoundTransitioned = roundTransitioned,
            NextRoundFirstTsumo = nextRoundFirstTsumo
        };
    }

    private static string SerializeHand(List<MahjongTile> hand)
    {
        if (hand == null || hand.Count == 0)
        {
            return string.Empty;
        }

        List<string> tiles = new List<string>(hand.Count);
        for (int i = 0; i < hand.Count; i++)
        {
            tiles.Add(hand[i].ToString());
        }

        return string.Join(",", tiles);
    }

    private static int FindFirstMismatch(List<TraceRecord> expected, List<TraceRecord> actual)
    {
        int commonCount = Math.Min(expected.Count, actual.Count);
        for (int i = 0; i < commonCount; i++)
        {
            if (expected[i].ToString() != actual[i].ToString())
            {
                return i;
            }
        }

        return expected.Count == actual.Count ? -1 : commonCount;
    }

    private static string BuildMismatchMessage(TraceResult expected, TraceResult actual, int firstMismatch)
    {
        if (firstMismatch < 0)
        {
            return string.Empty;
        }

        string expectedState = firstMismatch < expected.Records.Count
            ? expected.Records[firstMismatch].ToString()
            : "<missing>";
        string actualState = firstMismatch < actual.Records.Count
            ? actual.Records[firstMismatch].ToString()
            : "<missing>";
        return
            $"firstMismatchActionIndex={firstMismatch} " +
            $"expected={expectedState} actual={actualState}";
    }

    private sealed class TraceResult
    {
        public TraceResult(int seed)
        {
            Seed = seed;
        }

        public int Seed { get; }
        public List<TraceRecord> Records { get; } = new List<TraceRecord>();
        public bool ReachedNextRound { get; set; }
        public int AcceptedActions { get; set; }
        public string Diagnostic { get; set; } = string.Empty;
    }

    private sealed class TraceRecord
    {
        public int Seed;
        public int ActionIndex;
        public int AcceptedActions;
        public string Hand;
        public string DrawnTile;
        public string DiscardedTile;
        public int RiverCount;
        public bool RoundTransitioned;
        public string NextRoundFirstTsumo;

        public override string ToString()
        {
            return
                $"seed={Seed};actionIndex={ActionIndex};acceptedActions={AcceptedActions};" +
                $"hand={Hand};drawn={DrawnTile};discarded={DiscardedTile};" +
                $"riverCount={RiverCount};transition={RoundTransitioned};" +
                $"nextRoundFirstTsumo={NextRoundFirstTsumo}";
        }
    }
}
