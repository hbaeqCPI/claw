using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using LawPortal.Core.Entities.Patent;

namespace LawPortal.Infrastructure.Data.Patent.mappings
{
    public class PatDesCaseTypeFieldsDeleteExtMap : IEntityTypeConfiguration<PatDesCaseTypeFieldsDeleteExt>
    {
        public void Configure(EntityTypeBuilder<PatDesCaseTypeFieldsDeleteExt> builder)
        {
            builder.ToTable("tblPatDesCaseTypeFieldsDelete_Ext");
            builder.HasKey(e => new { e.DesCaseType, e.FromField, e.ToField, e.DesCaseTypeNew, e.FromFieldNew, e.ToFieldNew, e.Systems });
            builder.Ignore(e => e.IsNewRecord);
            builder.Ignore(e => e.OriginalSystems);
        }
    }
}