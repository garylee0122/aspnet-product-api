using Microsoft.AspNetCore.Mvc;

namespace DemoAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class HelloController : ControllerBase
    {
        [HttpGet]
        public string Get()
        {
            return "Hello ASP.NET";
        }

        [HttpGet("{name}")]
        public string Get(string name)
        {
            return "Hello " + name;
        }
    }
}