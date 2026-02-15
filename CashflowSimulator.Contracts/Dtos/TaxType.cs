using System.ComponentModel;

namespace CashflowSimulator.Contracts.Dtos;

/// <summary>
/// Steuerliche Behandlung des Fonds/Assets im deutschen Steuerrecht.
/// Bestimmt u. a. Teilfreistellung (Aktienfonds) vs. volle Besteuerung (Anleihenfonds).
/// </summary>
public enum TaxType
{
    /// <summary>
    /// Aktienfonds (Aktienquote &gt; 51 %). Teilfreistellung 30 % bei Veräußerung und Ausschüttung.
    /// </summary>
    [Description("Aktienfonds (Teilfreistellung)")]
    EquityFund,

    /// <summary>
    /// Mischfonds (Aktienquote 25–51 %). Teilfreistellung anteilig.
    /// </summary>
    [Description("Mischfonds")]
    MixedFund,

    /// <summary>
    /// Anleihenfonds (Aktienquote &lt; 25 %). Keine Teilfreistellung.
    /// </summary>
    [Description("Anleihenfonds")]
    BondFund,

    /// <summary>
    /// Keine Sonderbehandlung (z. B. Einzelaktien, Krypto). Volle Besteuerung.
    /// </summary>
    [Description("Voll steuerpflichtig")]
    None
}
