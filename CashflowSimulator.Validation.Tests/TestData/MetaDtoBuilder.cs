using CashflowSimulator.Contracts.Dtos;

namespace CashflowSimulator.Validation.Tests.TestData;

/// <summary>
/// Builder für gültige und ungültige <see cref="MetaDto"/>-Instanzen in Tests.
/// </summary>
public sealed class MetaDtoBuilder
{
    private string _scenarioName = "Test-Szenario";
    private DateTimeOffset _createdAt = DateTimeOffset.UtcNow;

    public MetaDtoBuilder WithScenarioName(string value)
    {
        _scenarioName = value ?? string.Empty;
        return this;
    }

    public MetaDtoBuilder WithCreatedAt(DateTimeOffset value)
    {
        _createdAt = value;
        return this;
    }

    public MetaDto Build() => new()
    {
        ScenarioName = _scenarioName,
        CreatedAt = _createdAt
    };
}
