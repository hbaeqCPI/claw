using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities;
using R10.Core.Entities.Patent;

namespace R10.Infrastructure.Data.Patent.mappings
{
    public class PatDocketRequestRespMap : IEntityTypeConfiguration<PatDocketRequestResp>
    {
        public void Configure(EntityTypeBuilder<PatDocketRequestResp> builder)
        {
            builder.ToTable("tblPatDocketRequestResp");
            builder.HasOne(a => a.PatDocketRequest).WithMany(c => c.PatDocketRequestResps).HasForeignKey(a => a.ReqId).HasPrincipalKey(c => c.ReqId);
        }
    }    
}
