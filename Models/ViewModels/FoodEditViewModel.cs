using FoodOrderingSystem.Models.Entities;

namespace FoodOrderingSystem.Models.ViewModels
{
    //yemek editleme sayfası için view modelS
    public class FoodEditViewModel
    {// Yemek bilgileri
        public Food Food { get; set; }

        // Fiyat geçmişi
        public List<FoodPriceHistory> PriceHistory { get; set; }
    }
}
