using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.Trademark;

namespace R10.Infrastructure.Data.Trademark.mappings
{
    public class TmkTrademarkMap : IEntityTypeConfiguration<TmkTrademark>
    {
        public void Configure(EntityTypeBuilder<TmkTrademark> builder)
        {
            builder.ToTable("tblTmkTrademark");
            builder.HasIndex(t => new { t.CaseNumber, t.Country, t.SubCase }).IsUnique();
            //builder.HasOne(t => t.ParentTrademark); //.WithMany(p => p.ChildrenTrademark).HasForeignKey(t => t.ParentTmkId);
            builder.Property(p => p.SubCase).HasDefaultValue(string.Empty);
            //builder.HasMany(t => t.DocFolders).WithOne(f => f.TmkTrademark).HasForeignKey(f => f.DataKeyValue).HasPrincipalKey(t => t.TmkId);
        }
    }
}
