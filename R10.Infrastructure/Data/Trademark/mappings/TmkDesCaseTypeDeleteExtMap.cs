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
            builder.HasNoKey();
        }
    }
}