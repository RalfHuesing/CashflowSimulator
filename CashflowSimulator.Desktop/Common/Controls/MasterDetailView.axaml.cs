using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;

namespace CashflowSimulator.Desktop.Common.Controls;

/// <summary>
/// Wiederverwendbarer Container für Listen-Ansichten mit Bearbeitungsmaske (Master oben, Detail unten).
/// Wird innerhalb von FeatureLayoutView als Content verwendet. Bindet NewCommand, SaveCommand, DeleteCommand an die Button-Leiste.
/// </summary>
public partial class MasterDetailView : UserControl
{
    public static readonly StyledProperty<object?> MasterContentProperty =
        AvaloniaProperty.Register<MasterDetailView, object?>(nameof(MasterContent));

    public static readonly StyledProperty<object?> DetailContentProperty =
        AvaloniaProperty.Register<MasterDetailView, object?>(nameof(DetailContent));

    public static readonly StyledProperty<ICommand?> NewCommandProperty =
        AvaloniaProperty.Register<MasterDetailView, ICommand?>(nameof(NewCommand));

    public static readonly StyledProperty<ICommand?> SaveCommandProperty =
        AvaloniaProperty.Register<MasterDetailView, ICommand?>(nameof(SaveCommand));

    public static readonly StyledProperty<ICommand?> DeleteCommandProperty =
        AvaloniaProperty.Register<MasterDetailView, ICommand?>(nameof(DeleteCommand));

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

    public ICommand? NewCommand
    {
        get => GetValue(NewCommandProperty);
        set => SetValue(NewCommandProperty, value);
    }

    public ICommand? SaveCommand
    {
        get => GetValue(SaveCommandProperty);
        set => SetValue(SaveCommandProperty, value);
    }

    public ICommand? DeleteCommand
    {
        get => GetValue(DeleteCommandProperty);
        set => SetValue(DeleteCommandProperty, value);
    }

    public MasterDetailView()
    {
        InitializeComponent();
    }
}
