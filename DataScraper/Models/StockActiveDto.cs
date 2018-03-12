using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataScraper.Models
{
    public class StockActiveDto
    {
        public string Symbol { get; set; }
        public string Url { get; set; }
        public string Name { get; set; }
        public decimal? Price { get; set; }
        public decimal? Change { get; set; }
        public decimal? ChangePercentage { get; set; }
        public decimal? Volume { get; set; }
        public decimal? AvgVol3Mth { get; set; }
        public decimal? MarketCap { get; set; }
        public decimal? PERatioTTM { get; set; }
        public string Week52Range { get; set; }
    }
}
