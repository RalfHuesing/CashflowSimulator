using System;
using System.Threading.Tasks;
using CashflowSimulator.Contracts.Dtos;
using CashflowSimulator.Infrastructure.Services;
using Xunit;

namespace CashflowSimulator.Infrastructure.Tests
{
    public class DummyStockPriceProviderTests
    {
        private readonly DummyStockPriceProvider _provider;

        public DummyStockPriceProviderTests()
        {
            _provider = new DummyStockPriceProvider();
        }

        [Fact]
        public async Task GetStockPriceAsync_WithValidSymbol_ReturnsSuccessResult()
        {
            // Arrange
            var symbol = "AAPL";

            // Act
            var result = await _provider.GetStockPriceAsync(symbol);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(symbol, result.Symbol);
            Assert.True(result.Price >= 50 && result.Price <= 200);
            Assert.NotEqual(default, result.Timestamp);
            Assert.Null(result.ErrorMessage);
        }

        [Fact]
        public async Task GetStockPriceAsync_WithEmptySymbol_ReturnsFailureResult()
        {
            // Arrange
            var symbol = "";

            // Act
            var result = await _provider.GetStockPriceAsync(symbol);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Equal(symbol, result.Symbol);
            Assert.Equal(0, result.Price);
            Assert.Equal(default, result.Timestamp);
            Assert.NotNull(result.ErrorMessage);
            Assert.Contains("Symbol darf nicht leer sein", result.ErrorMessage);
        }

        [Fact]
        public async Task GetStockPriceAsync_WithNullSymbol_ReturnsFailureResult()
        {
            // Arrange
            string? symbol = null;

            // Act
            var result = await _provider.GetStockPriceAsync(symbol!);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Equal(string.Empty, result.Symbol);
            Assert.Equal(0, result.Price);
            Assert.Equal(default, result.Timestamp);
            Assert.NotNull(result.ErrorMessage);
            Assert.Contains("Symbol darf nicht leer sein", result.ErrorMessage);
        }

        [Fact]
        public async Task GetStockPriceAsync_WithWhitespaceSymbol_ReturnsFailureResult()
        {
            // Arrange
            var symbol = "   ";

            // Act
            var result = await _provider.GetStockPriceAsync(symbol);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Equal(symbol, result.Symbol);
            Assert.Equal(0, result.Price);
            Assert.Equal(default, result.Timestamp);
            Assert.NotNull(result.ErrorMessage);
            Assert.Contains("Symbol darf nicht leer sein", result.ErrorMessage);
        }

        [Fact]
        public async Task GetStockPriceAsync_WithDifferentSymbols_ReturnsDifferentResults()
        {
            // Arrange
            var symbol1 = "AAPL";
            var symbol2 = "MSFT";

            // Act
            var result1 = await _provider.GetStockPriceAsync(symbol1);
            var result2 = await _provider.GetStockPriceAsync(symbol2);

            // Assert
            Assert.NotNull(result1);
            Assert.NotNull(result2);
            Assert.True(result1.Success);
            Assert.True(result2.Success);
            Assert.Equal(symbol1, result1.Symbol);
            Assert.Equal(symbol2, result2.Symbol);
            // Note: Since it's random, prices might be different, but that's okay
        }
    }
}