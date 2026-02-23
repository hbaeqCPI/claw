using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.FormExtract;

namespace R10.Infrastructure.Data.FormExtract.mappings
{
    public class FormIFWActMapMap : IEntityTypeConfiguration<FormIFWActMap>
    {
        public void Configure(EntityTypeBuilder<FormIFWActMap> builder)
        {
            builder.ToTable("tblFRIFWActMap");
            builder.HasOne(m => m.FormIFWDocType).WithMany(d => d.FormIFWActMaps).HasForeignKey(m => m.DocTypeId).HasPrincipalKey(d => d.DocTypeId);
            builder.HasMany(m => m.FormIFWActMapPats).WithOne(p => p.FormIFWActMap).HasForeignKey(m => m.MapHdrId).HasPrincipalKey(m => m.MapHdrId);
            builder.HasMany(m => m.FormIFWActMapTmks).WithOne(p => p.FormIFWActMap).HasForeignKey(m => m.MapHdrId).HasPrincipalKey(m => m.MapHdrId);

        }
    }
}
