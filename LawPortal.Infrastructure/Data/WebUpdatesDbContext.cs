using Microsoft.EntityFrameworkCore;
using LawPortal.Core.Entities.Patent;
using LawPortal.Core.Entities.Trademark;

namespace LawPortal.Infrastructure.Data
{
    public class WebUpdatesDbContext : DbContext
    {
        public WebUpdatesDbContext(DbContextOptions<WebUpdatesDbContext> options) : base(options) { }

        // Patent
        public DbSet<PatAreaDelete> Upd5PatAreaDeletes { get; set; }
        public DbSet<PatAreaCountryDelete> Upd5PatAreaCountryDeletes { get; set; }
        public DbSet<PatCountryExpDelete> Upd5PatCountryExpDeletes { get; set; }
        public DbSet<PatCountryLawExt> Upd5PatCountryLawExts { get; set; }
        public DbSet<PatDesCaseTypeExt> Upd5PatDesCaseTypeExts { get; set; }
        public DbSet<PatDesCaseTypeDelete> Upd5PatDesCaseTypeDeletes { get; set; }
        public DbSet<PatDesCaseTypeDeleteExt> Upd5PatDesCaseTypeDeleteExts { get; set; }
        public DbSet<PatDesCaseTypeFieldsExt> Upd5PatDesCaseTypeFieldsExts { get; set; }
        public DbSet<PatDesCaseTypeFieldsDelete> Upd5PatDesCaseTypeFieldsDeletes { get; set; }
        public DbSet<PatDesCaseTypeFieldsDeleteExt> Upd5PatDesCaseTypeFieldsDeleteExts { get; set; }

        // Trademark
        public DbSet<TmkAreaDelete> Upd5TmkAreaDeletes { get; set; }
        public DbSet<TmkAreaCountryDelete> Upd5TmkAreaCountryDeletes { get; set; }
        public DbSet<TmkDesCaseTypeExt> Upd5TmkDesCaseTypeExts { get; set; }
        public DbSet<TmkDesCaseTypeDelete> Upd5TmkDesCaseTypeDeletes { get; set; }
        public DbSet<TmkDesCaseTypeDeleteExt> Upd5TmkDesCaseTypeDeleteExts { get; set; }
        public DbSet<TmkDesCaseTypeFieldsExt> Upd5TmkDesCaseTypeFieldsExts { get; set; }
        public DbSet<TmkDesCaseTypeFieldsDelete> Upd5TmkDesCaseTypeFieldsDeletes { get; set; }
        public DbSet<TmkDesCaseTypeFieldsDeleteExt> Upd5TmkDesCaseTypeFieldsDeleteExts { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            // Patent
            builder.Entity<PatAreaDelete>(e => { e.ToTable("upd5PatAreaDelete"); e.HasKey(x => new { x.Area, x.AreaNew }); });
            builder.Entity<PatAreaCountryDelete>(e => { e.ToTable("upd5PatAreaCountryDelete"); e.HasKey(x => new { x.Area, x.Country, x.AreaNew, x.CountryNew }); });
            builder.Entity<PatCountryExpDelete>(e => { e.ToTable("upd5PatCountryExpDelete"); e.HasKey(x => x.CExpId); });
            builder.Entity<PatCountryLawExt>(e => { e.ToTable("upd5PatCountryLaw_Ext"); e.HasKey(x => new { x.Country, x.CaseType }); });
            builder.Entity<PatDesCaseTypeExt>(e => { e.ToTable("upd5PatDesCaseType_Ext"); e.HasKey(x => new { x.IntlCode, x.CaseType, x.DesCountry, x.DesCaseType }); });
            builder.Entity<PatDesCaseTypeDelete>(e => { e.ToTable("upd5PatDesCaseTypeDelete"); e.HasKey(x => new { x.IntlCode, x.CaseType, x.DesCountry, x.DesCaseType, x.IntlCodeNew, x.CaseTypeNew, x.DesCountryNew, x.DesCaseTypeNew }); });
            builder.Entity<PatDesCaseTypeDeleteExt>(e => { e.ToTable("upd5PatDesCaseTypeDelete_Ext"); e.HasKey(x => new { x.IntlCode, x.CaseType, x.DesCountry, x.DesCaseType, x.IntlCodeNew, x.CaseTypeNew, x.DesCountryNew, x.DesCaseTypeNew }); });
            builder.Entity<PatDesCaseTypeFieldsExt>(e => { e.ToTable("upd5PatDesCaseTypeFields_Ext"); e.HasKey(x => new { x.DesCaseType, x.FromField, x.ToField }); });
            builder.Entity<PatDesCaseTypeFieldsDelete>(e => { e.ToTable("upd5PatDesCaseTypeFieldsDelete"); e.HasKey(x => new { x.DesCaseType, x.FromField, x.ToField, x.DesCaseTypeNew, x.FromFieldNew, x.ToFieldNew }); });
            builder.Entity<PatDesCaseTypeFieldsDeleteExt>(e => { e.ToTable("upd5PatDesCaseTypeFieldsDelete_Ext"); e.HasKey(x => new { x.DesCaseType, x.FromField, x.ToField, x.DesCaseTypeNew, x.FromFieldNew, x.ToFieldNew }); });

            // Trademark
            builder.Entity<TmkAreaDelete>(e => { e.ToTable("upd5TmkAreaDelete"); e.HasKey(x => new { x.Area, x.AreaNew }); });
            builder.Entity<TmkAreaCountryDelete>(e => { e.ToTable("upd5TmkAreaCountryDelete"); e.HasKey(x => new { x.Area, x.Country, x.AreaNew, x.CountryNew }); });
            builder.Entity<TmkDesCaseTypeExt>(e => { e.ToTable("upd5TmkDesCaseType_Ext"); e.HasKey(x => new { x.IntlCode, x.CaseType, x.DesCountry, x.DesCaseType }); });
            builder.Entity<TmkDesCaseTypeDelete>(e => { e.ToTable("upd5TmkDesCaseTypeDelete"); e.HasKey(x => new { x.IntlCode, x.CaseType, x.DesCountry, x.DesCaseType, x.IntlCodeNew, x.CaseTypeNew, x.DesCountryNew, x.DesCaseTypeNew }); });
            builder.Entity<TmkDesCaseTypeDeleteExt>(e => { e.ToTable("upd5TmkDesCaseTypeDelete_Ext"); e.HasKey(x => new { x.IntlCode, x.CaseType, x.DesCountry, x.DesCaseType, x.IntlCodeNew, x.CaseTypeNew, x.DesCountryNew, x.DesCaseTypeNew }); });
            builder.Entity<TmkDesCaseTypeFieldsExt>(e => { e.ToTable("upd5TmkDesCaseTypeFields_Ext"); e.HasKey(x => new { x.DesCaseType, x.FromField, x.ToField }); });
            builder.Entity<TmkDesCaseTypeFieldsDelete>(e => { e.ToTable("upd5TmkDesCaseTypeFieldsDelete"); e.HasKey(x => new { x.DesCaseType, x.FromField, x.ToField, x.DesCaseTypeNew, x.FromFieldNew, x.ToFieldNew }); });
            builder.Entity<TmkDesCaseTypeFieldsDeleteExt>(e => { e.ToTable("upd5TmkDesCaseTypeFieldsDelete_Ext"); e.HasKey(x => new { x.DesCaseType, x.FromField, x.ToField, x.DesCaseTypeNew, x.FromFieldNew, x.ToFieldNew }); });

            base.OnModelCreating(builder);
        }
    }
}
