﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataScraper.Services
{
    public class IPOQueryParameters
    {
        public DateTime Day { get; set; } = DateTime.Now.Date;
        public List<string> Actions { get; set; }
        public List<string> Exchanges { get; set; }

        //public bool HasQuery { get { return !String.IsNullOrEmpty(Query); } }
        //public string Query { get; set; }
    }
}
