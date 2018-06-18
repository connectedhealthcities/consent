using CHC.Consent.EFCore.Security;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CHC.Consent.Api.Infrastructure.Identity
{
    public class ConsentIdentityDbContext : IdentityDbContext<ConsentUser, ConsentRole, string>
    {
        /// <inheritdoc />
        public ConsentIdentityDbContext(DbContextOptions<ConsentIdentityDbContext> options) : base(options)
        {
        }

        /// <inheritdoc />
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            
            builder.HasDefaultSchema("Authentication");
        }
    }
}