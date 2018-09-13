using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityModel;
using IdentityServer4.Extensions;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace IDS4WithAspIdAndEF.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class LogoutModel : PageModel
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly ILogger<LogoutModel> _logger;
        private readonly IIdentityServerInteractionService _interaction;

        public LogoutModel(SignInManager<IdentityUser> signInManager, ILogger<LogoutModel> logger, IIdentityServerInteractionService interaction)
        {
            _signInManager = signInManager;
            _logger = logger;
            _interaction = interaction;
        }

        public string LogoutId { get; set; }
        public string ReturnUrl { get; set; }
        public bool ShowLogoutPrompt { get; set; }

        public async Task<IActionResult> OnGetAsync(string logoutId)
        {
            LogoutId = logoutId;
            var context = await _interaction.GetLogoutContextAsync(logoutId);
            if (User?.Identity.IsAuthenticated == true)
            {
                ShowLogoutPrompt = false;
            }
            if (context?.ShowSignoutPrompt == false)
            {
                // it's safe to automatically sign-out
                ShowLogoutPrompt = false;
            }
            if (ShowLogoutPrompt)
            {
                return Page();
            }
            return await OnPost(logoutId, null);
        }
        

        public async Task<IActionResult> OnPost(string logoutId, string returnUrl)
        {            
            if (!string.IsNullOrWhiteSpace(logoutId))
            {
                // get context information (client name, post logout redirect URI and iframe for federated signout)
                var logout = await _interaction.GetLogoutContextAsync(logoutId);
                await _interaction.RevokeTokensForCurrentSessionAsync();
                returnUrl = logout?.PostLogoutRedirectUri;
            }

            await _signInManager.SignOutAsync();
            _logger.LogInformation("User logged out.");

            returnUrl = returnUrl ?? Url.Content("~/");
            if (Url.IsLocalUrl(returnUrl))
            {
                return LocalRedirect(returnUrl);
            }
            else
            {
                return Redirect(returnUrl);
            }
        }
    }
}