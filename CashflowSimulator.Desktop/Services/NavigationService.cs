using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;

namespace CashflowSimulator.Desktop.Services;

/// <summary>
/// Standard-Implementierung des <see cref="INavigationService"/>.
/// Verwendet <see cref="ActivatorUtilities"/> für flexible ViewModel-Erstellung mit DI-Unterstützung.
/// </summary>
public class NavigationService : INavigationService
{
    private readonly IServiceProvider _serviceProvider;

    public NavigationService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    /// <inheritdoc />
    public TViewModel Create<TViewModel>() where TViewModel : ObservableObject
    {
        return _serviceProvider.GetRequiredService<TViewModel>();
    }

    /// <inheritdoc />
    public TViewModel Create<TViewModel>(params object[] parameters) where TViewModel : ObservableObject
    {
        // ActivatorUtilities.CreateInstance kombiniert DI-Services mit manuellen Parametern
        return ActivatorUtilities.CreateInstance<TViewModel>(_serviceProvider, parameters);
    }
}
