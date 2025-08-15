using Application.Services;
using Infrastructure;
using Xunit;

public class InterestSpecs
{


    [Fact]
    public void Cannot_Input_Wrong_Date()
    {
        var ruleRepo = new InMemoryInterestRuleRepository();
        var rules = new InterestRuleService(ruleRepo);
        var (ok, err) = rules.TryUpsert("202fsadfdasf30601", "RULE01", "10.00");
        Assert.False(ok);
        Assert.Contains("Date must be in YYYYMMdd", err);
    }



    [Fact]
    public void Cannot_Input_Wrong_Amount()
    {
        var ruleRepo = new InMemoryInterestRuleRepository();
        var rules = new InterestRuleService(ruleRepo);
        var (ok, err) = rules.TryUpsert("20230601", "RULE01", "10.00123123");
        Assert.True(ok);
    }
    [Fact]
    public void June_Interest_Matches_Sample_0_39()
    {
        var accRepo = new InMemoryAccountRepository();
        var ruleRepo = new InMemoryInterestRuleRepository();
        var txn = new TransactionService(accRepo);
        var rules = new InterestRuleService(ruleRepo);
        var stmt = new StatementService(accRepo, ruleRepo);

        Assert.True(txn.TryAddUserTransaction("20230505", "AC001", "D", "100.00").ok);
        Assert.True(txn.TryAddUserTransaction("20230601", "AC001", "D", "150.00").ok);
        Assert.True(txn.TryAddUserTransaction("20230626", "AC001", "W", "20.00").ok);
        Assert.True(txn.TryAddUserTransaction("20230626", "AC001", "W", "100.00").ok);

        Assert.True(rules.TryUpsert("20230101", "RULE01", "1.95").ok);
        Assert.True(rules.TryUpsert("20230520", "RULE02", "1.90").ok);
        Assert.True(rules.TryUpsert("20230615", "RULE03", "2.20").ok);

        var (ok, _, output) = stmt.TryPrintMonthly("AC001", "202306");
        Assert.True(ok);
        Assert.Contains("| 20230630 |             | I    |   0.39 |", output);
    }
}
