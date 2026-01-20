using System.ComponentModel.DataAnnotations;
//model sadece login formu için kullanılacak, SP veya DB modelinden bağımsız.
namespace FoodOrderingSystem.Models.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Required Email.")]
        [EmailAddress(ErrorMessage = "Geçerli bir email giriniz.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Required pssword.")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }

}
