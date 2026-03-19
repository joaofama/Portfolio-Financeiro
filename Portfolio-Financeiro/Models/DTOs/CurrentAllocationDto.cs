namespace Portfolio_Financeiro.Models.DTOs
{
    public class CurrentAllocationDto
    {
        public string Symbol { get; set; } = string.Empty;
        public decimal CurrentWeight { get; set; }
        public decimal TargetWeight { get; set; }
        public decimal Deviation { get; set; }
    }
}
