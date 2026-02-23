using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.Patent;

namespace R10.Infrastructure.Data.Patent.mappings
{
    public class EPOApplicationMap : IEntityTypeConfiguration<EPOApplication>
    {
        public void Configure(EntityTypeBuilder<EPOApplication> builder)
        {
            builder.ToTable("tblEPOApplication");
            builder.HasIndex(a => new { a.AppProcedure, a.IpOfficeCode, a.AppNumber, a.PortfolioId }).IsUnique();
        }
    }
}
