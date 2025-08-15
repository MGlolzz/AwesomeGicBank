namespace Domain.Entities;

public sealed class Account
{
    public string AccountId { get; }
    private readonly List<BankTransaction> _transactions = new();

    public Account(string accountId) => AccountId = accountId;

    public IReadOnlyList<BankTransaction> Transactions =>
        _transactions.OrderBy(t => t.Date).ThenBy(t => t.TransactionId).ToList();

    public void Add(BankTransaction txn) => _transactions.Add(txn);
}
