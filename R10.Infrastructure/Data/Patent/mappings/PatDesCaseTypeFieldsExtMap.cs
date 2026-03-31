using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.Patent;

namespace R10.Infrastructure.Data.Patent.mappings
{
    public class PatDesCaseTypeFieldsExtMap : IEntityTypeConfiguration<PatDesCaseTypeFieldsExt>
    {
        public void Configure(EntityTypeBuilder<PatDesCaseTypeFieldsExt> builder)
        {
            builder.ToTable("tblPatDesCaseTypeFields_Ext");
            builder.HasKey(e => new { e.DesCaseType, e.FromField, e.ToField, e.Systems });
            builder.Ignore(e => e.IsNewRecord);
            builder.Ignore(e => e.OriginalSystems);
        }
    }
}
