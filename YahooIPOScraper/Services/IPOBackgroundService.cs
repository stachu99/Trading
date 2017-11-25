using Newtonsoft.Json;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using YahooIPOScraper.Services;

namespace YahooIPOScraper.Services
{
    public static class IPOBackgroundService
    {
        private static IPOQueryParameters _iPOQueryParameters;
        private static IPOScraperService _iPOScraperService;
        private static Logger logger = LogManager.GetCurrentClassLogger();
        public static void Start()
        {
            logger.Info($"{nameof(IPOBackgroundService)} done");
        }

        public static async Task StartAsync()
        {
            _iPOQueryParameters = new IPOQueryParameters()
            {
                Actions = Startup.Configuration["YahooIPOUri:ActionsParameter"].Split(",").ToList<string>(),
                Exchange = Startup.Configuration["YahooIPOUri:ExchangeParameter"].Split(",").ToList<string>(),
                Day = DateTime.Parse("2017-11-15")
            };
            _iPOScraperService = new IPOScraperService();
            var iPOResult = _iPOScraperService.GetIPOs(_iPOQueryParameters);
            if (iPOResult.Any())
            { await IPOSenderService.PostIPO(iPOResult); }
            logger.Info($"{nameof(IPOBackgroundService)} job done");
        }


    }
}
