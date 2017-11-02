using System.Security.Claims;
using CHC.Consent.Security;
using CHC.Consent.WebApi.Abstractions;
using Microsoft.AspNetCore.Http;

namespace CHC.Consent.WebApi.Infrastructure
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
        public IAuthenticatable GetUser()
        {
            var (issuer, subject) = GetJwtCredentials();
            return users.FindUserBy(issuer, subject);
        }

        public (string issuer, string subject) GetJwtCredentials()
        {
            var webUser = contextAccessor.HttpContext.User;
            return (issuer:webUser.FindFirstValue("iss"),subject: webUser.FindFirstValue("sub"));
        }
    }
}