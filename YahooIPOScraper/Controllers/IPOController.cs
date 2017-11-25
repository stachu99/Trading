using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using YahooIPOScraper.Services;
using YahooIPOScraper.Models;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging;
using NLog.Web;

namespace YahooIPOScraper.IPOController
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
            _logger.LogInformation($"{nameof(iPOResult)} - {iPOResult.GetType()}, Count: {iPOResult.Count()}");
            return Ok(iPOResult);
        }
    }
}
