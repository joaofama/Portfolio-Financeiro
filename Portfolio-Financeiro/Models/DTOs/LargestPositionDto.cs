namespace Portfolio_Financeiro.Models.DTOs
{
    public class LargestPositionDto
    {
        public string Symbol { get; set; } = string.Empty;
        public decimal Percentage { get; set; }
    }
}
