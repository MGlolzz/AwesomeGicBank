using System.Collections.Concurrent;
using Domain.Abstractions;
using Domain.Entities;

namespace Infrastructure;

public sealed class InMemoryAccountRepository : IAccountRepository
{
    private readonly ConcurrentDictionary<string, Account> _accounts = new();
    private readonly ConcurrentDictionary<DateOnly, int> _dateCounters = new();

    public Account GetOrCreate(string accountId) =>
        _accounts.GetOrAdd(accountId, id => new Account(id));

    public Account? TryGet(string accountId) =>
        _accounts.TryGetValue(accountId, out var acc) ? acc : null;

    public IEnumerable<Account> All() => _accounts.Values;

    public string NextTransactionId(DateOnly date)
    {
        int next = _dateCounters.AddOrUpdate(date, 1, (_, n) => n + 1);
        return $"{date:yyyyMMdd}-{next:D2}";
    }
}
