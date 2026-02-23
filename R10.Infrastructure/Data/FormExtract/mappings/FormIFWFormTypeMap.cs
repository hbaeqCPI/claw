using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.FormExtract;

namespace R10.Infrastructure.Data.FormExtract.mappings
{
    public class FormIFWFormTypeMap : IEntityTypeConfiguration<FormIFWFormType>
    {
        public void Configure(EntityTypeBuilder<FormIFWFormType> builder)
        {
            builder.ToTable("tblFRIFWFormType");
        }
    }
}
