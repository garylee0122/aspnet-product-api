using DemoAPI.Data;
using DemoAPI.DTOs;
using DemoAPI.Enums;
using DemoAPI.Infrastructure.Queues;
using DemoAPI.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DemoAPI.Infrastructure.Services
{
    public class OrderService
    {
        private const int DefaultPage = 1;
        private const int PageSize = 5;

        private readonly AppDbContext _context;
        private readonly OrderQueue _orderQueue;
        private readonly ILogger<OrderService> _logger;
        private readonly RedisQueueService _redisQueue;

        public OrderService(AppDbContext context, OrderQueue orderQueue, RedisQueueService redisQueue, ILogger<OrderService> logger)
        {
            _context = context;
            _orderQueue = orderQueue;
            _redisQueue = redisQueue;
            _logger = logger;
        }

        public async Task<CreateOrderResult> Create(CreateOrderDto dto, int userId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // 檢查訂單內容是否有 item，且數量是否有效
                if (dto.Items == null || dto.Items.Count == 0)
                {
                    return CreateOrderResult.InvalidOrder("Order must contain at least one item");
                }

                // 檢查是否有商品數量小於等於 0 的 item，若有則回傳錯誤
                var invalidItem = dto.Items.FirstOrDefault(item => item.Quantity <= 0);
                if (invalidItem != null)
                {
                    return CreateOrderResult.InvalidOrder($"Product {invalidItem.ProductId} has invalid quantity");
                }

                // 從資料庫查詢所有相關的商品 ID
                var productIds = dto.Items
                    .Select(item => item.ProductId)
                    .Distinct()
                    .ToList();

                // 將商品 ID 與商品資料建立對照字典 (不存在的商品 ID 將不會出現在字典中)
                var products = await _context.Products
                    .Where(product => productIds.Contains(product.Id))
                    .ToDictionaryAsync(product => product.Id);

                // 檢查是否有任何商品 ID 在資料庫中找不到對應的商品，若有則回傳錯誤
                var missingProductId = productIds
                    .Cast<int?>()
                    .FirstOrDefault(productId => !products.ContainsKey(productId!.Value));

                if (missingProductId.HasValue)
                {
                    return CreateOrderResult.ProductNotFound(missingProductId.Value);
                }

                // 組出訂單中各項商品所需的總數量
                var requestedQuantities = dto.Items
                    .GroupBy(item => item.ProductId)
                    .ToDictionary(group => group.Key, group => group.Sum(item => item.Quantity));

                // 檢查是否有任何商品的庫存不足以滿足訂單需求，若有則回傳錯誤
                var insufficientStockProduct = requestedQuantities
                    .Select(entry => new
                    {
                        ProductId = entry.Key,
                        RequestedQuantity = entry.Value,
                        Product = products[entry.Key]
                    })
                    .FirstOrDefault(entry => entry.Product.Stock < entry.RequestedQuantity);

                if (insufficientStockProduct != null)
                {
                    return CreateOrderResult.InsufficientStock(
                        insufficientStockProduct.ProductId,
                        insufficientStockProduct.Product.Stock,
                        insufficientStockProduct.RequestedQuantity);
                }

                // 扣除庫存
                foreach (var requestedQuantity in requestedQuantities)
                {
                    products[requestedQuantity.Key].Stock -= requestedQuantity.Value;
                }

                // 建立訂單與訂單項目
                var orderItems = dto.Items
                    .Select(item =>
                    {
                        var product = products[item.ProductId];

                        return new OrderItem
                        {
                            ProductId = product.Id,
                            Price = product.Price,
                            Quantity = item.Quantity
                        };
                    })
                    .ToList();

                var order = new Order
                {
                    UserId = userId,
                    TotalPrice = orderItems.Sum(item => item.Price * item.Quantity),
                    Status = OrderStatus.Pending,
                    Items = orderItems
                };

                // 儲存訂單與更新庫存
                _context.Orders.Add(order);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Order {OrderId} created.", order.Id);

                // 將訂單加入 Queue 以供背景工作者處理
                // _orderQueue.Enqueue(new OrderQueueItem { OrderId = order.Id }); // 使用 memory queue
                await _redisQueue.EnqueueAsync(new OrderQueueItem { OrderId = order.Id }); // 使用 Redis queue
                _logger.LogInformation("Order {OrderId} enqueued to Redis queue for processing.", order.Id);

                // 提交交易
                await transaction.CommitAsync();

                return CreateOrderResult.Success(MapToOrderDto(order));
            }
            catch
            {
                // 發生任何錯誤都回滾交易，確保資料一致性
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<List<OrderDto>> GetMyOrders(int userId, int page = DefaultPage)
        {
            page = page < 1 ? DefaultPage : page;

            var orders = await _context.Orders
                .Where(order => order.UserId == userId)
                .Include(order => order.Items)
                .ThenInclude(item => item.Product)
                .OrderByDescending(order => order.Id)
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            return orders.Select(MapToOrderDto).ToList();
        }

        public async Task<OrderDto?> GetById(int id, int userId)
        {
            var order = await _context.Orders
                .Where(order => order.Id == id && order.UserId == userId)
                .Include(order => order.Items)
                .ThenInclude(item => item.Product)
                .FirstOrDefaultAsync();

            return order == null ? null : MapToOrderDto(order);
        }

        private static OrderDto MapToOrderDto(Order order)
        {
            return new OrderDto
            {
                Id = order.Id,
                TotalPrice = order.TotalPrice,
                Status = order.Status.ToString(),
                Items = order.Items.Select(item => new OrderItemDto
                {
                    ProductId = item.ProductId,
                    ProductName = item.Product?.Name ?? string.Empty,
                    Price = item.Price,
                    Quantity = item.Quantity
                }).ToList()
            };
        }
    }

    public class CreateOrderResult
    {
        public bool IsSuccess { get; init; }
        public int StatusCode { get; init; }
        public string? ErrorMessage { get; init; }
        public OrderDto? Data { get; init; }

        public static CreateOrderResult Success(OrderDto data) => new()
        {
            IsSuccess = true,
            StatusCode = StatusCodes.Status200OK,
            Data = data
        };

        public static CreateOrderResult ProductNotFound(int productId) => new()
        {
            IsSuccess = false,
            StatusCode = StatusCodes.Status404NotFound,
            ErrorMessage = $"Product {productId} not found"
        };

        public static CreateOrderResult InsufficientStock(int productId, int availableStock, int requestedQuantity) => new()
        {
            IsSuccess = false,
            StatusCode = StatusCodes.Status409Conflict,
            ErrorMessage = $"Product {productId} stock is insufficient. Available: {availableStock}, Requested: {requestedQuantity}"
        };

        public static CreateOrderResult InvalidOrder(string errorMessage) => new()
        {
            IsSuccess = false,
            StatusCode = StatusCodes.Status400BadRequest,
            ErrorMessage = errorMessage
        };
    }
}
