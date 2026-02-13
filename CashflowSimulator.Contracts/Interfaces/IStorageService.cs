using CashflowSimulator.Contracts.Common;

namespace CashflowSimulator.Contracts.Interfaces;

/// <summary>
/// Generischer Service zum Laden und Speichern von Objekten (z. B. als JSON).
/// Implementierung in Infrastructure; Aufrufer übergeben den vollen Dateipfad (z. B. .json).
/// </summary>
public interface IStorageService<T>
{
    Task<Result<T>> LoadAsync(string path, CancellationToken cancellationToken = default);
    Task<Result> SaveAsync(string path, T data, CancellationToken cancellationToken = default);
}
