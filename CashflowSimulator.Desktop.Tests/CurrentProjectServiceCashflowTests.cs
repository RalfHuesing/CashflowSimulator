using CashflowSimulator.Contracts.Dtos;
using CashflowSimulator.Desktop.Services;
using Xunit;

namespace CashflowSimulator.Desktop.Tests;

public sealed class CurrentProjectServiceCashflowTests
{
    [Fact]
    public void UpdateStreams_ReplacesStreamsInCurrentProject()
    {
        var service = new CurrentProjectService();
        var project = new SimulationProjectDto
        {
            Meta = new MetaDto { ScenarioName = "Test", CreatedAt = DateTimeOffset.UtcNow },
            Parameters = new SimulationParametersDto(),
            Streams = [new CashflowStreamDto { Id = "old", Name = "Alt", Type = CashflowType.Income, Amount = 100, StartDate = DateOnly.FromDateTime(DateTime.Today) }],
            Events = [],
            UiSettings = new UiSettingsDto()
        };
        service.SetCurrent(project);

        var newStreams = new List<CashflowStreamDto>
        {
            new() { Id = "new1", Name = "Neu1", Type = CashflowType.Income, Amount = 200, StartDate = DateOnly.FromDateTime(DateTime.Today) }
        };
        service.UpdateStreams(newStreams);

        var current = service.Current;
        Assert.NotNull(current);
        Assert.Single(current.Streams);
        Assert.Equal("Neu1", current.Streams[0].Name);
        Assert.Equal(200, current.Streams[0].Amount);
    }

    [Fact]
    public void UpdateEvents_ReplacesEventsInCurrentProject()
    {
        var service = new CurrentProjectService();
        var project = new SimulationProjectDto
        {
            Meta = new MetaDto { ScenarioName = "Test", CreatedAt = DateTimeOffset.UtcNow },
            Parameters = new SimulationParametersDto(),
            Streams = [],
            Events = [new CashflowEventDto { Id = "old", Name = "Alt", Type = CashflowType.Expense, Amount = 500, TargetDate = DateOnly.FromDateTime(DateTime.Today) }],
            UiSettings = new UiSettingsDto()
        };
        service.SetCurrent(project);

        var newEvents = new List<CashflowEventDto>
        {
            new() { Id = "new1", Name = "NeuEvent", Type = CashflowType.Expense, Amount = 1000, TargetDate = DateOnly.FromDateTime(DateTime.Today.AddYears(1)) }
        };
        service.UpdateEvents(newEvents);

        var current = service.Current;
        Assert.NotNull(current);
        Assert.Single(current.Events);
        Assert.Equal("NeuEvent", current.Events[0].Name);
        Assert.Equal(1000, current.Events[0].Amount);
    }
}
