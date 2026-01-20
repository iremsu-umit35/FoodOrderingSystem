namespace FoodOrderingSystem.Models.ViewModels
{
    public class UserDetailView
    {
        // kullancı detaylarını ve istatistiklerini tutan ViewModel
        public int UserId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public string Role { get; set; }
        public bool IsApproved { get; set; }

        public int TotalOrders { get; set; }
        public int TotalReviews { get; set; }
        public int TotalRestaurants { get; set; }
    }
}
