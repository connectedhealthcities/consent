using System.Threading.Tasks;
using CHC.Consent.EFCore.Security;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CHC.Consent.Api.Infrastructure.Web
{
    [UsedImplicitly]
    public class ConsentUserSignInManager : SignInManager<ConsentUser>
    {
        /// <inheritdoc />
        public ConsentUserSignInManager(
            UserManager<ConsentUser> userManager,
            IHttpContextAccessor contextAccessor,
            IUserClaimsPrincipalFactory<ConsentUser> claimsFactory,
            IOptions<IdentityOptions> optionsAccessor,
            ILogger<SignInManager<ConsentUser>> logger,
            IAuthenticationSchemeProvider schemes) : base(
            userManager,
            contextAccessor,
            claimsFactory,
            optionsAccessor,
            logger,
            schemes)
        {
        }

        /// <inheritdoc />
        public override Task<bool> CanSignInAsync(ConsentUser user)
        {
            return user.Deleted == null ? base.CanSignInAsync(user) : Task.FromResult(false);
        }
    }
}