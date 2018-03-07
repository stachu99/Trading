using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace YahooScraper.Services
{
    public class StockActiveQueryParameters
    {
        public List<string> Countries { get; set; } = Startup.Configuration["YahooStockActive:DefaultQueryParameters:Countries"].Split(new char[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries).ToList();
        public string OrderBy { get; set; }
        private bool _descending = false;
        public bool Descending
        {
            get { return _descending; }
            set
            {
                if (value)
                {
                    _descending = value;
                    if (OrderBy!=null)
                    {
                        OrderBy += " desc";
                    }
                }
            }
        }

        //public bool HasQuery { get { return !String.IsNullOrEmpty(Query); } }
        //public string Query { get; set; }
    }
}
