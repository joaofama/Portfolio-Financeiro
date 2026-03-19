using Microsoft.Extensions.Logging.Abstractions;
using Portfolio_Financeiro.Data;
using Portfolio_Financeiro.Services;
using Portfolio_Financeiro.Services.Interfaces;
using Xunit;

namespace Portfolio_Financeiro.Tests.ServicesTests
{
    public class RebalancingCalculatorTest
    {
        private readonly IRebalancingCalculator _calculator;
        private readonly DataContext _context;

        public RebalancingCalculatorTest()
        {
            _context = new DataContext();
            var logger = new NullLogger<RebalancingCalculator>();
            _calculator = new RebalancingCalculator(_context, logger);
        }

        [Fact]
        public void GetRebalancing_UnbalancedPortfolio_ShouldSuggestTrades()
        {
            string userId = "user-002";

            var result = _calculator.GetRebalancing(userId);

            Assert.NotNull(result);
            Assert.True(result.NeedsRebalancing);
            Assert.NotEmpty(result.SuggestedTrades); 

            var sellTots = result.SuggestedTrades.FirstOrDefault(s => s.Symbol == "TOTS3"); 
            Assert.NotNull(sellTots);
            Assert.Equal("SELL", sellTots.Action);

            Assert.True(sellTots.Quantity > 0);
            Assert.Contains("Reduzir de", sellTots.Reason);
        }

        [Fact]
        public void GetRebalancing_UserNotFound_ShouldReturnNull()
        {
            var result = _calculator.GetRebalancing("wrong-id-123");

            Assert.Null(result);
        }

        [Fact]
        public void GetRebalancing_ShouldPopulateCurrentAllocation()
        {
            var result = _calculator.GetRebalancing("user-001");

            Assert.NotNull(result);

            Assert.NotEmpty(result.CurrentAllocation);

            Assert.All(result.CurrentAllocation, allocation => Assert.True(allocation.Deviation >= 0));
        }
    }
}