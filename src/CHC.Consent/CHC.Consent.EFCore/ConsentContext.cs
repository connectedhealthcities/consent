using Microsoft.EntityFrameworkCore;

namespace CHC.Consent.EFCore
{
    public class ConsentContext : DbContext
    {
        /// <inheritdoc />
        public ConsentContext(DbContextOptions<ConsentContext> options) : base(options)
        {
        }


        public virtual DbSet<PersonEntity> People { get; set; }

        /// <inheritdoc />
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PersonEntity>().Ignore(_ => _.BirthOrder);
            modelBuilder.Entity<PersonEntity>().Property(_ => _.BirthOrderValue).HasColumnName(nameof(PersonEntity.BirthOrder));
        }
    }
}