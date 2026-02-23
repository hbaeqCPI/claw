using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.Trademark;

namespace R10.Infrastructure.Data.Trademark.mappings
{
    public class TmkLicenseeMap : IEntityTypeConfiguration<TmkLicensee>
    {
        public void Configure(EntityTypeBuilder<TmkLicensee> builder)
        {
            builder.ToTable("tblTmkLicensee");
            builder.HasIndex(l => new { l.TmkId, l.Licensee });
            builder.HasIndex(l => l.Licensee);
            builder.HasIndex(l => l.Licensor);
            builder.HasOne(l => l.TmkTrademark).WithMany(t => t.Licensees).HasForeignKey(l => l.TmkId).HasPrincipalKey(t => t.TmkId);
        }
    }
}
