using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities;
using R10.Core.Entities.Patent;

namespace R10.Infrastructure.Data.Patent.mappings
{
    public class TmkDocketRequestMap : IEntityTypeConfiguration<TmkDocketRequest>
    {
        public void Configure(EntityTypeBuilder<TmkDocketRequest> builder)
        {
            builder.ToTable("tblTmkDocketRequest");
            builder.HasOne(a => a.TmkTrademark).WithMany(c => c.TmkDocketRequests).HasForeignKey(a => a.TmkId).HasPrincipalKey(c => c.TmkId);
        }
    }

    
}
