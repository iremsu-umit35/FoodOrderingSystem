using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FoodOrderingSystem.Models.Entities
{
    [Table("User")]
    public class User
    {
        [Key]
        public int UserId { get; set; }

        [Required]
        public string FullName { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }

        [Required]
        public string Role { get; set; }  // Customer / RestaurantOwner / Admin

        [Required]
        public string Phone { get; set; }

        [Required]
        public string Address { get; set; }

        public DateTime? CreatedAt { get; set; }

        public bool IsApproved { get; set; } = false;

        // Navigation properties (nullable ve boş liste ile başlatılmış)
        public ICollection<Restaurant>? Restaurants { get; set; } = new List<Restaurant>();
        public ICollection<Order>? Orders { get; set; } = new List<Order>();
        public ICollection<Cart>? Carts { get; set; } = new List<Cart>();
        public ICollection<Review>? Reviews { get; set; } = new List<Review>();
    }
}
