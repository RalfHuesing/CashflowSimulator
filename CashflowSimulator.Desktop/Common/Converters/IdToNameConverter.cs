using System.Collections;
using System.Globalization;
using System.Reflection;
using Avalonia.Data.Converters;

namespace CashflowSimulator.Desktop.Common.Converters;

/// <summary>
/// Löst eine ID gegen eine Liste von Optionen (mit Id/Name) auf und liefert den Anzeigenamen.
/// Für MultiBinding: Werte = [id, options]. Fallback wenn nicht gefunden: "---".
/// </summary>
public class IdToNameConverter : IMultiValueConverter
{
    /// <inheritdoc />
    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        const string fallback = "---";
        if (values is null || values.Count < 2)
            return fallback;
        var id = values[0]?.ToString();
        if (string.IsNullOrEmpty(id))
            return fallback;
        if (values[1] is not IEnumerable options)
            return fallback;
        foreach (var item in options)
        {
            if (item is null)
                continue;
            var type = item.GetType();
            var idProp = type.GetProperty("Id", BindingFlags.Public | BindingFlags.Instance);
            var nameProp = type.GetProperty("Name", BindingFlags.Public | BindingFlags.Instance);
            if (idProp?.GetValue(item)?.ToString() != id)
                continue;
            var name = nameProp?.GetValue(item)?.ToString();
            return string.IsNullOrEmpty(name) ? fallback : name;
        }
        return fallback;
    }
}
