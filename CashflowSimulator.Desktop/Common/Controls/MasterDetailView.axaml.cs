using Avalonia;
using Avalonia.Controls;

namespace CashflowSimulator.Desktop.Common.Controls;

/// <summary>
/// Wiederverwendbarer Container für Listen-Ansichten mit Bearbeitungsmaske (Master oben, Detail unten).
/// Wird innerhalb von FeatureLayoutView als Content verwendet. Aktionen (Neu, Speichern, Löschen) liegen im Feature-Header (ActionContent).
/// </summary>
public partial class MasterDetailView : UserControl
{
    public static readonly StyledProperty<object?> MasterContentProperty =
        AvaloniaProperty.Register<MasterDetailView, object?>(nameof(MasterContent));

    public static readonly StyledProperty<object?> DetailContentProperty =
        AvaloniaProperty.Register<MasterDetailView, object?>(nameof(DetailContent));

    public object? MasterContent
    {
        get => GetValue(MasterContentProperty);
        set => SetValue(MasterContentProperty, value);
    }

    public object? DetailContent
    {
        get => GetValue(DetailContentProperty);
        set => SetValue(DetailContentProperty, value);
    }

    public MasterDetailView()
    {
        InitializeComponent();
    }
}
