using System;

namespace CashflowSimulator.Contracts.Dtos;

/// <summary>
/// Repräsentiert das Ergebnis einer Kursabfrage mit Kurs und Datum.
/// </summary>
public record StockPriceResultDto
{
    /// <summary>
    /// Das Symbol, für das der Kurs abgefragt wurde (z. B. "AAPL", "MSFT").
    /// </summary>
    public string Symbol { get; init; } = string.Empty;

    /// <summary>
    /// Der aktuelle Kurs des Symbols.
    /// </summary>
    public decimal Price { get; init; }

    /// <summary>
    /// Das Datum/Zeitstempel des Kurses.
    /// </summary>
    public DateTime Timestamp { get; init; }

    /// <summary>
    /// Gibt an, ob die Kursabfrage erfolgreich war.
    /// </summary>
    public bool Success { get; init; }

    /// <summary>
    /// Fehlermeldung, falls die Abfrage fehlgeschlagen ist.
    /// </summary>
    public string? ErrorMessage { get; init; }

    /// <summary>
    /// Erstellt ein erfolgreiches StockPriceResultDto.
    /// </summary>
    public static StockPriceResultDto SuccessResult(string symbol, decimal price, DateTime timestamp)
    {
        return new StockPriceResultDto
        {
            Symbol = symbol,
            Price = price,
            Timestamp = timestamp,
            Success = true
        };
    }

    /// <summary>
    /// Erstellt ein fehlgeschlagenes StockPriceResultDto.
    /// </summary>
    public static StockPriceResultDto FailureResult(string symbol, string errorMessage)
    {
        return new StockPriceResultDto
        {
            Symbol = symbol,
            Success = false,
            ErrorMessage = errorMessage
        };
    }
}