using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities;
using R10.Core.Entities.Patent;

namespace R10.Infrastructure.Data.Patent.mappings
{
    public class GMDocketRequestRespMap : IEntityTypeConfiguration<GMDocketRequestResp>
    {
        public void Configure(EntityTypeBuilder<GMDocketRequestResp> builder)
        {
            builder.ToTable("tblGMDocketRequestResp");
            builder.HasOne(a => a.GMDocketRequest).WithMany(c => c.GMDocketRequestResps).HasForeignKey(a => a.ReqId).HasPrincipalKey(c => c.ReqId);
        }
    }    
}
