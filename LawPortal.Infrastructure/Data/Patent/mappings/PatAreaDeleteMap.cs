using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using LawPortal.Core.Entities.Patent;

namespace LawPortal.Infrastructure.Data.Patent.mappings
{
    public class PatAreaDeleteMap : IEntityTypeConfiguration<PatAreaDelete>
    {
        public void Configure(EntityTypeBuilder<PatAreaDelete> builder)
        {
            builder.ToTable("tblPatAreaDelete");
            builder.HasKey(e => new { e.Area, e.AreaNew, e.Systems });
            builder.Ignore(e => e.IsNewRecord);
            builder.Ignore(e => e.OriginalSystems);
        }
    }
}