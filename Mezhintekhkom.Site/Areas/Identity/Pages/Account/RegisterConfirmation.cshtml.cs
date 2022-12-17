#nullable disable

using Mezhintekhkom.Site.Data.Entities;
using Mezhintekhkom.Site.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;

namespace Mezhintekhkom.Site.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class RegisterConfirmationModel : PageModel
    {
        public string Email { get; set; }
        public string Code { get; set; }
        public string ReturnUrl { get; set; }
        private readonly UserManager<User> _userManager;
        private readonly IEmailSender _emailSender;

        public RegisterConfirmationModel(UserManager<User> userManager, IEmailSender emailSender)
        {
            _userManager = userManager;
            _emailSender = emailSender;
        }

        public string EmailConfirmationUrl { get; set; }

        public async Task<IActionResult> OnGetAsync(string email, string returnUrl = null)
        {
            ReturnUrl = returnUrl ??= Url.Page("./Manage/Index");
            email ??= string.Empty;
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return RedirectToPage("Login", new {
                    Caption = "Пользователь с указанным Email адресом не зарегистрирован",
                    State = "danger",
                    ReturnUrl = returnUrl });
            }

            Email = email;

            var userId = await _userManager.GetUserIdAsync(user);
            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            EmailConfirmationUrl = Url.Page(
                "/Account/ConfirmEmail",
                pageHandler: null,
                values: new { area = "Identity", id = userId, code = code, returnUrl = returnUrl },
                protocol: Request.Scheme);
            string subject = _emailSender.GetTemplateSubject(MessageType.EmailConfirmation);
            string message = _emailSender.GetTemplateBody(MessageType.EmailConfirmation, EmailConfirmationUrl);
            await _emailSender.SendEmailAsync(email, subject, message);

            return RedirectToPage("Login", new {
                Caption = $"Подтверждение отправлено по адресу {Email}",
                State = "success",
                ReturnUrl = returnUrl });
        }
    }
}
