using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.Trademark;

namespace R10.Infrastructure.Data.TL.mappings
{
    public class TLInfoSettingsMenuCountryMap : IEntityTypeConfiguration<TLInfoSettingsMenuCountry>
    {
        public void Configure(EntityTypeBuilder<TLInfoSettingsMenuCountry> builder)
        {

            builder.ToTable("tblTLInfoSettingsMenuCountry");
            builder.HasOne(m => m.InfoSettingsMenu).WithMany(s => s.CountryInfoSettings).HasForeignKey(s => s.InfoMenuId).HasPrincipalKey(m => m.InfoMenuId);

        }
    }
}
