using Portfolio_Financeiro.Models.DTOs;

namespace Portfolio_Financeiro.Services.Interfaces
{
    public interface IRiskAnalysisCalculator
    {
        RiskAnalysisResponse GetRiskAnalysis(string userId);
    }
}
