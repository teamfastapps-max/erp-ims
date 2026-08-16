using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using IMS.Models.Session;
using IMS.Services;
using IMS.Services.Interfaces;

namespace IMS.Web.Extensions
{
    public static class KeycloakExtensions
    {
        public static IServiceCollection AddKeycloakAuth(this IServiceCollection services, IConfiguration config)
        {
            var keycloak = config.GetSection("Keycloak");

            services.AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
            })
            .AddCookie(options =>
            {
                options.AccessDeniedPath = "/Home/AccessDenied";
            })
            .AddOpenIdConnect(options =>
            {
                options.Authority = keycloak["Authority"];
                options.ClientId = keycloak["ClientId"];
                options.ClientSecret = keycloak["ClientSecret"];
                options.MapInboundClaims = false;

                options.ResponseType = "code";
                options.SaveTokens = true;
                options.RequireHttpsMetadata = false;

                //  LOGIN CALLBACK
                options.CallbackPath = "/signin-oidc";

                //  LOGOUT CALLBACK 
                options.SignedOutCallbackPath = "/signout-oidc";

                options.GetClaimsFromUserInfoEndpoint = true;

                options.Scope.Clear();
                options.Scope.Add("openid");
                options.Scope.Add("profile");
                options.Scope.Add("email");
                options.Scope.Add("roles");

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    NameClaimType = "preferred_username",
                    RoleClaimType = "roles"
                };

                options.Events = new OpenIdConnectEvents
                {
                    // STORE USER + ROLES IN REDIS
                    OnTokenValidated = async context =>
                    {
                        var sessionService = context.HttpContext.RequestServices.GetRequiredService<IUserSessionService>();
                        var accessToken = context.TokenEndpointResponse?.AccessToken;
                        await sessionService.StoreUserSessionAsync(context.Principal, accessToken);

                    },

                    // FIXED LOGOUT 
                    OnRedirectToIdentityProviderForSignOut = async context =>
                    {
                        var request = context.HttpContext.Request;
                        var idToken = await context.HttpContext.GetTokenAsync("id_token");
                        if (string.IsNullOrEmpty(idToken))
                        {
                            idToken = context.Properties?.GetTokenValue("id_token");
                        }

                        var postLogoutUri = $"{request.Scheme}://{request.Host}/Home/Index";

                        var logoutUrl =
                            $"{context.Options.Authority}/protocol/openid-connect/logout" +
                            $"?client_id={context.Options.ClientId}" +
                            $"&post_logout_redirect_uri={Uri.EscapeDataString(postLogoutUri)}";

                        // Only add id_token_hint if available
                        if (!string.IsNullOrEmpty(idToken))
                        {
                            logoutUrl += $"&id_token_hint={idToken}";
                        }
                        // FORCE Keycloak logout
                        context.Response.Redirect(logoutUrl);
                        context.HandleResponse();
                    }
                };
            });

            services.AddAuthorization();
            return services;
        }
    }
}