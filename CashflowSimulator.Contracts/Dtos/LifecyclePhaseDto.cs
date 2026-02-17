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

    /// <summary>
    /// ID des <see cref="AllocationProfileDto"/>, das die Zielallokation in dieser Phase definiert.
    /// Leer = Engine nutzt <see cref="AssetAllocationOverrides"/> bzw. globale <see cref="AssetClassDto.TargetWeight"/>.
    /// </summary>
    public string AllocationProfileId { get; init; } = string.Empty;

    /// <summary>
    /// Anzahl Monate vor Start dieser Phase, in denen die Zielallokation gleitend (Glidepath) von der
    /// Vorgängerphase auf dieses Profil umgeschichtet wird. 0 = sofortiger Wechsel beim Phasenstart.
    /// </summary>
    public int GlidepathMonths { get; init; }
}
