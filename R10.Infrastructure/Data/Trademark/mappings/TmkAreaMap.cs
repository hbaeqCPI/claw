using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.Trademark;

namespace R10.Infrastructure.Data.Trademark.mappings
{
    public class TmkAreaMap : IEntityTypeConfiguration<TmkArea>
    {
        public void Configure(EntityTypeBuilder<TmkArea> builder)
        {
            builder.ToTable("tblTmkArea");
            builder.HasIndex(a => a.Area).IsUnique();
            builder.HasMany(a => a.TmkAreaCountries).WithOne(ca => ca.Area).HasPrincipalKey(a => a.AreaID).HasForeignKey(ca => ca.AreaID);
        }
    }
}
