using Portfolio_Financeiro.Models.DTOs;

namespace Portfolio_Financeiro.Services.Interfaces
{
    public interface IRebalancingCalculator
    {
        RebalancingResponse GetRebalancing(string userId);
    }
}
