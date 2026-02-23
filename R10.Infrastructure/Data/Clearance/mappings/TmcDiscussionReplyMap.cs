using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.Clearance;

namespace R10.Infrastructure.Data.Clearance.mappings
{
    public class TmcDiscussionReplyMap : IEntityTypeConfiguration<TmcDiscussionReply>
    {
        public void Configure(EntityTypeBuilder<TmcDiscussionReply> builder)
        {
            builder.ToTable("tblTmcDiscussionReply");
            builder.HasIndex(d => new { d.ReplyId, d.DiscussId }).IsUnique();
            builder.HasOne(d => d.Discussion).WithMany(disc => disc.Replies).HasForeignKey(r => r.DiscussId).HasPrincipalKey(d => d.DiscussId);            
        }
    }
}
