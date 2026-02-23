using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.PatClearance;

namespace R10.Infrastructure.Data.PatClearance.mappings
{
    public class PacClearanceCopySettingMap : IEntityTypeConfiguration<PacClearanceCopySetting>
    {
        public void Configure(EntityTypeBuilder<PacClearanceCopySetting> builder)
        {
            builder.ToTable("tblPacClearanceCopySetting");
        }
    }
}
