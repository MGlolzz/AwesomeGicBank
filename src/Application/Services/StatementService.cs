using System.Text;
using Domain.Abstractions;
using Domain.Entities;
using Domain.Enums;

namespace Application.Services;

/// <summary>
/// Statement/reporting service (read-mostly).
/// NOTE: <see cref="TryPrintMonthly"/> will CREDIT monthly interest as a new transaction
/// on the last day of the month when applicable (intentional side-effect).
/// </summary>
public sealed class StatementService
{
    private readonly IAccountRepository _accounts;           // data source for accounts/transactions
    private readonly IInterestRuleRepository _rules;         // interest rules (effective-date based)

    public StatementService(IAccountRepository accounts, IInterestRuleRepository rules)
    {
        _accounts = accounts;
        _rules = rules;
    }

    /// <summary>
    /// Prints all transactions for a given account (chronological), without balances.
    /// Returns (ok, error, output). Never throws; errors are in the tuple.
    /// </summary>
    public (bool ok, string error, string output) TryPrintAccountAll(string accountId)
    {
        var acc = _accounts.TryGet(accountId);
        if (acc is null) return (false, "Account not found", string.Empty);

        var sb = new StringBuilder();
        sb.AppendLine($"Account: {acc.AccountId}");
        sb.AppendLine("| Date     | Txn Id      | Type | Amount |");

        // Transactions are already ordered in the domain entity; we print as-is.
        foreach (var t in acc.Transactions)
            sb.AppendLine($"| {t.Date:yyyyMMdd} | {t.TransactionId,-12}| {TypeToChar(t.Type)}    | {t.Amount,6:0.00} |");

        return (true, string.Empty, sb.ToString());
    }

    /// <summary>
    /// Prints the statement for a specific month (YYYYMM), computing daily EOD balances,
    /// applying the effective interest rule per day, and CREDITING the month-end interest
    /// as a transaction on the last day of the month (if > 0).
    /// </summary>
    public (bool ok, string error, string output) TryPrintMonthly(string accountId, string ym)
    {
        // Basic input validation: "YYYYMM"
        if (!Application.Util.DateParsing.TryParseYyyyMm(ym, out var year, out var month))
            return (false, "Month must be YYYYMM", string.Empty);

        var acc = _accounts.TryGet(accountId);
        if (acc is null) return (false, "Account not found", string.Empty);

        // Month boundaries (inclusive)
        var first = new DateOnly(year, month, 1);
        var last = first.AddMonths(1).AddDays(-1);

        // Snapshot all transactions and sort once for stable calculations
        var trans = acc.Transactions
            .OrderBy(t => t.Date)
            .ThenBy(t => t.TransactionId)
            .ToList();

        // Local function: computes running balance up to and including day 'd'
        // using the sorted list 'trans'. This is called for each day in the month.
        decimal BalanceUpTo(DateOnly d) =>
            trans.Where(t => t.Date <= d).Sum(t => t.Type switch
            {
                TransactionType.Deposit => t.Amount,
                TransactionType.Withdrawal => -t.Amount,
                TransactionType.Interest => t.Amount,
                _ => 0m
            });

        // Accumulate (EOD_balance * annual_rate%) for each day in the month.
        // We'll divide by 365 at the end to get the monthly interest amount.
        decimal annualizedSum = 0m;
        var day = first;
        while (day <= last)
        {
            var eod = BalanceUpTo(day);                       
            var rule = _rules.LatestOnOrBefore(day);          
            var rate = rule?.RatePercent ?? 0m;              
            annualizedSum += eod * rate / 100m;              
            day = day.AddDays(1);
        }

        // Convert the daily accumulation to monthly interest
        // then round to 2 d.p.
        var monthlyInterest = decimal.Round(
            annualizedSum / 365m,
            2,
            MidpointRounding.AwayFromZero
        );

        // CREDIT interest as a transaction on the last day (if any).
        // This mutates account state (by design) so future prints include the credit.
        if (monthlyInterest > 0m)
        {
            acc.Add(new BankTransaction(last, "", TransactionType.Interest, monthlyInterest));
            // Refresh 'trans' so subsequent balance prints include the credited interest.
            trans = acc.Transactions
                .OrderBy(t => t.Date)
                .ThenBy(t => t.TransactionId)
                .ToList();
        }

        // Build the printable table including per-transaction running balance for the month.
        var sb = new StringBuilder();
        sb.AppendLine($"Account: {acc.AccountId}");
        sb.AppendLine("| Date     | Txn Id      | Type | Amount | Balance |");

        // Opening balance is the balance before the first day of the month.
        decimal running = BalanceUpTo(first.AddDays(-1));

        // Print only the transactions that fall within [first, last], updating 'running'.
        foreach (var t in trans.Where(t => t.Date >= first && t.Date <= last))
        {
            running += t.Type switch
            {
                TransactionType.Deposit => t.Amount,
                TransactionType.Withdrawal => -t.Amount,
                TransactionType.Interest => t.Amount,
                _ => 0m
            };

            sb.AppendLine(
                $"| {t.Date:yyyyMMdd} | {t.TransactionId,-12}| {TypeToChar(t.Type)}    | {t.Amount,6:0.00} | {running,7:0.00} |"
            );
        }

        return (true, string.Empty, sb.ToString());
    }

    // Maps enum to the single-character code used in the table.
    private static string TypeToChar(TransactionType t) =>
        t switch
        {
            TransactionType.Deposit => "D",
            TransactionType.Withdrawal => "W",
            TransactionType.Interest => "I",
            _ => "?"
        };
}
