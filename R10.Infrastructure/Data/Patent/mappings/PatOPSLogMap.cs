using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.Patent;

namespace R10.Infrastructure.Data.Patent.mappings
{
    public class PatOPSLogMap : IEntityTypeConfiguration<PatOPSLog>
    {
        public void Configure(EntityTypeBuilder<PatOPSLog> builder)
        {

            builder.ToTable("tblPatOPSLog");
            builder.Property(s => s.EntityId).ValueGeneratedOnAdd();
            builder.Property(m => m.EntityId).UseIdentityColumn();            
        }
    }
}