using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TradingCore.Models
{
    public class IPODts
    {

            public string Symbol { get; set; }
            public string Url { get; set; }
            public string Company { get; set; }
            public string Exchange { get; set; }
            public string Date { get; set; }
            public string PriceRang { get; set; }
            public string Price { get; set; }
            public string Currency { get; set; }
            public string Shares { get; set; }
            public string Actions { get; set; }

    }
}
