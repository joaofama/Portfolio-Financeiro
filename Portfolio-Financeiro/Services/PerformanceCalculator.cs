using Portfolio_Financeiro.Data;
using Portfolio_Financeiro.Models;
using Portfolio_Financeiro.Models.DTOs;
using Portfolio_Financeiro.Services.Interfaces;

namespace Portfolio_Financeiro.Services
{
    public class PerformanceCalculator : IPerformanceCalculator
    {
        private readonly DataContext _context;
        private readonly ILogger<PerformanceCalculator> _logger; 

        
        public PerformanceCalculator(DataContext context, ILogger<PerformanceCalculator> logger)
        {
            _context = context;
            _logger = logger;
        }

        public PerformanceResponse? GetPerformance(string userId)
        {

            _logger.LogInformation("Iniciando cálculo de performance para o portfólio do usuário: {UserId}", userId);

            // Busca o portfólio. Retorna null se não achar (para o controller dar 404)
            var portfolio = _context.Database.Portfolios.FirstOrDefault(p => p.UserId == userId);
            if (portfolio == null)
            {
                
                _logger.LogWarning("Cálculo abortado. Portfólio não encontrado para o usuário: {UserId}", userId);
                return null;
            }

            if (portfolio.Positions == null || !portfolio.Positions.Any())
            {
                _logger.LogWarning("O portfólio do usuário {UserId} não possui posições ativas.", userId);
            }

            var response = new PerformanceResponse();
            var positionsList = new List<PositionPerformance>();

            decimal totalInvested = 0m;
            decimal currentTotalValue = 0m;

            // 1. Processar Posições Individuais
            foreach (var pos in portfolio.Positions)
            {
                var assetInfo = _context.Database.Assets.FirstOrDefault(a => a.Symbol == pos.AssetSymbol);

                // Edge Case: Se o preço atual for nulo/não encontrado, assume 0
                decimal currentPrice = assetInfo?.CurrentPrice ?? 0m;

                decimal investedAmount = pos.Quantity * pos.AveragePrice;
                decimal currentValue = pos.Quantity * currentPrice;

                totalInvested += investedAmount;
                currentTotalValue += currentValue;

                // Calcula o retorno da posição (evitando divisão por zero)
                decimal posReturn = 0m;
                if (investedAmount > 0)
                {
                    posReturn = ((currentValue - investedAmount) / investedAmount) * 100m;
                }

                positionsList.Add(new PositionPerformance
                {
                    Symbol = pos.AssetSymbol,
                    InvestedAmount = Math.Round(investedAmount, 2),
                    CurrentValue = Math.Round(currentValue, 2),
                    Return = Math.Round(posReturn, 2)
                });
            }

            // 2. Calcular o Peso (Weight)
            foreach (var pos in positionsList)
            {
                pos.Weight = currentTotalValue == 0 ? 0m : Math.Round((pos.CurrentValue / currentTotalValue) * 100m, 2);
            }

            // 3. Métricas Globais (Total Return)
            decimal totalReturn = 0m;
            if (totalInvested > 0)
            {
                // Fórmula: (ValorAtual - ValorInvestido) / ValorInvestido * 100
                totalReturn = ((currentTotalValue - totalInvested) / totalInvested) * 100m;
            }

            response.TotalInvestment = Math.Round(totalInvested, 2);
            response.CurrentValue = Math.Round(currentTotalValue, 2);
            response.TotalReturnAmount = Math.Round(currentTotalValue - totalInvested, 2);
            response.TotalReturn = Math.Round(totalReturn, 2);
            response.PositionsPerformance = positionsList;

            // 4. Retorno Anualizado (Annualized Return)
            double daysInvested = (DateTime.UtcNow - portfolio.CreatedAt).TotalDays;
            if (daysInvested > 0 && totalInvested > 0)
            {
                // Fórmula: ((1 + TotalReturnDecimal)^(365/dias) - 1) * 100
                double totalReturnDecimal = (double)(currentTotalValue - totalInvested) / (double)totalInvested;
                double baseValue = 1.0 + totalReturnDecimal;
                double exponent = 365.0 / daysInvested;

                decimal annualized = (decimal)(Math.Pow(baseValue, exponent) - 1.0) * 100m;
                response.AnnualizedReturn = Math.Round(annualized, 2);
            }
            else
            {
                response.AnnualizedReturn = 0m;
            }

            // 5. Volatilidade
            response.Volatility = CalculatePortfolioVolatility(portfolio, totalInvested);

            _logger.LogInformation("Cálculo concluído com sucesso para {UserId}. Total Return: {TotalReturn}%, Volatility: {Volatility}",
                userId, response.TotalReturn, response.Volatility);

            return response;
        }

        // ==========================================
        // MÉTODOS PRIVADOS (Lógica e Cálculos)
        // ==========================================

        private decimal? CalculatePortfolioVolatility(Portfolio portfolio, decimal totalInvested)
        {
            if (portfolio.Positions == null || !portfolio.Positions.Any() || totalInvested == 0)
                return null;

            // Edge Case: Verifica se todos os ativos da carteira têm histórico de preço
            bool hasCompleteHistory = portfolio.Positions.All(p =>
                _context.Database.PriceHistory.ContainsKey(p.AssetSymbol) &&
                _context.Database.PriceHistory[p.AssetSymbol].Any());

            if (!hasCompleteHistory)
            {
                _logger.LogWarning("Não há histórico de preços suficiente para calcular a volatilidade.");
                return null;
            }

            // Usa as datas do primeiro ativo como base cronológica
            var firstAssetSymbol = portfolio.Positions.First().AssetSymbol;
            var historyDates = _context.Database.PriceHistory[firstAssetSymbol].Select(h => h.Date.Date).ToList();

            var dailyPortfolioValues = new List<decimal>();

            // Calcula o valor total do portfólio dia após dia
            foreach (var date in historyDates)
            {
                decimal dailyValue = 0m;
                foreach (var pos in portfolio.Positions)
                {
                    var assetHistory = _context.Database.PriceHistory[pos.AssetSymbol];
                    var historyPoint = assetHistory.FirstOrDefault(h => h.Date.Date == date);

                    // Se o ativo não tiver preço naquele dia específico, assume o preço médio de compra
                    decimal priceOnDate = historyPoint?.Price ?? pos.AveragePrice;
                    dailyValue += pos.Quantity * priceOnDate;
                }
                dailyPortfolioValues.Add(dailyValue);
            }

            // Calcula a variação diária (retornos diários)
            var dailyReturns = new List<decimal>();
            for (int i = 1; i < dailyPortfolioValues.Count; i++)
            {
                decimal previous = dailyPortfolioValues[i - 1];
                decimal current = dailyPortfolioValues[i];

                if (previous > 0)
                {
                    dailyReturns.Add((current - previous) / previous);
                }
            }

            if (dailyReturns.Count < 2) return null;

            // Desvio Padrão
            decimal avg = dailyReturns.Average();
            decimal sumOfSquares = dailyReturns.Sum(v => (v - avg) * (v - avg));
            decimal dailyStdDev = (decimal)Math.Sqrt((double)(sumOfSquares / (dailyReturns.Count - 1)));

            // Volatilidade Anualizada (Desvio Padrão * Raiz de 252 dias úteis)
            decimal annualizedVolatility = dailyStdDev * (decimal)Math.Sqrt(252) * 100m;

            return Math.Round(annualizedVolatility, 2);
        }
    }
}