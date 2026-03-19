using Portfolio_Financeiro.Data;
using Portfolio_Financeiro.Models.DTOs;
using Portfolio_Financeiro.Services.Interfaces;

namespace Portfolio_Financeiro.Services
{
    public class RiskAnalysisCalculator : IRiskAnalysisCalculator
    {
        private readonly DataContext _context;
        private readonly ILogger<RiskAnalysisCalculator> _logger;
        private readonly IPerformanceCalculator _performanceCalculator;

        public RiskAnalysisCalculator(
            DataContext context,
            ILogger<RiskAnalysisCalculator> logger,
            IPerformanceCalculator performanceCalculator) // <-- Injetado aqui!
        {
            _context = context;
            _logger = logger;
            _performanceCalculator = performanceCalculator;
        }

        public RiskAnalysisResponse? GetRiskAnalysis(string userId)
        {
            _logger.LogInformation("Iniciando análise de risco para o portfólio do usuário: {UserId}", userId);

            var portfolio = _context.Database.Portfolios.FirstOrDefault(p => p.UserId == userId);
            if (portfolio == null || !portfolio.Positions.Any())
            {
                _logger.LogWarning("Análise abortada. Portfólio não encontrado ou vazio para o usuário: {UserId}", userId);
                return null;
            }

            var performance = _performanceCalculator.GetPerformance(userId);
            if (performance == null) return null;

            var response = new RiskAnalysisResponse();
            decimal totalValue = performance.CurrentValue;

            if (totalValue == 0)
            {
                _logger.LogInformation("Portfólio com valor zero. Retornando risco zerado.");
                return response;
            }

            // 1. Calcular valores por Ativo e por Setor
            var assetPercentages = new List<LargestPositionDto>();
            var sectorValues = new Dictionary<string, decimal>();

            foreach (var pos in portfolio.Positions)
            {
                var assetInfo = _context.Database.Assets.FirstOrDefault(a => a.Symbol == pos.AssetSymbol);
                decimal currentPrice = assetInfo?.CurrentPrice ?? 0m;
                string sector = assetInfo?.Sector ?? "Unknown";

                decimal positionValue = pos.Quantity * currentPrice;
                decimal positionPercentage = (positionValue / totalValue) * 100m;

                assetPercentages.Add(new LargestPositionDto
                {
                    Symbol = pos.AssetSymbol,
                    Percentage = positionPercentage
                });

                if (sectorValues.ContainsKey(sector))
                    sectorValues[sector] += positionValue;
                else
                    sectorValues[sector] = positionValue;
            }

            // 2. Análise de Concentração (Concentration Risk)
            // Ordena os ativos do maior para o menor percentual
            assetPercentages = assetPercentages.OrderByDescending(a => a.Percentage).ToList();

            var largestPosition = assetPercentages.First();
            decimal top3Concentration = assetPercentages.Take(3).Sum(a => a.Percentage);

            response.ConcentrationRisk.LargestPosition = new LargestPositionDto
            {
                Symbol = largestPosition.Symbol,
                Percentage = Math.Round(largestPosition.Percentage, 2)
            };
            response.ConcentrationRisk.Top3Concentration = Math.Round(top3Concentration, 2);

            // 3. Diversificação por Setor (Sector Diversification)
            decimal maxSectorPercentage = 0m;

            foreach (var sector in sectorValues)
            {
                decimal sectorPercentage = (sector.Value / totalValue) * 100m;
                if (sectorPercentage > maxSectorPercentage) maxSectorPercentage = sectorPercentage;

                string sectorRisk = "Low";
                if (sectorPercentage > 40m) sectorRisk = "High";
                else if (sectorPercentage >= 25m) sectorRisk = "Medium";

                response.SectorDiversification.Add(new SectorDiversificationDto
                {
                    Sector = sector.Key,
                    Percentage = Math.Round(sectorPercentage, 2),
                    Risk = sectorRisk
                });
            }

            // 4. Calcular o Risco Global (Overall Risk)
            // Alto: posição > 25% OU setor > 40%
            // Médio: posição 15-25% OU setor 25-40%
            // Baixo: posição < 15% E setor < 25%
            if (largestPosition.Percentage > 25m || maxSectorPercentage > 40m)
            {
                response.OverallRisk = "High";
            }
            else if (largestPosition.Percentage >= 15m || maxSectorPercentage >= 25m)
            {
                response.OverallRisk = "Medium";
            }
            else
            {
                response.OverallRisk = "Low";
            }

            // 5. Índice de Sharpe (Sharpe Ratio)
            // Fórmula: (RetornoPortfolio - TaxaSelic) / Volatilidade
            decimal selicRate = _context.Database.MarketData.SelicRate;

            if (performance.Volatility.HasValue && performance.Volatility.Value > 0)
            {
                decimal sharpe = (performance.TotalReturn - selicRate) / performance.Volatility.Value;
                response.SharpeRatio = Math.Round(sharpe, 2);
            }

            // 6. Gerar Recomendações
            foreach (var sec in response.SectorDiversification)
            {
                if (sec.Risk == "High")
                {
                    response.Recommendations.Add($"Reduzir exposição ao setor {sec.Sector} ({sec.Percentage}%)");
                }
            }

            if (largestPosition.Percentage > 20m)
            {
                response.Recommendations.Add($"Posição {largestPosition.Symbol} representa {Math.Round(largestPosition.Percentage, 2)}% do portfólio (ideal < 20%)");
            }

            _logger.LogInformation("Análise concluída. Risco Global: {Risk}, Sharpe: {Sharpe}", response.OverallRisk, response.SharpeRatio);

            return response;
        }
    }
}