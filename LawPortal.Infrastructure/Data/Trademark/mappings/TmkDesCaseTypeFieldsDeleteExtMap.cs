using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using LawPortal.Core.Entities.Trademark;

namespace LawPortal.Infrastructure.Data.Trademark.mappings
{
    public class TmkDesCaseTypeFieldsDeleteExtMap : IEntityTypeConfiguration<TmkDesCaseTypeFieldsDeleteExt>
    {
        public void Configure(EntityTypeBuilder<TmkDesCaseTypeFieldsDeleteExt> builder)
        {
            builder.ToTable("tblTmkDesCaseTypeFieldsDelete_Ext");
            builder.HasKey(e => new { e.DesCaseType, e.FromField, e.ToField, e.DesCaseTypeNew, e.FromFieldNew, e.ToFieldNew, e.Systems });
            builder.Ignore(e => e.IsNewRecord);
            builder.Ignore(e => e.OriginalSystems);
        }
    }
}