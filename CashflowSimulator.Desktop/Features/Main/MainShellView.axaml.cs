using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;

namespace CashflowSimulator.Desktop.Features.Main;

public partial class MainShellView : UserControl
{
    public MainShellView()
    {
        InitializeComponent();
    }

    private void StatusBarHost_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (DataContext is not MainShellViewModel vm || !vm.HasValidationMessages)
            return;
        if (sender is Control control)
            FlyoutBase.ShowAttachedFlyout(control);
    }
}
