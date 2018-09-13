using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using IdentityServer4.Services;
using IdentityServer4.Stores;
using IdentityServer4.Models;

namespace IDS4WithAspIdAndEF.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class LoginModel : PageModel
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly ILogger<LoginModel> _logger;
        private readonly IIdentityServerInteractionService _interaction;
        private readonly IClientStore _clientStore;
        private readonly IAuthenticationSchemeProvider _schemeProvider;

        public LoginModel(SignInManager<IdentityUser> signInManager, ILogger<LoginModel> logger, IIdentityServerInteractionService interaction,
            IClientStore clientStore,
            IAuthenticationSchemeProvider schemeProvider)
        {
            _signInManager = signInManager;
            _logger = logger;
            _interaction = interaction;
            _clientStore = clientStore;
            _schemeProvider = schemeProvider;
        }

        [BindProperty]
        public InputModel Input { get; set; }       
        

        [TempData]
        public string ErrorMessage { get; set; }

        public string ReturnUrl { get; set; }
        public bool AllowRememberLogin => true;
        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public bool EnableLocalLogin { get; set; }

        public bool IsExternalLoginOnly => EnableLocalLogin == false && ExternalLogins?.Count() == 1;
        public string ExternalLoginScheme => IsExternalLoginOnly ? ExternalLogins?.SingleOrDefault()?.Name : null;

        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; }

            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            [Display(Name = "Remember me?")]
            public bool RememberMe { get; set; }            
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                ModelState.AddModelError(string.Empty, ErrorMessage);
            }

            returnUrl = returnUrl ?? Url.Content("~/");
            ReturnUrl = returnUrl;

            var schemes = await _schemeProvider.GetAllSchemesAsync();
            var context = await _interaction.GetAuthorizationContextAsync(ReturnUrl);
            if (context?.IdP != null)
            {
                var scheme = schemes.FirstOrDefault(p => p.Name == context.IdP);
                // this is meant to short circuit the UI and only trigger the one external IdP
                EnableLocalLogin = false;
                ExternalLogins = new List<AuthenticationScheme>() { new AuthenticationScheme(context.IdP, scheme.DisplayName, scheme.HandlerType) };

            }
            else
            {
                var providers = schemes.Where(x => x.DisplayName != null ||
                            (x.Name.Equals(Microsoft.AspNetCore.Server.IISIntegration.IISDefaults.AuthenticationScheme, StringComparison.OrdinalIgnoreCase)));
                var allowLocal = true;
                if (context?.ClientId != null)
                {
                    var client = await _clientStore.FindEnabledClientByIdAsync(context.ClientId);
                    if (client != null)
                    {
                        allowLocal = client.EnableLocalLogin;                       
                        if (client.IdentityProviderRestrictions != null && client.IdentityProviderRestrictions.Any())
                        {
                            providers = providers.Where(provider => client.IdentityProviderRestrictions.Contains(provider.Name)).ToList();
                        }
                    }
                }
                EnableLocalLogin = allowLocal;
                ExternalLogins = providers.ToList();
            }
            // Clear the existing external cookie to ensure a clean login process
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
        }

        public async Task<IActionResult> OnPostAsync(string button, string returnUrl = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");
            ReturnUrl = returnUrl;
            if (button != "login")
            {
                // the user clicked the "cancel" button
                var context = await _interaction.GetAuthorizationContextAsync(returnUrl);
                if (context != null)
                {
                    // if the user cancels, send a result back into IdentityServer as if they 
                    // denied the consent (even if this client does not require consent).
                    // this will send back an access denied OIDC error response to the client.
                    await _interaction.GrantConsentAsync(context, ConsentResponse.Denied);
                    
                    // we can trust model.ReturnUrl since GetAuthorizationContextAsync returned non-null
                    return Redirect(returnUrl);
                }
                else
                {
                    // since we don't have a valid context, then we just go back to the home page
                    return Redirect("~/");
                }
            }

            if (ModelState.IsValid)
            {
                if (!(_interaction.IsValidReturnUrl(ReturnUrl) || Url.IsLocalUrl(ReturnUrl)))
                {
                    ModelState.AddModelError(string.Empty, "Invalid redirect url attempt.");
                    return Page();
                }
                // This doesn't count login failures towards account lockout
                // To enable password failures to trigger account lockout, set lockoutOnFailure: true
                var result = await _signInManager.PasswordSignInAsync(Input.Email, Input.Password, Input.RememberMe, lockoutOnFailure: true);
                if (result.Succeeded)
                {
                    _logger.LogInformation("User logged in.");                   
                    return LocalRedirect(ReturnUrl);
                }
                if (result.RequiresTwoFactor)
                {
                    //return RedirectToPage("./LoginWith2fa", new { ReturnUrl = returnUrl, RememberMe = Input.RememberMe });
                    return RedirectToPage("./SendCode", new { ReturnUrl = returnUrl, RememberMe = Input.RememberMe });
                }
                if (result.IsLockedOut)
                {
                    _logger.LogWarning("User account locked out.");
                    return RedirectToPage("./Lockout");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    return Page();
                }
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }
    }
}
