using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.Patent;

namespace R10.Infrastructure.Data.Patent.mappings
{
    public class PatEPOAppLogMap : IEntityTypeConfiguration<PatEPOAppLog>
    {
        public void Configure(EntityTypeBuilder<PatEPOAppLog> builder)
        {

            builder.ToTable("tblPatEPOAppLog");
            builder.Property(s => s.KeyId).ValueGeneratedOnAdd();
            builder.Property(m => m.KeyId).UseIdentityColumn();
            builder.HasIndex(a => new { a.AppId, a.Procedure, a.IpOfficeCode, a.AppNumber, a.FilDate }).IsUnique();
        }
    }
}