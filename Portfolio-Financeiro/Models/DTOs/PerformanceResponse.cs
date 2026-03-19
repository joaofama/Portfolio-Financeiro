namespace Portfolio_Financeiro.Models.DTOs
{
    public class PerformanceResponse
    {
        public decimal TotalInvestment { get; set; }
        public decimal CurrentValue { get; set; }
        public decimal TotalReturn { get; set; }
        public decimal TotalReturnAmount { get; set; }
        public decimal AnnualizedReturn { get; set; }
        public decimal? Volatility { get; set; }
        public List<PositionPerformance> PositionsPerformance { get; set; } = new();
    }
}
