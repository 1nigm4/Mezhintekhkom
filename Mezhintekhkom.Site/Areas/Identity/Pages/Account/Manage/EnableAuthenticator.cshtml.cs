#nullable disable

using Mezhintekhkom.Site.Data.Entities;
using Mezhintekhkom.Site.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Mezhintekhkom.Site.Areas.Identity.Pages.Account.Manage
{
    public class EnableAuthenticatorModel : PageModel
    {
        [BindProperty]
        public AuthenticatorViewModel Input { get; set; }
        [TempData]
        public string StatusMessage { get; set; }
        [TempData]
        public string[] RecoveryCodes { get; set; }
        public string SharedKey { get; set; }
        public string AuthenticatorUri { get; set; }
        private readonly UserManager<User> _userManager;
        private readonly ILogger<EnableAuthenticatorModel> _logger;

        public EnableAuthenticatorModel(
            UserManager<User> userManager,
            ILogger<EnableAuthenticatorModel> logger)
        {
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<IActionResult> OnGetAsync(string returnUrl)
        {
            returnUrl ??= Url.Page("TwoFactorAuthentication");
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            bool isTwoFactorEnabled = await _userManager.GetTwoFactorEnabledAsync(user);
            if (isTwoFactorEnabled) return LocalRedirect(returnUrl);

            var result = await _userManager.SetTwoFactorEnabledAsync(user, true);
            if (result.Succeeded)
            {
                _logger.LogInformation($"User {user.UserName} set 2fa to enabled");
            }
            return LocalRedirect(returnUrl);
        }
    }
}
