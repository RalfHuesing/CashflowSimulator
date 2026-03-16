using CommunityToolkit.Mvvm.ComponentModel;

namespace CashflowSimulator.Desktop.Services;

/// <summary>
/// Service zur Auflösung von ViewModels aus dem DI-Container.
/// Ersetzt direkte Factory-Abhängigkeiten in der MainShell.
/// </summary>
public interface INavigationService
{
    /// <summary>
    /// Erstellt ein ViewModel ohne zusätzliche Parameter.
    /// </summary>
    /// <typeparam name="TViewModel">Typ des ViewModels (muss von ObservableObject erben).</typeparam>
    /// <returns>Eine neue Instanz des ViewModels.</returns>
    TViewModel Create<TViewModel>() where TViewModel : ObservableObject;

    /// <summary>
    /// Erstellt ein ViewModel mit zusätzlichen Parametern.
    /// Dependencies werden aus dem DI-Container aufgelöst, während die angegebenen Parameter
    /// verwendet werden, um fehlende Konstruktor-Parameter zu erfüllen.
    /// </summary>
    /// <typeparam name="TViewModel">Typ des ViewModels (muss von ObservableObject erben).</typeparam>
    /// <param name="parameters">Zusätzliche Parameter für den Konstruktor.</param>
    /// <returns>Eine neue Instanz des ViewModels.</returns>
    TViewModel Create<TViewModel>(params object[] parameters) where TViewModel : ObservableObject;

    /// <summary>
    /// Erstellt ein ViewModel zur Laufzeit anhand des Typs.
    /// </summary>
    ObservableObject Create(Type viewModelType);

    /// <summary>
    /// Erstellt ein ViewModel zur Laufzeit anhand des Typs und zusätzlicher Parameter.
    /// </summary>
    ObservableObject Create(Type viewModelType, params object[] parameters);
}
