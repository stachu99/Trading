using Newtonsoft.Json;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using YahooScraper.Services;

namespace YahooScraper.Services
{
    public static class IPOBackgroundService
    {
        private static IPOQueryParameters _iPOQueryParameters;
        private static IPOScraperService _iPOScraperService;
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        public static void Run()
        {
            _logger.Info($"{nameof(IPOBackgroundService)} done");
        }

        public static async Task RunAsync()
        {
            _iPOQueryParameters = new IPOQueryParameters()
            {
                Actions = Startup.Configuration["YahooIPO:BackgroudServiceQueryParameters:Actions"].Split(",").ToList<string>(),
                Exchanges = Startup.Configuration["YahooIPO:BackgroudServiceQueryParameters:Exchanges"].Split(",").ToList<string>(),
            };
            _iPOScraperService = new IPOScraperService();
            var iPOResult = _iPOScraperService.GetIPOs(_iPOQueryParameters);
            if (iPOResult == null)
            {
                _logger.Warn($"{nameof(IPOBackgroundService)} job done with {nameof(iPOResult)} = null");
                return;
            }

            if (iPOResult.Any())
            { await IPOSenderService.PostIPO(iPOResult); }
            _logger.Info($"{nameof(IPOBackgroundService)} job done, IPO count: {iPOResult.Count()}");
        }


    }
}
