using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using R10.Core.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Infrastructure.Identity.Mappings
{
    public class CPiRespOfficeMap : IEntityTypeConfiguration<CPiRespOffice>
    {
        public void Configure(EntityTypeBuilder<CPiRespOffice> builder)
        {
            builder.ToTable("tblCPiRespOffices");
            builder.HasKey(x => x.RespOffice);
            //builder.HasMany<CPiUserSystemRole>().WithOne().HasForeignKey(x => x.RespOffice).IsRequired(false);
            builder.HasMany(ro => ro.UserSystemRoles).WithOne(u => u.UserRespOffice).HasForeignKey(u => u.RespOffice).HasPrincipalKey(ro => ro.RespOffice).IsRequired(false);
            //builder.HasMany<CPiUserTypeSystemRole>().WithOne().HasForeignKey(x => x.RespOffice).IsRequired(false);
        }
    }
}
