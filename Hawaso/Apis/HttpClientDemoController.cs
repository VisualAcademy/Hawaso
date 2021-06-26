//[2] HttpClient 사용하기 - 데모 Web API
using Microsoft.AspNetCore.Mvc;

namespace Hawaso.Apis
{
    [Route("api/[controller]")]
    [ApiController]
    public class HttpClientDemoController : ControllerBase
    {
        [HttpGet]
        public string Get()
        {
            return "Hello HttpClient!";
        }
    }
}
