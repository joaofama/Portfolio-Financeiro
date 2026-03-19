using Microsoft.AspNetCore.Mvc;
using Portfolio_Financeiro.Services.Interfaces;

namespace Portfolio_Financeiro.Controllers
{
    [ApiController]
    [Route("api/portfolios")]
    public class AnalyticsController : ControllerBase
    {
        private readonly IPerformanceCalculator _performanceCalculator;
        private readonly IRebalancingCalculator _rebalancingCalculator;
        private readonly IRiskAnalysisCalculator _riskAnalysisCalculator;

        public AnalyticsController(
            IPerformanceCalculator performanceCalculator,
            IRebalancingCalculator rebalancingCalculator,
            IRiskAnalysisCalculator riskAnalysisCalculator)
        {
            _performanceCalculator = performanceCalculator;
            _rebalancingCalculator = rebalancingCalculator;
            _riskAnalysisCalculator = riskAnalysisCalculator;
        }

        [HttpGet("{id}/performance")]
        public IActionResult GetPerformance(string id)
        {
            var result = _performanceCalculator.GetPerformance(id);

            if (result == null)
            {
                return NotFound(new { Message = $"Portfólio com ID '{id}' não encontrado." });
            }

            return Ok(result);
        }

        [HttpGet("{id}/rebalancing")]
        public IActionResult GetRebalancing(string id)
        {
            var result = _rebalancingCalculator.GetRebalancing(id);

            if (result == null)
                return NotFound(new { Message = $"Portfólio com ID '{id}' não encontrado." });

            return Ok(result);
        }

        [HttpGet("{id}/risk-analysis")]
        public IActionResult GetRiskAnalysis(string id)
        {
            var result = _riskAnalysisCalculator.GetRiskAnalysis(id);

            if (result == null)
                return NotFound(new { Message = $"Portfólio com ID '{id}' não encontrado." });

            return Ok(result);
        }
    }
}