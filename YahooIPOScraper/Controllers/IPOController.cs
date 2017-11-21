using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using YahooIPOScraper.Services;
using YahooIPOScraper.Models;
using Newtonsoft.Json;

namespace YahooIPOScraper.IPOController
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    public class IPOController : Controller
    {
        private IPOScraperService _iPOScraperService;
        // GET: api/IPO
        [HttpGet()]
        public IActionResult Get(IPOQueryParameters iPOQueryParameters)
        {
            _iPOScraperService = new IPOScraperService();
            var iPOResult = _iPOScraperService.GetIPOs(iPOQueryParameters);
            return Ok(iPOResult);
        }
        
    }
}
