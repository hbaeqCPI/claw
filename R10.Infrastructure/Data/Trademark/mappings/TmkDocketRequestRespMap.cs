using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities;
using R10.Core.Entities.Patent;

namespace R10.Infrastructure.Data.Patent.mappings
{
    public class TmkDocketRequestRespMap : IEntityTypeConfiguration<TmkDocketRequestResp>
    {
        public void Configure(EntityTypeBuilder<TmkDocketRequestResp> builder)
        {
            builder.ToTable("tblTmkDocketRequestResp");
            builder.HasOne(a => a.TmkDocketRequest).WithMany(c => c.TmkDocketRequestResps).HasForeignKey(a => a.ReqId).HasPrincipalKey(c => c.ReqId);
        }
    }

    
}
