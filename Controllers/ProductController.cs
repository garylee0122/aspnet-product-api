using Microsoft.AspNetCore.Mvc;
using DemoAPI.Services;
using DemoAPI.DTOs;
using DemoAPI.Helpers;

namespace DemoAPI.Controllers
{
    [ApiController]
    [Route("api/products")]
    public class ProductController : ControllerBase
    {
        private readonly ProductService _service;

        public ProductController(ProductService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? keyword, [FromQuery] int page = 1)
        {
            var data = await _service.GetAll(keyword, page);

            return Ok(new ApiResponse<PagedResultDto<ProductResponseDto>>
            {
                Data = data
            });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var data = await _service.GetById(id);

            if (data == null)
            {
                return NotFound(new ApiResponse<string>
                {
                    Status = "error",
                    Message = "Product not found"
                });
            }

            return Ok(new ApiResponse<ProductResponseDto>
            {
                Data = data
            });
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateProductDto dto)
        {
            var data = await _service.Create(dto);

            return Ok(new ApiResponse<ProductResponseDto>
            {
                Data = data
            });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, UpdateProductDto dto)
        {
            var data = await _service.Update(id, dto);

            return Ok(new ApiResponse<ProductResponseDto>
            {
                Data = data
            });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _service.Delete(id);

            if (!result)
            {
                return NotFound(new ApiResponse<string>
                {
                    Status = "error",
                    Message = "Product not found"
                });
            }

            return Ok(new ApiResponse<string>
            {
                Status = "success",
                Message = "Product deleted successfully"
            });
        }
    }
}
