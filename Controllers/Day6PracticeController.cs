using Microsoft.AspNetCore.Mvc;
using DemoAPI.Data;
using DemoAPI.Models;
using DemoAPI.DTOs;
using Microsoft.EntityFrameworkCore;

namespace DemoAPI.Controllers
{
    [ApiController]
    [Route("api/day6/products")]
    public class Day6PracticeController : ControllerBase
    {
        private readonly AppDbContext _context;

        public Day6PracticeController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, UpdateProductDto dto)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound(new
                {
                    status = "error",
                    message = "Product not found"
                });
            }

            if (await _context.Products.AnyAsync(p => p.Name == dto.Name && p.Id != id))
            {
                return BadRequest(new
                {
                    status = "error",
                    message = "Name already exists"
                });
            }

            // 更新資料
            UpdateProduct(product, dto);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                status = "success",
                data = product
            });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound(new
                {
                    status = "error",
                    message = "Product not found"
                });
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                status = "success",
                message = "Product deleted"
            });
        }

        private void UpdateProduct(Product product, UpdateProductDto dto)
        {
            product.Name = dto.Name;
            product.Price = dto.Price;
            product.Stock = dto.Stock;
        }
    }
}