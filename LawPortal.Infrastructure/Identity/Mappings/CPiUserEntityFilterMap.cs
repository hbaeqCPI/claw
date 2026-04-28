using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using LawPortal.Core.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace LawPortal.Infrastructure.Identity.Mappings
{
    public class CPiUserEntityFilterMap : IEntityTypeConfiguration<CPiUserEntityFilter>
    {
        public void Configure(EntityTypeBuilder<CPiUserEntityFilter> builder)
        {
            builder.ToTable("tblCPiUserEntityFilter");
            builder.HasKey(x => new { x.UserId, x.EntityId });
        }
    }
}
