using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using LawPortal.Core.Entities;

namespace LawPortal.Infrastructure.Data.Shared.mappings
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