using CashflowSimulator.Contracts.Dtos;
using CashflowSimulator.Contracts.Interfaces;

namespace CashflowSimulator.Desktop.Services;

/// <summary>
/// In-Memory-Implementierung des zentralen Projekt-States.
/// </summary>
public sealed class CurrentProjectService : ICurrentProjectService
{
    private SimulationProjectDto? _current;
    private string? _currentFilePath;

    /// <inheritdoc />
    public SimulationProjectDto? Current => _current;

    /// <inheritdoc />
    public string? CurrentFilePath => _currentFilePath;

    /// <inheritdoc />
    public event EventHandler? ProjectChanged;

    /// <inheritdoc />
    public void SetCurrent(SimulationProjectDto project, string? filePath = null)
    {
        _current = project;
        _currentFilePath = filePath;
        ProjectChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <inheritdoc />
    public void UpdateMeta(MetaDto meta)
    {
        if (_current is null)
            return;
        _current = _current with { Meta = meta };
        ProjectChanged?.Invoke(this, EventArgs.Empty);
    }
}
