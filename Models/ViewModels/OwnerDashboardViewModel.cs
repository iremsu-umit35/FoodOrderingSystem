using FoodOrderingSystem.Models.Entities;

namespace FoodOrderingSystem.Models.ViewModels
{
    //rstorant sahibi için oluşturulacak dashboard view model
    public class OwnerDashboardViewModel
    {
        public int TodayOrders { get; set; }
        public decimal TodayRevenue { get; set; }
        public int TotalOrders { get; set; }
        public decimal AverageRating { get; set; }

        public List<Order> LatestOrders { get; set; }
    }
}
