using CashflowSimulator.Contracts.Dtos;

namespace CashflowSimulator.Contracts.Interfaces
{
    public interface IStockPriceService
    {
        Task<StockPriceResultDto> GetStockPriceAsync(string symbol);
    }
}
