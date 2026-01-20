using FoodOrderingSystem.Models.DTOs;
using FoodOrderingSystem.Models.Entities;

namespace FoodOrderingSystem.Models.ViewModels
{
    public class RestaurantDetailsViewModel
    {
        //user yemek detay sayfası için gerekli olan view model
        public Restaurant Restaurant { get; set; }
        public List<Food> Foods { get; set; }

        public List<CartItemDTO> CartItems { get; set; }
        public decimal CartTotal { get; set; }
        // ⭐ Yorum listeleme için eklenen satır:
        public List<ReviewDTO> Reviews { get; set; }
    }
}
