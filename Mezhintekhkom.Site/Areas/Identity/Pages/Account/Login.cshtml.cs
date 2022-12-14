#nullable disable

using Mezhintekhkom.Site.Data.Entities;
using Mezhintekhkom.Site.Services;
using Mezhintekhkom.Site.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;

namespace Mezhintekhkom.Site.Areas.Identity.Pages.Account
{
    public class AuthModel : PageModel
    {
        [BindProperty]
        public LoginViewModel LoginInput { get; set; }

        [BindProperty]
        public RegisterViewModel RegisterInput { get; set; }

        [TempData]
        public string Caption { get; set; }
        [TempData]
        public string ErrorMessage { get; set; }
        public IList<AuthenticationScheme> ExternalLogins { get; set; }
        public string ReturnUrl { get; set; }

        private readonly SignInManager<User> _signInManager;
        private readonly UserManager<User> _userManager;
        private readonly IUserStore<User> _userStore;
        private readonly IUserEmailStore<User> _emailStore;
        private readonly ILogger<AuthModel> _logger;
        private readonly IEmailSender _emailSender;

        public AuthModel(
            SignInManager<User> signInManager,
            UserManager<User> userManager,
            IUserStore<User> userStore,
            ILogger<AuthModel> logger,
            IEmailSender emailSender)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _logger = logger;
            _emailSender = emailSender;
        }

        public async Task OnGetAsync(string caption = null, string returnUrl = null)
        {
            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                ModelState.AddModelError(string.Empty, ErrorMessage);
            }

            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            Caption = caption;
            ReturnUrl = returnUrl ?? Url.Content("~/");
        }

        public async Task<IActionResult> OnPostLoginAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl ?? Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            var context = new ValidationContext(LoginInput, serviceProvider: null, items: null);
            var validationResults = new List<ValidationResult>();

            bool isValid = Validator.TryValidateObject(LoginInput, context, validationResults, true);
            if (isValid)
            {
                var result = await _signInManager.PasswordSignInAsync(LoginInput.Email, LoginInput.Password, LoginInput.RememberMe, lockoutOnFailure: false);
                if (result.Succeeded)
                {
                    _logger.LogInformation("User logged in.");
                    return LocalRedirect(returnUrl);
                }
                if (result.RequiresTwoFactor)
                {
                    return RedirectToPage("./LoginWith2fa", new { ReturnUrl = returnUrl, RememberMe = LoginInput.RememberMe });
                }
                if (result.IsNotAllowed)
                {
                    User user = await _userManager.FindByEmailAsync(LoginInput.Email);
                    if (user != null)
                    {
                        var isValidPass = await _userManager.CheckPasswordAsync(user, LoginInput.Password);
                        if (isValidPass)
                            return RedirectToPage("RegisterConfirmation", new { email = LoginInput.Email, returnUrl = returnUrl });
                    }
                }
                if (result.IsLockedOut)
                {
                    _logger.LogWarning("User account locked out.");
                    return RedirectToPage("./Lockout");
                }
                else
                {
                    foreach (var field in ModelState.Where(v => !v.Key.Contains("LoginInput")))
                        ModelState.Remove(field.Key);
                    ModelState.AddModelError(string.Empty, "Неверный логин или пароль");
                    return Page();
                }
            }

            return Page();
        }

        public async Task<IActionResult> OnPostRegisterAsync(string returnUrl)
        {
            returnUrl ??= Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            var context = new ValidationContext(RegisterInput, serviceProvider: null, items: null);
            var validationResults = new List<ValidationResult>();

            bool isValid = Validator.TryValidateObject(RegisterInput, context, validationResults, true);
            if (isValid)
            {
                var user = CreateUser();
                user.Passport = new Passport();

                await _userStore.SetUserNameAsync(user, RegisterInput.Email, CancellationToken.None);
                await _emailStore.SetEmailAsync(user, RegisterInput.Email, CancellationToken.None);
                var result = await _userManager.CreateAsync(user, RegisterInput.Password);

                if (result.Succeeded)
                {
                    _logger.LogInformation("User created a new account with password.");

                    if (_userManager.Options.SignIn.RequireConfirmedAccount)
                    {
                        return RedirectToPage("RegisterConfirmation", new { email = RegisterInput.Email, returnUrl = returnUrl });
                    }
                    else
                    {
                        await _signInManager.SignInAsync(user, isPersistent: false);
                        return LocalRedirect(returnUrl);
                    }
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                ViewData["IsRegErrors"] = true;
            }

            return Page();
        }

        private User CreateUser()
        {
            try
            {
                return Activator.CreateInstance<User>();
            }
            catch
            {
                throw new InvalidOperationException($"Can't create an instance of '{nameof(User)}'. " +
                    $"Ensure that '{nameof(User)}' is not an abstract class and has a parameterless constructor, or alternatively " +
                    $"override the register page in /Areas/Identity/Pages/Account/Register.cshtml");
            }
        }

        private IUserEmailStore<User> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("The default UI requires a user store with email support.");
            }
            return (IUserEmailStore<User>)_userStore;
        }
    }
}
