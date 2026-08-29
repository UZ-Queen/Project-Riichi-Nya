using NUnit.Framework;

public class MahjongRoundTraceTests
{
    [Test]
    public void SameSeedAndActions_ProduceIdenticalTrace()
    {
        Assert.Fail("RED: deterministic round trace is not implemented yet.");
    }

    [Test]
    public void ActionCap_ReportsFirstMismatch()
    {
        Assert.Fail("RED: bounded trace diagnostics are not implemented yet.");
    }

    [Test]
    public void TraceContract_ContainsOnlyDecisionFields()
    {
        Assert.Fail("RED: compact trace contract is not implemented yet.");
    }
}
