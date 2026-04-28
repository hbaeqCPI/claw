using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using LawPortal.Core.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace LawPortal.Infrastructure.Identity.Mappings
{
    public class CPiMenuPageMap : IEntityTypeConfiguration<CPiMenuPage>
    {
        public void Configure(EntityTypeBuilder<CPiMenuPage> builder)
        {
            builder.ToTable("tblCPiMenuPages");
            builder.HasKey(x => new { x.Id });
            builder.HasMany<CPiMenuItem>().WithOne(menu => menu.Page).HasForeignKey(menu => menu.PageId).HasPrincipalKey(page => page.Id);
        }
    }
}
