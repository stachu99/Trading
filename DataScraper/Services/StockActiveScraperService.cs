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
        private string _uriHost = Startup.Configuration["YahooFinance:UriHost"];
        private List<StockActiveDto> _stockActiveList;
        private int _filtersFailed = 0;
        private int _maxFilterFailed = 5;
        private StockActiveQueryParameters _stockActiveQueryParameters;
        private string _xPathScreenerCriteriaBase = "//*[@id=\"screener-criteria\"]/div[2]/div[1]/div[1]";
        private string _xPathScreenerCriteriaSectionFilterMenu = "";
        private string _xPathScreenerCriteriaSectionFilterAddAnotherFilterButton = "";



        //Region "//*[@id="screener-criteria"]/div[2]/div[1]/div[1]/div[1]/div/div[1]/span[1]/span"
        //    //*[@id="screener-criteria"]/div[2]/div[1]/div[1]/div/div[2]/div/div[2]/div[1]/div/ul/li[29]/label

        //    Volume (Intraday) "//*[@id="screener-criteria"]/div[2]/div[1]/div[1]/div[2]/div/div[1]/span[1]/span"
        //    //*[@id="screener-criteria"]/div[2]/div[1]/div[1]/div[2]/div/div/div[2]/div[1]/div/ul/li[6]/label
        //      *[@id="screener-criteria"]/div[2]/div[1]/div[1]/div[3]/div/div/div[2]/div[1]/div/ul/li[6]/label

        //    Percent Change "//*[@id="screener-criteria"]/div[2]/div[1]/div[1]/div[3]/div/div[1]/span[1]/span"
        //    //*[@id="screener-criteria"]/div[2]/div[1]/div[1]/div[2]/div/div/div[2]/div[1]/div/ul/li[28]/label

        //    Market Cap (Intraday)
        //    //*[@id="screener-criteria"]/div[2]/div[1]/div[1]/div[2]/div/div/div[2]/div[3]/div/ul/li[6]/label




        public IEnumerable<StockActiveDto> GetStockskActive(StockActiveQueryParameters stockActiveQueryParameters)
        {
            _stockActiveQueryParameters = stockActiveQueryParameters;
            IEnumerable<StockActiveDto> allStocksActive = ScrapStocksActive(_stockActiveQueryParameters);
            if (allStocksActive == null)
            {
                return null;
            }
            return allStocksActive;
        }

        private Uri SetUri()
        {
            UriBuilder ub = new UriBuilder
            {
                Scheme = Startup.Configuration["YahooFinance:UriScheme"],
                Host = _uriHost,
                Port = Int16.Parse(Startup.Configuration["YahooFinance:UriPort"]),
                Path = Startup.Configuration["YahooFinance:YahooStockActive:UriPath"]
            };
            return ub.Uri;
        }


        public IEnumerable<StockActiveDto> ScrapStocksActive(StockActiveQueryParameters stockActiveQueryParameters)
        {
            var options = new ChromeOptions();
            // set some options
            options.ToCapabilities();
            //IWebDriver driver = new RemoteWebDriver(new Uri(""), options); for remote in a Docker container
            //Local Chrome driver for debug mode
            IWebDriver driver = new ChromeDriver(Environment.CurrentDirectory + "\\ThirdPart\\Selenium\\ChromeDriver", options);
            try
            {
                _stockActiveList = new List<StockActiveDto>();
                Uri uri = SetUri();
                int pageCount = 100;
                int pageOffset = 0;
                HtmlNodeCollection nodes = new HtmlNodeCollection(null);
                driver.Url = uri.ToString() + $"?offset={pageOffset}&count={pageCount}";
                SetFilters(driver, _stockActiveQueryParameters);




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
                        nextPageRecordsExists = false;
                    }
                } while (nextPageRecordsExists);

                if (nodes == null)
                    return _stockActiveList;
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
                        ChangePercentage = StrToDecParse(node.ChildNodes.ElementAt(5).FirstChild.InnerText.Replace("%", string.Empty)),
                        Volume = StrToDecParse(node.ChildNodes.ElementAt(6).InnerText),
                        AvgVol3Mth = StrToDecParse(node.ChildNodes.ElementAt(7).InnerText),
                        MarketCap = StrToDecParse(node.ChildNodes.ElementAt(8).InnerText),
                        PERatioTTM = StrToDecParse((node.ChildNodes.ElementAt(9).FirstChild.InnerText ?? node.ChildNodes.ElementAt(9).InnerText)),
                        //Week52Range = node.ChildNodes.ElementAt(10).InnerText - there is a picture loaded by JS.
                        Week52Range = string.Empty
                    };

                    _stockActiveList.Add(stockActiveDto);
                }

            }
            catch (Exception e)
            {
                _logger.Error(e, "Error");
                _stockActiveList = null;
                driver.Quit();
            }
            return _stockActiveList;
        }
        private void CheckFiltersFailed(IWebDriver driver)
        {
            if (_filtersFailed>_maxFilterFailed)
            {
                throw new System.ArgumentException($"Filters Failed over {_maxFilterFailed} times.");
            }

            if (driver.FindElementSafe(By.XPath("//*[@id=\"screener-criteria\"]/div[2]/div/div[1]/span")) != null)
            {
                _filtersFailed++;
                driver.Navigate().Refresh();
                SetFilters(driver, _stockActiveQueryParameters);
            }
            return;
        }
        private void SetFilters(IWebDriver driver, StockActiveQueryParameters stockActiveQueryParameters)
        {
            if (_filtersFailed > _maxFilterFailed)
            {
               throw new ArgumentException("Error - {this.GetType()} failed to set up filters {_maxFilterFailed.ToString()} timea.");
            }
            ClearFilters(driver);
            SetFilterRegion(driver, _stockActiveQueryParameters.Countries);
            SetFilterMarketCap(driver , _stockActiveQueryParameters.MarketCapIntraday);
            SetFilterVolumeIntraday(driver, _stockActiveQueryParameters.VolumeIntradayCondition, _stockActiveQueryParameters.VolumeIntraday, _stockActiveQueryParameters.VolumeIntraday2);
        }


        private void SetFilterMarketCap(IWebDriver driver, List<string> marketCapsIntraday)
        {
            List<string> marketCapsIntradayAll = new List<string>() { "Small Cap", "Mid Cap", "Large Cap", "Mega Cap" };
            List<string> marketCapsIntradayFilter = new List<string>();
            foreach (var marketCap in marketCapsIntraday)
            {
                if (marketCapsIntradayAll.Contains(marketCap))
                {
                    marketCapsIntradayFilter.Add(marketCap);
                }
            }
            if (marketCapsIntradayFilter.Count == 0)
            {
                return;
            }
            try
            {
                //Click a button Add another filter
                AddAnotherFilter(driver);
                //click a checkbox Region
                driver.FindElement(By.XPath($"{_xPathScreenerCriteriaBase}{_xPathScreenerCriteriaSectionFilterMenu}/div/div[2]/div[3]/div/ul/li[6]/label")).Click();
                //filters-menu
                SetXPathScreenerCriteriaSectionAddAnotherFilterAndMenu(driver);
                //click a button Close dropdown add another filter
                driver.FindElement(By.XPath($"{_xPathScreenerCriteriaBase}{_xPathScreenerCriteriaSectionFilterMenu}/div/div/button")).Click();
                //Click an add Market Caps Intraday
                var _xPathScreenerCriteriaSection = SetXPathScreenerCriteriaSection(driver, "Market Cap (Intraday)");
                var buttons = driver.FindElementSafe(By.XPath($"{_xPathScreenerCriteriaBase}{_xPathScreenerCriteriaSection}/div/div[2]")).FindElements(By.TagName("button")).ToList();
                if (buttons != null)
                {
                    foreach (var marketCap in marketCapsIntradayFilter)
                    {
                        foreach (var button in buttons)
                        {
                            try
                            {
                                if (button.GetAttribute("Title").Contains(marketCap))
                                {
                                    button.Click();
                                }
                            }
                            catch (StaleElementReferenceException)
                            {
                                ;
                            }

                        }
                    }
                }
            }
            catch (Exception e)
            {
                CheckFiltersFailed(driver);
                throw;
            }
            return;
        }

        private void SetFilterVolumeIntraday(IWebDriver driver, string volumeIntradayCondition, int volumeIntraday, int volumeIntraday2)
        {
            List<string> VolumeIntradayConditionsAll = new List<string>() { "greater than", "less than", "equals", "between" };
            if (!VolumeIntradayConditionsAll.Contains(volumeIntradayCondition))
            {
                return;
            }
            try
            {
                //Click a button Add another filter
                AddAnotherFilter(driver);
                //click a checkbox Volume (Intraday)
                driver.FindElement(By.XPath($"{_xPathScreenerCriteriaBase}{_xPathScreenerCriteriaSectionFilterMenu}/div/div[2]/div[1]/div/ul/li[6]/label")).Click();
                //filters-menu                      //*[@id="screener-criteria"]/div[2]/div[1]/div[1]/div[2]/div   /div/div[2]/div[1]/div/ul/li[6]/label
                SetXPathScreenerCriteriaSectionAddAnotherFilterAndMenu(driver);
                //click a button Close dropdown add another filter
                driver.FindElement(By.XPath($"{_xPathScreenerCriteriaBase}{_xPathScreenerCriteriaSectionFilterMenu}/div/div/button")).Click();
                //Click an add Volume (Intraday)
                var _xPathScreenerCriteriaSection = SetXPathScreenerCriteriaSection(driver, "Volume (Intraday)");
                driver.FindElementSafe(By.XPath($"{_xPathScreenerCriteriaBase}{_xPathScreenerCriteriaSection}/div/div[1]/span[2]/div")).Click();
                var g = driver.FindElementSafe(By.XPath($"{_xPathScreenerCriteriaBase}{_xPathScreenerCriteriaSection}/div/div[1]/span[2]/div[2]"));
                var volumeIntradayConditions = g.FindElements(By.Name("div")).ToList();
                //< div class="Pstart(16px) Pend(32px) Pt(13px) Pb(13px) Cur(p) Bgc($extraLightBlue):h Bgc($extraLightBlue):f Fw(b)" data-value="gt"><span>greater than</span></div><div class="Pstart(16px) Pend(32px) Pt(13px) Pb(13px) Cur(p) Bgc($extraLightBlue):h Bgc($extraLightBlue):f" data-value="lt"><span>less than</span></div><div class="Pstart(16px) Pend(32px) Pt(13px) Pb(13px) Cur(p) Bgc($extraLightBlue):h Bgc($extraLightBlue):f" data-value="eq"><span>equals</span></div><div class="Pstart(16px) Pend(32px) Pt(13px) Pb(13px) Cur(p) Bgc($extraLightBlue):h Bgc($extraLightBlue):f" data-value="btwn"><span>between</span></div>
                if (volumeIntradayConditions != null)
                {
                    foreach (var item in volumeIntradayConditions)
                    {
                        try
                        {
                            if (item.FindElement(By.Name("span")).Text == volumeIntradayCondition)
                        {
                                item.Click();
                                var inputs = driver.FindElementSafe(By.XPath($"{_xPathScreenerCriteriaBase}{_xPathScreenerCriteriaSection}/div/div[2]")).FindElements(By.Name("input")).ToList();
                                if (inputs != null)
                                {
                                    inputs[0].SendKeys(volumeIntraday.ToString());
                                    if (inputs.Count == 2)
                                    {
                                        inputs[1].SendKeys(volumeIntraday2.ToString());
                                    }
                                }
                            }
                        }
                        catch (StaleElementReferenceException)
                        {
                            ;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                CheckFiltersFailed(driver);
                throw;
            }
            return;
        }

        private void SetFilterRegion(IWebDriver driver, List<string> regions)
        {
            string exceptionAnyRegionFalse = $"Warm - {this.GetType()} failed to set up filters. {regions.GetType()} has not contained even one proper Region.";
            Dictionary<string, int> regionXPathLiIndex = new Dictionary<string, int>
            {
                { "Austria", 2 },
                { "Belgium", 4 },
                { "Germany", 12 },
                { "France", 17 },
                { "United Kingdom", 18 },
                { "Luxemburg", 32 },
                { "Poland", 41 },
                { "United States", 54 }
            };
            Dictionary<string, int> regionXPathLiIndexFilter = new Dictionary<string, int>();
            foreach (var region in regions)
            {
                if (regionXPathLiIndex.ContainsKey(region))
                {
                    regionXPathLiIndexFilter.Add(region, regionXPathLiIndex.GetValueOrDefault(region));
                }
            }
            if (regionXPathLiIndexFilter.Count==0)
            {
                throw new ArgumentException(exceptionAnyRegionFalse);
            }
            IWebElement element;
            List<string> regionXPath = new List<string>()
            {
                "//*[@id=\"dropdown-menu\"]/div/div[2]/ul/li[",
                "]/label"
            };
            try
            {
                //Click a button Add another filter
                AddAnotherFilter(driver);
                //click a checkbox Region
                driver.FindElement(By.XPath($"{_xPathScreenerCriteriaBase}{_xPathScreenerCriteriaSectionFilterMenu}/div/div[2]/div[1]/div/ul/li[29]/label")).Click();
                //filters-menu
                SetXPathScreenerCriteriaSectionAddAnotherFilterAndMenu(driver);
                //click a button Close dropdown add another filter
                driver.FindElement(By.XPath($"{_xPathScreenerCriteriaBase}{_xPathScreenerCriteriaSectionFilterMenu}/div/div/button")).Click();
                //Click an add regions
                var _xPathScreenerCriteriaSection = SetXPathScreenerCriteriaSection(driver, "Region");
                driver.FindElement(By.XPath($"{_xPathScreenerCriteriaBase}{_xPathScreenerCriteriaSection}/div/div[2]/ul/li/div/div")).Click();
                bool anyRegion = false;
                foreach (var regionIntex in regionXPathLiIndexFilter.Values)
                {
                    element = driver.FindElementSafe(By.XPath(regionXPath[0] + regionIntex.ToString() + regionXPath[1]));
                    if (element != null)
                    {
                        element.Click();
                        anyRegion = true;
                    }
                    else
                    {
                        CheckFiltersFailed(driver);
                    }
                }
                if (!anyRegion)
                {
                    throw new ArgumentException($"Warm - {this.GetType()} failed to set up filters. {regions.GetType()} has not contained even one proper Region.");
                }
                //Click a close regions dropdown
                driver.FindElement(By.XPath("//*[@id=\"dropdown-menu\"]/button")).Click();
            }
            catch (ArgumentException)
            { throw; }
            catch (Exception)
            {
                CheckFiltersFailed(driver);
                throw;
            }
            return;
        }

        private void AddAnotherFilter(IWebDriver driver)
        {
            SetXPathScreenerCriteriaSectionAddAnotherFilterAndMenu(driver);
            driver.FindElement(By.XPath(_xPathScreenerCriteriaBase+_xPathScreenerCriteriaSectionFilterAddAnotherFilterButton)).Click();
        }

        private string SetXPathScreenerCriteriaSection(IWebDriver driver, string section)
        {
            string exceptionSetXPathScreenerCriteriaSectionFalse = $"Warm - {this.GetType()} failed to set up section - {section}.";

            try
            {
                CheckFiltersFailed(driver);
                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(driver.PageSource);
                var criteriaSectionsCount = doc.DocumentNode.SelectSingleNode(_xPathScreenerCriteriaBase).ChildNodes.Count(x => x.Name == "div");
                string div = "";
                string xPathSectionEndPath = "/div/div[1]/span/span";
                for (int i = 1; i <= criteriaSectionsCount; i++)
                {
                    div = $"/div[{i}]";
                    var b = (doc.DocumentNode.SelectSingleNode($"{_xPathScreenerCriteriaBase}{div}{xPathSectionEndPath}"));
                    //*[@id="screener-criteria"]/div[2]/div[1]/div[1]   /div[1] /div/div[1]/span[1]/span
                    if (b != null && b.InnerText.Contains(section))
                    {
                        return $"{div}";
                    }
                }
                CheckFiltersFailed(driver);
                throw new ArgumentException(exceptionSetXPathScreenerCriteriaSectionFalse);
            }
            catch (Exception e)
            {
                CheckFiltersFailed(driver);
                throw;
            }
        }
        // Set XPathScreenerCriteriaSectionAddAnotherFilter - section of a button Add another filter
        private void SetXPathScreenerCriteriaSectionAddAnotherFilterAndMenu(IWebDriver driver)
        {
            try
            {
                CheckFiltersFailed(driver);
                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(driver.PageSource);
                var criteriaSectionsCount = doc.DocumentNode.SelectSingleNode(_xPathScreenerCriteriaBase).ChildNodes.Count(x => x.Name == "div");
                string xPathScreenerCriteriaSectionFilterMenu;
                string div = "";
                string xPathAddAnotherFilterEnd = "/button/span/span";
                for (int i = criteriaSectionsCount; i>0; i--)
                {
                    div = $"/div[{i}]";
                    xPathScreenerCriteriaSectionFilterMenu = $"{div}/div";
                    if (i==1)
                    {
                        //div = "/div";
                        xPathScreenerCriteriaSectionFilterMenu = $"{div}/div[2]";
                        xPathAddAnotherFilterEnd = "/button/span[2]/span";
                    }//*[@id="screener-criteria"]/div[2]/div[1]/div[1]/div/button
                    //*[@id="screener-criteria"]/div[2]/div[1]/div[1]/div[2]/button
                    //*[@id="screener-criteria"]/div[2]/div[1]/div[1]/div[2]/button/span/span
                    var b = (doc.DocumentNode.SelectSingleNode($"{_xPathScreenerCriteriaBase}{div}{xPathAddAnotherFilterEnd}"));
                    if (b != null && b.InnerText.Contains("Add another filter"))
                    {//*[@id="screener-criteria"]/div[2]/div[1]/div[1]/div[2]/div/div/button
                        _xPathScreenerCriteriaSectionFilterMenu = xPathScreenerCriteriaSectionFilterMenu;
                        _xPathScreenerCriteriaSectionFilterAddAnotherFilterButton = $"{div}/button";
                        return;
                    }
                }
                CheckFiltersFailed(driver);
            }
            catch (Exception e)
            {
                CheckFiltersFailed(driver);
                throw;
            }
        }
        private void ClearFilters(IWebDriver driver)
        {
            try
            {
                //Click an edit button for StocksActive filters
                driver.FindElement(By.XPath("//*[@id=\"screener-criteria\"]/div[2]/div[1]/div/button[1]")).Click();
                //Remove all set up filters
                var buttons = driver.FindElementSafe(By.XPath("//*[@id=\"screener-criteria\"]")).FindElements(By.TagName("button")).ToList();
                //Reverse the buttons list for proper work
                buttons.Reverse();
                foreach (var item in buttons)
                {
                    try
                    {
                        if (item.GetAttribute("title").Contains("Remove"))
                        {
                            item.Click();
                        }
                    }
                    catch (StaleElementReferenceException)
                    {
                        ;
                    }
                }
            }
            catch (Exception e)
            {
                CheckFiltersFailed(driver);
            }
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
