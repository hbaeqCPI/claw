using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.FormExtract;

namespace R10.Infrastructure.Data.FormExtract.mappings
{
    public class FormIFWDocTypeMap : IEntityTypeConfiguration<FormIFWDocType>
    {
        public void Configure(EntityTypeBuilder<FormIFWDocType> builder)
        {
            builder.ToTable("tblFRIFWDocType");
            builder.HasOne(d => d.FormIFWFormType).WithMany(f => f.FormIFWDocTypes).HasForeignKey(d => d.FormTypeId).HasPrincipalKey(d => d.FormTypeId);
            builder.HasMany(d=>d.RTSSearchUSIFWs).WithOne(ifw=>ifw.FormIFWDocType).HasForeignKey(d => d.DocTypeId).HasPrincipalKey(d => d.DocTypeId);
            builder.HasMany(d => d.TLSearchDocuments).WithOne(ifw => ifw.FormIFWDocType).HasForeignKey(d => d.DocTypeId).HasPrincipalKey(d => d.DocTypeId);
        }
    }
}
