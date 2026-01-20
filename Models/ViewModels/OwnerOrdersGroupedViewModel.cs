using FoodOrderingSystem.Models.Entities;
using System.Collections.Generic;

namespace FoodOrderingSystem.Models.ViewModels
{
    // resotarn sahibinin siparişleri gurpladığı view model
    public class OwnerOrdersGroupedViewModel
    {
        public List<Order> Preparing { get; set; }
        public List<Order> OnTheWay { get; set; }
        public List<Order> Delivered { get; set; }
        public List<Order> Cancelled { get; set; }
    }
}
