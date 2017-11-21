using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace YahooIPOScraper.Services
{
    public class IPOQueryParameters
    {
        public DateTime Day { get; set; } = DateTime.Now.Date;
        public List<string> Actions { get; set; }
        public List< string> Exchange { get; set; }

        //public bool HasQuery { get { return !String.IsNullOrEmpty(Query); } }
        //public string Query { get; set; }
    }
}
