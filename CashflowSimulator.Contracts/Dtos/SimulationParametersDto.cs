namespace CashflowSimulator.Contracts.Dtos;

/// <summary>
/// Zeithorizont und Stammparameter der Simulation.
/// Alle Daten sind konventionsgemäß der 1. des jeweiligen Monats (monatliche Cashflow-Iteration).
/// </summary>
public record SimulationParametersDto
{
    /// <summary>Simulationsstart: 1. des aktuellen Monats.</summary>
    public DateOnly SimulationStart { get; init; }

    /// <summary>Simulationsende: 1. des Monats, in dem die Lebenserwartung (z. B. 95) erreicht wird.</summary>
    public DateOnly SimulationEnd { get; init; }

    /// <summary>Geburtsdatum (Basis für Altersberechnungen).</summary>
    public DateOnly DateOfBirth { get; init; }

    /// <summary>Steuerlicher Verlustvortrag (allgemeiner Verlusttopf) zum Simulationsstart. Wird mit sonstigen Gewinnen verrechnet.</summary>
    public decimal InitialLossCarryforwardGeneral { get; init; }

    /// <summary>Steuerlicher Verlustvortrag (Aktienverlusttopf) zum Simulationsstart. Nur mit Aktiengewinnen verrechenbar.</summary>
    public decimal InitialLossCarryforwardStocks { get; init; }

    public decimal InitialLiquidCash { get; init; }
    public string CurrencyCode { get; init; } = "EUR";
}
