using HtmlAgilityPack;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic;
using System.Threading.Tasks;
using YahooScraper.Models;

namespace YahooScraper.Services
{
    public class StockActiveScraperService
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        string _uriHost = Startup.Configuration["YahooStockActive:UriHost"];

        public async Task<IEnumerable<StockActiveDto>> GetStockActives(StockActiveQueryParameters stockActiveQueryParameters)
        {

            IEnumerable<StockActiveDto> allStockActives = await ScrapStockActives(stockActiveQueryParameters.Countries);

            //_allIPOs = (from p in _allIPOs where iPOQueryParameters.Actions.Contains(p.Actions) select p);
            if (allStockActives == null)
            {
                return null;
            }
            //allStockActives = allStockActives.Select(x => x)
            //    .Where(x => (stockActiveQueryParameters.Actions != null) ? stockActiveQueryParameters.Actions.Contains(x.Actions) : true)
            //    .Where(x => (stockActiveQueryParameters.Exchange != null) ? stockActiveQueryParameters.Exchange.Contains(x.Exchange) : true);

            return allStockActives;
        }

        private Uri SetUri(string country)
        {
            string _uriScheme = Startup.Configuration["YahooStockActive:UriScheme"];
            int _uriPort = Int16.Parse(Startup.Configuration["YahooStockActive:UriPort"]);
            string _uriSubDomainCountry = Startup.Configuration[$"YahooStockActive:UriSubDomainCountry:{country}"];
            string _uriHost = String.IsNullOrWhiteSpace(_uriSubDomainCountry) ? Startup.Configuration["YahooStockActive:UriHost"] : $"{_uriSubDomainCountry}.{Startup.Configuration["YahooStockActive:UriHost"]}";
            string _uriPath = Startup.Configuration["YahooStockActive:UriPath"];
            UriBuilder uriBuilder = new UriBuilder(_uriScheme, _uriHost, _uriPort, _uriPath);
            return uriBuilder.Uri;
        }

        
        public async Task<IEnumerable<StockActiveDto>> ScrapStockActives(List<string> countries)
        {
            List<StockActiveDto> StockActiveList;
            try
            {
                StockActiveList = new List<StockActiveDto>();
                foreach (var c in countries)
                {
                    Uri uri = SetUri(c);
                    int pageCount = 100;
                    int pageOffset = 0;
                    int matchingResults = 0;
                    int lastRecord = 0;
                    HtmlNodeCollection nodes = new HtmlNodeCollection(null);
                    do
                    {
                        var web1 = new HtmlWeb();
                        var doc1 = await web1.LoadFromWebAsync(uri.ToString() + $"?offset={pageOffset}&count={pageCount}");
                        var _nodes = doc1.DocumentNode.SelectNodes("//*[@id=\"scr-res-table\"]/table/tbody/tr");
                        if (_nodes == null)
                        {
                            break;
                        }

                        foreach (var node in _nodes)
                        {
                            nodes.Add(node);
                        }
                        var mr = doc1.DocumentNode.SelectSingleNode("//*[@id=\"fin-scr-res-table\"]/div[1]/div[1]/span[2]/span").InnerText.Split(' ')[2];
                        matchingResults = Int16.Parse(mr);
                        var lrt = doc1.DocumentNode.SelectSingleNode("//*[@id=\"fin-scr-res-table\"]/div[1]/div[1]/span[2]/span").InnerText.Split(' ')[0];
                        var lr = doc1.DocumentNode.SelectSingleNode("//*[@id=\"fin-scr-res-table\"]/div[1]/div[1]/span[2]/span").InnerText.Split(' ')[0].Split(new char[]{(char)45, (char)8211})[0];
                        lastRecord = Int16.Parse(lr);
                        pageOffset += pageCount;
                    } while (lastRecord < matchingResults);


                    if (nodes == null)
                        return StockActiveList;
                    StockActiveDto stockActiveDto;
                    int i = 0;
                    foreach (var node in nodes)
                    {
                        var nodeUriPath = node.ChildNodes.ElementAt(1).FirstChild.GetAttributeValue("href", "");
                        string nodeUri = "";
                        if (nodeUriPath != "")
                        {
                            nodeUri = _uriHost + nodeUriPath;
                        }
                        stockActiveDto = new StockActiveDto
                        {
                            Symbol = node.ChildNodes.ElementAt(1).FirstChild.InnerText,
                            Url = nodeUri,
                            Name = node.ChildNodes.ElementAt(2).InnerText,
                            Price = StrToDecParse(node.ChildNodes.ElementAt(3).FirstChild.InnerText),
                            Change = StrToDecParse(node.ChildNodes.ElementAt(4).FirstChild.InnerText),
                            ChangePercentage = StrToDecParse(node.ChildNodes.ElementAt(5).FirstChild.InnerText.Replace("%",string.Empty)),
                            Volume = StrToDecParse(node.ChildNodes.ElementAt(6).InnerText),
                            AvgVol3Mth = StrToDecParse(node.ChildNodes.ElementAt(7).InnerText),
                            MarketCap = StrToDecParse(node.ChildNodes.ElementAt(8).InnerText),
                            PERatioTTM = StrToDecParse((node.ChildNodes.ElementAt(9).FirstChild.InnerText ?? node.ChildNodes.ElementAt(9).InnerText)),
                            //Week52Range = node.ChildNodes.ElementAt(10).InnerText - there is a picture loaded by JS.
                            Week52Range = string.Empty
                        };

                        StockActiveList.Add(stockActiveDto);
                    } 
                }
            }
            catch (Exception e)
            {
                _logger.Error(e, "Error");
                StockActiveList = null;
            }

            return StockActiveList;



        }

        private decimal? StrToDecParse(string stringToDecimalParse)
        {
            decimal? valueToParse = null;
            decimal? parsedValue = null;
            switch (stringToDecimalParse.Trim().ToLowerInvariant().Last<char>())
            {
                case 'm':
                    valueToParse = decimal.Parse(stringToDecimalParse.Trim().ToLowerInvariant().Replace("m", string.Empty));
                    parsedValue = valueToParse * 1000000;
                    break;
                case 'b':
                    valueToParse = decimal.Parse(stringToDecimalParse.Trim().ToLowerInvariant().Replace("b", string.Empty));
                    parsedValue = valueToParse * 1000000000;
                    break;
                case 't':
                    valueToParse = decimal.Parse(stringToDecimalParse.Trim().ToLowerInvariant().Replace("t", string.Empty));
                    parsedValue = valueToParse * 1000000000000;
                    break;
                case 'a':
                     parsedValue = null;
                    break;
                default:
                    parsedValue = decimal.Parse(stringToDecimalParse.Trim());
                    break;
            }

            return parsedValue;
        }
    }
}
