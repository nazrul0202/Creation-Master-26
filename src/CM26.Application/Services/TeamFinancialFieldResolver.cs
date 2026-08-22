using CM26.Application.Models;

namespace CM26.Application.Services;

/// <summary>
/// Resolves the honest per-team financial field exposed by the installed schema.
/// FC26 normally stores club valuation in <c>clubworth</c>; career transfer budgets
/// are not interchangeable with that value and must never be labelled as such.
/// </summary>
public static class TeamFinancialFieldResolver
{
    public sealed record Selection(string FieldName, string DisplayName, bool IsTransferBudget);

    public static Selection? Resolve(DbTable? table)
    {
        if (table?.FindColumn("transferbudget") is not null)
            return new Selection("transferbudget", "Transfer Budget", true);
        if (table?.FindColumn("clubworth") is not null)
            return new Selection("clubworth", "Club Worth", false);
        return null;
    }
}
