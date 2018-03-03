using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using YahooScraper.Services;
using YahooScraper.Models;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging;
using NLog.Web;

namespace YahooScraper.IPOController
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    public class IPOController : Controller
    {
        private ILogger<IPOController> _logger;
        public IPOController(ILogger<IPOController> logger)
        {
            _logger = logger;
        }

        private IPOScraperService _iPOScraperService;
        // GET: api/IPO
        [HttpGet()]
        public IActionResult Get(IPOQueryParameters iPOQueryParameters)
        {
            _iPOScraperService = new IPOScraperService();


            var iPOResult = _iPOScraperService.GetIPOs(iPOQueryParameters);
            if (iPOResult == null)
            {
                _logger.LogWarning($"{nameof(IPOController)} - {nameof(iPOResult)} = null");
                return NotFound();
            }

            _logger.LogInformation($"{nameof(iPOResult)} - {iPOResult.GetType()}, Count: {iPOResult.Count()}");
            return Ok(iPOResult);






        }
    }
}
