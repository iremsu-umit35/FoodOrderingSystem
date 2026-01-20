namespace FoodOrderingSystem.Models.ViewModels
{
    public class ReviewViewModel
    {
        // Properties for submitting a review
        public int RestaurantId { get; set; }
        public int Rating { get; set; }  // 1–5
        public string Comment { get; set; }
    }
}
