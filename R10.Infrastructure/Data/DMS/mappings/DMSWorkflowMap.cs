using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.DMS;

namespace R10.Infrastructure.Data.DMS.mappings
{
    public class DMSWorkflowMap : IEntityTypeConfiguration<DMSWorkflow>
    {
        public void Configure(EntityTypeBuilder<DMSWorkflow> builder)
        {
            builder.ToTable("tblDMSWorkflow");
            builder.HasIndex(wrk => new { wrk.Workflow, wrk.TriggerTypeId, wrk.TriggerValueId }).IsUnique();
            builder.HasMany(wrk => wrk.DMSWorkflowActions).WithOne(d => d.DMSWorkflow).HasPrincipalKey(c => c.WrkId).HasForeignKey(d => d.WrkId);
        }
    }
}
