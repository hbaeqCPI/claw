using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.FormExtract;

namespace R10.Infrastructure.Data.FormExtract.mappings
{
    public class FormIFWActMapPatMap : IEntityTypeConfiguration<FormIFWActMapPat>
    {
        public void Configure(EntityTypeBuilder<FormIFWActMapPat> builder)
        {
            builder.ToTable("tblFRIFWActMapPat");
        }
    }
}
