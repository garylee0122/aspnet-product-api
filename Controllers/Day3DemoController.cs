using Microsoft.AspNetCore.Mvc;

namespace DemoAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class Day3DemoController : ControllerBase
    {
        [HttpGet("hello/{name}")]
        public IActionResult Hello(string name)
        {
            return Ok(new
            {
                message = $"Hello, {name}"
            });
        }

        [HttpGet("user/{id:int}")]
        public IActionResult GetUser(int id)
        {
            return Ok(new
            {
                id = id,
                name = $"User{id}"
            });
        }

        [HttpGet("product/{id:int}")]
        public IActionResult GetProduct(int id)
        {
            return Ok(new
            {
                id = id,
                name = "iPhone",
                price = 30000
            });
        }

        [HttpGet("orders/{id:int}")]
        public IActionResult GetOrder(int id)
        {
            return Ok(new
            {
                id = id,
                products = new[]
                {
                   new { name = "iPhone", price = 30000 },
                   new { name = "AirPods", price = 6000 }
                }
            });
        }
    }
}