using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities;
using R10.Core.Entities;

namespace R10.Infrastructure.Data.Shared.mappings
{
    public class EFSLogMap : IEntityTypeConfiguration<EFSLog>
    {
        public void Configure(EntityTypeBuilder<EFSLog> builder)
        {
            builder.ToTable("tblEFS_Log");
        }
    }

    public class EFSMap : IEntityTypeConfiguration<EFS>
    {
        public void Configure(EntityTypeBuilder<EFS> builder)
        {
            builder.ToTable("tblEFS");
            builder.HasOne(e=> e.QEMain).WithMany(q=> q.EFSForSignature).HasPrincipalKey(q=> q.QESetupID).HasForeignKey(e => e.SignatureQESetupId);
        }
    }
}
