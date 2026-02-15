using CashflowSimulator.Contracts.Dtos;
using CashflowSimulator.Desktop.Features.CashflowStreams;
using CashflowSimulator.Desktop.Services;
using Xunit;

namespace CashflowSimulator.Desktop.Tests;

public sealed class CashflowStreamsViewModelTests
{
    private static SimulationProjectDto ProjectWithParameters() => new()
    {
        Meta = new MetaDto { ScenarioName = "T", CreatedAt = DateTimeOffset.UtcNow },
        Parameters = new SimulationParametersDto
        {
            SimulationStart = DateOnly.FromDateTime(DateTime.Today),
            SimulationEnd = DateOnly.FromDateTime(DateTime.Today.AddYears(30)),
            DateOfBirth = DateOnly.FromDateTime(DateTime.Today.AddYears(-40)),
            RetirementDate = DateOnly.FromDateTime(DateTime.Today.AddYears(27)),
            InitialLiquidCash = 0,
            CurrencyCode = "EUR"
        },
        Streams = [],
        Events = [],
        UiSettings = new UiSettingsDto()
    };
    [Fact]
    public void Constructor_WithIncome_SetsTitleLaufendeEinnahmen()
    {
        var projectService = new CurrentProjectService();
        projectService.SetCurrent(ProjectWithParameters());
        var vm = new CashflowStreamsViewModel(projectService, null!, CashflowType.Income);

        Assert.Equal("Laufende Einnahmen", vm.Title);
    }

    [Fact]
    public void Constructor_WithExpense_SetsTitleLaufendeAusgaben()
    {
        var projectService = new CurrentProjectService();
        projectService.SetCurrent(ProjectWithParameters());
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

    [Fact]
    public void Save_WhenInvalid_SetsValidationErrorsAndDoesNotPersist()
    {
        var projectService = new CurrentProjectService();
        projectService.SetCurrent(ProjectWithParameters());
        var vm = new CashflowStreamsViewModel(projectService, null!, CashflowType.Income);
        vm.Name = "";
        vm.StartDate = null;

        vm.SaveCommand.Execute(null);

        Assert.True(vm.HasErrors);
        Assert.Empty(vm.Items);
    }

    [Fact]
    public void Save_WhenValid_PersistsNewStream()
    {
        var projectService = new CurrentProjectService();
        projectService.SetCurrent(ProjectWithParameters());
        var vm = new CashflowStreamsViewModel(projectService, null!, CashflowType.Income);
        vm.Name = "Gehalt";
        vm.Amount = 2000;
        vm.StartDate = DateOnly.FromDateTime(DateTime.Today);
        vm.Interval = "Monthly";

        vm.SaveCommand.Execute(null);

        Assert.False(vm.HasErrors);
        Assert.Single(vm.Items);
        Assert.Equal("Gehalt", vm.Items[0].Name);
        Assert.Equal(2000, vm.Items[0].Amount);
    }

    [Fact]
    public void New_ClearsValidationErrors()
    {
        var projectService = new CurrentProjectService();
        projectService.SetCurrent(ProjectWithParameters());
        var vm = new CashflowStreamsViewModel(projectService, null!, CashflowType.Income);
        vm.Name = "";
        vm.StartDate = null;
        vm.SaveCommand.Execute(null);
        Assert.True(vm.HasErrors);

        vm.NewCommand.Execute(null);

        Assert.False(vm.HasErrors);
    }
}
