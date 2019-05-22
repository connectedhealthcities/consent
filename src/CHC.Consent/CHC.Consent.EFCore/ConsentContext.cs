using System.Linq;
using CHC.Consent.EFCore.Configuration;
using CHC.Consent.EFCore.Consent;
using CHC.Consent.EFCore.Entities;
using CHC.Consent.EFCore.Identity;
using CHC.Consent.EFCore.Security;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CHC.Consent.EFCore
{
    public class ConsentContext : IdentityDbContext<ConsentUser, ConsentRole, long>
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
        public virtual DbSet<SubjectIdentifierEntity> SubjectIdentifiers { get; set; }
        public virtual DbSet<PersonIdentifierEntity> PersonIdentifiers { get; set; }
        
        public virtual DbSet<EvidenceDefinitionEntity> EvidenceDefinition { get; set; }
        public virtual DbSet<IdentifierDefinitionEntity> IdentifierDefinition { get; set; }

        /// <inheritdoc />
        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            if(!ChangeTracker.AutoDetectChangesEnabled) return base.SaveChanges(acceptAllChangesOnSuccess);

            var newStudies = ChangeTracker.Entries<StudyEntity>().Where(_ => _.State == EntityState.Added).ToList();
            var result = base.SaveChanges(acceptAllChangesOnSuccess);

            foreach (var study in newStudies)
            {
                study.Entity.ACL.Description = $"Study {study.Entity.Id}";
            }

            base.SaveChanges(acceptAllChangesOnSuccess);

            return result;
        }

        /// <inheritdoc />
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {            
            base.OnModelCreating(modelBuilder);
            //TODO: Do we need to get (some of) these from somewhere else configurable
            
            modelBuilder.ApplyConfiguration(new StudyConfiguration());
            modelBuilder.ApplyConfiguration(new StudySubjectEntityConfiguration());
            modelBuilder.ApplyConfiguration(new ConsentEntityConfiguration());
            var evidenceEntityConfiguration = new EvidenceEntityConfiguration();
            modelBuilder.ApplyConfiguration<EvidenceEntity>(evidenceEntityConfiguration);
            modelBuilder.ApplyConfiguration<GivenEvidenceEntity>(evidenceEntityConfiguration);
            modelBuilder.ApplyConfiguration<WithdrawnEvidenceEntity>(evidenceEntityConfiguration);
            
            modelBuilder.ApplyConfiguration(new PersonEntityConfiguration());
            modelBuilder.ApplyConfiguration(new PersonIdentifierEntityConfiguration());

            modelBuilder.ApplyConfiguration(new SubjectIdentifierEntityConfiguration());


            modelBuilder.ApplyConfiguration(new AccessControlEntityConfiguration());
            modelBuilder.ApplyConfiguration(new PermissionEntityConfiguration());
            modelBuilder.ApplyConfiguration(new AccessControlListConfiguration());

            modelBuilder.ApplyConfiguration(new AuthorityEntityConfiguration());
            
            modelBuilder.Entity<SecurityPrinicipal>();
            
            modelBuilder.Entity<UserSecurityPrincipal>().Property<long>("ConsentUserId");
            modelBuilder.Entity<UserSecurityPrincipal>().HasOne(_ => _.User).WithOne(_ => _.Principal)
                .HasForeignKey<UserSecurityPrincipal>("ConsentUserId")
                .HasPrincipalKey<ConsentUser>();
                
            modelBuilder.Entity<RoleSecurityPrincipal>().Property<long>("ConsentRoleId");
            modelBuilder.Entity<RoleSecurityPrincipal>()
                .HasOne(_ => _.Role).WithOne(_ => _.Principal)
                .HasPrincipalKey<ConsentRole>()
                .HasForeignKey<RoleSecurityPrincipal>("ConsentRoleId");

            foreach (var securableEntity in modelBuilder.Model.GetEntityTypes()
                .Where(_ => _.BaseType == null)
                .Where(_ => typeof(ISecurable).IsAssignableFrom(_.ClrType)))
            {
                var type = securableEntity.ClrType;
                var entityBuilder = modelBuilder.Entity(type);
                entityBuilder.Property<long>("AccessControlListId").IsRequired();
                entityBuilder.HasOne(typeof(AccessControlList), nameof(ISecurable.ACL)).WithOne()
                    .HasPrincipalKey(typeof(AccessControlList), nameof(AccessControlList.Id))
                    .HasForeignKey(type, "AccessControlListId")
                    .IsRequired();
            }
            
            AddDefinitionEntity<EvidenceDefinitionEntity>(modelBuilder);
            AddDefinitionEntity<IdentifierDefinitionEntity>(modelBuilder);
            
        }

        private static void AddDefinitionEntity<TDefinition>(ModelBuilder modelBuilder) where TDefinition : class, IDefinitionEntity
        {
            modelBuilder.Entity<TDefinition>()
                .HasIndex(_ => _.Name).IsUnique();
                
            modelBuilder.Entity<TDefinition>()
                .Property(_ => _.Name)
                .IsRequired()
                .HasMaxLength(255);
                
            modelBuilder.Entity<TDefinition>()
                .Property(_ => _.Definition).IsRequired().HasMaxLength(int.MaxValue);
        }
    }
}