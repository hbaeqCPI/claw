using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.Trademark;

namespace R10.Infrastructure.Data.TL.mappings
{
    public class TLInfoSettingsMenuMap : IEntityTypeConfiguration<TLInfoSettingsMenu>
    {
        public void Configure(EntityTypeBuilder<TLInfoSettingsMenu> builder)
        {

            builder.ToTable("tblTLInfoSettingsMenu");
            
        }
    }
}
