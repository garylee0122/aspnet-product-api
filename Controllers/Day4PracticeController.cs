using Microsoft.AspNetCore.Mvc;
using DemoAPI.Data;
using DemoAPI.Models;

namespace DemoAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class Day4PracticeController : ControllerBase
    {
        private readonly AppDbContext _context;

        public Day4PracticeController(AppDbContext context)
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

        [HttpPost]
        public IActionResult Create(Product product)
        {
            _context.Products.Add(product);
            _context.SaveChanges();

            return Ok(product);
        }

        // 查全部
        [HttpGet]
        public IActionResult GetAll()
        {
            return Ok(_context.Products.ToList());
        }

        // 查單筆
        [HttpGet("{id:int}")]
        public IActionResult Get(int id)
        {
            var product = _context.Products.Find(id);

            if (product == null)
                return NotFound();

            return Ok(product);
        }

        // 更新
        [HttpPut("{id:int}")]
        public IActionResult Update(int id, Product updatedProduct)
        {
            var product = _context.Products.Find(id);

            if (product == null)
                return NotFound();

            product.Name = updatedProduct.Name;
            product.Price = updatedProduct.Price;

            _context.SaveChanges();

            return Ok(product);
        }

        // 刪除
        [HttpDelete("{id:int}")]
        public IActionResult Delete(int id)
        {
            var product = _context.Products.Find(id);

            if (product == null)
                return NotFound();

            _context.Products.Remove(product);
            _context.SaveChanges();

            return Ok(new { message = "Deleted" });
        }

    }
}