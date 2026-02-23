using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.Patent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R10.Infrastructure.Data.Patent.mappings
{
    public class PatInventorInvWebSvcMap : IEntityTypeConfiguration<PatInventorInvWebSvc>
    {
        public void Configure(EntityTypeBuilder<PatInventorInvWebSvc> builder)
        {
            builder.ToTable("tblPatInventorInvWebSvc");
            builder.Property(pi => pi.EntityId).ValueGeneratedOnAdd();
            builder.Property(pi => pi.EntityId).UseIdentityColumn();
            builder.HasOne(pi => pi.Invention).WithMany(i => i.Inventors).HasForeignKey(pi => pi.InvEntityId).HasPrincipalKey(i => i.EntityId);
        }
    }
}
