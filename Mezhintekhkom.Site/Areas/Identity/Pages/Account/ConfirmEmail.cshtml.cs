#nullable disable

using Mezhintekhkom.Site.Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;

namespace Mezhintekhkom.Site.Areas.Identity.Pages.Account
{
    public class ConfirmEmailModel : PageModel
    {
        [TempData]
        public string StatusMessage { get; set; }
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;

        public ConfirmEmailModel(UserManager<User> userManager, SignInManager<User> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public async Task<IActionResult> OnGetAsync(string id, string code, string returnUrl = null)
        {
            returnUrl ??= Url.Page("./Manage/Index");

            if (id == null || code == null)
            {
                return LocalRedirect(returnUrl);
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                await _signInManager.SignOutAsync();
                return RedirectToPage();
            }

            code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
            var result = await _userManager.ConfirmEmailAsync(user, code);
            return RedirectToPage("./Login", new {
                Caption = result.Succeeded ? "Адрес электронной почты подтвержден" : "Ошибка подтверждения почты",
                State = result.Succeeded ? "success" : "danger",
                ReturnUrl = returnUrl });
        }
    }
}
