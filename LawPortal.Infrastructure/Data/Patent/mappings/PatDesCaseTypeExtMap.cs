using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using LawPortal.Core.Entities.Patent;

namespace LawPortal.Infrastructure.Data.Patent.mappings
{
    public class PatDesCaseTypeExtMap : IEntityTypeConfiguration<PatDesCaseTypeExt>
    {
        public void Configure(EntityTypeBuilder<PatDesCaseTypeExt> builder)
        {
            builder.ToTable("tblPatDesCaseType_Ext");
            builder.HasKey(e => new { e.IntlCode, e.CaseType, e.DesCountry, e.DesCaseType, e.Systems });
            builder.Ignore(e => e.IsNewRecord);
            builder.Ignore(e => e.OriginalSystems);
        }
    }
}
