namespace FoodOrderingSystem.Models.ViewModels
{
    // admin anasayfası view modeli home.htlm için kullanılır
    public class AdminDashboardViewModel
    {   
        public int TotalUsers { get; set; }
        public int TotalRestaurants { get; set; }
        public int PendingRestaurantOwners { get; set; }
        public int TotalOrders { get; set; }
        public int TodayOrders { get; set; }
    }
}
