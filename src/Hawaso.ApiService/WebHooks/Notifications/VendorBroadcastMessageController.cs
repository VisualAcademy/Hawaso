using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Hawaso.ApiService.WebHooks.Notifications
{
    [Route("api/[controller]")]
    [ApiController]
    public class VendorBroadcastMessageController : ControllerBase
    {
        // GET: api/<VendorBroadcastMessageController>
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "Vendor 1", "Vendor 2" };
        }

        // GET api/<VendorBroadcastMessageController>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "안녕하세요";
        }

        // POST api/<VendorBroadcastMessageController>
        [HttpPost]
        public void Post([FromBody]string value)
        {
        }

        // PUT api/<VendorBroadcastMessageController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/<VendorBroadcastMessageController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
