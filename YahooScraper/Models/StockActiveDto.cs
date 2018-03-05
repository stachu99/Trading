using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace YahooScraper.Models
{
    public class StockActiveDto
    {
        public string Symbol { get; set; }
        public string Url { get; set; }
        public string Name { get; set; }
        public string Price { get; set; }
        public string Change { get; set; }
        public string ChangePercentage { get; set; }
        public string Volume { get; set; }
        public string AvgVol3Mth { get; set; }
        public string MarketCap { get; set; }
        public string PERatioTTM { get; set; }
        public string Week52Range { get; set; }
    }
}
