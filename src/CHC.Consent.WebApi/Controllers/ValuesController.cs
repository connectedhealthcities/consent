using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CHC.Consent.WebApi.Controllers
{
    [Authorize]
    [ApiVersion("0.1-dev")]
    [Route("/v{version:apiVersion}/[controller]")]
    public class ValuesController : Controller
    {
        private IList<string> values = new List<string> {"value1", "value2"};
        
        // GET api/values
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return values.Concat(new [] {User.Identity?.Name})
                .Concat(User.Claims.SelectMany(_ => new [] { _.Type, _.Value }));
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return values.ElementAtOrDefault(id);
        }

        // POST api/values
        [HttpPost]
        public void Post([FromBody] string value)
        {
            values.Add(value);
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
            values[id] = value;
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
            values.RemoveAt(id);
        }
    }
}