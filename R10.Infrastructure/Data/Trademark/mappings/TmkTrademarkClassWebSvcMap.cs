using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.Trademark;

namespace R10.Infrastructure.Data.Trademark.mappings
{
    public class TmkTrademarkClassWebSvcMap : IEntityTypeConfiguration<TmkTrademarkClassWebSvc>
    {
        public void Configure(EntityTypeBuilder<TmkTrademarkClassWebSvc> builder)
        {
            builder.ToTable("tblTmkTrademarkClassWebSvc");
            builder.Property(s => s.EntityId).ValueGeneratedOnAdd();
            builder.Property(m => m.EntityId).UseIdentityColumn();
        }
    }
}
