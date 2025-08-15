using Domain.Entities;

namespace Domain.Abstractions;

public interface IInterestRuleRepository
{
    void Upsert(InterestRule rule);
    IReadOnlyList<InterestRule> AllOrdered(); // asc by EffectiveDate
    InterestRule? LatestOnOrBefore(DateOnly date);
}
