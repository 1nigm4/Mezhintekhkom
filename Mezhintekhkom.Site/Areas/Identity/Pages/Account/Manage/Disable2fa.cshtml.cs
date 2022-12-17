#nullable disable

using Mezhintekhkom.Site.Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Mezhintekhkom.Site.Areas.Identity.Pages.Account.Manage
{
    public class Disable2faModel : PageModel
    {
        private readonly UserManager<User> _userManager;
        private readonly ILogger<Disable2faModel> _logger;

        public Disable2faModel(
            UserManager<User> userManager,
            ILogger<Disable2faModel> logger)
        {
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<IActionResult> OnGetAsync(string returnUrl = null)
        {
            returnUrl = returnUrl ?? Url.Page("TwoFactorAuthentication");
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (await _userManager.GetTwoFactorEnabledAsync(user))
            {
                var disable2faResult = await _userManager.SetTwoFactorEnabledAsync(user, false);
                if (!disable2faResult.Succeeded)
                {
                    throw new InvalidOperationException($"Unexpected error occurred disabling 2FA.");
                }

                _logger.LogInformation("User with ID '{UserId}' has disabled 2fa.", _userManager.GetUserId(User));
                return RedirectToPage("./TwoFactorAuthentication");
            }

            return LocalRedirect(returnUrl);
        }
    }
}
