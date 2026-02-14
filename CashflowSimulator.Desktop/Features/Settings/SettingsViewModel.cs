using CashflowSimulator.Contracts.Interfaces;
using CashflowSimulator.Desktop.ViewModels;
using CommunityToolkit.Mvvm.ComponentModel;

namespace CashflowSimulator.Desktop.Features.Settings;

/// <summary>
/// ViewModel für die Einstellungsseite (später weitere Optionen).
/// Erbt von <see cref="ValidatingViewModelBase"/> für FeatureLayout/Info-Panel; keine Validierung.
/// </summary>
public partial class SettingsViewModel : ValidatingViewModelBase
{
    /// <inheritdoc />
    protected override string HelpKeyPrefix => "Einstellungen";

    public SettingsViewModel(ICurrentProjectService currentProjectService, IHelpProvider helpProvider)
        : base(helpProvider)
    {
        PageHelpKey = "Einstellungen";
    }
}
