using System.Collections.Concurrent;
using Domain.Abstractions;
using Domain.Entities;

namespace Infrastructure;

public sealed class InMemoryInterestRuleRepository : IInterestRuleRepository
{
    private readonly ConcurrentDictionary<DateOnly, InterestRule> _rulesByDate = new();

    public void Upsert(InterestRule rule) => _rulesByDate[rule.EffectiveDate] = rule;

    public IReadOnlyList<InterestRule> AllOrdered() =>
        _rulesByDate.Values.OrderBy(r => r.EffectiveDate).ToList();

    public InterestRule? LatestOnOrBefore(DateOnly date)
    {
        // Find all rule dates <= the requested date
        var eligible = _rulesByDate.Keys.Where(d => d <= date);
        if (!eligible.Any()) return null;

        // Choose the most recent eligible date and return its rule
        var max = eligible.Max();
        return _rulesByDate[max];
    }
}
