using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.Trademark;

namespace R10.Infrastructure.Data.Trademark.mappings
{
    public class TmkDesCaseTypeFieldsMap : IEntityTypeConfiguration<TmkDesCaseTypeFields>
    {
        public void Configure(EntityTypeBuilder<TmkDesCaseTypeFields> builder)
        {
            builder.ToTable("tblTmkDesCaseTypeFields");
            builder.HasNoKey();
        }
    }
}
