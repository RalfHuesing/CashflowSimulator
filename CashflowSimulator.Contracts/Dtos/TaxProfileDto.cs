using CashflowSimulator.Contracts.Interfaces;

namespace CashflowSimulator.Contracts.Dtos;

/// <summary>
/// Steuer-Profil für eine Lebensphase (z. B. Erwerbsleben vs. Rente).
/// Wird von <see cref="LifecyclePhaseDto"/> referenziert.
/// </summary>
public record TaxProfileDto : IIdentifiable
{
    /// <summary>
    /// Eindeutige ID des Profils (Guid-String).
    /// </summary>
    public string Id { get; init; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Anzeigename des Profils (z. B. "Standard", "Rentenbesteuerung").
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Kapitalertragsteuer-Satz (inkl. Soli, Kirchensteuer) als Dezimal 0–1 (z. B. 0,26375 = 26,375 %).
    /// </summary>
    public decimal CapitalGainsTaxRate { get; init; }

    /// <summary>
    /// Freibetrag für Kapitalerträge (z. B. Sparerpauschbetrag in EUR).
    /// </summary>
    public decimal TaxFreeAllowance { get; init; }

    /// <summary>
    /// Einkommensteuer-Satz für nachgelagerte Besteuerung (z. B. Rente) als Dezimal 0–1.
    /// </summary>
    public decimal IncomeTaxRate { get; init; }
}
