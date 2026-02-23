using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.Trademark;


namespace R10.Infrastructure.Data.Trademark.mappings
{
    public class TmkWorkflowActionMap : IEntityTypeConfiguration<TmkWorkflowAction>
    {
        public void Configure(EntityTypeBuilder<TmkWorkflowAction> builder)
        {
            builder.ToTable("tblTmkWorkflowAction");
            builder.HasIndex(c => new { c.WrkId, c.ActionTypeId, c.ActionValueId }).IsUnique();
        }
    }
}
