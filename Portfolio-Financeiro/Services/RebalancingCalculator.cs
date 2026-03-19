using Portfolio_Financeiro.Data;
using Portfolio_Financeiro.Models.DTOs;
using Portfolio_Financeiro.Services.Interfaces;

namespace Portfolio_Financeiro.Services
{
    public class RebalancingCalculator : IRebalancingCalculator
    {
        private readonly DataContext _context;
        private readonly ILogger<RebalancingCalculator> _logger;

        public RebalancingCalculator(DataContext context, ILogger<RebalancingCalculator> logger)
        {
            _context = context;
            _logger = logger;
        }

        public RebalancingResponse? GetRebalancing(string userId)
        {
            _logger.LogInformation("Iniciando cálculo de rebalanceamento para o portfólio do usuário: {UserId}", userId);

            var portfolio = _context.Database.Portfolios.FirstOrDefault(p => p.UserId == userId);
            if (portfolio == null)
            {
                _logger.LogWarning("Rebalanceamento abortado. Portfólio não encontrado para o usuário: {UserId}", userId);
                return null;
            }

            var response = new RebalancingResponse();
            decimal totalValue = 0m;
            var assetPrices = new Dictionary<string, decimal>();

            // 1. Calcula o Valor Total do Portfólio
            foreach (var pos in portfolio.Positions)
            {
                var assetInfo = _context.Database.Assets.FirstOrDefault(a => a.Symbol == pos.AssetSymbol);
                decimal currentPrice = assetInfo?.CurrentPrice ?? 0m;

                assetPrices[pos.AssetSymbol] = currentPrice;
                totalValue += pos.Quantity * currentPrice;
            }

            if (totalValue == 0)
            {
                _logger.LogInformation("O portfólio {UserId} possui valor zero. Nenhum rebalanceamento necessário.", userId);
                return response;
            }

            decimal totalDeviationCorrected = 0m;

            // 2. Analisar cada posição para encontrar Desvios
            foreach (var pos in portfolio.Positions)
            {
                decimal currentPrice = assetPrices[pos.AssetSymbol];
                if (currentPrice == 0) continue; 

                decimal currentValue = pos.Quantity * currentPrice;
                decimal currentWeightPercent = (currentValue / totalValue) * 100m;
                decimal targetWeightPercent = pos.TargetAllocation * 100m;
                decimal deviationPercent = Math.Abs(currentWeightPercent - targetWeightPercent);

                // Preenche o quadro de alocação atual
                response.CurrentAllocation.Add(new CurrentAllocationDto
                {
                    Symbol = pos.AssetSymbol,
                    CurrentWeight = Math.Round(currentWeightPercent, 2),
                    TargetWeight = Math.Round(targetWeightPercent, 2),
                    Deviation = Math.Round(deviationPercent, 2)
                });

                // Regra: Identificar desvios > 2%
                if (deviationPercent > 2.0m)
                {
                    decimal targetValue = totalValue * pos.TargetAllocation;
                    decimal differenceValue = targetValue - currentValue; // Positivo = BUY, Negativo = SELL
                    decimal absoluteDifference = Math.Abs(differenceValue);

                    // Descobre a QUANTIDADE inteira de ativos a comprar/vender
                    int quantity = (int)Math.Floor(absoluteDifference / currentPrice);
                    decimal actualTradeValue = quantity * currentPrice;

                    // Regra: Não sugerir trades < R$ 100,00
                    if (actualTradeValue >= 100m)
                    {
                        // Regra: Custo de transação de 0.3% por operação
                        decimal transactionCost = actualTradeValue * 0.003m;
                        string action = differenceValue > 0 ? "BUY" : "SELL";

                        string reason = action == "BUY"
                            ? $"Aumentar de {Math.Round(currentWeightPercent, 1)}% para {Math.Round(targetWeightPercent, 1)}%"
                            : $"Reduzir de {Math.Round(currentWeightPercent, 1)}% para {Math.Round(targetWeightPercent, 1)}%";

                        response.SuggestedTrades.Add(new TradeSuggestion
                        {
                            Symbol = pos.AssetSymbol,
                            Action = action,
                            Quantity = quantity,
                            EstimatedValue = Math.Round(actualTradeValue, 2),
                            TransactionCost = Math.Round(transactionCost, 2),
                            Reason = reason
                        });

                        response.TotalTransactionCost += transactionCost;
                        totalDeviationCorrected += deviationPercent;
                    }
                }
            }

            response.NeedsRebalancing = response.SuggestedTrades.Any();
            response.TotalTransactionCost = Math.Round(response.TotalTransactionCost, 2);

            if (response.NeedsRebalancing)
            {
                response.SuggestedTrades = response.SuggestedTrades
                    .OrderByDescending(t => response.CurrentAllocation.First(c => c.Symbol == t.Symbol).Deviation)
                    .ToList();

                response.ExpectedImprovement = $"Redução de {Math.Round(totalDeviationCorrected, 1)}% no risco de desvio da carteira";
            }

            _logger.LogInformation("Rebalanceamento concluído para {UserId}. Trades: {Count}. Custo: R$ {Costs}",
                userId, response.SuggestedTrades.Count, response.TotalTransactionCost);

            return response;
        }
    }
}