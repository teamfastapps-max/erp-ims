using IMS.Services.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using System.Globalization;
using System.Text.Json;


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
                options.Events = new CookieAuthenticationEvents
                {
                    // Runs on every authenticated request, before the controller executes.
                    // If the access token stored in the cookie is at/near expiry, silently
                    // exchange the refresh_token for a new pair via Keycloak's token
                    // endpoint and re-issue the cookie - no redirect, no visible re-login.
                    OnValidatePrincipal = async context => await TryRefreshTokenAsync(context, keycloak)
                };
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
        private static async Task TryRefreshTokenAsync(CookieValidatePrincipalContext context, IConfigurationSection keycloak)
        {
            var expiresAtValue = context.Properties.GetTokenValue("expires_at");
            if (string.IsNullOrEmpty(expiresAtValue)) return; 

            var expiresAt = DateTimeOffset.Parse(expiresAtValue, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);

            // Refresh a little early (60s buffer) rather than racing the exact expiry moment.
            if (expiresAt > DateTimeOffset.UtcNow.AddSeconds(60)) return;

            var refreshToken = context.Properties.GetTokenValue("refresh_token");
            if (string.IsNullOrEmpty(refreshToken))
            {
                context.RejectPrincipal();
                context.HttpContext.Response.Cookies.Delete(CookieAuthenticationDefaults.AuthenticationScheme);
                return;
            }

            var httpClientFactory = context.HttpContext.RequestServices.GetRequiredService<IHttpClientFactory>();
            var client = httpClientFactory.CreateClient();
            var tokenEndpoint = $"{keycloak["Authority"]}/protocol/openid-connect/token";

            HttpResponseMessage response;
            try
            {
                response = await client.PostAsync(tokenEndpoint, new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    ["grant_type"] = "refresh_token",
                    ["client_id"] = keycloak["ClientId"],
                    ["client_secret"] = keycloak["ClientSecret"],
                    ["refresh_token"] = refreshToken
                }));
            }
            catch
            {
                // Network/Keycloak unreachable - don't kill the session over a transient
                // blip; let the request proceed with the (possibly stale) existing token.
                return;
            }

            if (!response.IsSuccessStatusCode)
            {
                // Refresh token itself is expired/revoked - force a real re-login.
                context.RejectPrincipal();
                await context.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                return;
            }

            var payload = await response.Content.ReadFromJsonAsync<JsonElement>();
            var newAccessToken = payload.GetProperty("access_token").GetString();
            var newRefreshToken = payload.GetProperty("refresh_token").GetString();
            var expiresIn = payload.GetProperty("expires_in").GetInt32();
            var newExpiresAt = DateTimeOffset.UtcNow.AddSeconds(expiresIn).ToString("o", CultureInfo.InvariantCulture);

            context.Properties.UpdateTokenValue("access_token", newAccessToken);
            context.Properties.UpdateTokenValue("refresh_token", newRefreshToken);
            context.Properties.UpdateTokenValue("expires_at", newExpiresAt);

            context.ShouldRenew = true; // re-issues the cookie with the updated tokens
        }
    }
}