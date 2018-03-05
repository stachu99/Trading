using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using YahooScraper.Services;

namespace YahooScraper.Controllers
{
    [Produces("application/json")]
    [Route("api/StockActive")]
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

            _logger.LogInformation($"{nameof(StockActiveResult)} - {StockActiveResult.GetType()}, Count: {StockActiveResult.Count()}");
            return Ok(StockActiveResult);

        }



    }
}
