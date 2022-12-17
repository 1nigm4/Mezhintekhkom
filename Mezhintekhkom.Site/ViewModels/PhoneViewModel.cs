using System.ComponentModel.DataAnnotations;

namespace Mezhintekhkom.Site.ViewModels
{
    public class PhoneViewModel
    {
        [Phone]
        [Display(Name = "Номер телефона")]
        public string PhoneNumber { get; set; }
    }
}
