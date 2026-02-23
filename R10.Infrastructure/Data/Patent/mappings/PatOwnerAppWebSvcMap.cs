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
    public class PatOwnerAppWebSvcMap : IEntityTypeConfiguration<PatOwnerAppWebSvc>
    {
        public void Configure(EntityTypeBuilder<PatOwnerAppWebSvc> builder)
        {
            builder.ToTable("tblPatOwnerAppWebSvc");
            builder.Property(i => i.EntityId).ValueGeneratedOnAdd();
            builder.Property(i => i.EntityId).UseIdentityColumn();
        }
    }
}
