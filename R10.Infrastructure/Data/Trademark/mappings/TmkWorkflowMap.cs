using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.Trademark;

namespace R10.Infrastructure.Data.Trademark.mappings
{
    public class TmkWorkflowMap : IEntityTypeConfiguration<TmkWorkflow>
    {
        public void Configure(EntityTypeBuilder<TmkWorkflow> builder)
        {
            builder.ToTable("tblTmkWorkflow");
            builder.HasIndex(wrk => new { wrk.Workflow, wrk.TriggerTypeId, wrk.TriggerValueId }).IsUnique();
            builder.HasMany(wrk => wrk.WorkflowActions).WithOne(d => d.Workflow).HasPrincipalKey(c => c.WrkId).HasForeignKey(d => d.WrkId);
            builder.HasOne(wrk => wrk.SystemScreen).WithMany(s => s.TmkWorkflows).HasPrincipalKey(c => c.ScreenId).HasForeignKey(d => d.ScreenId);
            builder.HasMany(wrk => wrk.WorkflowActionParameters).WithOne(d => d.Workflow).HasPrincipalKey(c => c.WrkId).HasForeignKey(d => d.WrkId);
        }
    }
}
