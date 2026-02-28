using CashflowSimulator.Contracts.Dtos;

namespace CashflowSimulator.Engine.Services.Simulation;

/// <summary>
/// Ermittelt die aktuelle Lebensphase und das zugehörige Allokationsprofil anhand des Alters.
/// Wird vom LiquidityProcessor (und ggf. weiteren Processors) genutzt.
/// </summary>
public static class LifecyclePhaseResolver
{
    /// <summary>
    /// Liefert die Zielallokation (AssetClassId → TargetWeight) für den gegebenen Monat.
    /// Ermittelt das Alter aus <paramref name="project"/>.Parameters.DateOfBirth und
    /// <paramref name="currentDate"/>, wählt die passende LifecyclePhase (größtes StartAge ≤ Alter)
    /// und gibt die Entries des referenzierten AllocationProfile zurück.
    /// </summary>
    /// <returns>Liste der Allokationseinträge, oder leer/null wenn keine Phase oder kein Profil existiert.</returns>
    public static IReadOnlyList<AllocationProfileEntryDto>? GetAllocationProfileEntries(
        SimulationProjectDto project,
        DateOnly currentDate)
    {
        ArgumentNullException.ThrowIfNull(project);
        var parameters = project.Parameters;
        if (parameters == null)
            return null;

        var phases = project.LifecyclePhases;
        if (phases == null || phases.Count == 0)
            return null;

        var dateOfBirth = parameters.DateOfBirth;
        var ageInYears = CalculateAgeInYears(dateOfBirth, currentDate);

        // Phase mit größtem StartAge wählen, das noch ≤ Alter ist
        var applicablePhase = phases
            .Where(p => p.StartAge <= ageInYears)
            .OrderByDescending(p => p.StartAge)
            .FirstOrDefault();

        if (applicablePhase == null)
            return null;

        if (string.IsNullOrEmpty(applicablePhase.AllocationProfileId))
            return null;

        var profile = project.AllocationProfiles?
            .FirstOrDefault(p => p.Id == applicablePhase.AllocationProfileId);
        if (profile?.Entries == null || profile.Entries.Count == 0)
            return null;

        return profile.Entries;
    }

    /// <summary>
    /// Alter in Jahren (Gleitkomma) aus Geburtsdatum und Stichtag.
    /// </summary>
    public static double CalculateAgeInYears(DateOnly dateOfBirth, DateOnly currentDate)
    {
        var totalDays = (currentDate.ToDateTime(TimeOnly.MinValue) - dateOfBirth.ToDateTime(TimeOnly.MinValue)).TotalDays;
        return totalDays / 365.25;
    }
}
