using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.GeneralMatter;

namespace R10.Infrastructure.Data.GeneralMatter.mappings
{
    public class GMWorkflowMap : IEntityTypeConfiguration<GMWorkflow>
    {
        public void Configure(EntityTypeBuilder<GMWorkflow> builder)
        {
            builder.ToTable("tblGMWorkflow");
            builder.HasIndex(wrk => new { wrk.Workflow, wrk.TriggerTypeId, wrk.TriggerValueId }).IsUnique();
            builder.HasMany(wrk => wrk.WorkflowActions).WithOne(d => d.Workflow).HasPrincipalKey(c => c.WrkId).HasForeignKey(d => d.WrkId);
            builder.HasOne(wrk => wrk.SystemScreen).WithMany(s => s.GMWorkflows).HasPrincipalKey(c => c.ScreenId).HasForeignKey(d => d.ScreenId);
            builder.HasMany(wrk => wrk.WorkflowActionParameters).WithOne(d => d.Workflow).HasPrincipalKey(c => c.WrkId).HasForeignKey(d => d.WrkId);
        }
    }
}
