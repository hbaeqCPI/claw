using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.PatClearance;

namespace R10.Infrastructure.Data.PatClearance.mappings
{
    public class PacClearanceCopyDisclosureSettingMap : IEntityTypeConfiguration<PacClearanceCopyDisclosureSetting>
    {
        public void Configure(EntityTypeBuilder<PacClearanceCopyDisclosureSetting> builder)
        {
            builder.ToTable("tblPacClearanceCopyDisclosureSetting");
        }
    }
}
