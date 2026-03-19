namespace Portfolio_Financeiro.Models.DTOs
{
    public class TradeSuggestion
    {
        public string Symbol { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty; // "BUY" ou "SELL"
        public int Quantity { get; set; }
        public decimal EstimatedValue { get; set; }
        public decimal TransactionCost { get; set; }
        public string Reason { get; set; } = string.Empty;
    }
}
