using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.Trademark;

namespace R10.Infrastructure.Data.Trademark.mappings
{
    public class TmkDesCaseTypeFieldsDeleteExtMap : IEntityTypeConfiguration<TmkDesCaseTypeFieldsDeleteExt>
    {
        public void Configure(EntityTypeBuilder<TmkDesCaseTypeFieldsDeleteExt> builder)
        {
            builder.ToTable("tblTmkDesCaseTypeFieldsDelete_Ext");
            builder.HasNoKey();
        }
    }
}