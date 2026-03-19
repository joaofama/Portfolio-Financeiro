namespace Portfolio_Financeiro.Models.DTOs
{
    public class RebalancingResponse
    {
        public bool NeedsRebalancing { get; set; }
        public List<CurrentAllocationDto> CurrentAllocation { get; set; } = new();
        public List<TradeSuggestion> SuggestedTrades { get; set; } = new();
        public decimal TotalTransactionCost { get; set; }
        public string ExpectedImprovement { get; set; } = string.Empty;
    }
}
