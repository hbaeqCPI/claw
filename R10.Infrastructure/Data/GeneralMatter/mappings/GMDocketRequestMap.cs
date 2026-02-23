using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities;
using R10.Core.Entities.Patent;

namespace R10.Infrastructure.Data.Patent.mappings
{
    public class GMDocketRequestMap : IEntityTypeConfiguration<GMDocketRequest>
    {
        public void Configure(EntityTypeBuilder<GMDocketRequest> builder)
        {
            builder.ToTable("tblGMDocketRequest");
            builder.HasOne(a => a.GMMatter).WithMany(c => c.GMDocketRequests).HasForeignKey(a => a.MatId).HasPrincipalKey(c => c.MatId);
        }
    }

    
}
