using CHC.Consent.Common.Core;
using Microsoft.EntityFrameworkCore;

namespace CHC.Consent.EntityFramework
{
    public class CoreContext : DbContext
    {
        public CoreContext(DbContextOptions options) : base(options)
        {
        }

        public virtual DbSet<Study> Studies { get; set; }
        
    }

    public class CoreContextDesigner : DbContextDesignerBase<CoreContext>
    {
        
    }
}