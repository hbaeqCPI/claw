using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.Patent;

namespace R10.Infrastructure.Data.Patent.mappings
{
    public class PatDesCaseTypeMap : IEntityTypeConfiguration<PatDesCaseType>
    {
        public void Configure(EntityTypeBuilder<PatDesCaseType> builder)
        {
            builder.ToTable("tblPatDesCaseType");
            builder.HasKey(e => new { e.IntlCode, e.CaseType, e.DesCountry, e.DesCaseType, e.Systems });
            builder.Ignore(e => e.IsNewRecord);
            builder.Ignore(e => e.OriginalSystems);
            builder.Ignore(e => e.ParentTStamp);
        }
    }
}
