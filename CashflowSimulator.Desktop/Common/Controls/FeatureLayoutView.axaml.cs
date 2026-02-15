using Avalonia;
using Avalonia.Controls;

namespace CashflowSimulator.Desktop.Common.Controls;

/// <summary>
/// Rahmen für Feature-Seiten: Content-Bereich links (mit Title/Description-Header), GridSplitter, InfoPanel rechts (Fehler + Hilfetext).
/// Erbt von ContentControl; das Layout kommt aus dem Style-Template in FeatureLayoutViewStyle.axaml.
/// Title und Description werden zentral im Template gerendert; Content ist nur der Body (z. B. Formular-Grid).
/// DataContext ist das Feature-ViewModel (z. B. ValidatingViewModelBase).
/// </summary>
public partial class FeatureLayoutView : ContentControl
{
    public static readonly StyledProperty<string?> TitleProperty =
        AvaloniaProperty.Register<FeatureLayoutView, string?>(nameof(Title));

    public static readonly StyledProperty<string?> DescriptionProperty =
        AvaloniaProperty.Register<FeatureLayoutView, string?>(nameof(Description));

    public static readonly StyledProperty<object?> ActionContentProperty =
        AvaloniaProperty.Register<FeatureLayoutView, object?>(nameof(ActionContent));

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

    public object? ActionContent
    {
        get => GetValue(ActionContentProperty);
        set => SetValue(ActionContentProperty, value);
    }

    public FeatureLayoutView()
    {
        InitializeComponent();
    }
}
