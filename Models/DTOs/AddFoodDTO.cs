using System.ComponentModel.DataAnnotations;

namespace FoodOrderingSystem.Models.DTOs
{
    public class AddFoodDTO
    {
        [Required]
        public string FoodName { get; set; }

        public string Description { get; set; }

        [Required]
        public string Price { get; set; }
    }
}
