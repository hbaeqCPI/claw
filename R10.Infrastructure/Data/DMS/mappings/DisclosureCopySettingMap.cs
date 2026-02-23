using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.DMS;


namespace R10.Infrastructure.Data.DMS.mappings
{
    public class DisclosureCopySettingMap : IEntityTypeConfiguration<DisclosureCopySetting>
    {
        public void Configure(EntityTypeBuilder<DisclosureCopySetting> builder)
        {
            builder.ToTable("tblDMSDisclosureCopySetting");
        }
    }
   
}
