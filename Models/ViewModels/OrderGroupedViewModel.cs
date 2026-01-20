using FoodOrderingSystem.Models.Entities;

namespace FoodOrderingSystem.Models.ViewModels
{
    // adimn tarafında  siparişlerin durumlarına göre gruplanması için kullanılan ViewModel
    public class OrderGroupedViewModel
    {
        public List<Order> Preparing { get; set; }
        public List<Order> OnTheWay { get; set; }
        public List<Order> Delivered { get; set; }
        public List<Order> Cancelled { get; set; }
    }

}
