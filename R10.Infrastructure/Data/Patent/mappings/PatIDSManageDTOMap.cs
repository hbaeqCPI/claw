using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Entities;
using R10.Core.Entities.Patent;
using System;
using System.Collections.Generic;
using System.Text;
using R10.Core.DTOs;

namespace R10.Infrastructure.Data.Patent.mappings
{
    public class PatIDSManageDTOMap : IEntityTypeConfiguration<PatIDSManageDTO>
    {
        public void Configure(EntityTypeBuilder<PatIDSManageDTO> builder)
        {
            builder.ToView("vwPatIDSManage");
            builder.HasOne(m => m.Invention).WithMany(i => i.IDSManageCases).HasForeignKey(m => m.InvId).HasPrincipalKey(i=> i.InvId);
            builder.HasOne(m => m.PatApplicationStatus).WithMany(i => i.IDSManageCases).HasForeignKey(m => m.ApplicationStatus).HasPrincipalKey(i => i.ApplicationStatus);
            //builder.HasOne(m => m.PatInventorApp).WithMany(i => i.IDSManageCases).HasForeignKey(m => m.AppId).HasPrincipalKey(i => i.AppId).IsRequired(false);
            
        }
    }
}
