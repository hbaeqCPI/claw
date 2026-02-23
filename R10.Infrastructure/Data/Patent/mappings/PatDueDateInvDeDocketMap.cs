using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.Patent;

namespace R10.Infrastructure.Data.Patent.mappings
{
    public class PatDueDateInvDeDocketMap : IEntityTypeConfiguration<PatDueDateInvDeDocket>
    {
        public void Configure(EntityTypeBuilder<PatDueDateInvDeDocket> builder)
        {
            builder.ToTable("tblPatDueDateInvDeDocket");
            builder.HasOne(c => c.PatDueDateInv).WithMany(d => d.DueDateDeDockets).HasForeignKey(c => c.DDId).HasPrincipalKey(d => d.DDId);
        }
    }
}
