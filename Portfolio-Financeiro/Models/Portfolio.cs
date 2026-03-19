namespace Portfolio_Financeiro.Models
{
    public class Portfolio
    {
        public string Name { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public decimal TotalInvestment { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<Position> Positions { get; set; } = new();
    }   
}
