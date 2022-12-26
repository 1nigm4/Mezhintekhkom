#nullable disable

using Mezhintekhkom.Site.Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net;
using System.Security.Claims;

namespace Mezhintekhkom.Site.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class ExternalLoginModel : PageModel
    {
        [TempData]
        public string ErrorMessage { get; set; }
        public string ProviderDisplayName { get; set; }
        public string ReturnUrl { get; set; }
        private readonly SignInManager<User> _signInManager;
        private readonly UserManager<User> _userManager;
        private readonly IUserStore<User> _userStore;
        private readonly IUserEmailStore<User> _emailStore;
        private readonly ILogger<ExternalLoginModel> _logger;
        private readonly IWebHostEnvironment _environment;

        public ExternalLoginModel(
            SignInManager<User> signInManager,
            UserManager<User> userManager,
            IUserStore<User> userStore,
            ILogger<ExternalLoginModel> logger,
            IWebHostEnvironment environment)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = (IUserEmailStore<User>)userStore;
            _logger = logger;
            _environment = environment;
        }

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
                returnUrl = Url.Page("Manage/Index");
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
                IdentityResult createResult;
                if (existUser != null)
                {
                    createResult = await _userManager.AddLoginAsync(existUser, info);
                    if (createResult.Succeeded)
                    {
                        await _signInManager.SignInAsync(existUser, isPersistent: false, info.LoginProvider);
                        return LocalRedirect(returnUrl);
                    }
                }
                else
                {
                    var user = await CreateUserAsync(info);

                    createResult = await _userManager.CreateAsync(user);
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
                }

                if (createResult.Errors.Any(c => c.Code == "DuplicateUserName"))
                    ErrorMessage = "Пользователь с таким E-mail уже зарегистрирован";
                else
                    ErrorMessage = "Ошибка авторизации";
                return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
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
                "female" or "f" or "1" => "Женский",
                "male" or "m" or "2" => "Мужской",
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
    }
}
