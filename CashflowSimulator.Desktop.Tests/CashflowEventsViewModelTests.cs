using CashflowSimulator.Contracts.Dtos;
using CashflowSimulator.Desktop.Features.CashflowEvents;
using CashflowSimulator.Desktop.Services;
using Xunit;

namespace CashflowSimulator.Desktop.Tests;

public sealed class CashflowEventsViewModelTests
{
    [Fact]
    public void Constructor_WithIncome_SetsTitleGeplanteEinnahmen()
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
        var vm = new CashflowEventsViewModel(projectService, null!, CashflowType.Income);

        Assert.Equal("Geplante Einnahmen", vm.Title);
    }

    [Fact]
    public void Constructor_WithExpense_SetsTitleGeplanteAusgaben()
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
        var vm = new CashflowEventsViewModel(projectService, null!, CashflowType.Expense);

        Assert.Equal("Geplante Ausgaben", vm.Title);
    }

    [Fact]
    public void Items_FilteredByType_WhenProjectHasEvents()
    {
        var projectService = new CurrentProjectService();
        var incomeEvent = new CashflowEventDto { Id = "i1", Name = "Bonus", Type = CashflowType.Income, Amount = 2000, TargetDate = DateOnly.FromDateTime(DateTime.Today.AddYears(1)) };
        var expenseEvent = new CashflowEventDto { Id = "e1", Name = "Auto", Type = CashflowType.Expense, Amount = 15000, TargetDate = DateOnly.FromDateTime(DateTime.Today.AddYears(2)) };
        projectService.SetCurrent(new SimulationProjectDto
        {
            Meta = new MetaDto { ScenarioName = "T", CreatedAt = DateTimeOffset.UtcNow },
            Parameters = new SimulationParametersDto(),
            Streams = [],
            Events = [incomeEvent, expenseEvent],
            UiSettings = new UiSettingsDto()
        });
        var vm = new CashflowEventsViewModel(projectService, null!, CashflowType.Expense);

        Assert.Single(vm.Items);
        Assert.Equal("Auto", vm.Items[0].Name);
    }
}
