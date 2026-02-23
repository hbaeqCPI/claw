using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.Clearance;

namespace R10.Infrastructure.Data.Clearance.mappings
{
    public class TmcWorkflowMap : IEntityTypeConfiguration<TmcWorkflow>
    {
        public void Configure(EntityTypeBuilder<TmcWorkflow> builder)
        {
            builder.ToTable("tblTmcWorkflow");
            builder.HasIndex(wrk => new { wrk.Workflow, wrk.TriggerTypeId, wrk.TriggerValueId }).IsUnique();
            builder.HasMany(wrk => wrk.WorkflowActions).WithOne(d => d.Workflow).HasPrincipalKey(c => c.WrkId).HasForeignKey(d => d.WrkId);
        }
    }
}
