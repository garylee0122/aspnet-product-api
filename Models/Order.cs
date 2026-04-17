using System.ComponentModel.DataAnnotations;

namespace DemoAPI.Models
{
    public class Order
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public int TotalPrice { get; set; }

        public string Status { get; set; } = "pending";

        public List<OrderItem> Items { get; set; } = new List<OrderItem>();
    }
}