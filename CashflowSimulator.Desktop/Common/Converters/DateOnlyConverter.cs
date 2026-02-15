using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace CashflowSimulator.Desktop.Common.Converters;

/// <summary>
/// Konvertiert zwischen DateOnly? (ViewModel) und DateTime? (CalendarDatePicker).
/// </summary>
public class DateOnlyConverter : IValueConverter
{
    /// <inheritdoc />
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is DateOnly dateOnly)
            return dateOnly.ToDateTime(TimeOnly.MinValue);
        return null;
    }

    /// <inheritdoc />
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is DateTime dateTime)
            return DateOnly.FromDateTime(dateTime);
        if (value is DateTimeOffset dto)
            return DateOnly.FromDateTime(dto.DateTime);
        return null;
    }
}
