using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using DataScraper.Services;

namespace DataScraper.Controllers
{
    [Produces("application/json")]
    [Route("api/YahooFinance/StockActive")]
    public class StockActiveController : Controller
    {
        private ILogger<StockActiveController> _logger;
        public StockActiveController(ILogger<StockActiveController> logger)
        {
            _logger = logger;
        }



        private StockActiveScraperService _StockActiveScraperService;
        // GET: api/StockActive
        [HttpGet]
        public async Task<IActionResult> Get(StockActiveQueryParameters stockActiveQueryParameters)
        {

            _StockActiveScraperService = new StockActiveScraperService();

            var StockActiveResult = await _StockActiveScraperService.GetStockActives(stockActiveQueryParameters);
            if (StockActiveResult == null)
            {
                _logger.LogWarning($"{nameof(StockActiveController)} - {nameof(StockActiveResult)} = null");
                return NotFound();
            }

            switch (stockActiveQueryParameters.OrderBy)
            {
                case "Symbol":
                    StockActiveResult = StockActiveResult.OrderBy(x => x.Symbol);
                    break;
                case "Symbol desc":
                    StockActiveResult = StockActiveResult.OrderByDescending(x => x.Symbol);
                    break;
                case "Name":
                    StockActiveResult = StockActiveResult.OrderBy(x => x.Name);
                    break;
                case "Name desc":
                    StockActiveResult = StockActiveResult.OrderByDescending(x => x.Name);
                    break;
                case "ChangePercentage":
                    StockActiveResult = StockActiveResult.OrderBy(x => x.ChangePercentage);
                    break;
                case "ChangePercentage desc":
                    StockActiveResult = StockActiveResult.OrderByDescending(x => x.ChangePercentage);
                    break;
                case "Volume":
                    StockActiveResult = StockActiveResult.OrderBy(x => x.Volume);
                    break;
                case "Volume desc":
                    StockActiveResult = StockActiveResult.OrderByDescending(x => x.Volume);
                    break;
                case "AvgVol3Mth":
                    StockActiveResult = StockActiveResult.OrderBy(x => x.AvgVol3Mth);
                    break;
                case "AvgVol3Mth desc":
                    StockActiveResult = StockActiveResult.OrderByDescending(x => x.AvgVol3Mth);
                    break;
                case "MarketCap":
                    StockActiveResult = StockActiveResult.OrderBy(x => x.MarketCap);
                    break;
                case "MarketCap desc":
                    StockActiveResult = StockActiveResult.OrderByDescending(x => x.MarketCap);
                    break;
                case "PERatioTTM":
                    StockActiveResult = StockActiveResult.OrderBy(x => x.PERatioTTM);
                    break;
                case "PERatioTTM desc":
                    StockActiveResult = StockActiveResult.OrderByDescending(x => x.PERatioTTM);
                    break;
                default:
                    StockActiveResult = StockActiveResult.OrderBy(x => x.ChangePercentage);
                    break;
            }

            _logger.LogInformation($"{nameof(StockActiveResult)} - {StockActiveResult.GetType()}, Count: {StockActiveResult.Count()}");
            return Ok(StockActiveResult);

        }



    }
}
