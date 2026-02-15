namespace CashflowSimulator.Engine.Services.Defaults;

/// <summary>
/// Berechnet die zeitlichen Standard-Parameter für ein Default-Szenario
/// (Geburtsdatum, Renteneintritt, Simulationsstart/-ende, Währung).
/// </summary>
public interface ISimulationTimeService
{
    /// <summary>
    /// Liefert den aktuellen Zeitpunkt und die berechneten Simulationsparameter
    /// (Start/Ende, Geburtsdatum, Renteneintritt, Initiales Bargeld, Währung).
    /// </summary>
    DefaultTimeContext GetDefaultTimeContext();
}
