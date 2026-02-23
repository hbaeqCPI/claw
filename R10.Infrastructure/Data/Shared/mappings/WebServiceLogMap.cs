using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities;

namespace R10.Infrastructure.Data.Shared.mappings
{
    public class WebServiceLogMap : IEntityTypeConfiguration<WebServiceLog>
    {
        public void Configure(EntityTypeBuilder<WebServiceLog> builder ) {

            builder.ToTable("tblWebServiceLog");
            builder.Property(s => s.LogId).ValueGeneratedOnAdd();
            builder.Property(m => m.LogId).UseIdentityColumn();
        }
    }
}