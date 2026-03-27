using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.Trademark;

namespace R10.Infrastructure.Data.Trademark.mappings
{
    public class TmkCaseTypeMap : IEntityTypeConfiguration<TmkCaseType>
    {
        public void Configure(EntityTypeBuilder<TmkCaseType> builder)
        {
            builder.ToTable("tblTmkCaseType");
            builder.HasNoKey();
        }
    }
}
