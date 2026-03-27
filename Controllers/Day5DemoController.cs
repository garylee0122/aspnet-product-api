using Microsoft.AspNetCore.Mvc;
using DemoAPI.Data;
using DemoAPI.Models;

namespace DemoAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class Day5DemoController : ControllerBase
    {
        private readonly AppDbContext _context;

        public Day5DemoController(AppDbContext context)
        {
            _context = context;
        }

        // 新增商品
        [HttpPost]
        public IActionResult Create(Product product)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.Products.Add(product);
            _context.SaveChanges();

            return CreatedAtAction(nameof(Get), new { id = product.Id }, new
            {
                status = "success",
                data = product
            });
        }

        // 查全部
        [HttpGet]
        public IActionResult GetAll()
        {
            return Ok(new
            {
                status = "success",
                data = _context.Products.ToList()
            });
        }

        // 查單筆
        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            var product = _context.Products.Find(id);

            if (product == null)
            {
                return NotFound(new
                {
                    status = "error",
                    message = "Product not found"
                });
            }

            return Ok(new
            {
                status = "success",
                data = product
            });
        }

    }

}