using Domain.Abstractions;
using Domain.Entities;
using Domain.Enums;

namespace Application.Services;

public sealed class TransactionService
{
    private readonly IAccountRepository _accounts;

    public TransactionService(IAccountRepository accounts) => _accounts = accounts;

    /// <summary>
    /// Tries to add a user-initiated transaction to an account.
    /// Applies business rules to validate date, amount, and account status.
    /// </summary>
    public (bool ok, string error, Account? account) TryAddUserTransaction(
        string dateStr, string accountId, string typeStr, string amountStr)
    {
        // Validate date format (must be YYYYMMDD)
        if (!Application.Util.DateParsing.TryParseYyyyMmDd(dateStr, out var date))
            return (false, "Date must be in YYYYMMdd", null);

        // Ensure accountId is not empty
        if (string.IsNullOrWhiteSpace(accountId))
            return (false, "Account is required", null);

        // Parse and validate transaction amount: > 0 and max 2 decimals
        if (!decimal.TryParse(amountStr, out var amount) || amount <= 0 ||
            decimal.Round(amount, 2) != amount)
            return (false, "Amount must be > 0 with up to 2 decimals", null);

        // Determine transaction type from user input (D = Deposit, W = Withdrawal)
        TransactionType type = typeStr.Equals("D", StringComparison.OrdinalIgnoreCase) ? TransactionType.Deposit
                              : typeStr.Equals("W", StringComparison.OrdinalIgnoreCase) ? TransactionType.Withdrawal
                              : throw new ArgumentException("Type must be D or W");

        // Retrieve account or create if not exists
        Account acc = _accounts.GetOrCreate(accountId);

        // Business rule: first transaction cannot be a withdrawal
        if (acc.Transactions.Count == 0 && type == TransactionType.Withdrawal)
            return (false, "First transaction cannot be withdrawal", null);

        // Generate a new transaction ID based on the date
        string txnId = _accounts.NextTransactionId(date);

        // Create new transaction entity
        BankTransaction candidate = new(date, txnId, type, amount);

        // Simulate inserting the new transaction and sort chronologically
        List<BankTransaction> simulated = acc.Transactions.Concat([candidate])
            .OrderBy(t => t.Date)
            .ThenBy(t => t.TransactionId)
            .ToList();

        // Simulate running balance to ensure it never goes below zero
        decimal running = 0m;
        foreach (var t in simulated)
        {
            running += t.Type switch
            {
                TransactionType.Deposit => t.Amount,
                TransactionType.Withdrawal => -t.Amount,
                TransactionType.Interest => t.Amount,
                _ => 0m
            };

            if (running < 0m)
                return (false, "Balance cannot go below 0", null);
        }

        // If all checks pass, add the transaction to the account
        acc.Add(candidate);
        return (true, string.Empty, acc);
    }
}
