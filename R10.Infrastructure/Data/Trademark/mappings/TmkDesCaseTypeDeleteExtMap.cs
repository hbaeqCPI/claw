using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.Trademark;

namespace R10.Infrastructure.Data.Trademark.mappings
{
    public class TmkDesCaseTypeDeleteExtMap : IEntityTypeConfiguration<TmkDesCaseTypeDeleteExt>
    {
        public void Configure(EntityTypeBuilder<TmkDesCaseTypeDeleteExt> builder)
        {
            builder.ToTable("tblTmkDesCaseTypeDelete_Ext");
            builder.HasKey(e => new { e.IntlCode, e.CaseType, e.DesCountry, e.DesCaseType, e.IntlCodeNew, e.CaseTypeNew, e.DesCountryNew, e.DesCaseTypeNew, e.Systems });
            builder.Ignore(e => e.IsNewRecord);
            builder.Ignore(e => e.OriginalSystems);
        }
    }
}