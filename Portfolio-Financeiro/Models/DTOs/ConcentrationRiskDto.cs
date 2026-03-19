namespace Portfolio_Financeiro.Models.DTOs
{
    public class ConcentrationRiskDto
    {
        public LargestPositionDto LargestPosition { get; set; } = new();
        public decimal Top3Concentration { get; set; }
    }
}
