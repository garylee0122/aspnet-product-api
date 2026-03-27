using Microsoft.AspNetCore.Mvc;

namespace DemoAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class Day2PracticeController : ControllerBase
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

        [HttpGet("post/{id:int}", Name = "post.show")]
        public IActionResult GetPost(int id)
        {
            return Ok(Url.Link("post.show", new { id }));
        }

        [HttpGet("product/{id:int}/{name?}")]
        public IActionResult GetProduct(int id, string? name)
        {
            var result = new
            {
                Id = id,
                Name = name ?? "No Name"
            };

            return Ok(result);
        }
    }
}