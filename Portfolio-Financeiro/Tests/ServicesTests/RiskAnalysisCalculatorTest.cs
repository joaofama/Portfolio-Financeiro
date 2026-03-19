using Microsoft.Extensions.Logging.Abstractions;
using Portfolio_Financeiro.Data;
using Portfolio_Financeiro.Models;
using Portfolio_Financeiro.Services;
using Portfolio_Financeiro.Services.Interfaces;
using Xunit;

namespace Portfolio_Financeiro.Tests.ServicesTests
{
    public class RiskAnalysisCalculatorTest
    {
        private readonly IRiskAnalysisCalculator _calculator;
        private readonly DataContext _context;

        public RiskAnalysisCalculatorTest()
        {
            _context = new DataContext();
            var riskLogger = new NullLogger<RiskAnalysisCalculator>();
            var perfLogger = new NullLogger<PerformanceCalculator>();
            var performanceCalculator = new PerformanceCalculator(_context, perfLogger);
            _calculator = new RiskAnalysisCalculator(_context, riskLogger, performanceCalculator);
        }

        [Fact]
        public void GetRiskAnalysis_HighConcentration_ShouldTriggerAlerts()
        {
            if (!_context.Database.Assets.Any(a => a.Symbol == "VALE3"))
            {
                _context.Database.Assets.Add(new Asset { Symbol = "VALE3", CurrentPrice = 65.20m, Sector = "Mining" });
            }

            _context.Database.Portfolios.Add(new Portfolio
            {
                UserId = "user-risk-test",
                CreatedAt = System.DateTime.UtcNow.AddDays(-10),
                Positions = new List<Position> {
                    new Position { AssetSymbol = "VALE3", Quantity = 1000, AveragePrice = 65.20m }
                }
            });

            var result = _calculator.GetRiskAnalysis("user-risk-test");

            Assert.NotNull(result);
            Assert.Equal("High", result.OverallRisk);
            Assert.True(result.ConcentrationRisk.LargestPosition.Percentage >= 80m);
            Assert.Contains(result.Recommendations, r => r.Contains("ideal < 20%"));
        }

        [Fact]
        public void GetRiskAnalysis_UserNotFound_ShouldReturnNull()
        {
            var result = _calculator.GetRiskAnalysis("invalid-user-id");

            Assert.Null(result);
        }

        [Fact]
        public void GetRiskAnalysis_SeedDataPortfolio_ShouldCalculateStructureCorrectly()
        {
            var result = _calculator.GetRiskAnalysis("user-001");

            Assert.NotNull(result);
            Assert.False(string.IsNullOrEmpty(result.OverallRisk));
            Assert.NotNull(result.ConcentrationRisk);
            Assert.NotNull(result.SectorDiversification);
        }
    }
}