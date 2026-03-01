using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;
using CashflowSimulator.Desktop.ViewModels;

namespace CashflowSimulator.Desktop.Common.Converters;

/// <summary>
/// Konvertiert <see cref="StatusType"/> (oder StatusEntry.Type) in einen Brush für Background oder Foreground.
/// Parameter: "Foreground" / "Text" für Textfarbe, sonst Background.
/// </summary>
public class StatusTypeToBrushConverter : IValueConverter
{
    private static readonly Brush BackgroundInfo = new SolidColorBrush(Color.Parse("#E3F2FD"));
    private static readonly Brush BackgroundWarning = new SolidColorBrush(Color.Parse("#FFF8E1"));
    private static readonly Brush BackgroundError = new SolidColorBrush(Color.Parse("#6B2D3A"));
    private static readonly Brush BackgroundSuccess = new SolidColorBrush(Color.Parse("#E8F5E9"));
    private static readonly Brush ForegroundInfo = new SolidColorBrush(Color.Parse("#1565C0"));
    private static readonly Brush ForegroundWarning = new SolidColorBrush(Color.Parse("#F57C00"));
    private static readonly Brush ForegroundError = new SolidColorBrush(Color.Parse("#FFFFFF"));
    private static readonly Brush ForegroundSuccess = new SolidColorBrush(Color.Parse("#2E7D32"));

    /// <inheritdoc />
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var type = value as StatusType?;
        if (value is StatusEntry entry)
            type = entry.Type;
        if (type is null)
            return null;
        var isForeground = parameter?.ToString() is "Foreground" or "Text";
        return (type.Value, isForeground) switch
        {
            (StatusType.Info, false) => BackgroundInfo,
            (StatusType.Info, true) => ForegroundInfo,
            (StatusType.Warning, false) => BackgroundWarning,
            (StatusType.Warning, true) => ForegroundWarning,
            (StatusType.Error, false) => BackgroundError,
            (StatusType.Error, true) => ForegroundError,
            (StatusType.Success, false) => BackgroundSuccess,
            (StatusType.Success, true) => ForegroundSuccess,
            _ => null
        };
    }

    /// <inheritdoc />
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException("Einweg-Bindung für Status-Darstellung.");
}
