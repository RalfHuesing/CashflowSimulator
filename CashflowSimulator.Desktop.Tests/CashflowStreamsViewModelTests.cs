using CashflowSimulator.Contracts.Dtos;
using CashflowSimulator.Desktop.Features.CashflowStreams;
using CashflowSimulator.Desktop.Services;
using Xunit;

namespace CashflowSimulator.Desktop.Tests;

public sealed class CashflowStreamsViewModelTests
{
    [Fact]
    public void Constructor_WithIncome_SetsTitleLaufendeEinnahmen()
    {
        var projectService = new CurrentProjectService();
        projectService.SetCurrent(new SimulationProjectDto
        {
            Meta = new MetaDto { ScenarioName = "T", CreatedAt = DateTimeOffset.UtcNow },
            Parameters = new SimulationParametersDto(),
            Streams = [],
            Events = [],
            UiSettings = new UiSettingsDto()
        });
        var vm = new CashflowStreamsViewModel(projectService, null!, CashflowType.Income);

        Assert.Equal("Laufende Einnahmen", vm.Title);
    }

    [Fact]
    public void Constructor_WithExpense_SetsTitleLaufendeAusgaben()
    {
        var projectService = new CurrentProjectService();
        projectService.SetCurrent(new SimulationProjectDto
        {
            Meta = new MetaDto { ScenarioName = "T", CreatedAt = DateTimeOffset.UtcNow },
            Parameters = new SimulationParametersDto(),
            Streams = [],
            Events = [],
            UiSettings = new UiSettingsDto()
        });
        var vm = new CashflowStreamsViewModel(projectService, null!, CashflowType.Expense);

        Assert.Equal("Laufende Ausgaben", vm.Title);
    }

    [Fact]
    public void Items_FilteredByType_WhenProjectHasStreams()
    {
        var projectService = new CurrentProjectService();
        var incomeStream = new CashflowStreamDto { Id = "i1", Name = "Gehalt", Type = CashflowType.Income, Amount = 1000, StartDate = DateOnly.FromDateTime(DateTime.Today) };
        var expenseStream = new CashflowStreamDto { Id = "e1", Name = "Miete", Type = CashflowType.Expense, Amount = 500, StartDate = DateOnly.FromDateTime(DateTime.Today) };
        projectService.SetCurrent(new SimulationProjectDto
        {
            Meta = new MetaDto { ScenarioName = "T", CreatedAt = DateTimeOffset.UtcNow },
            Parameters = new SimulationParametersDto(),
            Streams = [incomeStream, expenseStream],
            Events = [],
            UiSettings = new UiSettingsDto()
        });
        var vm = new CashflowStreamsViewModel(projectService, null!, CashflowType.Income);

        Assert.Single(vm.Items);
        Assert.Equal("Gehalt", vm.Items[0].Name);
    }
}
