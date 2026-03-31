using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.Trademark;

namespace R10.Infrastructure.Data.Trademark.mappings
{
    public class TmkDesCaseTypeFieldsDeleteMap : IEntityTypeConfiguration<TmkDesCaseTypeFieldsDelete>
    {
        public void Configure(EntityTypeBuilder<TmkDesCaseTypeFieldsDelete> builder)
        {
            builder.ToTable("tblTmkDesCaseTypeFieldsDelete");
            builder.HasKey(e => new { e.DesCaseType, e.FromField, e.ToField, e.DesCaseTypeNew, e.FromFieldNew, e.ToFieldNew, e.Systems });
            builder.Ignore(e => e.IsNewRecord);
            builder.Ignore(e => e.OriginalSystems);
        }
    }
}