using CashflowSimulator.Contracts.Dtos;

namespace CashflowSimulator.Engine.Services.Defaults;

/// <summary>
/// Kontext der zeitlichen Standard-Parameter (Stichtag und Simulationsparameter).
/// Wird vom <see cref="ISimulationTimeService"/> geliefert.
/// </summary>
/// <param name="Now">Aktueller Zeitpunkt (z. B. für Meta.CreatedAt).</param>
/// <param name="Parameters">Berechnete Simulationsparameter (Start, Ende, Geburt, Rente, Währung).</param>
public record DefaultTimeContext(DateTimeOffset Now, SimulationParametersDto Parameters);
