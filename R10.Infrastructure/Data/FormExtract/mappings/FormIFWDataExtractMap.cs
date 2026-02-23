using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.FormExtract;

namespace R10.Infrastructure.Data.FormExtract.mappings
{
    public class FormIFWDataExtractMap : IEntityTypeConfiguration<FormIFWDataExtract>
    {
        public void Configure(EntityTypeBuilder<FormIFWDataExtract> builder)
        {
            builder.ToTable("tblFRIFWExtract");
        }
    }
}
