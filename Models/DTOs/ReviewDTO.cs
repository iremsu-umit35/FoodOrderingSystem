namespace FoodOrderingSystem.Models.DTOs
{
    public class ReviewDTO
    {
        public int ReviewId { get; set; }
        public int? UserId { get; set; }
        public string UserFullName { get; set; }
        public int? RestaurantId { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; }
        public DateTime CreatedAt { get; set; }

        public string ReplyText { get; set; }
        public DateTime? RepliedAt { get; set; }


    }
}
