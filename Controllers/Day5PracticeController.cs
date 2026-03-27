using Microsoft.AspNetCore.Mvc;
using DemoAPI.Data;
using DemoAPI.Models;
using DemoAPI.DTOs;
using Microsoft.EntityFrameworkCore;

namespace DemoAPI.Controllers
{
    [ApiController]
    [Route("api/[Controller]/Products")]
    public class Day5PracticeController : ControllerBase
    {
        private readonly AppDbContext _context;

        public Day5PracticeController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> CreateAsync(CreateProductDto dto)
        {
            var product = new Product
            {
                Name = dto.Name,
                Price = dto.Price,
                Stock = dto.Stock
            };

            if (await _context.Products.AnyAsync(p => p.Name == dto.Name))
            {
                return Fail("The Name field must be unique.");
            }

            _context.Products.Add(product);
            // 重要 : ✔ 不阻塞 thread
            //        ✔ 高併發必備
            //        ✔ 大型專案必備
            // 回傳要使用 async Task<IActionResult>
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(Get), new { id = product.Id }, new
            {
                status = "success",
                data = product
            });
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            var products = _context.Products.ToList();

            return Ok(new
            {
                status = "success",
                data = products
            });
        }

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

        private IActionResult Fail(string message)
        {
            return BadRequest(new
            {
                status = "error",
                message = message
            });
        }


    }
}