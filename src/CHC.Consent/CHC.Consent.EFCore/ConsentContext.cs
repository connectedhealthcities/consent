using CHC.Consent.Common.Consent;
using CHC.Consent.EFCore.Configuration;
using CHC.Consent.EFCore.Consent;
using CHC.Consent.EFCore.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

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
            //TODO: Do we need to get (some of) these from somewhere else configurable
            
            modelBuilder.ApplyConfiguration(new StudyConfiguration());
            modelBuilder.ApplyConfiguration(new StudySubjectEntityConfiguration());
            modelBuilder.ApplyConfiguration(new ConsentEntityConfiguration());
            modelBuilder.ApplyConfiguration(new CaseIdentifierEntityConfiguration());
            var evidenceEntityConfiguration = new EvidenceEntityConfiguration();
            modelBuilder.ApplyConfiguration<EvidenceEntity>(evidenceEntityConfiguration);
            modelBuilder.ApplyConfiguration<GivenEvidenceEntity>(evidenceEntityConfiguration);
            modelBuilder.ApplyConfiguration<WithdrawnEvidenceEntity>(evidenceEntityConfiguration);
            
            modelBuilder.ApplyConfiguration(new PersonEntityConfiguration());
            modelBuilder.ApplyConfiguration(new PersonIdentifierEntityConfiguration());
            
            
        }
    }
}