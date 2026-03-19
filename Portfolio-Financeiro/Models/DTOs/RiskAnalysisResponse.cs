namespace Portfolio_Financeiro.Models.DTOs
{
    public class RiskAnalysisResponse
    {
        public string OverallRisk { get; set; } = string.Empty;
        public decimal? SharpeRatio { get; set; }
        public ConcentrationRiskDto ConcentrationRisk { get; set; } = new();
        public List<SectorDiversificationDto> SectorDiversification { get; set; } = new();
        public List<string> Recommendations { get; set; } = new();
    }
}
