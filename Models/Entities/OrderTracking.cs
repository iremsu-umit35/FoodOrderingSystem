using System.ComponentModel.DataAnnotations;

namespace FoodOrderingSystem.Models.Entities
{
    public class OrderTracking
    {
        [Key]
        public int TrackingId { get; set; }
        public int OrderId { get; set; }
        public string StatusName { get; set; }
        public DateTime TrackingTime { get; set; }

        // Navigation Property (opsiyonel)
        public Order Order { get; set; }
    }
}
