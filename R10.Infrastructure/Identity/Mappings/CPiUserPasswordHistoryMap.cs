using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Identity.Mappings
{
    public class CPiUserPasswordHistoryMap : IEntityTypeConfiguration<CPiUserPasswordHistory>
    {
        public void Configure(EntityTypeBuilder<CPiUserPasswordHistory> builder)
        {
            builder.ToTable("tblCPiUserPasswordHistory");
            builder.HasKey(x => new { x.UserId, x.PasswordHash });
        }
    }
}
