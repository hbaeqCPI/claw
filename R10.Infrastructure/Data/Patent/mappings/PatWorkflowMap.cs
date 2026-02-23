using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.Patent;

namespace R10.Infrastructure.Data.Patent.mappings
{
    public class PatWorkflowMap : IEntityTypeConfiguration<PatWorkflow>
    {
        public void Configure(EntityTypeBuilder<PatWorkflow> builder)
        {
            builder.ToTable("tblPatWorkflow");
            builder.HasIndex(wrk => new { wrk.Workflow, wrk.TriggerTypeId, wrk.TriggerValueId }).IsUnique();
            builder.HasMany(wrk => wrk.WorkflowActions).WithOne(d => d.Workflow).HasPrincipalKey(c => c.WrkId).HasForeignKey(d => d.WrkId);
            builder.HasOne(wrk=> wrk.SystemScreen).WithMany(s=> s.PatWorkflows).HasPrincipalKey(c => c.ScreenId).HasForeignKey(d => d.ScreenId);
            builder.HasMany(wrk => wrk.WorkflowActionParameters).WithOne(d => d.Workflow).HasPrincipalKey(c => c.WrkId).HasForeignKey(d => d.WrkId);
        }
    }
}
