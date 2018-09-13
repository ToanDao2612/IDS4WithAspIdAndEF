# IDS4WithAspIdAndEF
This is a more complete sample about IdentityServer4 with Asp.Net Identity and EntityFramework.Core

Key Points
===
### 1. Generate Database
Set IDS4WithAspIdAndEF Project's application arguaments `/seed` and then Debug the project to generate DB and initial data. \
And then remove the argument.
### 2. Set multiple startup projects
Set each project's action to `Start`

### 3. If user cancel or deny login
SampleMvc project need to deal with this situation
```
services.AddAuthentication(options =>
{
    options.DefaultScheme = "Cookies";
    options.DefaultChallengeScheme = "oidc";
})
.AddCookie("Cookies")
.AddOpenIdConnect("oidc", options =>
{
    options.SignInScheme = "Cookies";

    options.Authority = "https://localhost:5000";
    options.RequireHttpsMetadata = false;
    options.Events.OnRemoteFailure = ctx =>
        {
            ctx.Response.Redirect("/Home/Error?errorMessage=" + UrlEncoder.Default.Encode(ctx.Failure.Message));
            ctx.HandleResponse();
            return Task.FromResult(0);

        };

    options.ClientId = "hacc.Client";
    options.ClientSecret = "hacc.Client.Secret";
    options.ResponseType = "code id_token";

    options.SaveTokens = true;
    options.GetClaimsFromUserInfoEndpoint = true;

    options.Scope.Add("Front.API.All");
    options.Scope.Add("offline_access");
});
```
### 4. When user logout, we need to remove the granted tokens
```
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
```

### 5. Try to use Asp.Net Identity Code as more as we can
```
var builder = services.AddIdentityServer(options =>
{
    options.Events.RaiseErrorEvents = true;
    options.Events.RaiseInformationEvents = true;
    options.Events.RaiseFailureEvents = true;
    options.Events.RaiseSuccessEvents = true;

    //options.PublicOrigin = "https://localhost:5000";
    options.UserInteraction.LoginUrl = "/Identity/Account/Login";
    options.UserInteraction.LogoutUrl = "/Identity/Account/Logout";
    options.UserInteraction.ConsentUrl = "/Identity/Account/Consent";
})
```
### 5. Other detailed info
Please check and view the source code of IdentityServer4 samples
https://github.com/IdentityServer/IdentityServer4.Samples

