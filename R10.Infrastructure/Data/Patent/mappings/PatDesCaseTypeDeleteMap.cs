using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.Patent;

namespace R10.Infrastructure.Data.Patent.mappings
{
    public class PatDesCaseTypeDeleteMap : IEntityTypeConfiguration<PatDesCaseTypeDelete>
    {
        public void Configure(EntityTypeBuilder<PatDesCaseTypeDelete> builder)
        {
            builder.ToTable("tblPatDesCaseTypeDelete");
            builder.HasNoKey();
        }
    }
}