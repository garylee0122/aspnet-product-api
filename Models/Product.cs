using System.ComponentModel.DataAnnotations;
namespace DemoAPI.Models
{
    public class Product
    {
        public int Id { get; set; }
        
        [Required]
        [MaxLength(255)]
        public required string Name { get; set; } = string.Empty;

        [Range(0, int.MaxValue)]
        public int Price { get; set; }

        [Range(0, int.MaxValue)]
        public int Stock { get; set; }
    }
}