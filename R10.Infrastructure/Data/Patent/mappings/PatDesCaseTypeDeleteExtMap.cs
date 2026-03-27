using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.Patent;

namespace R10.Infrastructure.Data.Patent.mappings
{
    public class PatDesCaseTypeDeleteExtMap : IEntityTypeConfiguration<PatDesCaseTypeDeleteExt>
    {
        public void Configure(EntityTypeBuilder<PatDesCaseTypeDeleteExt> builder)
        {
            builder.ToTable("tblPatDesCaseTypeDelete_Ext");
            builder.HasNoKey();
        }
    }
}