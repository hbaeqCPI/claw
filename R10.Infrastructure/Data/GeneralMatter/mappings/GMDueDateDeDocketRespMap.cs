using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities;
using R10.Core.Entities.Patent;

namespace R10.Infrastructure.Data.Patent.mappings
{
    public class GMDueDateDeDocketRespMap : IEntityTypeConfiguration<GMDueDateDeDocketResp>
    {
        public void Configure(EntityTypeBuilder<GMDueDateDeDocketResp> builder)
        {
            builder.ToTable("tblGMDueDateDeDocketResp");
            builder.HasOne(a => a.GMDueDateDeDocket).WithMany(c => c.GMDueDateDeDocketResps).HasForeignKey(a => a.DeDocketId).HasPrincipalKey(c => c.DeDocketId);
        }
    }    
}
