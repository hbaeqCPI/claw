using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.Patent;

namespace R10.Infrastructure.Data.Patent.mappings
{
    public class PatDesCaseTypeDeleteMap : IEntityTypeConfiguration<PatDesCaseTypeDelete>
    {
        public void Configure(EntityTypeBuilder<PatDesCaseTypeDelete> builder)
        {
            builder.ToTable("tblPatDesCaseTypeDelete");
            builder.HasKey(e => new { e.IntlCode, e.CaseType, e.DesCountry, e.DesCaseType, e.IntlCodeNew, e.CaseTypeNew, e.DesCountryNew, e.DesCaseTypeNew, e.Systems });
            builder.Ignore(e => e.IsNewRecord);
            builder.Ignore(e => e.OriginalSystems);
        }
    }
}