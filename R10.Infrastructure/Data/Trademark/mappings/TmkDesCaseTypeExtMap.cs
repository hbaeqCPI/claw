using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.Trademark;

namespace R10.Infrastructure.Data.Trademark.mappings
{
    public class TmkDesCaseTypeExtMap : IEntityTypeConfiguration<TmkDesCaseTypeExt>
    {
        public void Configure(EntityTypeBuilder<TmkDesCaseTypeExt> builder)
        {
            builder.ToTable("tblTmkDesCaseType_Ext");
            builder.HasNoKey();
        }
    }
}