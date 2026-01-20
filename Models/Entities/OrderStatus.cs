using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FoodOrderingSystem.Models.Entities
{
    [Table("OrderStatus")]
    public class OrderStatus
    {
        [Key]
        public int OrderStatusId { get; set; }
        public string StatusName { get; set; }

        // Navigation property
        public ICollection<Order> Orders { get; set; }
    }
}
