using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataScraper.Services
{
    public class StockActiveQueryParameters
    {

    private List<string> _countries = Startup.Configuration.GetSection("YahooFinance:YahooStockActive:DefaultQueryParameters:Countries").Get<List<string>>();
    public List<string> Countries
        {
            get
            {
                return _countries;
            }
            set
            {
                foreach (var item in value)
                {
                    item.Trim();
                }
                _countries = value;
            }
        }


        public string OrderBy { get; set; } = "ChangePercentage";
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
