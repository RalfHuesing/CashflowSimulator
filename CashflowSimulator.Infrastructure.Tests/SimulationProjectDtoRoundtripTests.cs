using System.Text.Json;
using System.Text.Json.Serialization;
using CashflowSimulator.Contracts.Dtos;
using Xunit;

namespace CashflowSimulator.Infrastructure.Tests;

/// <summary>
/// JSON-Roundtrip für <see cref="SimulationProjectDto"/>: Serialisierung und Deserialisierung mit denselben
/// Optionen wie in der Persistenz (ScenarioSnapshotWriter) müssen zu identischem Ergebnis führen.
/// </summary>
public sealed class SimulationProjectDtoRoundtripTests
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() }
    };

    [Fact]
    public void Roundtrip_ComplexSimulationProjectDto_ResultIdentical()
    {
        var project = BuildComplexProject();

        var json = JsonSerializer.Serialize(project, Options);
        var deserialized = JsonSerializer.Deserialize<SimulationProjectDto>(json, Options);
        Assert.NotNull(deserialized);

        var json2 = JsonSerializer.Serialize(deserialized, Options);
        Assert.Equal(json, json2);
    }

    [Fact]
    public void Roundtrip_ComplexProject_KeyPropertiesPreserved()
    {
        var project = BuildComplexProject();
        var json = JsonSerializer.Serialize(project, Options);
        var deserialized = JsonSerializer.Deserialize<SimulationProjectDto>(json, Options);
        Assert.NotNull(deserialized);

        Assert.Equal(project.Parameters.SimulationStart, deserialized.Parameters.SimulationStart);
        Assert.Equal(project.Parameters.SimulationEnd, deserialized.Parameters.SimulationEnd);
        Assert.Equal(project.Parameters.InitialLossCarryforwardGeneral, deserialized.Parameters.InitialLossCarryforwardGeneral);
        Assert.Equal(project.Parameters.InitialLossCarryforwardStocks, deserialized.Parameters.InitialLossCarryforwardStocks);
        Assert.Equal(project.Streams.Count, deserialized.Streams.Count);
        Assert.Equal(project.EconomicFactors.Count, deserialized.EconomicFactors.Count);
        Assert.Equal(project.Portfolio.Assets.Count, deserialized.Portfolio.Assets.Count);
        Assert.Equal(project.LifecyclePhases.Count, deserialized.LifecyclePhases.Count);
        Assert.Equal(project.RandomSeed, deserialized.RandomSeed);
        Assert.Equal(project.MonteCarloIterations, deserialized.MonteCarloIterations);

        var asset = project.Portfolio.Assets[0];
        var dAsset = deserialized.Portfolio.Assets[0];
        Assert.Equal(asset.Id, dAsset.Id);
        Assert.Equal(asset.CurrentPrice, dAsset.CurrentPrice);
        Assert.Equal(asset.Tranches.Count, dAsset.Tranches.Count);
        Assert.Equal(asset.Transactions.Count, dAsset.Transactions.Count);
    }

    private static SimulationProjectDto BuildComplexProject()
    {
        var taxProfileId = Guid.NewGuid().ToString();
        var strategyId = Guid.NewGuid().ToString();
        var allocationId = Guid.NewGuid().ToString();
        var factorId = "MSCI_World";
        var assetClassId = "equity";

        return new SimulationProjectDto
        {
            Meta = new MetaDto
            {
                ScenarioName = "Roundtrip-Test",
                CreatedAt = new DateTimeOffset(2025, 1, 15, 12, 0, 0, TimeSpan.Zero)
            },
            Parameters = new SimulationParametersDto
            {
                SimulationStart = new DateOnly(2020, 1, 1),
                SimulationEnd = new DateOnly(2060, 1, 1),
                DateOfBirth = new DateOnly(1990, 5, 15),
                InitialLossCarryforwardGeneral = 2000m,
                InitialLossCarryforwardStocks = 500m,
                InitialLiquidCash = 100_000m,
                CurrencyCode = "EUR"
            },
            Streams =
            [
                new CashflowStreamDto
                {
                    Id = "s1",
                    Name = "Gehalt",
                    Type = CashflowType.Income,
                    Amount = 3000m,
                    Interval = CashflowInterval.Monthly,
                    StartDate = new DateOnly(2020, 1, 1),
                    EndDate = null,
                    EconomicFactorId = "Inflation"
                }
            ],
            Events =
            [
                new CashflowEventDto
                {
                    Id = "e1",
                    Name = "Einmalzahlung",
                    Type = CashflowType.Income,
                    Amount = 5000m,
                    TargetDate = new DateOnly(2025, 6, 1)
                }
            ],
            EconomicFactors =
            [
                new EconomicFactorDto
                {
                    Id = "Inflation",
                    Name = "Inflation",
                    ExpectedReturn = 0.02,
                    InitialValue = 1.0
                },
                new EconomicFactorDto
                {
                    Id = factorId,
                    Name = "MSCI World",
                    ModelType = StochasticModelType.GeometricBrownianMotion,
                    ExpectedReturn = 0.06,
                    Volatility = 0.18,
                    InitialValue = 100.0
                }
            ],
            AssetClasses =
            [
                new AssetClassDto
                {
                    Id = assetClassId,
                    Name = "Aktien Welt"
                }
            ],
            TaxProfiles =
            [
                new TaxProfileDto
                {
                    Id = taxProfileId,
                    Name = "Standard",
                    CapitalGainsTaxRate = 0.26375m,
                    TaxFreeAllowance = 1000m,
                    IncomeTaxRate = 0.35m
                }
            ],
            StrategyProfiles =
            [
                new StrategyProfileDto
                {
                    Id = strategyId,
                    Name = "Aufbau"
                }
            ],
            AllocationProfiles =
            [
                new AllocationProfileDto
                {
                    Id = allocationId,
                    Name = "70/30",
                    Entries =
                    [
                        new AllocationProfileEntryDto { AssetClassId = assetClassId, TargetWeight = 0.7m }
                    ]
                }
            ],
            LifecyclePhases =
            [
                new LifecyclePhaseDto
                {
                    Id = "phase1",
                    StartAge = 0,
                    TaxProfileId = taxProfileId,
                    StrategyProfileId = strategyId,
                    AllocationProfileId = allocationId
                },
                new LifecyclePhaseDto
                {
                    Id = "phase2",
                    StartAge = 67,
                    TaxProfileId = taxProfileId,
                    StrategyProfileId = strategyId,
                    AllocationProfileId = allocationId
                }
            ],
            Portfolio = new PortfolioDto
            {
                Assets =
                [
                    new AssetDto
                    {
                        Id = "a1",
                        Name = "ETF",
                        EconomicFactorId = factorId,
                        AssetType = AssetType.Etf,
                        CurrentPrice = 100m,
                        CurrentQuantity = 50m,
                        CurrentValue = 5000m,
                        Tranches =
                        [
                            new AssetTrancheDto
                            {
                                PurchaseDate = new DateOnly(2019, 3, 1),
                                Quantity = 50m,
                                AcquisitionPricePerUnit = 100m
                            }
                        ],
                        Transactions =
                        [
                            new TransactionDto
                            {
                                Date = new DateOnly(2019, 3, 1),
                                Type = TransactionType.Buy,
                                Quantity = 50m,
                                PricePerUnit = 100m,
                                TotalAmount = 5000m
                            }
                        ]
                    }
                ]
            },
            Correlations =
            [
                new CorrelationEntryDto
                {
                    FactorIdA = "Inflation",
                    FactorIdB = factorId,
                    Correlation = 0.1
                }
            ],
            RandomSeed = 42,
            MonteCarloIterations = 1000
        };
    }
}
