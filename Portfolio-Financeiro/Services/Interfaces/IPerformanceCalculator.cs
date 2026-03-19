using Portfolio_Financeiro.Models.DTOs;

namespace Portfolio_Financeiro.Services.Interfaces
{
    public interface IPerformanceCalculator
    {
        PerformanceResponse GetPerformance(string userId);
    }
}
