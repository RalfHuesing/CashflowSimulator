using CashflowSimulator.Contracts.Dtos;
using CashflowSimulator.Validation;
using Xunit;

namespace CashflowSimulator.Validation.Tests;

public sealed class AssetTrancheDtoValidatorTests
{
    private static AssetDto AssetWithTranches(params AssetTrancheDto[] tranches) => new()
    {
        Id = "a1",
        Name = "ETF",
        EconomicFactorId = "f1",
        CurrentPrice = 100m,
        CurrentQuantity = tranches.Sum(t => t.Quantity),
        Tranches = tranches.ToList()
    };

    [Fact]
    public void Validate_AssetWithValidTranches_ReturnsIsValid()
    {
        var asset = AssetWithTranches(
            new AssetTrancheDto { PurchaseDate = new DateOnly(2020, 1, 1), Quantity = 5m, AcquisitionPricePerUnit = 100m });
        var result = ValidationRunner.Validate(asset);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_TrancheWithZeroQuantity_ReturnsError()
    {
        var asset = AssetWithTranches(
            new AssetTrancheDto { PurchaseDate = new DateOnly(2020, 1, 1), Quantity = 0m, AcquisitionPricePerUnit = 100m });
        var result = ValidationRunner.Validate(asset);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName.StartsWith("Tranches") && e.Message.Contains("Menge"));
    }

    [Fact]
    public void Validate_TrancheWithNegativeAcquisitionPrice_ReturnsError()
    {
        var asset = AssetWithTranches(
            new AssetTrancheDto { PurchaseDate = new DateOnly(2020, 1, 1), Quantity = 1m, AcquisitionPricePerUnit = -1m });
        var result = ValidationRunner.Validate(asset);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName.StartsWith("Tranches") && e.Message.Contains("Anschaffungspreis"));
    }
}
