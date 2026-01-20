using FoodOrderingSystem.Models.DTOs;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FoodOrderingSystem.Models.ViewModels
{
    public class CheckoutViewModel
    {
        // Formdan gelmeyen, sadece görüntü amaçlı
        [ValidateNever]
        public List<CartItemDTO> CartItems { get; set; }

        [ValidateNever]
        public decimal CartTotal { get; set; }

        [ValidateNever]
        public string RestaurantName { get; set; }

        [ValidateNever]
        public int RestaurantId { get; set; }


        // Kullanıcıdan gelenler (form inputu)
        [Required(ErrorMessage = "Teslimat adresi girmek zorunludur.")]
        public string DeliveryAddress { get; set; }

        [Required(ErrorMessage = "Ödeme yöntemi seçmek zorunludur.")]
        public string PaymentMethod { get; set; }
    }
}
