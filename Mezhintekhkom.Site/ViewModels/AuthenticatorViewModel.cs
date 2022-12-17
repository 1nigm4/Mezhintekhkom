using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace Mezhintekhkom.Site.ViewModels
{
    public class AuthenticatorViewModel
    {
        [Required]
        [StringLength(7, ErrorMessage = "Длина кода не менее {2} цифр", MinimumLength = 6)]
        [DataType(DataType.Text)]
        [Display(Name = "Код подтверждения")]
        public string Code { get; set; }
    }
}
