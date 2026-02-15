using System.ComponentModel;

namespace CashflowSimulator.Contracts.Dtos;

/// <summary>
/// Einnahme oder Ausgabe – für Streams und Events.
/// </summary>
public enum CashflowType
{
    [Description("Einnahme")]
    Income,

    [Description("Ausgabe")]
    Expense
}
