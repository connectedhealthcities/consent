using System;
using System.Collections.Generic;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IdentityModel;
using IdentityModel.Client;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

namespace CHC.Consent.Web.UI
{
    public class CookieAuthenticationEventsHandler : CookieAuthenticationEvents
    {
        public IAuthenticationHandlerProvider AuthenticationHandlerProvider { get; }
        public ILogger<CookieAuthenticationEventsHandler> Logger { get; }
        public ISystemClock Clock { get; }

        /// <inheritdoc />
        public CookieAuthenticationEventsHandler(
            IAuthenticationHandlerProvider authenticationHandlerProvider,
            ILogger<CookieAuthenticationEventsHandler> logger,
            ISystemClock clock)
        {
            AuthenticationHandlerProvider = authenticationHandlerProvider;
            Logger = logger;
            Clock = clock;
        }

        /// <inheritdoc />
        public override async Task ValidatePrincipal(CookieValidatePrincipalContext context)
        {
            if (!ShouldTryToRenewTicket(context))
            {
                context.ShouldRenew = true;
                return;
            }
            
            var handler = (OpenIdConnectHandler) await AuthenticationHandlerProvider.GetHandlerAsync(
                context.HttpContext,
                OpenIdConnectDefaults.AuthenticationScheme);

            var options = handler.Options;
            var config =
                await options.ConfigurationManager.GetConfigurationAsync(context.HttpContext.RequestAborted);

            var refreshToken = context.Properties.GetTokenValue(OpenIdConnectParameterNames.RefreshToken);

            if (refreshToken == null)
            {
                context.RejectPrincipal();
                return;
            }
            
            var oldAccessToken = context.Properties.GetTokenValue(OpenIdConnectParameterNames.AccessToken);

            var refreshUri = config.TokenEndpoint;
            var refreshResponse =
                await new TokenClient(refreshUri, options.ClientId, options.ClientSecret)
                    .RequestRefreshTokenAsync(refreshToken);

            if (refreshResponse.IsError)
            {
                Logger.LogInformation(
                    "Could not refresh token {0} {1} {2}",
                    refreshResponse.ErrorType,
                    refreshResponse.Error,
                    refreshResponse.ErrorDescription);
                context.RejectPrincipal();
                return;
            }
            
            var newAccessToken = refreshResponse.AccessToken;

            var validatedToken = ValidateRefreshedAccessToken(
                config,
                options,
                newAccessToken,
                oldAccessToken);

            UpdateDateProperties(context, options, validatedToken);

            UpdateToken(context, options, refreshResponse);

            context.HttpContext.Response.Headers.Add(
                "Test-RefreshToken",
                new JwtSecurityToken(refreshResponse.RefreshToken).Payload.Jti);

            context.ShouldRenew = false;
        }

        private static void UpdateDateProperties(
            CookieValidatePrincipalContext context, OpenIdConnectOptions options, SecurityToken validatedToken)
        {
            if (!options.UseTokenLifetime) return;
            
            var issued = validatedToken.ValidFrom;
            if (issued != DateTime.MinValue)
            {
                context.Properties.IssuedUtc = issued;
            }

            var expires = validatedToken.ValidTo;
            if (expires != DateTime.MinValue)
            {
                context.Properties.ExpiresUtc = expires;
            }
        }

        private void UpdateToken(CookieValidatePrincipalContext context, OpenIdConnectOptions options, TokenResponse refreshResponse)
        {
            if (!options.SaveTokens) return;
            
            context.Properties.UpdateTokenValue(
                OpenIdConnectParameterNames.AccessToken,
                refreshResponse.AccessToken);
            context.Properties.UpdateTokenValue(
                OpenIdConnectParameterNames.TokenType,
                refreshResponse.TokenType);
            context.Properties.UpdateTokenValue(
                OpenIdConnectParameterNames.RefreshToken,
                refreshResponse.RefreshToken
            );

            var value = refreshResponse.ExpiresIn;

            var expiresAt = Clock.UtcNow + TimeSpan.FromSeconds(value);
            // https://www.w3.org/TR/xmlschema-2/#dateTime
            // https://msdn.microsoft.com/en-us/library/az4se3k1(v=vs.110).aspx
            context.Properties.UpdateTokenValue(
                "expires_at",
                expiresAt.ToString("o", CultureInfo.InvariantCulture));
        }


