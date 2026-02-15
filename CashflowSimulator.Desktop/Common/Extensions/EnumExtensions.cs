using System.ComponentModel;
using System.Reflection;

namespace CashflowSimulator.Desktop.Common.Extensions;

/// <summary>
/// Eintrag für ComboBox-Bindung: Enum-Wert + Anzeigetext (aus <see cref="DescriptionAttribute"/> oder ToString).
/// </summary>
/// <param name="Value">Der Enum-Wert (z. B. für DTO/Zurückschreiben).</param>
/// <param name="Display">Anzeigetext in der UI (lokalisiert via Description).</param>
public record EnumDisplayEntry(object? Value, string Display);

/// <summary>
/// Zentrale Enum-Lokalisierung: liefert für ein beliebiges Enum eine Liste von Value/Display-Paaren.
/// Nutzt <see cref="DescriptionAttribute"/>; Fallback auf <see cref="Enum.ToString"/>.
/// </summary>
public static class EnumExtensions
{
    /// <summary>
    /// Erzeugt eine unveränderliche Liste von <see cref="EnumDisplayEntry"/> für alle Werte des Enums <typeparamref name="T"/>.
    /// Display-Text aus <see cref="DescriptionAttribute"/>; fehlt das Attribut, wird der Enum-Member-Name verwendet.
    /// </summary>
    /// <typeparam name="T">Enum-Typ.</typeparam>
    /// <returns>Liste von (Value, Display)-Paaren in Enum-Definitionsreihenfolge.</returns>
    public static IReadOnlyList<EnumDisplayEntry> ToDisplayList<T>() where T : struct, Enum
    {
        var type = typeof(T);
        var values = Enum.GetValues<T>();
        var list = new List<EnumDisplayEntry>(values.Length);
        foreach (var value in values)
        {
            var name = value.ToString();
            var field = type.GetField(name);
            var description = field?.GetCustomAttribute<DescriptionAttribute>()?.Description;
            var display = string.IsNullOrEmpty(description) ? name : description;
            list.Add(new EnumDisplayEntry(value, display));
        }
        return list;
    }
}
