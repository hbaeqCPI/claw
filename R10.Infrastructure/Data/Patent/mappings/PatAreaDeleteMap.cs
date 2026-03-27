using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.Patent;

namespace R10.Infrastructure.Data.Patent.mappings
{
    public class PatAreaDeleteMap : IEntityTypeConfiguration<PatAreaDelete>
    {
        public void Configure(EntityTypeBuilder<PatAreaDelete> builder)
        {
            builder.ToTable("tblPatAreaDelete");
            builder.HasNoKey();
        }
    }
}