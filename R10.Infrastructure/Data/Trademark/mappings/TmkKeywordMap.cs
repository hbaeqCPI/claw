using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.Trademark;

namespace R10.Infrastructure.Data.Trademark.mappings
{
    public class TmkKeywordMap : IEntityTypeConfiguration<TmkKeyword>
    {
        public void Configure(EntityTypeBuilder<TmkKeyword> builder)
        {
            builder.ToTable("tblTmkKeyword");
            builder.HasIndex(k => new { k.TmkId, k.Keyword }).IsUnique();
            builder.HasOne(k => k.TmkTrademark).WithMany(t => t.Keywords).HasForeignKey(t => t.TmkId).HasPrincipalKey(t => t.TmkId);
        }
    }
}
