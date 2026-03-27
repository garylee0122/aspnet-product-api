using Microsoft.AspNetCore.Mvc;

namespace DemoAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class RouteDemoController : ControllerBase
    {
        [HttpGet("hello/{name}")]
        public string hello(string name)
        {
            return "Hello " + name;
        }

        [HttpGet("user/{id:int}")]
        public string GetUser(int id)
        {
            return $"User ID: {id}";
        }

        [HttpGet("product/{id:int}/{name?}")]
        public string GetProduct(int id, string? name)
        {
            return $"Product: {id} - {name ?? "No Name"}";
        }

        [HttpGet("post/{id}", Name = "GetPost")]
        public IActionResult GetPost(int id)
        {
            var url = Url.Link("GetPost", new { id = id });
            return Ok(url);
        }
    }
}