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
    // .NET 9: Dedizierter Lock-Typ (besser als 'object')
    // Ermöglicht effizienteres Locking und Scope-basierten Zugriff.
    private readonly Lock _lock = new();

    private SimulationProjectDto? _current;
    private string? _currentFilePath;

    /// <inheritdoc />
    public SimulationProjectDto? Current
    {
        get
        {
            // Kurzer Read-Lock für Konsistenz (auf 64-bit zwar meist atomar, aber sicher ist sicher)
            using (_lock.EnterScope())
            {
                return _current;
            }
        }
    }

    /// <inheritdoc />
    public string? CurrentFilePath
    {
        get
        {
            using (_lock.EnterScope())
            {
                return _currentFilePath;
            }
        }
    }

    /// <inheritdoc />
    public event EventHandler? ProjectChanged;

    /// <inheritdoc />
    public void SetCurrent(SimulationProjectDto project, string? filePath = null)
    {
        // Mutationen atomar kapseln
        using (_lock.EnterScope())
        {
            _current = project;
            _currentFilePath = filePath;
        }

        // Event außerhalb des Locks feuern? 
        // Hier: Innerhalb, um sicherzustellen, dass Subscriber den NEUEN State lesen,
        // bevor ein anderer Thread wieder schreibt.
        OnProjectChanged();
    }

    /// <inheritdoc />
    public void UpdateMeta(MetaDto meta)
    {
        using (_lock.EnterScope())
        {
            if (_current is null)
            {
                return;
            }

            // Records sind immuntable -> "with" erzeugt saubere Kopie
            _current = _current with { Meta = meta };
        }

        OnProjectChanged();
    }

    private void OnProjectChanged()
    {
        // Null-Conditional Invoke ist Thread-Safe für den Delegate selbst
        ProjectChanged?.Invoke(this, EventArgs.Empty);
    }
}