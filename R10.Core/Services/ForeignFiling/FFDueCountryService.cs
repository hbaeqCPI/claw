using Microsoft.EntityFrameworkCore;
using R10.Core.Entities.ForeignFiling;
using R10.Core.Entities.Patent;
using R10.Core.Interfaces;
using R10.Core.Interfaces.ForeignFiling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace R10.Core.Services.ForeignFiling
{
    public class FFDueCountryService : EntityService<FFDueCountry>, IFFDueCountryService
    {
        public readonly IFFDueService _ffDueService;
        public FFDueCountryService(IFFDueService ffDueService, ICPiDbContext cpiDbContext, ClaimsPrincipal user) : base(cpiDbContext, user)
        {
            _ffDueService = ffDueService;
        }
        public IQueryable<FFDueCountry> GenAppList => QueryableList.Where(c => c.UpdateDate == null &&
                                                            //Show countries from closed actions or open but instructable actions
                                                            ((c.FFDue.IsActionClosed ?? false) || _ffDueService.InstructableList.Any(i => i.DDId == c.FFDue.DDId)) &&
                                                            //Show countries from All tab
                                                            //Country application record will be created for each country
                                                            //All selected EP des countries will be designated when EP country application record is created
                                                            //All selected WO des countries will be designated when WO country application record is created
                                                            (c.Source == "All" ||
                                                            //Show countries from EP tab if parent country is EP
                                                            //Designated country record will be created for each country
                                                            (c.Source == "EP" && c.FFDue.PatDueDate.PatActionDue.CountryApplication.Country == "EP") ||
                                                            //Show countries from WO tab if parent country is WO (designate)
                                                            //Designated country record will be created for each country
                                                            (c.Source == "WO" && c.FFDue.PatDueDate.PatActionDue.CountryApplication.Country == "WO"))
                                                            );

        public IQueryable<PatDesCaseType> DesCaseTypeList => _cpiDbContext.GetRepository<PatDesCaseType>().QueryableList;

        public IQueryable<PatCountry> CountryList => _cpiDbContext.GetRepository<PatCountry>().QueryableList;

        //Use ORD case type when generating applications
        //Use DES if country has no ORD
        public IQueryable<PatCountryLaw> CaseTypeList => _cpiDbContext.GetRepository<PatCountryLaw>().FromSql(
                    "SELECT Country, CaseType " +
                    "FROM ( " +
                    "   SELECT Country, CaseType, ROW_NUMBER() OVER(PARTITION BY Country ORDER BY Country, " +
                    "       CASE CaseType WHEN 'ORD' THEN 1 WHEN 'DES' THEN 2 ELSE 99 END) AS RowNo " +
                    "   FROM tblPatCountryLaw " +
                    ") CaseTypes " +
                    "WHERE RowNo = 1");

        public async Task<byte[]> SaveExclude(int id, bool exclude, byte[] tStamp, string userName)
        {
            var updated = new FFDueCountry()
            {
                DueCountryId = id,
                Exclude = null,
                tStamp = tStamp
            };

            _cpiDbContext.GetRepository<FFDueCountry>().Attach(updated);

            updated.Exclude = exclude;
            updated.UpdatedBy = userName;
            updated.LastUpdate = DateTime.Now;

            await _cpiDbContext.SaveChangesAsync();

            return updated.tStamp;
        }

        public async Task<byte[]> SaveGenApp(int id, bool genApp, byte[] tStamp, string userName)
        {
            var updated = new FFDueCountry()
            {
                DueCountryId = id,
                GenApp = null,
                tStamp = tStamp
            };

            _cpiDbContext.GetRepository<FFDueCountry>().Attach(updated);

            updated.GenApp = genApp;
            updated.UpdatedBy = userName;
            updated.LastUpdate = DateTime.Now;

            await _cpiDbContext.SaveChangesAsync();

            return updated.tStamp;
        }

        public async Task<int> GenerateApplication(CountryApplication tempApp, FFDueCountry updated)
        {
            var countryAppRepository = _cpiDbContext.GetRepository<CountryApplication>();
            var parentApp = await countryAppRepository.GetByIdAsync(tempApp.AppId);
            var newApp = new CountryApplication();
            var newDes = new PatDesignatedCountry();

            if (parentApp.AppId > 0)
            {
                var exists = false;
                var isGenerateApp = updated.Source.ToUpper() == "ALL";

                //generate application
                if (isGenerateApp)
                {
                    exists = await countryAppRepository.QueryableList.AnyAsync(ca => ca.CaseNumber == parentApp.CaseNumber && ca.Country == tempApp.Country && ca.SubCase == tempApp.SubCase);
                    if (!exists)
                    {
                        //todo: use country application service to add new record
                        newApp = new CountryApplication()
                        {
                            InvId = parentApp.InvId,
                            RespOffice = parentApp.RespOffice,
                            CaseNumber = parentApp.CaseNumber,
                            Country = tempApp.Country,
                            SubCase = tempApp.SubCase,
                            CaseType = tempApp.CaseType,
                            CreatedBy = tempApp.UpdatedBy,
                            UpdatedBy = tempApp.UpdatedBy,
                            DateCreated = tempApp.LastUpdate,
                            LastUpdate = tempApp.LastUpdate,
                            Remarks = tempApp.Remarks
                        };
                        countryAppRepository.Attach(newApp);
                    }
                }
                //designate application
                else
                {
                    exists = await _cpiDbContext.GetRepository<PatDesignatedCountry>().QueryableList.AnyAsync(des => des.AppId == parentApp.AppId && des.DesCountry == tempApp.Country);
                    if (!exists)
                    {
                        newDes = new PatDesignatedCountry()
                        {
                            AppId = parentApp.AppId,
                            DesCountry = tempApp.Country,
                            DesCaseType = tempApp.CaseType,
                            CreatedBy = tempApp.UpdatedBy,
                            UpdatedBy = tempApp.UpdatedBy,
                            DateCreated = tempApp.LastUpdate,
                            LastUpdate = tempApp.LastUpdate,
                            Remarks = tempApp.Remarks,
                            GenAppId = 0,
                            GenApp = true,
                            GenCaseNumber = parentApp.CaseNumber,
                            GenSubCase = parentApp.SubCase
                        };
                        _cpiDbContext.GetRepository<PatDesignatedCountry>().Attach(newDes);
                    }
                }

                await _cpiDbContext.SaveChangesAsync();
                var genId = isGenerateApp ? newApp.AppId : newDes.DesId;

                var dueCountry = new FFDueCountry()
                {
                    DueCountryId = updated.DueCountryId,
                    tStamp = updated.tStamp
                };
                _cpiDbContext.GetRepository<FFDueCountry>().Attach(dueCountry);

                dueCountry.GenId = genId;
                dueCountry.UpdateDate = updated.LastUpdate;
                dueCountry.UpdatedBy = updated.UpdatedBy;
                dueCountry.LastUpdate = updated.LastUpdate;

                await _cpiDbContext.SaveChangesAsync();

                return genId;
            }

            return 0;
        }

        public async Task SaveUpdateDate(FFDueCountry updated)
        {
            var dueCountry = new FFDueCountry()
            {
                DueCountryId = updated.DueCountryId,
                tStamp = updated.tStamp
            };
            _cpiDbContext.GetRepository<FFDueCountry>().Attach(dueCountry);

            dueCountry.UpdateDate = updated.LastUpdate;
            dueCountry.UpdatedBy = updated.UpdatedBy;
            dueCountry.LastUpdate = updated.LastUpdate;

            await _cpiDbContext.SaveChangesAsync();
        }
    }
}
