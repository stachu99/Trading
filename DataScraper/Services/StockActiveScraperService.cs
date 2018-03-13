using HtmlAgilityPack;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataScraper.Models;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace DataScraper.Services
{
    public class StockActiveScraperService
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        string _uriHost = Startup.Configuration["YahooFinance:UriHost"];

        public IEnumerable<StockActiveDto> GetStockActives(StockActiveQueryParameters stockActiveQueryParameters)
        {

            IEnumerable<StockActiveDto> allStockActives = ScrapStockActives(stockActiveQueryParameters.Countries);

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
            UriBuilder ub = new UriBuilder
            {
                Scheme = Startup.Configuration["YahooFinance:UriScheme"],
                Host = (!string.IsNullOrEmpty(Startup.Configuration["YahooFinance:UriSubDomainCountry:" + country])) ? Startup.Configuration["YahooFinance:UriSubDomainCountry:"+country]+ "."+ _uriHost : _uriHost,
                Port = Int16.Parse(Startup.Configuration["YahooFinance:UriPort"]),
                Path = Startup.Configuration["YahooFinance:YahooStockActive:UriPath"]
            };
            return ub.Uri;
        }


        public IEnumerable<StockActiveDto> ScrapStockActives(List<string> countries)
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
                    HtmlNodeCollection nodes = new HtmlNodeCollection(null);
                        var options = new ChromeOptions();
                        // set some options
                        options.ToCapabilities();
                        //IWebDriver driver = new RemoteWebDriver(new Uri(""), options); for remote in a Docker container
                        //Local Chrome driver for debug mode
                        IWebDriver driver = new ChromeDriver(Environment.CurrentDirectory + "\\ThirdPart\\Selenium\\ChromeDriver", options)
                        {
                            Url = uri.ToString() + $"?offset={pageOffset}&count={pageCount}"
                        };
                        IWebElement element;
                        driver.FindElement(By.XPath("//*[@id=\"screener-criteria\"]/div[2]/div[1]/div/button[1]")).Click();
                        driver.FindElement(By.XPath("//*[@id=\"screener-criteria\"]/div[2]/div[1]/div[1]/div[1]/div/div[2]/ul/li[1]/button")).Click();
                        driver.FindElement(By.XPath("//*[@id=\"screener-criteria\"]/div[2]/div[1]/div[1]/div[1]/div/div[2]/ul/li/div/div")).Click();

                        WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
                    wait.Until<IWebElement>(d => d.FindElement(By.XPath("//*[@id=\"dropdown-menu\"]/div/div[2]/ul/li[20]/label"))).Click();
                    wait.Until<IWebElement>(d => d.FindElement(By.XPath("//*[@id=\"dropdown-menu\"]/div/div[2]/ul/li[18]/label"))).Click();
                    wait.Until<IWebElement>(d => d.FindElement(By.XPath("//*[@id=\"dropdown-menu\"]/div/div[2]/ul/li[12]/label"))).Click();

                    //*[@id="dropdown-menu"]/div/div[2]/ul/li[20]/label/svg/path
                    driver.FindElement(By.XPath("//*[@id=\"dropdown-menu\"]/button")).Click();
                        driver.FindElement(By.XPath("//*[@id=\"screener-criteria\"]/div[2]/div[1]/div[3]/button[1]")).Click();
                        element =  wait.Until<IWebElement>(d => d.FindElement(By.XPath("//*[@id=\"scr-res-table\"]/table/tbody/tr[1]")));

                    bool nextPageRecordsExists = false;
                    do
                    {
                        if (nextPageRecordsExists)
                        {
                            driver.FindElement(By.XPath("//*[@id=\"fin-scr-res-table\"]/div[2]/div[2]/button[3]")).Click();
                        }
                        HtmlDocument doc = new HtmlDocument();
                        doc.LoadHtml(driver.PageSource);
                        var _nodes = doc.DocumentNode.SelectNodes("//*[@id=\"scr-res-table\"]/table/tbody/tr");
                        if (_nodes == null)
                        {
                            break;
                        }
                        foreach (var node in _nodes)
                        {
                            nodes.Add(node);
                        }
                        if (driver.FindElementSafe(By.XPath("//*[@id=\"fin-scr-res-table\"]/div[2]/div[2]/button[3]")) != null && driver.FindElement(By.XPath("//*[@id=\"fin-scr-res-table\"]/div[2]/div[2]/button[3]")).Displayed && driver.FindElement(By.XPath("//*[@id=\"fin-scr-res-table\"]/div[2]/div[2]/button[3]")).Enabled)
                        { nextPageRecordsExists = true; }
                        else
                        {
                            driver.Quit();
                            nextPageRecordsExists = false; }


                    } while (nextPageRecordsExists);


                if (nodes == null)
                        return StockActiveList;
                    StockActiveDto stockActiveDto;
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
