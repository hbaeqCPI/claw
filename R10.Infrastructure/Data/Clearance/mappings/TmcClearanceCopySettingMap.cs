using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.Clearance;

namespace R10.Infrastructure.Data.Clearance.mappings
{
    public class TmcClearanceCopySettingMap : IEntityTypeConfiguration<TmcClearanceCopySetting>
    {
        public void Configure(EntityTypeBuilder<TmcClearanceCopySetting> builder)
        {
            builder.ToTable("tblTmcClearanceCopySetting");
        }
    }
}
