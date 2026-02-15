using CashflowSimulator.Contracts.Dtos;
using CashflowSimulator.Validation;
using Xunit;

namespace CashflowSimulator.Validation.Tests;

/// <summary>
/// Tests für <see cref="CorrelationMatrixValidation.GetPositiveDefinitenessError"/>:
/// gültige Korrelationsmatrix (positiv definit) vs. mathematisch unmögliche/widersprüchliche Korrelationen.
/// </summary>
public sealed class CorrelationMatrixValidationTests
{
    [Fact]
    public void GetPositiveDefinitenessError_ValidMatrix_ReturnsNull()
    {
        var project = new SimulationProjectDto
        {
            EconomicFactors =
            [
                new EconomicFactorDto { Id = "A", Name = "Faktor A" },
                new EconomicFactorDto { Id = "B", Name = "Faktor B" },
                new EconomicFactorDto { Id = "C", Name = "Faktor C" }
            ],
            Correlations =
            [
                new CorrelationEntryDto { FactorIdA = "A", FactorIdB = "B", Correlation = 0.3 },
                new CorrelationEntryDto { FactorIdA = "B", FactorIdB = "C", Correlation = 0.2 },
                new CorrelationEntryDto { FactorIdA = "A", FactorIdB = "C", Correlation = 0.1 }
            ]
        };

        string? error = CorrelationMatrixValidation.GetPositiveDefinitenessError(project);

        Assert.Null(error);
    }

    [Fact]
    public void GetPositiveDefinitenessError_InvalidContradictoryCorrelations_ReturnsErrorMessage()
    {
        // A–B = 0.9, B–C = 0.9, A–C = -0.9 ist mathematisch unmöglich (Dreiecksungleichung für Korrelationen).
        // Die Korrelationsmatrix ist nicht positiv definit.
        var project = new SimulationProjectDto
        {
            EconomicFactors =
            [
                new EconomicFactorDto { Id = "A", Name = "Faktor A" },
                new EconomicFactorDto { Id = "B", Name = "Faktor B" },
                new EconomicFactorDto { Id = "C", Name = "Faktor C" }
            ],
            Correlations =
            [
                new CorrelationEntryDto { FactorIdA = "A", FactorIdB = "B", Correlation = 0.9 },
                new CorrelationEntryDto { FactorIdA = "B", FactorIdB = "C", Correlation = 0.9 },
                new CorrelationEntryDto { FactorIdA = "A", FactorIdB = "C", Correlation = -0.9 }
            ]
        };

        string? error = CorrelationMatrixValidation.GetPositiveDefinitenessError(project);

        Assert.NotNull(error);
        Assert.Contains("nicht positiv definit", error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void GetPositiveDefinitenessError_NoFactors_ReturnsNull()
    {
        var project = new SimulationProjectDto { EconomicFactors = [] };

        string? error = CorrelationMatrixValidation.GetPositiveDefinitenessError(project);

        Assert.Null(error);
    }

    [Fact]
    public void GetPositiveDefinitenessError_SingleFactor_ReturnsNull()
    {
        var project = new SimulationProjectDto
        {
            EconomicFactors = [new EconomicFactorDto { Id = "Only", Name = "Ein Faktor" }]
        };

        string? error = CorrelationMatrixValidation.GetPositiveDefinitenessError(project);

        Assert.Null(error);
    }
}
