// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Mezhintekhkom.Site.Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Mezhintekhkom.Site.Services;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Mezhintekhkom.Site.Areas.Identity.Pages.Account
{
    public class LoginWith2faModel : PageModel
    {
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

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public bool RememberMe { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public string ReturnUrl { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public class InputModel
        {
            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required]
            [StringLength(7, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Text)]
            [Display(Name = "Authenticator code")]
            public string TwoFactorCode { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Display(Name = "Remember this machine")]
            public bool RememberMachine { get; set; }
        }

        public async Task<IActionResult> OnGetAsync(bool rememberMe, string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            // Ensure the user has gone through the username & password screen first
            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();

            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Срок действия кода истек");
                return LocalRedirect(returnUrl);
                throw new InvalidOperationException($"Unable to load two-factor authentication user.");
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

            /* SMS 2fa
            string code = await _userManager.GenerateTwoFactorTokenAsync(user, "Phone");
            string phoneNumber = await _userManager.GetPhoneNumberAsync(user);
            var message = "Ваш код подтверждения: " + code;
            await _smsSender.SendSmsAsync(phoneNumber, message);
            */
            return Page();
        }

        public async Task<IActionResult> OnGetCodeAsync(string authCode, bool rememberMe, string returnUrl = null)
        {
            // Ensure the user has gone through the username & password screen first
            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();

            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Срок действия кода истек");
                return LocalRedirect(returnUrl);
                throw new InvalidOperationException($"Unable to load two-factor authentication user.");
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
                throw new InvalidOperationException($"Unable to load two-factor authentication user.");
            }

            /* Authenticator
            var authenticatorCode = Input.TwoFactorCode.Replace(" ", string.Empty).Replace("-", string.Empty);

            var result = await _signInManager.TwoFactorAuthenticatorSignInAsync(authenticatorCode, rememberMe, Input.RememberMachine);

            var userId = await _userManager.GetUserIdAsync(user);
             */

            /* SMS 2fa
            var result = await _signInManager.TwoFactorSignInAsync("Phone", Input.TwoFactorCode, rememberMe, Input.RememberMachine);
            */

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
