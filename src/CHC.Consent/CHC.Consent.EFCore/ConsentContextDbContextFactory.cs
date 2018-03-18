using System.ComponentModel.Design;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;

namespace CHC.Consent.EFCore
{
    /*public class ConsentContextDbContextFactory : IDesignTimeDbContextFactory<ConsentContext>
    {
        /// <inheritdoc />
        public ConsentContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ConsentContext>();
            optionsBuilder.UseSqlServer("Data Source=chc.consent");

            return new ConsentContext(optionsBuilder.Options);
        }
    }*/
}