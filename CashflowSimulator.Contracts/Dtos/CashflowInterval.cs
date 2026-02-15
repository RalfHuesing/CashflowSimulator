using System.ComponentModel;

namespace CashflowSimulator.Contracts.Dtos;

/// <summary>
/// Intervall für wiederkehrende Cashflows (Streams): monatlich oder jährlich.
/// </summary>
public enum CashflowInterval
{
    [Description("Monatlich")]
    Monthly,

    [Description("Jährlich")]
    Yearly
}
