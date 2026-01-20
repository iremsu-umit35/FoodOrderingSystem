using FoodOrderingSystem.Models.Entities;

namespace FoodOrderingSystem.Models.ViewModels
{
    // ViewModel to represent detailed information about an order for the restorant owner
    public class OwnerOrderDetailViewModel
    {
        public Order Order { get; set; }
        public List<OrderDetail> Items { get; set; }
        public List<OrderTracking> Tracking { get; set; }
        public Payment Payment { get; set; }
    }
}
