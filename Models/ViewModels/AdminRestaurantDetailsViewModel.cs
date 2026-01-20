using FoodOrderingSystem.Models.Entities;

namespace FoodOrderingSystem.Models.ViewModels
{
    //rstorant yöbetimi admin tarafı modeli 
    public class AdminRestaurantDetailsViewModel
    {
        public Restaurant Restaurant { get; set; }
        public User Owner { get; set; }

        public List<Food> Foods { get; set; }
        public List<Review> Reviews { get; set; }

        public int TotalOrders { get; set; }
        public int TotalReviews { get; set; }
    }
}
