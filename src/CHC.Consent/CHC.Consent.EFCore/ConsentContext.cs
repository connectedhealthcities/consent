using CHC.Consent.Common.Consent;
using CHC.Consent.EFCore.Configuration;
using CHC.Consent.EFCore.Consent;
using CHC.Consent.EFCore.Entities;
using Microsoft.EntityFrameworkCore;

namespace CHC.Consent.EFCore
{
    public class ConsentContext : DbContext
    {
        /// <inheritdoc />
        public ConsentContext(DbContextOptions<ConsentContext> options) : base(options)
        {
        }

        /// <inheritdoc />
        public ConsentContext(string connectionString) : base(CreateSqlServerOptions(connectionString))
        {
        }

        private static DbContextOptions CreateSqlServerOptions(string connectionString)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ConsentContext>();
            optionsBuilder.UseSqlServer(connectionString);
            return optionsBuilder.Options;
        }

        public virtual DbSet<PersonEntity> People { get; set; }
        public virtual DbSet<StudyEntity> Studies { get; set; }
        
        /// <inheritdoc />
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new StudyConfiguration());
            modelBuilder.ApplyConfiguration(new StudySubjectEntityConfiguration());
            modelBuilder.ApplyConfiguration(new ConsentEntityConfiguration());
            //TODO: Get these from somewhere else configurable
            modelBuilder.ApplyConfiguration(new PersonEntityConfiguration());
            modelBuilder.ApplyConfiguration(new PersonIdentifierEntityConfiguration());
            
            
        }
    }
}