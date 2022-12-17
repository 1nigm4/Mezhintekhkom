using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace Mezhintekhkom.Site.ViewModels
{
    public class Login2faViewModel
    {
        [Required]
        [StringLength(7, ErrorMessage = "Код подтверждения не может быть меньше {2} цифр", MinimumLength = 6)]
        [DataType(DataType.Text)]
        [Display(Name = "Код подтверждения")]
        public string TwoFactorCode { get; set; }
        [Display(Name = "Запомнить устройство")]
        public bool RememberMachine { get; set; }
    }
}
