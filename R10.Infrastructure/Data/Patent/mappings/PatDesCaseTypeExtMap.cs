using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.Patent;

namespace R10.Infrastructure.Data.Patent.mappings
{
    public class PatDesCaseTypeExtMap : IEntityTypeConfiguration<PatDesCaseTypeExt>
    {
        public void Configure(EntityTypeBuilder<PatDesCaseTypeExt> builder)
        {
            builder.ToTable("tblPatDesCaseType_Ext");
            builder.HasNoKey();
        }
    }
}