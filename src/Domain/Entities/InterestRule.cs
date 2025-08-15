namespace Domain.Entities;

public sealed class InterestRule
{
    public DateOnly EffectiveDate { get; }
    public string RuleId { get; }
    public decimal RatePercent { get; } // e.g., 2.20

    public InterestRule(DateOnly date, string ruleId, decimal ratePercent)
    {
        EffectiveDate = date;
        RuleId        = ruleId;
        RatePercent   = ratePercent;
    }
}
