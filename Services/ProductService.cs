using DemoAPI.Data;
using DemoAPI.Models;
using DemoAPI.DTOs;
using Microsoft.EntityFrameworkCore;

namespace DemoAPI.Services
{
    public class ProductService
    {
        private const int PageSize = 5;
        private readonly AppDbContext _context;

        public ProductService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<PagedResultDto<ProductResponseDto>> GetAll(string? keyword, int page)
        {
            page = page < 1 ? 1 : page;

            var query = _context.Products.AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(p => p.Name.Contains(keyword));
            }

            var totalCount = await query.CountAsync();
            var items = await query
                .OrderBy(p => p.Id)
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .Select(p => new ProductResponseDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Price = p.Price,
                    Stock = p.Stock
                })
                .ToListAsync();

            return new PagedResultDto<ProductResponseDto>
            {
                Items = items,
                Page = page,
                PageSize = PageSize,
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling(totalCount / (double)PageSize)
            };
        }

        public async Task<ProductResponseDto?> GetById(int id)
        {
            return await _context.Products
                .Where(p => p.Id == id)
                .Select(p => new ProductResponseDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Price = p.Price,
                    Stock = p.Stock
                })
                .FirstOrDefaultAsync();
        }

        public async Task<ProductResponseDto> Create(CreateProductDto dto)
        {
            var product = new Product
            {
                Name = dto.Name,
                Price = dto.Price,
                Stock = dto.Stock
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            return new ProductResponseDto
            {
                Id = product.Id,
                Name = product.Name,
                Price = product.Price,
                Stock = product.Stock
            };
        }

        public async Task<ProductResponseDto?> Update(int id, UpdateProductDto product)
        {
            var existing = await _context.Products.FindAsync(id);

            if (existing == null) return null;

            existing.Name = product.Name;
            existing.Price = product.Price;
            existing.Stock = product.Stock;

            await _context.SaveChangesAsync();

            return new ProductResponseDto
            {
                Id = existing.Id,
                Name = existing.Name,
                Price = existing.Price,
                Stock = existing.Stock
            };
        }

        public async Task<bool> Delete(int id)
        {
            var product = await _context.Products.FindAsync(id);

            if (product == null) return false;

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}
