namespace FoodOrderingSystem.Models.DTOs
{
    public class UpdateOrderStatusRequest
    {
        // sipariş durumlarını güncellemek için kullanılan DTO
        public int orderId { get; set; }
        public int newStatusId { get; set; }
    }
}
