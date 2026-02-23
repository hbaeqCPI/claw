using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.Patent;

namespace R10.Infrastructure.Data.Patent.mappings
{
    public class PatDueDateDeDocketMap : IEntityTypeConfiguration<PatDueDateDeDocket>
    {
        public void Configure(EntityTypeBuilder<PatDueDateDeDocket> builder)
        {
            builder.ToTable("tblPatDueDateDeDocket");
            builder.HasOne(c => c.PatDueDate).WithMany(d => d.DueDateDeDockets).HasForeignKey(c => c.DDId).HasPrincipalKey(d=>d.DDId);
        }
    }
}
