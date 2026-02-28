using System;
using System.Threading.Tasks;
using CashflowSimulator.Contracts.Dtos;
using CashflowSimulator.Contracts.Interfaces;

namespace CashflowSimulator.Infrastructure.Services
{
    /// <summary>
    /// Dummy-Implementierung des IStockPriceService für Test- und Entwicklungszwecke.
    /// Gibt feste Kurswerte zurück, um die Integration zu ermöglichen, ohne auf externe APIs angewiesen zu sein.
    /// </summary>
    public class DummyStockPriceProvider : IStockPriceService
    {
        private readonly Random _random = new();

        /// <summary>
        /// Simuliert eine Kursabfrage für ein gegebenes Symbol.
        /// Gibt einen zufälligen Kurs zwischen 50 und 200 zurück mit aktuellem Zeitstempel.
        /// </summary>
        /// <param name="symbol">Das Symbol, für das der Kurs abgefragt werden soll.</param>
        /// <returns>Ein StockPriceResultDto mit simuliertem Kurs und aktuellem Zeitstempel.</returns>
        public Task<StockPriceResultDto> GetStockPriceAsync(string symbol)
        {
            if (string.IsNullOrWhiteSpace(symbol))
            {
                return Task.FromResult(StockPriceResultDto.FailureResult(
                    symbol ?? string.Empty, 
                    "Symbol darf nicht leer sein."
                ));
            }

            // Simuliere einen zufälligen Kurs zwischen 50 und 200
            var price = Math.Round(50 + (_random.NextDouble() * 150), 2);
            var timestamp = DateTime.Now;

            return Task.FromResult(StockPriceResultDto.SuccessResult(symbol, (decimal)price, timestamp));
        }
    }
}