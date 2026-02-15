using System.Threading;
using CashflowSimulator.Contracts.Dtos;
using CashflowSimulator.Contracts.Interfaces;

namespace CashflowSimulator.Desktop.Services;

/// <summary>
/// In-Memory-Implementierung des zentralen Projekt-States.
/// Thread-Safe Implementierung mittels .NET 9 System.Threading.Lock.
/// </summary>
public sealed class CurrentProjectService : ICurrentProjectService
{
    private readonly Lock _lock = new();

    private SimulationProjectDto? _current;
    private string? _currentFilePath;

    public CurrentProjectService()
    {
    }

    /// <inheritdoc />
    public SimulationProjectDto? Current => WithLock(() => _current);

    /// <inheritdoc />
    public string? CurrentFilePath => WithLock(() => _currentFilePath);

    /// <inheritdoc />
    public event EventHandler? ProjectChanged;

    /// <inheritdoc />
    public void SetCurrent(SimulationProjectDto project, string? filePath = null)
    {
        WithLock(() =>
        {
            _current = project;
            _currentFilePath = filePath;
        });
        OnProjectChanged();
    }

    /// <inheritdoc />
    public void UpdateMeta(MetaDto meta)
    {
        WithLock(() =>
        {
            if (_current is null)
                return;
            _current = _current with { Meta = meta };
        });
        OnProjectChanged();
    }

    /// <inheritdoc />
    public void UpdateUiSettings(UiSettingsDto uiSettings)
    {
        WithLock(() =>
        {
            if (_current is null)
                return;
            _current = _current with { UiSettings = uiSettings };
        });
        OnProjectChanged();
    }

    /// <inheritdoc />
    public void UpdateParameters(SimulationParametersDto parameters)
    {
        WithLock(() =>
        {
            if (_current is null)
                return;
            _current = _current with { Parameters = parameters };
        });
        OnProjectChanged();
    }

    /// <inheritdoc />
    public void UpdateStreams(IReadOnlyList<CashflowStreamDto> streams)
    {
        WithLock(() =>
        {
            if (_current is null)
                return;
            _current = _current with { Streams = streams.ToList() };
        });
        OnProjectChanged();
    }

    /// <inheritdoc />
    public void UpdateEvents(IReadOnlyList<CashflowEventDto> events)
    {
        WithLock(() =>
        {
            if (_current is null)
                return;
            _current = _current with { Events = events.ToList() };
        });
        OnProjectChanged();
    }

    /// <inheritdoc />
    public void UpdateEconomicFactors(IReadOnlyList<EconomicFactorDto> economicFactors)
    {
        WithLock(() =>
        {
            if (_current is null)
                return;
            _current = _current with { EconomicFactors = economicFactors.ToList() };
        });
        OnProjectChanged();
    }

    /// <inheritdoc />
    public void UpdateCorrelations(IReadOnlyList<CorrelationEntryDto> correlations)
    {
        WithLock(() =>
        {
            if (_current is null)
                return;
            _current = _current with { Correlations = correlations.ToList() };
        });
        OnProjectChanged();
    }

    /// <inheritdoc />
    public void UpdatePortfolio(PortfolioDto portfolio)
    {
        WithLock(() =>
        {
            if (_current is null)
                return;
            _current = _current with { Portfolio = portfolio };
        });
        OnProjectChanged();
    }

    private void WithLock(Action action)
    {
        using (_lock.EnterScope())
            action();
    }

    private T WithLock<T>(Func<T> fn)
    {
        using (_lock.EnterScope())
            return fn();
    }

    private void OnProjectChanged()
    {
        ProjectChanged?.Invoke(this, EventArgs.Empty);
    }
}
