namespace FoodOrderingSystem.Models.ViewModels
{
    public class OwnerRestaurantSettingsViewModel
    {
        // restornant sahibinin restoran ayarlari icin view model
        public int RestaurantId { get; set; }
        public string RestaurantName { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
        public string Description { get; set; }
        public int? MinOrderAmount { get; set; }
        public bool IsOpen { get; set; }
    }
}
