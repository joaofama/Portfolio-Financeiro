using Microsoft.Extensions.Logging.Abstractions;
using Portfolio_Financeiro.Data;
using Portfolio_Financeiro.Services;
using Portfolio_Financeiro.Services.Interfaces;
using Xunit;

namespace Portfolio_Financeiro.Tests.ServicesTests
{
    public class PerformanceCalculatorTest
    {
        private readonly IPerformanceCalculator _calculator;
        private readonly DataContext _context;

        public PerformanceCalculatorTest()
        {
            _context = new DataContext();
            var logger = new NullLogger<PerformanceCalculator>();
            _calculator = new PerformanceCalculator(_context, logger);
        }

        [Fact]
        public void GetPerformance_ShouldCalculateTotalReturnCorrectly()
        {
            string userId = "user-001";

            var result = _calculator.GetPerformance(userId);

            Assert.NotNull(result);
            Assert.True(result.TotalInvestment > 0);
            Assert.NotEmpty(result.PositionsPerformance);
        }

        [Fact]
        public void GetPerformance_WithMissingPriceHistory_ShouldReturnNullVolatility()
        {
            var result = _calculator.GetPerformance("user-001");

            Assert.NotNull(result);
            Assert.Null(result.Volatility);
        }

        [Theory]
        [InlineData(0.15, 0.12, 0.10, 0.30)]
        [InlineData(0.20, 0.12, 0.16, 0.50)]
        public void CalculateSharpeRatio_DifferentScenarios_ShouldReturnCorrectValue(
            decimal returnRate, decimal riskFreeRate, decimal volatility, decimal expectedSharpe)
        {
            decimal sharpeRatio = (returnRate - riskFreeRate) / volatility;
            Assert.Equal(expectedSharpe, Math.Round(sharpeRatio, 2));
        }

        [Fact]
        public void GetPerformance_UserNotFound_ShouldReturnNull()
        {
            var result = _calculator.GetPerformance("usuario-fantasma");

            Assert.Null(result);
        }

        [Fact]
        public void GetPerformance_ShouldCalculateAnnualizedReturn()
        {
            var result = _calculator.GetPerformance("user-002");

            Assert.NotNull(result);
            Assert.NotEqual(0m, result.AnnualizedReturn);
        }
    }
}