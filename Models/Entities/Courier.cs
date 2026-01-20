using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FoodOrderingSystem.Models.Entities
{
    [Table("Courier")]
    public class Courier
    {
        [Key]
        public int CourierId { get; set; }
        public string FullName { get; set; }
        public string Phone { get; set; }
        public DateTime CreatedAt { get; set; }

        // Navigation properties
        public ICollection<Order> Orders { get; set; }
    }
}
