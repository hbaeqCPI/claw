using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.FormExtract;

namespace R10.Infrastructure.Data.FormExtract.mappings
{
    public class FormSourceMap : IEntityTypeConfiguration<FormSource>
    {
        public void Configure(EntityTypeBuilder<FormSource> builder)
        {
            builder.ToTable("tblFRControlSource");
        }
    }
}
