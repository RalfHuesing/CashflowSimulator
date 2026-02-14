using CashflowSimulator.Desktop.Features.Settings;
using Xunit;

namespace CashflowSimulator.Desktop.Tests;

/// <summary>
/// Unit-Tests für <see cref="SettingsViewModel"/> (Konstruktor, Abhängigkeiten).
/// </summary>
public sealed class SettingsViewModelTests
{
    [Fact]
    public void SettingsViewModel_CanBeConstructed_WithCurrentProjectServiceAndHelpProvider()
    {
        // Konstruktor verwendet nur IHelpProvider (base) und setzt PageHelpKey; ICurrentProjectService wird nicht im Ctor verwendet.
        var vm = new SettingsViewModel(null!, null!);

        Assert.NotNull(vm);
        Assert.Equal("Einstellungen", vm.PageHelpKey);
    }
}
