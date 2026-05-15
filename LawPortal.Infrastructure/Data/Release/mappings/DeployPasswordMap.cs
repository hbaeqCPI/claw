using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using LawPortal.Core.Entities;

namespace LawPortal.Infrastructure.Data.Release.mappings
{
    public class DeployPasswordMap : IEntityTypeConfiguration<DeployPassword>
    {
        public void Configure(EntityTypeBuilder<DeployPassword> builder)
        {
            builder.ToTable("tblDeployPassword");
            builder.HasIndex(d => new { d.Year, d.Quarter, d.PatentPassword, d.TrademarkPassword }).IsUnique();
        }
    }
}
