using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.Trademark;

namespace R10.Infrastructure.Data.Trademark.mappings
{
    public class TmkDesCaseTypeFieldsExtMap : IEntityTypeConfiguration<TmkDesCaseTypeFieldsExt>
    {
        public void Configure(EntityTypeBuilder<TmkDesCaseTypeFieldsExt> builder)
        {
            builder.ToTable("tblTmkDesCaseTypeFields_Ext");
            builder.HasNoKey();
        }
    }
}