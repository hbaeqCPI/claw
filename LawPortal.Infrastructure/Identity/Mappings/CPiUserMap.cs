using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using LawPortal.Core.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace LawPortal.Infrastructure.Identity.Mappings
{
    public class CPiUserMap : IEntityTypeConfiguration<CPiUser>
    {
        public void Configure(EntityTypeBuilder<CPiUser> builder)
        {
            builder.ToTable("tblCPiUsers");
            builder.Property(u => u.PkId).ValueGeneratedOnAdd();
            builder.Property(u => u.PkId).UseIdentityColumn();
            builder.Property(u => u.PkId).Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);
            builder.HasIndex(x => x.Locale).IsUnique(false);
            builder.HasIndex(x => x.UserType).IsUnique(false);
            //builder.HasMany<CPiUserPasswordHistory>().WithOne().HasForeignKey(x => x.UserId).IsRequired(true);
            //builder.HasMany<CPiUserWidget>().WithOne().HasForeignKey(x => x.UserId).IsRequired(true);
            builder.HasMany(u => u.CPiUserEntityFilters).WithOne(e => e.CPiUser).HasForeignKey(s => s.UserId).HasPrincipalKey(u => u.Id);
            //builder.HasMany(u => u.PatCostTracks).WithOne(e => e.CPiUser).HasForeignKey(s => s.BillingUserPkId).HasPrincipalKey(u => u.PkId);
            //builder.HasMany(u => u.TmkCostTracks).WithOne(e => e.CPiUser).HasForeignKey(s => s.BillingUserPkId).HasPrincipalKey(u => u.PkId);
            //builder.HasMany(u => u.PGMCostTracks).WithOne(e => e.CPiUser).HasForeignKey(s => s.BillingUserPkId).HasPrincipalKey(u => u.PkId);
            //CPiUserSettingMap
            //builder.HasMany<CPiUserSetting>().WithOne().HasForeignKey(x => x.UserId).IsRequired(true);
            //builder.ToTable(t => t.HasTrigger("tblCPiUsers_IU_Trig"));
        }
    }
}
