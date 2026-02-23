using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities.DMS;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Data.DMS.mappings
{
    public class DMSActionTypeMap : IEntityTypeConfiguration<DMSActionType>
    {
        public void Configure(EntityTypeBuilder<DMSActionType> builder)
        {
            builder.ToTable("tblDMSActionType");                        
            builder.HasIndex(a => a.ActionType).IsUnique();
            builder.HasMany(a => a.ActionParameters).WithOne(p => p.ActionType);
            builder.HasOne(a => a.Responsible).WithMany(r => r.AttorneyDMSActionTypes).HasForeignKey(a => a.ResponsibleID).HasPrincipalKey(a => a.AttorneyID);
        }
    }
}
