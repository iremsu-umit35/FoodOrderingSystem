using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FoodOrderingSystem.Models.Entities
{
    [Table("Review")]
    public class Review
    {
        [Key]
        public int ReviewId { get; set; }
        public int? UserId { get; set; }
        public int? RestaurantId { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; }
        public DateTime CreatedAt { get; set; }


        public User User { get; set; }
        public Restaurant Restaurant { get; set; }
    }
}
