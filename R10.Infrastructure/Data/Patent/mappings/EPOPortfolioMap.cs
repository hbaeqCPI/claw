using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.Patent;

namespace R10.Infrastructure.Data.Patent.mappings
{
    public class EPOPortfolioMap : IEntityTypeConfiguration<EPOPortfolio>
    {
        public void Configure(EntityTypeBuilder<EPOPortfolio> builder)
        {
            builder.ToTable("tblEPOPortfolio");
            builder.HasIndex(a => new { a.PortfolioId }).IsUnique();
        }
    }
}
