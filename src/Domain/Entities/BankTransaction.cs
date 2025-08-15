using Domain.Enums;

namespace Domain.Entities;

public sealed class BankTransaction
{
    public DateOnly Date { get; }
    public string TransactionId { get; } // e.g., "20230626-02" or "" for interest
    public TransactionType Type { get; }
    public decimal Amount { get; } // positive; sign handled by Type semantics

    public BankTransaction(DateOnly date, string txnId, TransactionType type, decimal amount)
    {
        Date       = date;
        TransactionId = txnId;
        Type       = type;
        Amount     = decimal.Round(amount, 2, MidpointRounding.AwayFromZero);
    }
}
