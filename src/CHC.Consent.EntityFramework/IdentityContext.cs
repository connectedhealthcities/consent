using System;
using System.Diagnostics;
using CHC.Consent.Common.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace CHC.Consent.EntityFramework
{
    public class IdentityContext : DbContext
    {
        public IdentityContext(DbContextOptions options) : base(options)
        {
            
        }
        
        public virtual DbSet<IdentityKind> IdentityKinds { get; set; }
        public virtual DbSet<Identity> Identities { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SimpleIdentity>()
                .HasBaseType<Identity>()
                .HasDiscriminator().HasValue("s");

            modelBuilder.Entity<CompositeIdentity>()
                .HasBaseType<Identity>()
                .HasDiscriminator().HasValue("c");

            modelBuilder.Entity<CompositeIdentity>()
                .Property(_ => _.CompositeValue)
                .UsePropertyAccessMode(PropertyAccessMode.Field);
            
            
            base.OnModelCreating(modelBuilder);
        }
    }


    public class DbContextDesignerBase<T> : IDesignTimeDbContextFactory<T> where T : DbContext
    {
        public T CreateDbContext(string[] args)
        {
            var optionsBuilder =  new DbContextOptionsBuilder<T>();
            
            optionsBuilder.UseSqlite("Data Source=test.db;");
            return (T)Activator.CreateInstance(typeof(T), optionsBuilder.Options);
        }

    }

    public class IdentityContextDesigner : DbContextDesignerBase<IdentityContext>
    {
    }
}