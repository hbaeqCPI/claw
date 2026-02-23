using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.FormExtract;

namespace R10.Infrastructure.Data.FormExtract.mappings
{
    public class FormIFWActMapTmkMap : IEntityTypeConfiguration<FormIFWActMapTmk>
    {
        public void Configure(EntityTypeBuilder<FormIFWActMapTmk> builder)
        {
            builder.ToTable("tblFRIFWActMapTmk");
        }
    }
}
