using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IDS4WithAspIdAndEF.Areas.Identity.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;

namespace IDS4WithAspIdAndEF.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class SendCodeModel : PageModel
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<SendCodeModel> _logger;
        private readonly IEmailSender _emailSender;
        private readonly ISmsSender _smsSender;

        public SendCodeModel(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            ILogger<SendCodeModel> logger,
            IEmailSender emailSender,
            ISmsSender smsSender)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
            _smsSender = smsSender;
        }

        [BindProperty]
        public string SelectedProvider { get; set; }

        public ICollection<SelectListItem> Providers { get; set; }

        public string ReturnUrl { get; set; }

        public bool RememberMe { get; set; }

        public async Task<IActionResult> OnGetAsync(string returnUrl = null, bool rememberMe = false)
        {
            returnUrl = returnUrl ?? Url.Content("~/");
            ReturnUrl = returnUrl;
            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();

            if (user == null)
            {
                throw new InvalidOperationException($"Unable to load two-factor authentication user.");
            }
            var userFactors = await _userManager.GetValidTwoFactorProvidersAsync(user);

            var factorOptions = userFactors.Select(purpose => new SelectListItem { Text = purpose, Value = purpose }).ToList();

            RememberMe = rememberMe;
            Providers = factorOptions;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            if (string.IsNullOrWhiteSpace(SelectedProvider))
            {
                return Page();
            }
            returnUrl = returnUrl ?? Url.Content("~/");
            ReturnUrl = returnUrl;
            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
            if (user == null)
            {
                throw new InvalidOperationException($"Unable to load two-factor authentication user.");
            }

            if (SelectedProvider == "Authenticator")
            {
                return RedirectToPage("./LoginWith2fa", new { ReturnUrl = returnUrl, RememberMe = RememberMe });
            }

            // Generate the token and send it
            var code = await _userManager.GenerateTwoFactorTokenAsync(user, SelectedProvider);
            if (string.IsNullOrWhiteSpace(code))
            {
                throw new InvalidOperationException($"Unable to generate Code for the selected provider.");
            }

            var message = "Your security code is: " + code;
            _logger.LogInformation(message);
            if (SelectedProvider == "Email")
            {
                await _emailSender.SendEmailAsync(await _userManager.GetEmailAsync(user), "Security Code", message);
            }
            else if (SelectedProvider == "Phone")
            {
                await _smsSender.SendSmsAsync(await _userManager.GetPhoneNumberAsync(user), message);
            }

            return RedirectToPage("./VerifyCode", new { Provider = SelectedProvider, ReturnUrl = returnUrl, RememberMe = RememberMe });
        }
    }
}