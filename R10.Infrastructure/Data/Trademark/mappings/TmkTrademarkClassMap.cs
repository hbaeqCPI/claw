using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.Trademark;

namespace R10.Infrastructure.Data.Trademark.mappings
{
    public class TmkTrademarkClassMap : IEntityTypeConfiguration<TmkTrademarkClass>
    {
        public void Configure(EntityTypeBuilder<TmkTrademarkClass> builder)
        {
            builder.ToTable("tblTmkTrademarkClass");
            builder.HasIndex(c => new { c.TmkId, c.ClassId }).IsUnique();
            builder.HasOne(c => c.TmkTrademark).WithMany(t => t.TrademarkClasses).HasForeignKey(c => c.TmkId).HasPrincipalKey(t => t.TmkId);
            builder.HasOne(c => c.TmkStandardGood).WithMany(sg => sg.TrademarkClasses).HasForeignKey(c => c.ClassId).HasPrincipalKey(sg => sg.ClassId);
        }
    }
}
