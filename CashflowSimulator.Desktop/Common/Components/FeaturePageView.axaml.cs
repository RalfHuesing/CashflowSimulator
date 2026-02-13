using Avalonia;
using Avalonia.Controls;
using Avalonia.Metadata;

namespace CashflowSimulator.Desktop.Common.Components;

/// <summary>
/// Wiederverwendbare Seite für Features: Titel, Beschreibung, beliebiger Inhalt.
/// </summary>
public partial class FeaturePageView : UserControl
{
    public static readonly StyledProperty<string?> TitleProperty =
        AvaloniaProperty.Register<FeaturePageView, string?>(nameof(Title));

    public static readonly StyledProperty<string?> DescriptionProperty =
        AvaloniaProperty.Register<FeaturePageView, string?>(nameof(Description));

    public static new readonly StyledProperty<object?> ContentProperty =
        AvaloniaProperty.Register<FeaturePageView, object?>(nameof(Content));

    public string? Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public string? Description
    {
        get => GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }

    [Content]
    public new object? Content
    {
        get => GetValue(ContentProperty);
        set => SetValue(ContentProperty, value);
    }
}
