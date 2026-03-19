namespace Portfolio_Financeiro.Models.DTOs
{
    public class SectorDiversificationDto
    {
        public string Sector { get; set; } = string.Empty;
        public decimal Percentage { get; set; }
        public string Risk { get; set; } = string.Empty;
    }
}
