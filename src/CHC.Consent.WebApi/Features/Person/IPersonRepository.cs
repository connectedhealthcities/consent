using System;
using System.Security.Claims;
using CHC.Consent.Common.Core;
using CHC.Consent.Security;
using CHC.Consent.WebApi.Abstractions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;

namespace CHC.Consent.WebApi.Features.Person
{
    public class HttpContextUserAccessor : IUserAccessor
    {
        private readonly IHttpContextAccessor contextAccessor;
        private readonly IJwtIdentifiedUserRepository users;

        public HttpContextUserAccessor(IHttpContextAccessor contextAccessor, IJwtIdentifiedUserRepository users)
        {
            this.contextAccessor = contextAccessor;
            this.users = users;
        }

        /// <inheritdoc />
        public IUser GetUser()
        {
            var webUser = contextAccessor.HttpContext.User;
            return users.FindUserBy(webUser.FindFirstValue("iss"), webUser.FindFirstValue("sub"));
        }
    }
}