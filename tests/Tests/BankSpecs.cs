using Application.Services;
using Infrastructure;
using Xunit;

public class BankSpecs
{
    [Fact]
    public void Cannot_Withdraw_First_Transaction()
    {
        var accRepo = new InMemoryAccountRepository();
        var svc = new TransactionService(accRepo);
        var (ok, err, _) = svc.TryAddUserTransaction("20230601", "AC001", "W", "10.00");
        Assert.False(ok);
        Assert.Contains("First transaction", err);
    }

    [Fact]
    public void Cannot_Input_Wrong_Date()
    {
        var accRepo = new InMemoryAccountRepository();
        var svc = new TransactionService(accRepo);
        var (ok, err, _) = svc.TryAddUserTransaction("202fsadfdasf30601", "AC001", "W", "10.00");
        Assert.False(ok);
        Assert.Contains("Date must be in YYYYMMdd", err);
    }

    [Fact]
    public void Cannot_Input_Wrong_Transaction_Type()
    {
        var accRepo = new InMemoryAccountRepository();
        var svc = new TransactionService(accRepo);
        var ex = Assert.Throws<ArgumentException>(() =>
        svc.TryAddUserTransaction("20230601", "AC001", "VWW", "10.00")
    );

        Assert.Equal("Type must be D or W", ex.Message);
    }

    [Fact]
    public void Cannot_Input_Wrong_Amount()
    {
        var accRepo = new InMemoryAccountRepository();
        var svc = new TransactionService(accRepo);
        var (ok, err, _) = svc.TryAddUserTransaction("20230601", "AC001", "W", "10.00123123");
        Assert.False(ok);
        Assert.Contains("Amount must be > 0 with up to 2 decimals", err);
    }

    [Fact]
    public void Deposit_Then_Withdraw_Tracks_NonNegative()
    {
        var accRepo = new InMemoryAccountRepository();
        var svc = new TransactionService(accRepo);

        Assert.True(svc.TryAddUserTransaction("20230601", "AC001", "D", "150.00").ok);
        Assert.True(svc.TryAddUserTransaction("20230626", "AC001", "W", "20.00").ok);
        Assert.True(svc.TryAddUserTransaction("20230626", "AC001", "W", "100.00").ok);
        var acc = accRepo.TryGet("AC001")!;
        Assert.Equal(3, acc.Transactions.Count);
    }
}
