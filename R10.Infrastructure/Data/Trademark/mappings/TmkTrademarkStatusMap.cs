using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.Trademark;

namespace R10.Infrastructure.Data.Trademark.mappings
{
    public class TmkTrademarkStatusMap : IEntityTypeConfiguration<TmkTrademarkStatus>
    {
        public void Configure(EntityTypeBuilder<TmkTrademarkStatus> builder)
        {
            builder.ToTable("tblTmkTrademarkStatus");
            builder.Property(s => s.TrademarkStatusId).ValueGeneratedOnAdd();
            builder.Property(s => s.TrademarkStatusId).UseIdentityColumn();
            builder.Property(s => s.TrademarkStatusId).Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);
            builder.HasIndex(s => s.TrademarkStatus).IsUnique();

            builder.HasMany(a => a.TmkTrademarks).WithOne(c => c.TmkTrademarkStatus).HasForeignKey(t => t.TrademarkStatus).HasPrincipalKey(t => t.TrademarkStatus);

        }
    }
}
