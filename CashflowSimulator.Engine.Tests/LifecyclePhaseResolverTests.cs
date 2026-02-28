using CashflowSimulator.Contracts.Dtos;
using CashflowSimulator.Engine.Services.Simulation;
using Xunit;

namespace CashflowSimulator.Engine.Tests;

public sealed class LifecyclePhaseResolverTests
{
    [Fact]
    public void GetAllocationProfileEntries_NullProject_Throws()
    {
        Assert.Throws<ArgumentNullException>(() =>
            LifecyclePhaseResolver.GetAllocationProfileEntries(null!, new DateOnly(2020, 6, 1)));
    }

    [Fact]
    public void GetAllocationProfileEntries_NoParameters_ReturnsNull()
    {
        var project = new SimulationProjectDto
        {
            Parameters = null!,
            LifecyclePhases = [new LifecyclePhaseDto { StartAge = 0, AllocationProfileId = "p1" }],
            AllocationProfiles = [new AllocationProfileDto { Id = "p1", Entries = [new AllocationProfileEntryDto { AssetClassId = "A", TargetWeight = 1m }] }]
        };

        var result = LifecyclePhaseResolver.GetAllocationProfileEntries(project, new DateOnly(2020, 6, 1));

        Assert.Null(result);
    }

    [Fact]
    public void GetAllocationProfileEntries_EmptyPhases_ReturnsNull()
    {
        var project = new SimulationProjectDto
        {
            Parameters = new SimulationParametersDto { DateOfBirth = new DateOnly(1990, 1, 1) },
            LifecyclePhases = [],
            AllocationProfiles = []
        };

        var result = LifecyclePhaseResolver.GetAllocationProfileEntries(project, new DateOnly(2020, 6, 1));

        Assert.Null(result);
    }

    [Fact]
    public void GetAllocationProfileEntries_PhaseWithoutProfileId_ReturnsNull()
    {
        var project = new SimulationProjectDto
        {
            Parameters = new SimulationParametersDto { DateOfBirth = new DateOnly(1990, 1, 1) },
            LifecyclePhases = [new LifecyclePhaseDto { StartAge = 0, AllocationProfileId = "" }],
            AllocationProfiles = []
        };

        var result = LifecyclePhaseResolver.GetAllocationProfileEntries(project, new DateOnly(2020, 6, 1));

        Assert.Null(result);
    }

    [Fact]
    public void GetAllocationProfileEntries_ValidPhase_ReturnsProfileEntries()
    {
        var profileId = "profile1";
        var entries = new List<AllocationProfileEntryDto>
        {
            new() { AssetClassId = "Aktien", TargetWeight = 0.7m },
            new() { AssetClassId = "Anleihen", TargetWeight = 0.3m }
        };
        var project = new SimulationProjectDto
        {
            Parameters = new SimulationParametersDto { DateOfBirth = new DateOnly(1990, 1, 1) },
            LifecyclePhases = [new LifecyclePhaseDto { StartAge = 0, AllocationProfileId = profileId }],
            AllocationProfiles = [new AllocationProfileDto { Id = profileId, Name = "70/30", Entries = entries }]
        };

        var result = LifecyclePhaseResolver.GetAllocationProfileEntries(project, new DateOnly(2020, 6, 1));

        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Equal("Aktien", result[0].AssetClassId);
        Assert.Equal(0.7m, result[0].TargetWeight);
        Assert.Equal("Anleihen", result[1].AssetClassId);
        Assert.Equal(0.3m, result[1].TargetWeight);
    }

    [Fact]
    public void GetAllocationProfileEntries_TwoPhases_SelectsByAge()
    {
        var youngProfileId = "young";
        var oldProfileId = "old";
        var project = new SimulationProjectDto
        {
            Parameters = new SimulationParametersDto { DateOfBirth = new DateOnly(1960, 1, 1) },
            LifecyclePhases =
            [
                new LifecyclePhaseDto { StartAge = 0, AllocationProfileId = youngProfileId },
                new LifecyclePhaseDto { StartAge = 67, AllocationProfileId = oldProfileId }
            ],
            AllocationProfiles =
            [
                new AllocationProfileDto { Id = youngProfileId, Entries = [new AllocationProfileEntryDto { AssetClassId = "A", TargetWeight = 1m }] },
                new AllocationProfileDto { Id = oldProfileId, Entries = [new AllocationProfileEntryDto { AssetClassId = "B", TargetWeight = 1m }] }
            ]
        };

        var resultYoung = LifecyclePhaseResolver.GetAllocationProfileEntries(project, new DateOnly(2020, 1, 1)); // age 60
        var resultOld = LifecyclePhaseResolver.GetAllocationProfileEntries(project, new DateOnly(2030, 1, 1));   // age 70

        Assert.NotNull(resultYoung);
        Assert.Single(resultYoung);
        Assert.Equal("A", resultYoung![0].AssetClassId);

        Assert.NotNull(resultOld);
        Assert.Single(resultOld);
        Assert.Equal("B", resultOld![0].AssetClassId);
    }

    [Fact]
    public void CalculateAgeInYears_ReturnsCorrectAge()
    {
        var dob = new DateOnly(1990, 1, 1);
        var current = new DateOnly(2020, 1, 1);

        var age = LifecyclePhaseResolver.CalculateAgeInYears(dob, current);

        Assert.Equal(30.0, age, precision: 1);
    }
}
