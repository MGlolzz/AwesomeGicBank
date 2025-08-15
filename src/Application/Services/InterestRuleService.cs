using Domain.Abstractions;
using Domain.Entities;

namespace Application.Services;

public sealed class InterestRuleService
{
    private readonly IInterestRuleRepository _rules;

    public InterestRuleService(IInterestRuleRepository rules) => _rules = rules;

    public (bool ok, string error) TryUpsert(string dateStr, string ruleId, string rateStr)
    {
        if (!Application.Util.DateParsing.TryParseYyyyMmDd(dateStr, out var date))
            return (false, "Date must be in YYYYMMdd");
        if (string.IsNullOrWhiteSpace(ruleId))
            return (false, "RuleId is required");
        if (!decimal.TryParse(rateStr, out var rate) || rate <= 0 || rate >= 100)
            return (false, "Rate must be > 0 and < 100");

        _rules.Upsert(new InterestRule(date, ruleId, decimal.Round(rate, 2)));
        return (true, string.Empty);
    }

    public IReadOnlyList<InterestRule> ListOrdered() => _rules.AllOrdered();
}
