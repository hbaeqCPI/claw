using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.Trademark;

namespace R10.Infrastructure.Data.Trademark.mappings
{
    public class TmkConflictMap : IEntityTypeConfiguration<TmkConflict>
    {
        public void Configure(EntityTypeBuilder<TmkConflict> builder)
        {
            builder.ToTable("tblTmkConflict");
            builder.HasIndex(c => new { c.TmkId, c.ConflictOppNumber }).IsUnique();
            builder.HasIndex(c => new { c.CaseNumber, c.Country, c.SubCase, c.ConflictOppNumber }).IsUnique();

            builder.HasOne(c => c.TmkCountry).WithMany(ctry => ctry.TmkConflicts).HasForeignKey(c => c.Country).HasPrincipalKey(ctry => ctry.Country);
        }
    }
}
