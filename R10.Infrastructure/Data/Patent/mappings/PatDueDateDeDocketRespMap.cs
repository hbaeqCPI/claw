using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities;
using R10.Core.Entities.Patent;

namespace R10.Infrastructure.Data.Patent.mappings
{
    public class PatDueDateDeDocketRespMap : IEntityTypeConfiguration<PatDueDateDeDocketResp>
    {
        public void Configure(EntityTypeBuilder<PatDueDateDeDocketResp> builder)
        {
            builder.ToTable("tblPatDueDateDeDocketResp");
            builder.HasOne(a => a.PatDueDateDeDocket).WithMany(c => c.PatDueDateDeDocketResps).HasForeignKey(a => a.DeDocketId).HasPrincipalKey(c => c.DeDocketId);
        }
    }    
}
