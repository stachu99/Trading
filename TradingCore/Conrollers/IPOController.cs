using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TradingCore.Models;

namespace TradingCore.Conrollers
{
    [Produces("application/json")]
    [Route("api/IPO")]
    public class IPOController : Controller
    {
        // GET: api/IPO
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET: api/IPO/5
        [HttpGet("{id}", Name = "Get")]
        public string Get(int id)
        {
            return "value";
        }
        
        // POST: api/IPO
        [HttpPost]
        public IActionResult Post([FromBody]IEnumerable<IPODts> iPOForCreate)
        {
            return Ok(iPOForCreate);
        }
        
        // PUT: api/IPO/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }
        
        // DELETE: api/ApiWithActions/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
