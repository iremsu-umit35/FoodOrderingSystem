using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FoodOrderingSystem.Models.Entities
{
    [Table("Restaurant")]
    public class Restaurant
    {
        [Key]
        public int RestaurantId { get; set; }
        public int? UserId { get; set; } // Restoran sahibi
        public string RestaurantName { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
        public string? Description { get; set; }
        public int? MinOrderAmount { get; set; }
        public bool? IsOpen { get; set; }  // nullable yapıyoruz
        public decimal AverageRating { get; set; }
        public string? ImageUrl { get; set; }


        // Navigation properties
        public User User { get; set; } // Restoran sahibi
        public ICollection<Food> Foods { get; set; }
        public ICollection<Order> Orders { get; set; }
        public ICollection<Review> Reviews { get; set; }
    }
}
