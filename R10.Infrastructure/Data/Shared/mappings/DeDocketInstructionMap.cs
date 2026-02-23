using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities;

namespace R10.Infrastructure.Data.Shared.mappings
{
    public class DeDocketInstructionMap : IEntityTypeConfiguration<DeDocketInstruction>
    {
        public void Configure(EntityTypeBuilder<DeDocketInstruction> builder)
        {
            builder.ToTable("tblDeDocketInstruction");
            
        }
    }
}
