namespace Portfolio_Financeiro.Models
{
    public class SeedDataRoot
    {
        public List<Asset> Assets { get; set; } = new();
        public List<Portfolio> Portfolios { get; set; } = new();
        public Dictionary<string, List<PricePoint>> PriceHistory { get; set; } = new();
        public MarketData MarketData { get; set; } = new();
    }
}
