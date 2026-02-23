using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using OpenIddict.EntityFrameworkCore.Models;
using R10.Core.DTOs;
using R10.Core.Entities;
using R10.Core.Identity;
using R10.Infrastructure.Identity.Mappings;

namespace R10.Infrastructure.Identity
{
    //TODO: USE ONE DBCONTEXT
    public class CPiUserDbContext : IdentityDbContext<CPiUser, CPiRole, string>
    {
        public DbSet<CPiRole> CPiRoles { get; set; }
        public DbSet<CPiUserSystemRole> CPiUserSystemRoles { get; set; }
        public DbSet<CPiUserTypeSystemRole> CPiUserTypeSystemRoles { get; set; }
        public DbSet<CPiSystem> CPiSystems { get; set; }
        public DbSet<CPiSystemRole> CPiSystemRoles { get; set; }
        public DbSet<CPiRespOffice> CPiRespOffices { get; set; }
        public DbSet<CPiUserPasswordHistory> CPiUserPasswordHistory { get; set; }
        public DbSet<EntityFilterDTO> EntityFilters { get; set; }
        public DbSet<CPiUserEntityFilter> CPiUserEntityFilters { get; set; }

        public CPiUserDbContext(DbContextOptions<CPiUserDbContext> options) : base(options)
        {
        }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.ApplyConfiguration(new CPiUserMap());
            builder.ApplyConfiguration(new CPiRoleMap());

            builder.Entity<IdentityUserRole<string>>().ToTable("tblCPiUserRoles");
            builder.Entity<IdentityUserClaim<string>>().ToTable("tblCPiUserClaims");
            builder.Entity<IdentityUserLogin<string>>().ToTable("tblCPiUserLogins");
            builder.Entity<IdentityUserToken<string>>().ToTable("tblCPiUserTokens");
            builder.Entity<IdentityRoleClaim<string>>().ToTable("tblCPiRoleClaims");
            
            builder.ApplyConfiguration(new CPiSystemMap());
            builder.ApplyConfiguration(new CPiSystemRoleMap());
            builder.ApplyConfiguration(new CPiUserSystemRoleMap());
            builder.ApplyConfiguration(new CPiRespOfficeMap());
            builder.ApplyConfiguration(new CPiUserTypeSystemRoleMap());
            builder.ApplyConfiguration(new CPiUserPasswordHistoryMap());
            builder.ApplyConfiguration(new CPiUserEntityFilterMap());
            builder.ApplyConfiguration(new CPiGroupMap());
            builder.ApplyConfiguration(new CPiUserGroupMap());

            builder.ApplyConfiguration(new CPiDefaultPageMap());
            builder.ApplyConfiguration(new CPiSettingMap());
            builder.ApplyConfiguration(new CPiUserSettingMap());
            builder.ApplyConfiguration(new CPiSystemSettingMap());

            // OpenIddict stores for authorization code flow, client credentials flow
            builder.Entity<OpenIddictEntityFrameworkCoreApplication>().ToTable("tblCPiOpenIddictApplications");
            builder.Entity<OpenIddictEntityFrameworkCoreAuthorization>().ToTable("tblCPiOpenIddictAuthorizations");
            builder.Entity<OpenIddictEntityFrameworkCoreToken>().ToTable("tblCPiOpenIddictTokens");
            builder.Entity<OpenIddictEntityFrameworkCoreScope>().ToTable("tblCPiOpenIddictScopes");
        }
        
        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        {
            // Mitigate breaking changes in EF7
            // SQL Server tables with triggers or certain computed columns now require special EF Core configuration
            configurationBuilder.Conventions.Add(_ => new BlankTriggerAddingConvention());
        }
    }
}
