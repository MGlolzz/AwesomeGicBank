using Domain.Entities;

namespace Domain.Abstractions;

public interface IAccountRepository
{
    Account GetOrCreate(string accountId);
    Account? TryGet(string accountId);
    IEnumerable<Account> All();
    // Transaction-id sequence per date:
    string NextTransactionId(DateOnly date);
}
