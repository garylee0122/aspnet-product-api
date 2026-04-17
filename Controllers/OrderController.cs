using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DemoAPI.Data;
using DemoAPI.DTOs;
using DemoAPI.Models;
using System.Security.Claims;
using DemoAPI.Helpers;

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
                        Product = product, // 🔥 關聯設定
                        Price = product.Price,
                        Quantity = item.Quantity
                    });
                }

                var order = new Order
                {
                    UserId = userId,
                    TotalPrice = totalPrice,
                    Status = "pending",
                    Items = orderItems
                };

                _context.Orders.Add(order);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                return Ok(new ApiResponse<object>
                {
                    Data = order
                });
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetMyOrders()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            int page = 1;
            int pageSize = 5;

            var orders = await _context.Orders
                .Where(o => o.UserId == userId) // 🔥 權限控制
                .Include(o => o.Items)
                .ThenInclude(i => i.Product) // 🔥 關聯載入
                .Skip((page - 1) * pageSize) // 🔥 分頁 -> 跳過前面的筆數
                .Take(pageSize) // 🔥 分頁 -> 取出頁面大小的筆數
                .OrderByDescending(o => o.Id)
                .ToListAsync();

            var orderDtos = orders.Select(o => new OrderDto
            {
                Id = o.Id,
                TotalPrice = o.TotalPrice,
                Status = o.Status,
                Items = o.Items.Select(i => new OrderItemDto
                {
                    ProductId = i.ProductId,
                    ProductName = i.Product?.Name ?? "", // 🔥 取產品名稱
                    Price = i.Price,
                    Quantity = i.Quantity
                }).ToList()
            }).ToList();

            return Ok(new ApiResponse<object>
            {
                Data = orderDtos
            });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrder(int id)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var order = await _context.Orders
                .Where(o => o.Id == id && o.UserId == userId) // 🔥 雙條件
                .Include(o => o.Items)
                .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync();

            if (order == null)
            {
                return NotFound(new
                {
                    status = "error",
                    message = "Order not found"
                });
            }

            var orderDto = new OrderDto
            {
                Id = order.Id,
                TotalPrice = order.TotalPrice,
                Status = order.Status,
                Items = order.Items.Select(i => new OrderItemDto
                {
                    ProductId = i.ProductId,
                    ProductName = i.Product?.Name ?? "",
                    Price = i.Price,
                    Quantity = i.Quantity
                }).ToList()
            };

            return Ok(new ApiResponse<object>
            {
                Data = orderDto
            });
        }
    }
}
