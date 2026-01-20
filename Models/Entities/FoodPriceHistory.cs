namespace FoodOrderingSystem.Models.Entities
{
    public class FoodPriceHistory
    {
        public int Id { get; set; }

        // DB'de NULL olduğu için nullable
        public int? FoodId { get; set; }
        public Food? Food { get; set; }

        // DB'de int + NULL
        public int? OldPrice { get; set; }
        public int? NewPrice { get; set; }

        // DB'de datetime2 + NULL
        public DateTime? ChangedAt { get; set; }
    }
}