        private bool ShouldTryToRenewTicket(CookieValidatePrincipalContext context)
        {
            var tokenExpireAt = context.Properties.GetTokenValue("expires_at");
            if (tokenExpireAt == null) return false;

            var expire = DateTimeOffset.Parse(tokenExpireAt);
            Logger.LogDebug("Token Expires in {0:F2} minutes", (expire - DateTimeOffset.Now).TotalMinutes);
            return expire < DateTimeOffset.Now.AddMinutes(1);
        }

        private SecurityToken ValidateRefreshedAccessToken(
            OpenIdConnectConfiguration config, OpenIdConnectOptions options, string newAccessToken,
            string oldAccessToken)
        {
            if (!options.SecurityTokenValidator.CanReadToken(newAccessToken))
            {
                Logger.LogError(
                    "Unable to read the 'id_token', no suitable ISecurityTokenValidator was found for: '{IdToken}'.",
                    newAccessToken);
                throw new SecurityTokenException(
                    String.Format(
                        CultureInfo.InvariantCulture,
                        "Unable to validate the 'id_token', no suitable ISecurityTokenValidator was found for: '{0}'.",
                        newAccessToken));
            }

            options.SecurityTokenValidator.ValidateToken(
                newAccessToken,
                GetTokenValidationParameters(config, options),
                out SecurityToken validatedToken);
            
            if (validatedToken == null)
            {
                Logger.LogError(
                    "Unable to validate the 'id_token', no suitable ISecurityTokenValidator was found for: '{IdToken}'.",
                    newAccessToken);
                throw new SecurityTokenException(
                    String.Format(
                        CultureInfo.InvariantCulture,
                        "Unable to validate the 'id_token', no suitable ISecurityTokenValidator was found for: '{0}'.",
                        newAccessToken));
            }

            if (!(validatedToken is JwtSecurityToken newToken))
            {
                Logger.LogError(
                    "The Validated Security Token must be of type JwtSecurityToken, but instead its type is: '{SecurityTokenType}'",
                    validatedToken?.GetType().ToString());
                throw new SecurityTokenException(
                    String.Format(
                        CultureInfo.InvariantCulture,
                        "The Validated Security Token must be of type JwtSecurityToken, but instead its type is: '{0}'.",
                        validatedToken?.GetType()));
            }

            var matchesOldToken = MatchesOldToken(oldAccessToken, newToken);
            if (!matchesOldToken)
            {
                throw new SecurityTokenException(
                    String.Format(
                        "Refreshed token does not match old token:\n\tnew: {0}\n\told: {1}",
                        newToken,
                        oldAccessToken));
            }

            return newToken;
        }

        private static TokenValidationParameters GetTokenValidationParameters(
            OpenIdConnectConfiguration config, OpenIdConnectOptions options)
        {
            var validationParameters = options.TokenValidationParameters.Clone();
            var issuer = new[] {config.Issuer};
            validationParameters.ValidIssuers =
                validationParameters.ValidIssuers?.Concat(issuer) ?? issuer;

            validationParameters.IssuerSigningKeys =
                validationParameters.IssuerSigningKeys?.Concat(config.SigningKeys)
                ?? config.SigningKeys;
            return validationParameters;
        }

        private static bool MatchesOldToken(string oldAccessToken, JwtSecurityToken newToken)
        {
            var oldToken = new JwtSecurityToken(oldAccessToken);

            var newPayload = newToken.Payload;
            var oldPayload = oldToken.Payload;
            var matchesOldToken = (newPayload.Iss == oldPayload.Iss
                                   && newPayload.Sub == oldPayload.Sub
                                   && (newPayload.AuthTime == null ||
                                       newPayload.AuthTime.Value == oldPayload.AuthTime)
                                   && (newPayload.Azp == oldPayload.Azp));
            return matchesOldToken;
        }
    }
}