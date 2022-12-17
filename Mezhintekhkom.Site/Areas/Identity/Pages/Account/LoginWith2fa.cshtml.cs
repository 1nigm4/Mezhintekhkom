#nullable disable

using Mezhintekhkom.Site.Data.Entities;
using Mezhintekhkom.Site.Services;
using Mezhintekhkom.Site.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Mezhintekhkom.Site.Areas.Identity.Pages.Account
{
    public class LoginWith2faModel : PageModel
    {
        [BindProperty]
        public Login2faViewModel Input { get; set; }
        public bool RememberMe { get; set; }
        public string ReturnUrl { get; set; }
        private readonly SignInManager<User> _signInManager;
        private readonly UserManager<User> _userManager;
        private readonly ILogger<LoginWith2faModel> _logger;
        private readonly IEmailSender _emailSender;

        public LoginWith2faModel(
            SignInManager<User> signInManager,
            UserManager<User> userManager,
            ILogger<LoginWith2faModel> logger,
            IEmailSender smsSender)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _logger = logger;
            _emailSender = smsSender;
        }

        public async Task<IActionResult> OnGetAsync(bool rememberMe = false, string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();

            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Срок действия кода истек");
                return LocalRedirect(returnUrl);
            }

            ReturnUrl = returnUrl;
            RememberMe = rememberMe;

            string provider = _emailSender.GetProvider();
            string code = await _userManager.GenerateTwoFactorTokenAsync(user, provider);
            string callbackUrl = Url.Page(
                "/Account/LoginWith2fa",
                pageHandler: "Code",
                values: new {
                    area = "Identity",
                    authCode = code,
                    rememberMe = rememberMe,
                    returnUrl = returnUrl
                },
                Request.Scheme);
            string email = await _userManager.GetEmailAsync(user);
            string subject = _emailSender.GetTemplateSubject(MessageType.TwoFactorVerification);
            string message = _emailSender.GetTemplateBody(MessageType.TwoFactorVerification, code, callbackUrl);
            await _emailSender.SendEmailAsync(email, subject, message);
            return Page();
        }

        public async Task<IActionResult> OnGetCodeAsync(string authCode, bool rememberMe, string returnUrl = null)
        {
            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();

            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Срок действия кода истек");
                return LocalRedirect(returnUrl);
            }

            ReturnUrl = returnUrl;
            RememberMe = rememberMe;

            var result = await _signInManager.TwoFactorSignInAsync("Email", authCode, rememberMe, false);

            if (result.Succeeded)
            {
                _logger.LogInformation("User with ID '{UserId}' logged in with 2fa.", user.Id);
                return LocalRedirect(returnUrl);
            }
            else if (result.IsLockedOut)
            {
                _logger.LogWarning("User with ID '{UserId}' account locked out.", user.Id);
                return RedirectToPage("./Lockout");
            }
            else
            {
                _logger.LogWarning("Invalid authenticator code entered for user with ID '{UserId}'.", user.Id);
                ModelState.AddModelError(string.Empty, "Неверный код подтверждениянн");
                return Page();
            }
        }


        public async Task<IActionResult> OnPostAsync(bool rememberMe, string returnUrl = null)
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            returnUrl = returnUrl ?? Url.Content("~/");

            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Срок действия кода истек");
                return LocalRedirect(returnUrl);
            }

            var result = await _signInManager.TwoFactorSignInAsync("Email", Input.TwoFactorCode, rememberMe, Input.RememberMachine);

            if (result.Succeeded)
            {
                _logger.LogInformation("User with ID '{UserId}' logged in with 2fa.", user.Id);
                return LocalRedirect(returnUrl);
            }
            else if (result.IsLockedOut)
            {
                _logger.LogWarning("User with ID '{UserId}' account locked out.", user.Id);
                return RedirectToPage("./Lockout");
            }
            else
            {
                _logger.LogWarning("Invalid authenticator code entered for user with ID '{UserId}'.", user.Id);
                ModelState.AddModelError(string.Empty, "Неверный код подтверждениянн");
                return Page();
            }
        }
    }
}
