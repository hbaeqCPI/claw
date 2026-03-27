using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.Patent;

namespace R10.Infrastructure.Data.Patent.mappings
{
    public class PatDesCaseTypeFieldsDeleteMap : IEntityTypeConfiguration<PatDesCaseTypeFieldsDelete>
    {
        public void Configure(EntityTypeBuilder<PatDesCaseTypeFieldsDelete> builder)
        {
            builder.ToTable("tblPatDesCaseTypeFieldsDelete");
            builder.HasNoKey();
        }
    }
}