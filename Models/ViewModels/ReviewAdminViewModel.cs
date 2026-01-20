namespace FoodOrderingSystem.Models.ViewModels
{
    public class ReviewAdminViewModel
    {
        // adminin için gerekli olan review bilgileri
        public int ReviewId { get; set; }
        public int RestaurantId { get; set; }
        public string RestaurantName { get; set; }
        public int? UserId { get; set; }
        public string? UserName { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
