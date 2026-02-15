using System.ComponentModel;

namespace CashflowSimulator.Contracts.Dtos;

/// <summary>
/// Art einer Transaktion im Asset – Kauf, Verkauf, Ausschüttung oder Vorabpauschale.
/// Wird für die Transaktionshistorie und die FIFO-Steuerberechnung benötigt.
/// </summary>
public enum TransactionType
{
    [Description("Kauf")]
    Buy,

    [Description("Verkauf")]
    Sell,

    [Description("Ausschüttung / Dividende")]
    Dividend,

    [Description("Vorabpauschale")]
    TaxPrepayment
}
