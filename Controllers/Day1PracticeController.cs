using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace DemoAPI.Controllers
{
    [ApiController]
    [Route("")]
    public class Day1PracticeController : ControllerBase
    {
        [HttpGet("hi")]
        public string hi()
        {
            return "Hi ASP.NET";
        }

        [HttpGet("user/{name}")]
        public string user(string name)
        {
            return "User: " + name;
        }

        [HttpGet("add/{a}/{b}")]
        public string add(int a, int b)
        {
            int total = a + b;
            return total.ToString();
        }
    }
}