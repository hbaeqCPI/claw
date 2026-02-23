using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities;
using R10.Core.Entities.Patent;

namespace R10.Infrastructure.Data.Patent.mappings
{
    public class PatDocketRequestMap : IEntityTypeConfiguration<PatDocketRequest>
    {
        public void Configure(EntityTypeBuilder<PatDocketRequest> builder)
        {
            builder.ToTable("tblPatDocketRequest");
            builder.HasOne(a => a.CountryApplication).WithMany(c => c.PatDocketRequests).HasForeignKey(a => a.AppId).HasPrincipalKey(c => c.AppId);
        }
    }

    public class PatDocketInvRequestMap : IEntityTypeConfiguration<PatDocketInvRequest>
    {
        public void Configure(EntityTypeBuilder<PatDocketInvRequest> builder)
        {
            builder.ToTable("tblPatDocketInvRequest");
            builder.HasOne(a => a.Invention).WithMany(c => c.PatDocketInvRequests).HasForeignKey(a => a.InvId).HasPrincipalKey(c => c.InvId);
        }
    }

}
