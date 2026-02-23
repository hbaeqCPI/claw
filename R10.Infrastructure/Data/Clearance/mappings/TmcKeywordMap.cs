using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.Clearance;

namespace R10.Infrastructure.Data.Clearance.mappings
{
    public class TmcKeywordMap : IEntityTypeConfiguration<TmcKeyword>
    {
        public void Configure(EntityTypeBuilder<TmcKeyword> builder)
        {
            builder.ToTable("tblTmcKeyword");
            builder.HasIndex(k => new { k.TmcId, k.Keyword }).IsUnique();
            builder.HasOne(k => k.Clearance).WithMany(t => t.Keywords).HasForeignKey(k => k.TmcId).HasPrincipalKey(t => t.TmcId);
        }
    }
}