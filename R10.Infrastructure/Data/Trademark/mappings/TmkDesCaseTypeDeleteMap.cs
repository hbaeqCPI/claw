using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.Trademark;

namespace R10.Infrastructure.Data.Trademark.mappings
{
    public class TmkDesCaseTypeDeleteMap : IEntityTypeConfiguration<TmkDesCaseTypeDelete>
    {
        public void Configure(EntityTypeBuilder<TmkDesCaseTypeDelete> builder)
        {
            builder.ToTable("tblTmkDesCaseTypeDelete");
            builder.HasNoKey();
        }
    }
}