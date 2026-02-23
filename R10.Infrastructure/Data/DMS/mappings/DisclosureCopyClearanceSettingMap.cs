using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.DMS;


namespace R10.Infrastructure.Data.DMS.mappings
{
    public class DisclosureCopyClearanceSettingMap : IEntityTypeConfiguration<DisclosureCopyClearanceSetting>
    {
        public void Configure(EntityTypeBuilder<DisclosureCopyClearanceSetting> builder)
        {
            builder.ToTable("tblDMSDisclosureCopyClearanceSetting");
        }
    }
   
}
