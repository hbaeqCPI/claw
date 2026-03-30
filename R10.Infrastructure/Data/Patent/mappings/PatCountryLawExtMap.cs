using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.Patent;

namespace R10.Infrastructure.Data.Patent.mappings
{
    public class PatCountryLawExtMap : IEntityTypeConfiguration<PatCountryLawExt>
    {
        public void Configure(EntityTypeBuilder<PatCountryLawExt> builder)
        {
            builder.ToTable("tblPatCountryLaw_Ext");
            builder.HasKey(e => new { e.Country, e.CaseType, e.Systems });
            builder.Ignore(e => e.IsNewRecord);
            builder.Ignore(e => e.OriginalSystems);
        }
    }
}
