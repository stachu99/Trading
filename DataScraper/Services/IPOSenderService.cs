using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using DataScraper.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Mvc;
using NLog;

namespace DataScraper.Services
{
    public static class IPOSenderService
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        public static async Task PostIPO(IEnumerable<IPODto> iPOs)
        {
            try
            {
                if (!iPOs.Any())
                {
                    _logger.Info($"No data to post. {iPOs.GetType()} count:{iPOs.Count()}");
                    return;
                }

                UriBuilder ub = new UriBuilder()
                {
                    Scheme = Startup.Configuration["TradingCore:UriScheme"],
                    Host = Startup.Configuration["TradingCore:UriHost"],
                    Port = Int16.Parse(Startup.Configuration["TradingCore:UriPort"]),
                    Path = Startup.Configuration["TradingCore:UriPath:YahooIPO"]
                };
                var servisAddress = ub.Uri;

                var client = new HttpClient
                {
                    BaseAddress = servisAddress
                };
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var jsonInString = JsonConvert.SerializeObject(iPOs);
                var response = await client.PostAsync(servisAddress, new StringContent(jsonInString, Encoding.UTF8, "application/json"));
                _logger.Info($"Post Response.StatusCode: {response.StatusCode}, {iPOs.GetType()} count:{iPOs.Count()}");
            }
            catch (Exception e)
            {
                _logger.Error(e,  $"IPO hasn't sent - {iPOs.GetType()} count:{iPOs.Count()}");
            }
        }
    }


}
