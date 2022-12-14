#nullable disable

using System;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Mezhintekhkom.Site.Data.Entities;
using AspNet.Security.OAuth.Vkontakte;
using System.Net;
using static AspNet.Security.OAuth.Vkontakte.VkontakteAuthenticationConstants;

namespace Mezhintekhkom.Site.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class ExternalLoginModel : PageModel
    {
        private readonly SignInManager<User> _signInManager;
        private readonly UserManager<User> _userManager;
        private readonly IUserStore<User> _userStore;
        private readonly IUserEmailStore<User> _emailStore;
        private readonly IEmailSender _emailSender;
        private readonly ILogger<ExternalLoginModel> _logger;
        private readonly IWebHostEnvironment _environment;

        public ExternalLoginModel(
            SignInManager<User> signInManager,
            UserManager<User> userManager,
            IUserStore<User> userStore,
            ILogger<ExternalLoginModel> logger,
            IEmailSender emailSender,
            IWebHostEnvironment environment)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _logger = logger;
            _emailSender = emailSender;
            _environment = environment;
        }

        [TempData]
        public string ErrorMessage { get; set; }
        public string ProviderDisplayName { get; set; }
        public string ReturnUrl { get; set; }
        
        public IActionResult OnGet() => RedirectToPage("./Login");

        public IActionResult OnPost(string provider, string returnUrl = null)
        {
            var redirectUrl = Url.Page("./ExternalLogin", pageHandler: "Callback", values: new { returnUrl });
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return new ChallengeResult(provider, properties);
        }

        public async Task<IActionResult> OnGetCallbackAsync(string returnUrl = null, string remoteError = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");
            if (remoteError != null)
            {
                ErrorMessage = $"Ошибка авторизации. Удаленный сервер не отвечает";
                return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
            }
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                ErrorMessage = "Ошибка авторизации";
                return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
            }

            var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: false);
            if (result.Succeeded)
            {
                _logger.LogInformation("{Name} logged in with {LoginProvider} provider.", info.Principal.Identity.Name, info.LoginProvider);
                return LocalRedirect(returnUrl);
            }
            if (result.RequiresTwoFactor)
            {
                return RedirectToPage("./LoginWith2fa", new { ReturnUrl = returnUrl });
            }
            if (result.IsLockedOut)
            {
                return RedirectToPage("./Lockout");
            }
            else
            {
                string email = info.Principal.FindFirstValue(ClaimTypes.Email) ?? string.Empty;
                var existUser = await _userManager.FindByEmailAsync(email);
                if (existUser != null)
                {
                    return RedirectToPage("RegisterConfirmation", new { email = email, returnUrl = returnUrl });
                }
                else
                {
                    var user = await CreateUserAsync(info);

                    var createResult = await _userManager.CreateAsync(user);
                    if (createResult.Succeeded)
                    {
                        createResult = await _userManager.AddLoginAsync(user, info);
                        if (createResult.Succeeded)
                        {
                            _logger.LogInformation("User created an account using {Name} provider.", info.LoginProvider);

                            var userId = await _userManager.GetUserIdAsync(user);

                            string imageUrl = info.Principal.FindFirstValue("image");
                            if (imageUrl != null)
                            {
                                if (info.LoginProvider == "Yandex")
                                    imageUrl = "https://avatars.yandex.net/get-yapic/" + $"{imageUrl}/100x100";
                                using (WebClient wc = new WebClient())
                                {
                                    string avatarUrl = $"images/avatars/{user.Id}.jpg";
                                    user.Passport.Avatar = "~/" + avatarUrl;
                                    string path = Path.Combine(_environment.WebRootPath, avatarUrl);
                                    wc.DownloadFile(imageUrl, path);
                                }
                            }

                            await _emailStore.SetEmailConfirmedAsync(user, true, CancellationToken.None);
                            await _userStore.UpdateAsync(user, CancellationToken.None);

                            await _signInManager.SignInAsync(user, isPersistent: false, info.LoginProvider);
                            return LocalRedirect(returnUrl);
                        }
                    }

                    if (createResult.Errors.Any(c => c.Code == "DuplicateUserName"))
                        ErrorMessage = "Пользователь с таким E-mail уже зарегистрирован";
                    else
                        ErrorMessage = "Ошибка авторизации";
                    return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
                }
            }
        }

        private async Task<User> CreateUserAsync(ExternalLoginInfo info)
        {
            User user = new User();
            ClaimsPrincipal claims = info.Principal;

            string email = claims.FindFirstValue(ClaimTypes.Email);
            string lastName = claims.FindFirstValue(ClaimTypes.Surname);
            string fullName = claims.FindFirstValue(ClaimTypes.GivenName);
            string[] names = fullName.Split();
            string firstName = names[0];
            string? patronymic = names.Length > 1 ? names[1] : null;
            string birth = claims.FindFirstValue(ClaimTypes.DateOfBirth);
            bool hasDate = DateTime.TryParse(birth, out DateTime date);
            DateTime? birthDate = hasDate ? date : null;
            string phoneNumber = claims.FindFirstValue(ClaimTypes.MobilePhone);
            string? gender = claims.FindFirstValue(ClaimTypes.Gender);
            gender = gender switch
            {
                "female" or "1" => "Женский",
                "male" or "2" => "Мужской",
                _ => null
            };

            Passport passport = new Passport()
            {
                LastName = lastName,
                FirstName = firstName,
                Patronymic = patronymic,
                BirthDate = birthDate,
                PhoneNumber = phoneNumber,
                Gender = gender
            };

            user.Passport = passport;

            await _userStore.SetUserNameAsync(user, email, CancellationToken.None);
            await _emailStore.SetEmailAsync(user, email, CancellationToken.None);

            return user;
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
