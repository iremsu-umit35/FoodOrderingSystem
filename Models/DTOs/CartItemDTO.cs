namespace FoodOrderingSystem.Models.DTOs
{
    public class CartItemDTO
    {
        public int FoodId { get; set; }
        public string FoodName { get; set; }
        
        public decimal Price { get; set; }
        public int Quantity { get; set; }
    }
}
