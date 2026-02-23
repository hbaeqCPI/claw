using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.GeneralMatter;


namespace R10.Infrastructure.Data.GeneralMatter.mappings
{
    public class GMMatterCopySettingMap : IEntityTypeConfiguration<GMMatterCopySetting>
    {
        public void Configure(EntityTypeBuilder<GMMatterCopySetting> builder)
        {
            builder.ToTable("tblGMMatterCopySetting");
        }
    }
   
}
