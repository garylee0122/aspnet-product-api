using Microsoft.AspNetCore.Mvc;

namespace DemoAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class Day3PracticeController : ControllerBase
    {
        [HttpGet("hello/{name?}")]
        public IActionResult Hello(string? name)
        {
            return Ok(new
            {
                message = $"Hello, {name ?? "World"}"
            });
        }

        [HttpGet("user/{id:int}")]
        public IActionResult GetUser(int id)
        {
            return Success(new
            {
                id,
                name = $"User{id}"
            });
        }

        [HttpGet("product/{id:int}")]
        public IActionResult GetProduct(int id)
        {
            return Success(new
            {
                id = id,
                name = "iPhone",
                price = 30000
            });
        }

        [HttpGet("orders/{id:int}")]
        public IActionResult GetOrder(int id)
        {
            return Success(new
            {
                id = id,
                products = new[] {
                   new {name = "iPhone", price = 30000},
                   new {name = "AirPods", price = 6000}
                }
            });
        }

        private IActionResult Success(object data)
        {
            return Ok(new
            {
                status = "success",
                data = data
            });
        }
    }
}