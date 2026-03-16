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

    public static readonly StyledProperty<bool> ShowSearchProperty =
        AvaloniaProperty.Register<MasterDetailView, bool>(nameof(ShowSearch));

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

    public bool ShowSearch
    {
        get => GetValue(ShowSearchProperty);
        set => SetValue(ShowSearchProperty, value);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == DataContextProperty)
        {
            ShowSearch = DataContext is ViewModels.IMasterDetailSearchable;
        }
    }

    public MasterDetailView()
    {
        InitializeComponent();
    }
}
