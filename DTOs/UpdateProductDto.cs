using System.ComponentModel.DataAnnotations;

namespace DemoAPI.DTOs
{
    public class UpdateProductDto
    {
        [Required]
        [MaxLength(255)]
        public string Name { get; set; } = string.Empty;

        [Range(0, int.MaxValue)]
        public int Price { get; set; }

        [Range(0, int.MaxValue)]
        public int Stock { get; set; }
    }
}