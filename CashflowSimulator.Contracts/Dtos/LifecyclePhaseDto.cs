using CashflowSimulator.Contracts.Interfaces;

namespace CashflowSimulator.Contracts.Dtos;

/// <summary>
/// Eine Lebensphase in der Simulation (z. B. Erwerbsleben, Rente).
/// Getriggert durch <see cref="StartAge"/>; verweist auf Steuer- und Strategie-Profil.
/// </summary>
public record LifecyclePhaseDto : IIdentifiable
{
    /// <inheritdoc />
    public string Id { get; init; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Alter (in Jahren), ab dem diese Phase aktiv ist. 0 = von Simulationsstart an.
    /// </summary>
    public int StartAge { get; init; }

    /// <summary>
    /// ID des <see cref="TaxProfileDto"/>, das in dieser Phase gilt.
    /// </summary>
    public string TaxProfileId { get; init; } = string.Empty;

    /// <summary>
    /// ID des <see cref="StrategyProfileDto"/>, das in dieser Phase gilt.
    /// </summary>
    public string StrategyProfileId { get; init; } = string.Empty;

    /// <summary>
    /// Optionale Überschreibungen der Zielallokation (AssetClassId → TargetWeight) in dieser Phase.
    /// Leer = Basis-<see cref="AssetClassDto.TargetWeight"/> aus dem Projekt verwenden.
    /// </summary>
    public List<AssetAllocationOverrideDto> AssetAllocationOverrides { get; init; } = [];
}
