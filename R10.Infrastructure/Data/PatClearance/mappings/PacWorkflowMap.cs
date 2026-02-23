using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.PatClearance;

namespace R10.Infrastructure.Data.PatClearance.mappings
{
    public class PacWorkflowMap : IEntityTypeConfiguration<PacWorkflow>
    {
        public void Configure(EntityTypeBuilder<PacWorkflow> builder)
        {
            builder.ToTable("tblPacWorkflow");
            builder.HasIndex(wrk => new { wrk.Workflow, wrk.TriggerTypeId, wrk.TriggerValueId }).IsUnique();
            builder.HasMany(wrk => wrk.WorkflowActions).WithOne(d => d.Workflow).HasPrincipalKey(c => c.WrkId).HasForeignKey(d => d.WrkId);
        }
    }
}
