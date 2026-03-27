using Microsoft.AspNetCore.Mvc;
using DemoAPI.Data;
using DemoAPI.Models;

namespace DemoAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class Day4DemoController : ControllerBase
    {
        private readonly AppDbContext _context;

        public Day4DemoController(AppDbContext context)
        {
            _context = context;
        }

        // 新增
        [HttpGet("products/create/{name:required}/{price:int}")]
        public IActionResult Create(string name, int price)
        {
            var product = new Product
            {
                Name = name,
                Price = price
            };

            _context.Products.Add(product);
            _context.SaveChanges();

            return Ok(product);
        }

        // 查全部
        [HttpGet("products")]
        public IActionResult GetAll()
        {
            return Ok(_context.Products.ToList());
        }

        // 查單筆
        [HttpGet("products/{id:int}")]
        public IActionResult Get(int id)
        {
            var product = _context.Products.Find(id);
            return Ok(product);
        }
    }
}