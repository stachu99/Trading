using HtmlAgilityPack;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataScraper.Models;

namespace DataScraper.Services
{
    public class IPOScraperService
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        string _uriHost = Startup.Configuration["YahooFinance:UriHost"];

        public IEnumerable<IPODto> GetIPOs(IPOQueryParameters iPOQueryParameters)
        {
            IEnumerable<IPODto> allIPOs = ScrapIPOs(SetUrl(iPOQueryParameters.Day));

            //_allIPOs = (from p in _allIPOs where iPOQueryParameters.Actions.Contains(p.Actions) select p);
            if(allIPOs == null)
            {
                return null;
            }
            allIPOs = allIPOs.Select(x => x)
                .Where(x => (iPOQueryParameters.Actions != null) ? iPOQueryParameters.Actions.Contains(x.Actions) : true)
                .Where(x => (iPOQueryParameters.Exchanges != null) ? iPOQueryParameters.Exchanges.Contains(x.Exchange) : true);

            return allIPOs;
        }


        private Uri SetUrl(DateTime date)
        {
            UriBuilder ub = new UriBuilder
            {
                Scheme = Startup.Configuration["YahooFinance:UriScheme"],
                Host = _uriHost,
                Port = Int16.Parse(Startup.Configuration["YahooFinance:UriPort"]),
                Path = Startup.Configuration["YahooFinance:YahooIPO:UriPath"],
                Query = Startup.Configuration["YahooFinance:YahooIPO:UriQuery"] + date.ToString("yyyy-MM-dd")
            };
            return ub.Uri;
        }

        private IEnumerable<IPODto> ScrapIPOs(Uri uri)
        {
            List<IPODto> iPOList;
            try
            {
                var web1 = new HtmlWeb();
                var doc1 = web1.Load(uri);
                var nodes = doc1.DocumentNode.SelectNodes("//*[@id=\"fin-cal-table\"]/div[1]/div/table/tbody/tr");
                iPOList = new List<IPODto>();
                if (nodes==null)
                    return iPOList;
                IPODto iPO;
                foreach (var node in nodes)
                {
                    var nodeUriPath = node.ChildNodes.ElementAt(0).FirstChild.GetAttributeValue("href", "");
                    string nodeUri = "";
                    if (nodeUriPath != "")
                    {
                        nodeUri = _uriHost + nodeUriPath;
                    }
                    iPO = new IPODto
                    {
                        Symbol = node.ChildNodes.ElementAt(0).FirstChild.GetAttributeValue("data-symbol", ""),
                        Url = nodeUri,
                        Company = node.ChildNodes.ElementAt(1).InnerHtml,
                        Exchange = node.ChildNodes.ElementAt(2).InnerHtml,
                        Date = node.ChildNodes.ElementAt(3).FirstChild.InnerHtml,
                        PriceRang = node.ChildNodes.ElementAt(4).InnerHtml,
                        Price = node.ChildNodes.ElementAt(5).InnerHtml,
                        Currency = node.ChildNodes.ElementAt(6).InnerHtml,
                        Shares = node.ChildNodes.ElementAt(7).InnerHtml,
                        Actions = node.ChildNodes.ElementAt(8).InnerHtml
                    };

                    iPOList.Add(iPO);
                }
            }
            catch (Exception e)
            {
                _logger.Error(e, "Error");
                iPOList = null;
            }

            return iPOList;



        }


    }
}
