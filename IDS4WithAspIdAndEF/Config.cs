using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityModel;
using IdentityServer4;
using IdentityServer4.Models;

namespace IDS4WithAspIdAndEF
{
    public class Config
    {
        // scopes define the resources in your system
        public static IEnumerable<IdentityResource> GetIdentityResources()
        {
            return new List<IdentityResource>
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile(),
                new IdentityResources.Email(),
                new IdentityResources.Phone(),
                new IdentityResources.Address(),
            };
        }

        public static IEnumerable<ApiResource> GetApiResources()
        {
            return new List<ApiResource>
            {
                new ApiResource()
                {
                    Enabled= true,
                    Name = "Front.API",
                    DisplayName = "Front API",
                    Description = "Front API, userd by Website, iOS, Android",
                    ApiSecrets = {
                        new Secret("Front.API.Secret".Sha256(), "Front.API.Secret",null)
                    },
                    UserClaims = {
                        JwtClaimTypes.Email,
                        JwtClaimTypes.Id,
                        JwtClaimTypes.PreferredUserName,
                        JwtClaimTypes.PhoneNumber,
                        JwtClaimTypes.EmailVerified,
                        JwtClaimTypes.PhoneNumberVerified
                    },
                    Scopes = {
                        new Scope()
                        {
                            Name = "Front.API.All",
                            DisplayName = "Front.API.All",
                            Description = "Front.API.All",
                            Required = true,
                            Emphasize = true,
                            ShowInDiscoveryDocument = true,
                            UserClaims = {
                                JwtClaimTypes.Name,
                                JwtClaimTypes.GivenName,
                                JwtClaimTypes.FamilyName,
                                JwtClaimTypes.Address,
                                "TwoFactorEnabled",
                                "IsOneFactorAuth",
                                "IsGoogleAuthenticatorEnabled",
                            },
                        },
                    }
                },
            };
        }

        // clients want to access resources (aka scopes)
        public static IEnumerable<Client> GetClients()
        {
            // client credentials client
            return new List<Client>
            {
                new Client
                {
                    Enabled = true,
                    ClientId = "cc.Client",
                    ClientName ="Client Credential Client",
                    AllowedGrantTypes = GrantTypes.ClientCredentials,
                    ClientSecrets =
                    {
                        new Secret("cc.Client.Secret".Sha256(), "cc.Client.Secret", null)
                    },
                    AllowedScopes =
                    {
                        "Front.API.All",
                    },
                     Properties = {
                        { "ClientType", "iOS, Android, Native Apps"}
                    },
                    AlwaysSendClientClaims = true,
                    Claims = {
                        new Claim("cc.Client.PKEY", "cc.Client.PValue"),
                    }
                },

                // resource owner password grant client
                new Client
                {
                    ClientId = "ro.Client",
                    ClientName = "Resource Owner Password Client",
                    AllowedGrantTypes = GrantTypes.ResourceOwnerPassword,

                    ClientSecrets =
                    {
                        new Secret("ro.Client.Secret".Sha256(), "ro.Client.Secret", null)
                    },
                    AllowedScopes =
                    {
                        "Front.API.All",
                        IdentityServerConstants.StandardScopes.OfflineAccess
                    },
                    AllowOfflineAccess = true,
                    AllowAccessTokensViaBrowser = true,
                    AlwaysSendClientClaims = true,
                    Properties = {
                        { "ClientType", "SPA, iOS, Android, Native Apps"}
                    },
                    Claims = {
                        new Claim("ro.Client.PKEY", "ro.Client.PValue"),
                    }
                },

                // OpenID Connect hybrid flow and client credentials client (MVC)
                new Client
                {
                    ClientId = "hacc.Client",
                    ClientName = "Hybrid And Client Credentials Client",
                    AllowedGrantTypes = GrantTypes.HybridAndClientCredentials,
                    ClientSecrets =
                    {
                        new Secret("hacc.Client.Secret".Sha256(), "hacc.Client.Secret", null)
                    },

                    RedirectUris = { "http://localhost:5002/signin-oidc" },
                    PostLogoutRedirectUris = { "http://localhost:5002/signout-callback-oidc" },
                    AlwaysSendClientClaims = true,
                    AllowedScopes =
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        "Front.API.All",
                        IdentityServerConstants.StandardScopes.OfflineAccess
                    },
                    AllowOfflineAccess = true,
                    AllowAccessTokensViaBrowser = true,
                    Properties = {
                        { "ClientType", "MVC, SPA, iOS, Android, Native Apps"}
                    },
                    Claims = {
                        new Claim("hacc.Client.PKEY", "hacc.Client.PValue"),
                    }
                }
            };
        }

    }
}
