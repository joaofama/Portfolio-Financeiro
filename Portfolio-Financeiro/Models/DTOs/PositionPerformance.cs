namespace Portfolio_Financeiro.Models.DTOs
{
    public class PositionPerformance
    {
        public string Symbol { get; set; } = string.Empty;
        public decimal InvestedAmount { get; set; }
        public decimal CurrentValue { get; set; }
        public decimal Return { get; set; }
        public decimal Weight { get; set; }
    }
}
