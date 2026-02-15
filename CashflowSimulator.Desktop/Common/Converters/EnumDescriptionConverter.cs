using System;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using Avalonia.Data.Converters;

namespace CashflowSimulator.Desktop.Common.Converters;

/// <summary>
/// Konvertiert einen Enum-Wert in den Anzeigetext aus <see cref="DescriptionAttribute"/> (für DataGrid/UI).
/// </summary>
public class EnumDescriptionConverter : IValueConverter
{
    /// <inheritdoc />
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is null)
            return null;
        var type = value.GetType();
        if (!type.IsEnum)
            return value.ToString();
        var name = value.ToString();
        if (string.IsNullOrEmpty(name))
            return null;
        var field = type.GetField(name);
        var description = field?.GetCustomAttribute<DescriptionAttribute>()?.Description;
        return string.IsNullOrEmpty(description) ? name : description;
    }

    /// <inheritdoc />
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException("Einweg-Bindung für Anzeige.");
}
