using CashflowSimulator.Contracts.Dtos;

namespace CashflowSimulator.Engine.Services.Defaults;

/// <summary>
/// Liefert die Standard-Cashflows für ein Default-Szenario:
/// wiederkehrende Streams (Einnahmen/Ausgaben) und einmalige Events.
/// </summary>
public interface ICashflowDefaultService
{
    /// <summary>
    /// Liefert alle Standard-Streams (Gehalt, Rente, Wohnen, Mobilität, Versicherungen, etc.)
    /// in Abhängigkeit von Simulationsstart und Renteneintritt.
    /// </summary>
    List<CashflowStreamDto> GetStreams(DateOnly simulationStart, DateOnly retirementDate);

    /// <summary>
    /// Liefert die Standard-Lebensereignisse (einmalige Ausgaben/Einnahmen)
    /// mit Zieltermin und Streuung.
    /// </summary>
    List<CashflowEventDto> GetEvents(DateOnly simulationStart, DateOnly retirementDate);
}
