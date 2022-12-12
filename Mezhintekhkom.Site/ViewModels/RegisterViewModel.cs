using System.ComponentModel.DataAnnotations;

namespace Mezhintekhkom.Site.ViewModels
{
    public class RegisterViewModel
    {
        [Required]
        [EmailAddress(ErrorMessage = "E-mail введен неккоректно")]
        public string Email { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "Длина пароля менее {2} символов", MinimumLength = 6)]
        [Display(Name = "Пароль")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Пароли не совпадают")]
        public string ConfirmPassword { get; set; }
    }
}
