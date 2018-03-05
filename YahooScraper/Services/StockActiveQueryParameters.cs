using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace YahooScraper.Services
{
    public class StockActiveQueryParameters
    {
        private const int maxCount = 100;
        private int _count = 10;
        public int Offset { get; set; } = 0;
        public int Count
        {
            get { return _count; }
            set { _count = (value > maxCount) ? maxCount : value; }
        }


        public List<string> Countries { get; set; } = new List<string>() { "USA", "UK" };
        public List<string> OrderBy { get; set; } = new List<string>() { "ChangePercentage" };
        private bool _descending = false;
        public bool Descending 
        {
            set
            {
                if (value)
                {
                    _descending = value;
                    if (OrderBy!=null)
                    OrderBy[OrderBy.Count-1] = $"{OrderBy.Last()} desc" ;
                }
            }
        }

        //public bool HasQuery { get { return !String.IsNullOrEmpty(Query); } }
        //public string Query { get; set; }
    }
}
