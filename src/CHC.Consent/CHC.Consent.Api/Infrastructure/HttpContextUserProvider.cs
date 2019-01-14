using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using CHC.Consent.Common.Infrastructure;
using IdentityModel;
using Microsoft.AspNetCore.Http;

namespace CHC.Consent.Api.Infrastructure
{
    public class HttpContextUserProvider : IUserProvider
    {
        private IHttpContextAccessor ContextAccessor { get; }

        /// <inheritdoc />
        public HttpContextUserProvider(IHttpContextAccessor contextAccessor)
        {
            ContextAccessor = contextAccessor;
        }

        private ClaimsPrincipal User => ContextAccessor.HttpContext?.User;

        /// <inheritdoc />
        public string UserName => User?.Identity?.Name;

        /// <inheritdoc />
        public IEnumerable<string> Roles =>
            User.Claims
                .Where(_ => _.Type == ClaimTypes.Role || _.Type == JwtClaimTypes.Role)
                .Select(_ => _.Value);
    }
}