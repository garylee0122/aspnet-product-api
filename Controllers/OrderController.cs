using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DemoAPI.Data;
using DemoAPI.DTOs;
using DemoAPI.Models;
using System.Security.Claims;

namespace DemoAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class OrderController : ControllerBase
    {
        private readonly AppDbContext _context;

        private readonly IConfiguration _config;

        public OrderController(AppDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateOrderDto dto)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                int totalPrice = 0;
                var orderItems = new List<OrderItem>();

                foreach (var item in dto.Items)
                {
                    var product = await _context.Products.FindAsync(item.ProductId);

                    if (product == null)
                    {
                        return NotFound($"Product {item.ProductId} not found");
                    }

                    int subtotal = product.Price * item.Quantity;
                    totalPrice += subtotal;

                    orderItems.Add(new OrderItem
                    {
                        ProductId = product.Id,
                        Price = product.Price,
                        Quantity = item.Quantity
                    });
                }

                var order = new Order
                {
                    UserId = userId,
                    TotalPrice = totalPrice,
                    Items = orderItems
                };

                _context.Orders.Add(order);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                return Ok(order);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}
