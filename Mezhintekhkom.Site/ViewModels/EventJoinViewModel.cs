using Mezhintekhkom.Site.Data.Entities;
using System.ComponentModel.DataAnnotations;

namespace Mezhintekhkom.Site.ViewModels
{
    public class EventJoinViewModel
    {
        [Display(Name = "Имя участника")]
        public string Name { get; set; }
        [Display(Name = "ИНН компании")]
        public string Inn { get; set; }
        [Required]
        [Display(Name = "Мероприятие")]
        public Event Event { get; set; }
        [Required]
        [Display(Name = "Формат участия")]
        public EventUserRole Role { get; set; }
        [Required]
        [Display(Name = "Контактный телефон")]
        [Phone(ErrorMessage = "Контактный телефон введен некорректно")]
        public string Phone { get; set; }
        [Required]
        [Display(Name = "E-mail")]
        [EmailAddress(ErrorMessage = "E-mail введен некорректно")]
        public string Email { get; set; }
        public decimal Amount { get; set; }
        [Required]
        [Compare("IsTrue", ErrorMessage = "Поле обязательно для заполнения")]
        [Display(Name = "Нажимая кнопку «Отправить заявку», я даю свое согласие на обработку моих персональных данных, в соответствии с Федеральным законом от 27.07.2006 года №152-ФЗ «О персональных данных», на условиях и для целей, определенных в Согласии на обработку персональных данных")]
        public bool IsAgree { get; set; }

        public bool IsTrue => true;
    }
}
