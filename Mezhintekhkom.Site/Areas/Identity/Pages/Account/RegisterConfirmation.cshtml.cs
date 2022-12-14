// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Mezhintekhkom.Site.Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Mezhintekhkom.Site.Services;

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
            if (email == null)
            {
                return RedirectToPage("/Index");
            }
            ReturnUrl = returnUrl ?? Url.Content("~/");

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return NotFound($"Unable to load user with email '{email}'.");
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

            return Page();
        }
    }
}
