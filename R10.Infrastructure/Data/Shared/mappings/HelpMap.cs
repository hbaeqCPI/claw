using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities;

namespace R10.Infrastructure.Data.Shared.mappings
{
    public class HelpMap : IEntityTypeConfiguration<Help>
    {
        public void Configure(EntityTypeBuilder<Help> builder)
        {
            builder.ToTable("tblHelp");
            builder.HasKey(h => h.HelpId);
            builder.HasIndex(h => new { h.ClientType, h.Page }).IsUnique();
        }
    }
}
