using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using LawPortal.Core.Entities.Patent;

namespace LawPortal.Infrastructure.Data.Patent.mappings
{
    public class PatDesCaseTypeFieldsDeleteMap : IEntityTypeConfiguration<PatDesCaseTypeFieldsDelete>
    {
        public void Configure(EntityTypeBuilder<PatDesCaseTypeFieldsDelete> builder)
        {
            builder.ToTable("tblPatDesCaseTypeFieldsDelete");
            builder.HasKey(e => new { e.DesCaseType, e.FromField, e.ToField, e.DesCaseTypeNew, e.FromFieldNew, e.ToFieldNew, e.Systems });
            builder.Ignore(e => e.IsNewRecord);
            builder.Ignore(e => e.OriginalSystems);
        }
    }
}