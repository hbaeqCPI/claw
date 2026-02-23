using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.Trademark;

namespace R10.Infrastructure.Data.TL.mappings
{
    public class TLMapActionDueMap : IEntityTypeConfiguration<TLMapActionDue>
    {
        public void Configure(EntityTypeBuilder<TLMapActionDue> builder)
        {
            builder.ToTable("tblTLMapActionDue");
        }
    }

    public class TLMapActionDueSourceMap : IEntityTypeConfiguration<TLMapActionDueSource>
    {
        public void Configure(EntityTypeBuilder<TLMapActionDueSource> builder)
        {
            builder.ToTable("tblTLMapActionDueSource");
            builder.HasMany(c => c.ActionsClose).WithOne(a => a.ActionSource).HasForeignKey(c => c.CloseSourceId);
        }
    }

    public class TLMapActionCloseMap : IEntityTypeConfiguration<TLMapActionClose>
    {
        public void Configure(EntityTypeBuilder<TLMapActionClose> builder)
        {
            builder.ToTable("tblTLMapActionClose");
        }
    }
}
