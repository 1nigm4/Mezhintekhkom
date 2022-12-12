using System.ComponentModel.DataAnnotations;

namespace Mezhintekhkom.Site.ViewModels
{
    public class LoginViewModel
    {
        [Required]
        [EmailAddress(ErrorMessage = "E-mail введен неккоректно")]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Display(Name = "Запомнить меня")]
        public bool RememberMe { get; set; }
    }
}
