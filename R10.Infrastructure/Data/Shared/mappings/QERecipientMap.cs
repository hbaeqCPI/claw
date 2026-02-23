using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities;
using R10.Core.Entities.Shared;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.Shared.mappings
{
    public class QERecipientMap : IEntityTypeConfiguration<QERecipient>
    {
        public void Configure(EntityTypeBuilder<QERecipient> builder)
        {
            builder.ToTable("tblQERecipient");
            builder.HasIndex(qe => new { qe.QESetupID, qe.RoleSourceID }).IsUnique(); //unique index
            builder.HasOne(qe => qe.QERoleSource).WithMany(d => d.QERoleSourceRecipients).HasForeignKey(d => d.RoleSourceID).HasPrincipalKey(d => d.RoleSourceID);
        }
    }
}



