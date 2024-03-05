using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ACLAuthorization.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OnlyAdminController : ControllerBase
    {
        // GET: api/<AllRolesController>
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        [Authorize(Policy = "ACLs")]
        // GET api/<AllRolesController>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<AllRolesController>
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/<AllRolesController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<AllRolesController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
