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
using YahooIPOScraper.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Mvc;
using NLog;

namespace YahooIPOScraper.Services
{
    public static class IPOSenderService
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        public static async Task PostIPO(IEnumerable<IPODto> iPOs)
        {
            if (!iPOs.Any())
            {
                logger.Info($"No data to post. {iPOs.GetType()} count:{iPOs.Count()}");
                return;
            }
            var client = new HttpClient
            {
                BaseAddress = new Uri(Startup.Configuration["IPOSenderService:Url"])
            };
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var jsonInString = JsonConvert.SerializeObject(iPOs);
            var response = await client.PostAsync(Startup.Configuration["IPOSenderService:Url"], new StringContent(jsonInString, Encoding.UTF8, "application/json"));
            logger.Info($"Post Response.StatusCode: {response.StatusCode}, {iPOs.GetType()} count:{iPOs.Count()}");
        }
    }


}
