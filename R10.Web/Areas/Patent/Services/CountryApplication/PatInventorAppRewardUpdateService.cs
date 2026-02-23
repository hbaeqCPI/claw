using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using R10.Core.DTOs;
using R10.Core.Entities.Patent;
using R10.Core.Interfaces.Patent;
using R10.Web.Areas.Patent.ViewModels;
using R10.Web.Areas.Shared.ViewModels;
using R10.Web.Extensions;
using R10.Web.Extensions.ActionResults;
using R10.Web.Helpers;
using R10.Web.Interfaces;
using R10.Web.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json;
using R10.Core.Entities;
using R10.Core.Interfaces;
using R10.Web.Models;
using R10.Web.Models.PageViewModels;
using R10.Core.Exceptions;
using R10.Core.Helpers;
using R10.Core;
using R10.Core.Interfaces.DMS;
using R10.Core.Entities.DMS;
using System.Data.SqlClient;
using System.Linq.Expressions;
using Microsoft.SharePoint.Client;
using R10.Web.Services;
using static iText.StyledXmlParser.Jsoup.Select.Evaluator;
using DocumentFormat.OpenXml.Wordprocessing;

namespace R10.Web.Areas.Patent.Services
{
    public class PatInventorAppRewardUpdateService : IPatInventorAppAwardUpdateService
    {
        private readonly IEntityService<PatInventorAwardCriteria> _awardCriteriaService;
        private readonly IEntityService<PatInventorAwardType> _awardTypeService;
        private readonly IEntityService<PatInventorApp> _inventorAppService;
        private readonly IParentEntityService<CountryApplication, PatInventorAppAward> _appAwardParentEntityService;
        private readonly IEntityService<PatInventorAppAward> _appAwardEntityService;
        private readonly IEntityService<PatInventorDMSAward> _dmsAwardEntityService;
        private readonly ICountryApplicationService _applicationService;
        private readonly IInventionService _inventionService;
        private readonly IDisclosureService _disclosureService;
        private readonly IParentEntityService<Disclosure, PatInventorDMSAward> _dmsAwardParentEntityService;
        private readonly IEntityService<DMSInventor> _inventorDMSService;
        private readonly IConfiguration _configuration;
        private readonly IPatInventorService _patInventorService;
        private readonly IWorkflowViewModelService _workflowViewModelService;
        private readonly ISystemSettings<DMSSetting> _dmsSettings;
        private readonly IDisclosureViewModelService _disclosureViewModelService;


        public PatInventorAppRewardUpdateService(ICountryApplicationService applicationService,
                                            IDisclosureService disclosureService,
                                            IEntityService<DMSInventor> inventorDMSService,
                                            IParentEntityService<Disclosure, PatInventorDMSAward> dmsAwardParentEntityService,
                                            IEntityService<PatInventorAwardCriteria> awardCriteriaService,
                                            IEntityService<PatInventorAwardType> awardTypeService,
                                            IEntityService<PatInventorApp> inventorAppService,
                                            IEntityService<PatInventorAppAward> appAwardEntityService,
                                            IEntityService<PatInventorDMSAward> dmsAwardEntityService,
                                            IParentEntityService<CountryApplication, PatInventorAppAward> appAwardParentEntityService,
                                            IConfiguration configuration,
                                            IPatInventorService patInventorService,
                                            IInventionService inventionService,
                                            IWorkflowViewModelService workflowViewModelService,
                                            ISystemSettings<DMSSetting> dmsSettings,
                                            IDisclosureViewModelService disclosureViewModelService)
        {
            _applicationService = applicationService;
            _awardCriteriaService = awardCriteriaService;
            _awardTypeService = awardTypeService;
            _inventorAppService = inventorAppService;
            _appAwardParentEntityService = appAwardParentEntityService;
            _disclosureService = disclosureService;
            _inventorDMSService = inventorDMSService;
            _appAwardEntityService = appAwardEntityService;
            _dmsAwardEntityService = dmsAwardEntityService;
            _dmsAwardParentEntityService = dmsAwardParentEntityService;
            _configuration = configuration;
            _patInventorService = patInventorService;
            _inventionService = inventionService;
            _workflowViewModelService = workflowViewModelService;
            _dmsSettings = dmsSettings;
            _disclosureViewModelService = disclosureViewModelService;
        }

        #region AppAward

        //public IQueryable<PatInventorAppAward> PatInventorAppAwards => _appAwardEntityService.QueryableList.AsNoTracking();
        public IQueryable<PatInventorAppAward> PatInventorAppAwards
        {
            get
            {
                var patInventorAppAwards = _appAwardEntityService.QueryableList.AsNoTracking();
                var patInventor = _patInventorService.QueryableList.AsNoTracking().Select(i => i.InventorID); // EntityFilter
                patInventorAppAwards = patInventorAppAwards.Where(a => patInventor.Contains(a.InventorID));

                return patInventorAppAwards;
            }
        }

        public IQueryable<PatInventorDMSAward> PatInventorDMSAwards => _dmsAwardEntityService.QueryableList.AsNoTracking();

        public async Task<List<PatInventorAppAward>> GetPatInventorAppAwardByInvId(int invId)
        {
            var appIds = await _applicationService.CountryApplications.Where(c => c.InvId == invId).Select(c => c.AppId).ToListAsync();
            return await PatInventorAppAwards.Where(a => appIds.Contains(a.AppId)).ToListAsync();
        }

        public async Task UpdateInventorInvAwards(int invId, List<PatInventorInv> inventorInvs, string userName)
        {
            var applications = _applicationService.CountryApplications.Where(c => c.InvId == invId);
            var updates = new List<PatInventorApp>();
            var updateAppIds = new List<int>();
            foreach (var ca in applications)
            {
                var inventors = _inventorAppService.QueryableList.Where(c => c.AppId == ca.AppId);
                foreach (var inventorInv in inventorInvs)
                {
                    var inventor = inventors.FirstOrDefault(c => c.InventorID == inventorInv.InventorID);
                    if (inventor == null)
                        continue;
                    if (inventorInv.EligibleForBasicAward == inventor.EligibleForBasicAward)
                        continue;
                    inventor.EligibleForBasicAward = inventorInv.EligibleForBasicAward;
                    updates.Add(inventor);
                }
                if (updates.Count() == 0)
                    return;
                updateAppIds.Add(ca.AppId);
            }
            await _inventorAppService.Update(updates);
            foreach (var appId in updateAppIds)
            {
                await UpdateInventorAppAwards(appId, null, true, userName);
            }
        }

        public async Task UpdateInventorAppAwards(int AppId, ApplicationModifiedFields applicationModifieds, bool fromInventorUpdate, string userName)
        {
            await CheckInventorAppAwards(await _applicationService.GetInventorAwardInfo(AppId), applicationModifieds, fromInventorUpdate, userName);
        }

        public async Task UpdateInventorInvAwards(int invId, string userName)
        {
            UpdateInventorAwards("Pat", "Disclosure", invId, userName);
        }

        public async Task UpdateInventorAppAwards(List<PatInventorAppAward> updated, string userName)
        {
            if (updated.Count() == 0) return;
            //await _appAwardParentEntityService.ChildService.Update(updated.FirstOrDefault().AppId, userName, updated, new List<PatInventorAppAward>(), new List<PatInventorAppAward>());
            //await _appAwardEntityService.Update(updated);
            foreach (var appId in updated.Select(u => u.AppId).Distinct().ToList())
            {
                await _appAwardParentEntityService.ChildService.Update(appId, userName, updated.Where(u => u.AppId == appId), new List<PatInventorAppAward>(), new List<PatInventorAppAward>());
            }
        }

        public async Task DeleteInventorAppAwards(List<PatInventorApp> deleted, string userName)
        {
            if (deleted.Count() < 1) return;
            IList<PatInventorAppAward> deletedAppAwards = new List<PatInventorAppAward>();
            IList<PatInventorAppAward> existAwards = (await _applicationService.GetInventorAwardInfo(deleted.FirstOrDefault().AppId)).Awards.ToList();
            foreach (PatInventorApp inventorApp in deleted)
            {
                IList<PatInventorAppAward> deletedInventorAwards = existAwards.Where(a => a.InventorID == inventorApp.InventorID && a.PaymentDate == null).ToList();
                deletedAppAwards.AddRange(deletedInventorAwards);
            }
            foreach (var delete in deletedAppAwards)
            {
                delete.PatCountryApplication = null;
                delete.PatInventor = null;
                delete.PatInventorAwardCriteria = null;
            }
            await _appAwardParentEntityService.ChildService.Update(deleted.FirstOrDefault().AppId, userName, new List<PatInventorAppAward>(), new List<PatInventorAppAward>(), deletedAppAwards);
        }

        public async Task UpdateInventorAppAwardsByInventorAppId(int InventorAppId, string userName)
        {
            PatInventorApp inventorApp = _inventorAppService.QueryableList.FirstOrDefault(c => c.InventorIDApp == InventorAppId && c.InventorAppInventor.EligibleForBasicAward);
            if (inventorApp != null)
            {
                await UpdateInventorAppAwards(inventorApp.AppId, null, true, userName);
            }
        }

        private async Task CheckInventorAppAwards(CountryApplication ca, ApplicationModifiedFields applicationModifieds, bool fromInventorUpdate, string userName)
        {
            if (ca.PatApplicationStatus.ActiveSwitch == false) return;
            List<PatInventorAwardCriteria> awardCriteria = GetInventorAwardCriteria();
            ca.Awards.Each(item => item.PatInventorAwardCriteria = awardCriteria.FirstOrDefault(c => c.AwardCriteriaId == item.AwardCriteriaId));


            if (applicationModifieds == null || applicationModifieds.IsChgIssDate)
                //await CheckInventorAppAwards(ca, applicationModifieds, "Issue", awardCriteria, fromInventorUpdate, userName);
                UpdateInventorAwards("Pat", "Issue", ca.AppId, userName);
            if (applicationModifieds == null || applicationModifieds.IsChgFilDate)
                //await CheckInventorAppAwards(ca, applicationModifieds, "Filing", awardCriteria, fromInventorUpdate, userName);
                UpdateInventorAwards("Pat", "Filing", ca.AppId, userName);
            if (ca.Invention.DisclosureDate != null)
                //await CheckInventorAppAwards(ca, applicationModifieds, "Disclosure", awardCriteria, fromInventorUpdate, userName);
                UpdateInventorAwards("Pat", "Disclosure", ca.InvId, userName);
        }

        private void UpdateInventorAwards(string system, string basedOn, int Id, string userName)
        {
            using (SqlConnection sqlConnection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.CommandText = "procWebPatInventorAwardsUpdate";
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Connection = sqlConnection;
                    cmd.CommandTimeout = 0;

                    SqlParameter param = new SqlParameter();
                    param.ParameterName = "@System";
                    param.Value = system;
                    cmd.Parameters.Add(param);

                    SqlParameter param2 = new SqlParameter();
                    param2.ParameterName = "@BasedOn";
                    param2.Value = basedOn;
                    cmd.Parameters.Add(param2);

                    SqlParameter param3 = new SqlParameter();
                    param3.ParameterName = "@Id";
                    param3.Value = Id;
                    cmd.Parameters.Add(param3);

                    SqlParameter param4 = new SqlParameter();
                    param4.ParameterName = "@UserName";
                    param4.Value = userName;
                    cmd.Parameters.Add(param4);

                    sqlConnection.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private async Task CheckInventorAppAwards(CountryApplication ca, ApplicationModifiedFields applicationModifieds, string basedOn, List<PatInventorAwardCriteria> criteria, bool fromInventorUpdate, string userName)
        {
            if (applicationModifieds != null)
            {
                if (basedOn == "Issue" && ca.IssDate == null)
                {
                    IList<PatInventorAppAward> RecordsTodeleted = ca.Awards.Where(a => a.PatInventorAwardCriteria.PatInventorAwardType.BasedOn == "Issue" && a.PaymentDate == null && (string.IsNullOrEmpty(a.PatInventorAwardCriteria.PatInventorAwardType.ClientIds) || ("," + a.PatInventorAwardCriteria.PatInventorAwardType.ClientIds + ",").Contains("," + ca.Invention.ClientID + ","))).ToList();
                    foreach (var delete in RecordsTodeleted)
                    {
                        delete.PatCountryApplication = null;
                        delete.PatInventor = null;
                        delete.PatInventorAwardCriteria = null;
                    }
                    await _appAwardParentEntityService.ChildService.Update(ca.AppId, userName, new List<PatInventorAppAward>(), new List<PatInventorAppAward>(), RecordsTodeleted);
                    return;
                }
                if (basedOn == "Filing" && ca.FilDate == null)
                {
                    IList<PatInventorAppAward> RecordsTodeleted = ca.Awards.Where(a => a.PatInventorAwardCriteria.PatInventorAwardType.BasedOn == "Filing" && a.PaymentDate == null && (string.IsNullOrEmpty(a.PatInventorAwardCriteria.PatInventorAwardType.ClientIds) || ("," + a.PatInventorAwardCriteria.PatInventorAwardType.ClientIds + ",").Contains("," + ca.Invention.ClientID + ","))).ToList();
                    foreach (var delete in RecordsTodeleted)
                    {
                        delete.PatCountryApplication = null;
                        delete.PatInventor = null;
                        delete.PatInventorAwardCriteria = null;
                    }
                    await _appAwardParentEntityService.ChildService.Update(ca.AppId, userName, new List<PatInventorAppAward>(), new List<PatInventorAppAward>(), RecordsTodeleted);
                    return;
                }

            }
            else
            {
                if (basedOn == "Issue" && ca.IssDate == null)
                {
                    return;
                }
                if (basedOn == "Filing" && ca.FilDate == null)
                {
                    return;
                }
            }

            List<PatInventorAwardCriteria> awardCriteria = criteria.Where(c => c.PatInventorAwardType != null && c.PatInventorAwardType.BasedOn == basedOn && (string.IsNullOrEmpty(c.PatInventorAwardType.ClientIds) || ("," + c.PatInventorAwardType.ClientIds + ",").Contains("," + ca.Invention.ClientID + ","))).ToList();
            if (awardCriteria.Count() == 0) return;

            DateTime awardDate = basedOn == "Issue" ? (DateTime)ca.IssDate :
                                (basedOn == "Filing" ? (DateTime)ca.FilDate :
                                (basedOn == "Disclousure" ? (DateTime)ca.Invention.DisclosureDate : DateTime.Now));

            var avaliableAwardCriteria = awardCriteria.Where(a => a.Country == ca.Country && a.CaseType == ca.CaseType && (a.EffStartDate == null || a.EffStartDate <= awardDate) && (a.EffEndDate == null || a.EffEndDate >= awardDate));
            if (avaliableAwardCriteria.Count() == 0) avaliableAwardCriteria = awardCriteria.Where(a => a.Country == null && a.CaseType == ca.CaseType && (a.EffStartDate == null || a.EffStartDate <= awardDate) && (a.EffEndDate == null || a.EffEndDate >= awardDate));
            if (avaliableAwardCriteria.Count() == 0) avaliableAwardCriteria = awardCriteria.Where(a => a.Country == ca.Country && a.CaseType == null && (a.EffStartDate == null || a.EffStartDate <= awardDate) && (a.EffEndDate == null || a.EffEndDate >= awardDate));
            if (avaliableAwardCriteria.Count() == 0) avaliableAwardCriteria = awardCriteria.Where(a => a.Country == null && a.CaseType == null && (a.EffStartDate == null || a.EffStartDate <= awardDate) && (a.EffEndDate == null || a.EffEndDate >= awardDate));
            if (avaliableAwardCriteria.Count() == 0) return;

            foreach (PatInventorAwardCriteria generatedAwardCriteria in avaliableAwardCriteria)
            {
                if (isNullOrZero(generatedAwardCriteria.NoOfInventors) ||
                ((isNullOrZero(generatedAwardCriteria.IndividualAmount)) && (isNullOrZero(generatedAwardCriteria.LeadAmount)))) continue;

                IList<PatInventorApp> inventorApps = ca.Inventors.Where(c => c.InventorAppInventor.EligibleForBasicAward).OrderBy(c => c.OrderOfEntry).ToList();
                IList<PatInventorAppAward> added = new List<PatInventorAppAward>();
                IList<PatInventorAppAward> updateded = new List<PatInventorAppAward>();
                IList<PatInventorAppAward> deleted = ca.Awards.Where(c => c.AwardCriteriaId == generatedAwardCriteria.AwardCriteriaId).ToList();

                if (deleted.Count() == 0 && ca.Inventors.Count() == 0) continue;

                //add from CA Update or from App Inventor Update
                if (applicationModifieds != null || fromInventorUpdate)
                {
                    var noOfInventorsToAward = generatedAwardCriteria.NoOfInventors <= inventorApps.Count() ? generatedAwardCriteria.NoOfInventors : inventorApps.Count();
                    if (noOfInventorsToAward == 0)
                        return;

                    AwardAmount awardAmount = GetAwardAmount(generatedAwardCriteria.MaxAmount, generatedAwardCriteria.PatInventorAwardType.MaxAmount, noOfInventorsToAward, generatedAwardCriteria.LeadAmount, generatedAwardCriteria.IndividualAmount, generatedAwardCriteria.IndividualMaxAmount, generatedAwardCriteria.DivideMaxAmount);

                    for (int i = 0; i < awardAmount.AwardNoOfInventors; i++)
                    {
                        PatInventorApp inventorApp = inventorApps.ElementAt(i);
                        if (inventorApp != null)
                        {
                            var existAward = deleted.Where(a => a.InventorID == inventorApp.InventorID).FirstOrDefault();
                            if (existAward != null)
                            {
                                if (UpdateWhenNeed(existAward, AwardInventor(inventorApp, generatedAwardCriteria, (!isNullOrZero(generatedAwardCriteria.LeadAmount) && i == 0) ? (decimal)awardAmount.LeadAmount : (decimal)awardAmount.IndividualAmount, awardDate)))
                                {
                                    updateded.Add(existAward);
                                }
                                deleted.Remove(existAward);
                                //existAward.PatCountryApplication = null;
                            }
                            else
                            {
                                added.Add(AwardInventor(inventorApp, generatedAwardCriteria, (!isNullOrZero(generatedAwardCriteria.LeadAmount) && i == 0) ? (decimal)awardAmount.LeadAmount : (decimal)awardAmount.IndividualAmount, awardDate));
                            }
                        }
                    }
                }
                //add from new CA case
                else if (!fromInventorUpdate)
                {
                    var noOfInventorsToAward = generatedAwardCriteria.NoOfInventors <= inventorApps.Count() ? generatedAwardCriteria.NoOfInventors : inventorApps.Count();
                    if (noOfInventorsToAward == 0)
                        return;

                    AwardAmount awardAmount = GetAwardAmount(generatedAwardCriteria.MaxAmount, generatedAwardCriteria.PatInventorAwardType.MaxAmount, noOfInventorsToAward, generatedAwardCriteria.LeadAmount, generatedAwardCriteria.IndividualAmount, generatedAwardCriteria.IndividualMaxAmount, generatedAwardCriteria.DivideMaxAmount);

                    for (int i = 0; i < awardAmount.AwardNoOfInventors; i++)
                    {
                        PatInventorApp inventorApp = inventorApps.ElementAt(i);
                        if (inventorApp != null)
                        {
                            added.Add(AwardInventor(inventorApp, generatedAwardCriteria, (!isNullOrZero(generatedAwardCriteria.LeadAmount) && i == 0) ? (decimal)awardAmount.LeadAmount : (decimal)awardAmount.IndividualAmount, awardDate));
                        }
                    }
                }

                foreach (var update in updateded)
                {
                    update.PatCountryApplication = null;
                    update.PatInventor = null;
                    update.PatInventorAwardCriteria = null;
                }

                IList<PatInventorAppAward> noDeleted = new List<PatInventorAppAward>();
                noDeleted.AddRange(deleted.Where(c => c.PaymentDate != null));

                foreach (var noDelete in noDeleted)
                {
                    deleted.Remove(noDelete);
                }

                foreach (var delete in deleted)
                {
                    delete.PatCountryApplication = null;
                    delete.PatInventor = null;
                    delete.PatInventorAwardCriteria = null;
                }

                await _appAwardParentEntityService.ChildService.Update(ca.AppId, userName, updateded, added, deleted);
            }
        }
        //private async Task CheckInventorAppAwards(CountryApplication ca, ApplicationModifiedFields applicationModifieds, string basedOn, List<PatInventorAwardCriteria> criteria, bool fromInventorUpdate, string userName)
        //{
        //    if (applicationModifieds != null)
        //    {
        //        if (basedOn == "Issue" && ca.IssDate == null)
        //        {
        //            IList<PatInventorAppAward> RecordsTodeleted = ca.Awards.Where(a => a.PatInventorAwardCriteria.PatInventorAwardType.BasedOn == "Issue" && a.PaymentDate == null && (string.IsNullOrEmpty(a.PatInventorAwardCriteria.PatInventorAwardType.ClientIds) || (","+a.PatInventorAwardCriteria.PatInventorAwardType.ClientIds+",").Contains(","+ca.Invention.ClientID+","))).ToList();
        //            foreach (var delete in RecordsTodeleted)
        //            {
        //                delete.PatCountryApplication = null;
        //                delete.PatInventor = null;
        //                delete.PatInventorAwardCriteria = null;
        //            }
        //            await _appAwardParentEntityService.ChildService.Update(ca.AppId, userName, new List<PatInventorAppAward>(), new List<PatInventorAppAward>(), RecordsTodeleted);
        //            return;
        //        }
        //        if (basedOn == "Filing" && ca.FilDate == null)
        //        {
        //            IList<PatInventorAppAward> RecordsTodeleted = ca.Awards.Where(a => a.PatInventorAwardCriteria.PatInventorAwardType.BasedOn == "Filing" && a.PaymentDate == null && (string.IsNullOrEmpty(a.PatInventorAwardCriteria.PatInventorAwardType.ClientIds) || ("," + a.PatInventorAwardCriteria.PatInventorAwardType.ClientIds + ",").Contains("," + ca.Invention.ClientID + ","))).ToList();
        //            foreach (var delete in RecordsTodeleted)
        //            {
        //                delete.PatCountryApplication = null;
        //                delete.PatInventor = null;
        //                delete.PatInventorAwardCriteria = null;
        //            }
        //            await _appAwardParentEntityService.ChildService.Update(ca.AppId, userName, new List<PatInventorAppAward>(), new List<PatInventorAppAward>(), RecordsTodeleted);
        //            return;
        //        }

        //    }
        //    else
        //    {
        //        if (basedOn == "Issue" && ca.IssDate == null)
        //        {
        //            return;
        //        }
        //        if (basedOn == "Filing" && ca.FilDate == null)
        //        {
        //            return;
        //        }
        //    }

        //    List<PatInventorAwardCriteria> awardCriteria = criteria.Where(c => c.PatInventorAwardType != null && c.PatInventorAwardType.BasedOn == basedOn && (string.IsNullOrEmpty(c.PatInventorAwardType.ClientIds) || ("," + c.PatInventorAwardType.ClientIds + ",").Contains("," + ca.Invention.ClientID + ","))).ToList();
        //    if (awardCriteria.Count() == 0) return;

        //    DateTime awardDate = basedOn == "Issue" ? (DateTime)ca.IssDate :
        //                        (basedOn == "Filing" ? (DateTime)ca.FilDate :
        //                        (basedOn == "Disclousure" ? (DateTime)ca.Invention.DisclosureDate : DateTime.Now));

        //    var avaliableAwardCriteria = awardCriteria.Where(a => a.Country == ca.Country && a.CaseType == ca.CaseType && (a.EffStartDate == null || a.EffStartDate <= awardDate) && (a.EffEndDate == null || a.EffEndDate >= awardDate));
        //    if (avaliableAwardCriteria.Count() == 0) avaliableAwardCriteria = awardCriteria.Where(a => a.Country == null && a.CaseType == ca.CaseType && (a.EffStartDate == null || a.EffStartDate <= awardDate) && (a.EffEndDate == null || a.EffEndDate >= awardDate));
        //    if (avaliableAwardCriteria.Count() == 0) avaliableAwardCriteria = awardCriteria.Where(a => a.Country == ca.Country && a.CaseType == null && (a.EffStartDate == null || a.EffStartDate <= awardDate) && (a.EffEndDate == null || a.EffEndDate >= awardDate));
        //    if (avaliableAwardCriteria.Count() == 0) avaliableAwardCriteria = awardCriteria.Where(a => a.Country == null && a.CaseType == null && (a.EffStartDate == null || a.EffStartDate <= awardDate) && (a.EffEndDate == null || a.EffEndDate >= awardDate));
        //    if (avaliableAwardCriteria.Count() == 0) return;

        //    foreach (PatInventorAwardCriteria generatedAwardCriteria in avaliableAwardCriteria)
        //    {
        //        if (isNullOrZero(generatedAwardCriteria.NoOfInventors) ||
        //        ((isNullOrZero(generatedAwardCriteria.IndividualAmount)) && (isNullOrZero(generatedAwardCriteria.LeadAmount)))) continue;

        //        IList<PatInventorApp> inventorApps = ca.Inventors.Where(c=>c.InventorAppInventor.EligibleForBasicAward).OrderBy(c => c.OrderOfEntry).ToList();
        //        IList<PatInventorAppAward> added = new List<PatInventorAppAward>();
        //        IList<PatInventorAppAward> updateded = new List<PatInventorAppAward>();
        //        IList<PatInventorAppAward> deleted = ca.Awards.Where(c => c.AwardCriteriaId == generatedAwardCriteria.AwardCriteriaId).ToList();

        //        if (deleted.Count() == 0 && ca.Inventors.Count() == 0) continue;

        //        //add from CA Update or from App Inventor Update
        //        if (applicationModifieds != null || fromInventorUpdate)
        //        {
        //            var noOfInventorsToAward = generatedAwardCriteria.NoOfInventors <= inventorApps.Count() ? generatedAwardCriteria.NoOfInventors : inventorApps.Count();
        //            if (noOfInventorsToAward == 0)
        //                return;

        //            AwardAmount awardAmount = GetAwardAmount(generatedAwardCriteria.MaxAmount, generatedAwardCriteria.PatInventorAwardType.MaxAmount, noOfInventorsToAward, generatedAwardCriteria.LeadAmount, generatedAwardCriteria.IndividualAmount, generatedAwardCriteria.IndividualMaxAmount, generatedAwardCriteria.DivideMaxAmount);

        //            for (int i = 0; i < awardAmount.AwardNoOfInventors; i++)
        //            {
        //                PatInventorApp inventorApp = inventorApps.ElementAt(i);
        //                if (inventorApp != null)
        //                {
        //                    var existAward = deleted.Where(a => a.InventorID == inventorApp.InventorID).FirstOrDefault();
        //                    if (existAward != null)
        //                    {
        //                        if (UpdateWhenNeed(existAward, AwardInventor(inventorApp, generatedAwardCriteria, (!isNullOrZero(generatedAwardCriteria.LeadAmount) && i == 0)? (decimal) awardAmount.LeadAmount: (decimal) awardAmount.IndividualAmount, awardDate)))
        //                        {
        //                            updateded.Add(existAward);
        //                        }
        //                        deleted.Remove(existAward);
        //                        //existAward.PatCountryApplication = null;
        //                    }
        //                    else
        //                    {
        //                        added.Add(AwardInventor(inventorApp, generatedAwardCriteria, (!isNullOrZero(generatedAwardCriteria.LeadAmount) && i == 0) ? (decimal)awardAmount.LeadAmount : (decimal)awardAmount.IndividualAmount, awardDate));
        //                    }
        //                }
        //            }
        //        }
        //        //add from new CA case
        //        else if (!fromInventorUpdate)
        //        {
        //            var noOfInventorsToAward = generatedAwardCriteria.NoOfInventors <= inventorApps.Count() ? generatedAwardCriteria.NoOfInventors : inventorApps.Count();
        //            if (noOfInventorsToAward == 0)
        //                return;

        //            AwardAmount awardAmount = GetAwardAmount(generatedAwardCriteria.MaxAmount, generatedAwardCriteria.PatInventorAwardType.MaxAmount, noOfInventorsToAward, generatedAwardCriteria.LeadAmount, generatedAwardCriteria.IndividualAmount, generatedAwardCriteria.IndividualMaxAmount, generatedAwardCriteria.DivideMaxAmount);

        //            for (int i = 0; i < awardAmount.AwardNoOfInventors; i++)
        //            {
        //                PatInventorApp inventorApp = inventorApps.ElementAt(i);
        //                if (inventorApp != null)
        //                {
        //                    added.Add(AwardInventor(inventorApp, generatedAwardCriteria, (!isNullOrZero(generatedAwardCriteria.LeadAmount) && i == 0) ? (decimal)awardAmount.LeadAmount : (decimal)awardAmount.IndividualAmount, awardDate));
        //                }
        //            }
        //        }

        //        foreach (var update in updateded)
        //        {
        //            update.PatCountryApplication = null;
        //            update.PatInventor = null;
        //            update.PatInventorAwardCriteria = null;
        //        }

        //        IList<PatInventorAppAward> noDeleted = new List<PatInventorAppAward>();
        //        noDeleted.AddRange(deleted.Where(c=>c.PaymentDate != null));

        //        foreach(var noDelete in noDeleted)
        //        {
        //            deleted.Remove(noDelete);
        //        }

        //        foreach (var delete in deleted)
        //        {
        //            delete.PatCountryApplication = null;
        //            delete.PatInventor = null;
        //            delete.PatInventorAwardCriteria = null;
        //        }

        //        await _appAwardParentEntityService.ChildService.Update(ca.AppId, userName, updateded, added, deleted);
        //    }
        //}

        private AwardAmount GetAwardAmount(decimal? criteriaMaxAmount, decimal? awardTypeMaxAmount, int? noOfInventorsToAward, decimal? leadAmount, decimal? individualAmount, decimal? individualMaxAmount, bool divide)
        {
            AwardAmount result = new AwardAmount();

            var maxAmount = isNullOrZero(criteriaMaxAmount) ? (isNullOrZero(awardTypeMaxAmount) ? null : awardTypeMaxAmount) : criteriaMaxAmount;
            if (isNullOrZero(leadAmount))
            {
                if (isNullOrZero(maxAmount))
                {
                    if (isNullOrZero(individualMaxAmount))
                    {
                        result.LeadAmount = individualAmount;
                        result.IndividualAmount = individualAmount;
                        result.AwardNoOfInventors = (int)noOfInventorsToAward;
                    }
                    else
                    {
                        result.LeadAmount = individualMaxAmount;
                        result.IndividualAmount = individualMaxAmount;
                        result.AwardNoOfInventors = (int)noOfInventorsToAward;
                    }
                }
                else
                {
                    if (isNullOrZero(individualMaxAmount))
                    {
                        if (divide)
                        {
                            result.LeadAmount = maxAmount / noOfInventorsToAward;
                            result.IndividualAmount = maxAmount / noOfInventorsToAward;
                            result.AwardNoOfInventors = (int)noOfInventorsToAward;
                        }
                        else
                        {
                            result.LeadAmount = individualAmount;
                            result.IndividualAmount = individualAmount;
                            int maxCanAwardInventors = (int)Math.Floor((decimal)(maxAmount / individualAmount));
                            result.AwardNoOfInventors = noOfInventorsToAward >= maxCanAwardInventors ? (int)noOfInventorsToAward : maxCanAwardInventors;
                        }
                    }
                    else
                    {
                        if (maxAmount >= individualMaxAmount * noOfInventorsToAward)
                        {
                            result.LeadAmount = individualMaxAmount;
                            result.IndividualAmount = individualMaxAmount;
                            result.AwardNoOfInventors = (int)noOfInventorsToAward;
                        }
                        else
                        {
                            if (divide)
                            {
                                result.LeadAmount = maxAmount / noOfInventorsToAward;
                                result.IndividualAmount = maxAmount / noOfInventorsToAward;
                                result.AwardNoOfInventors = (int)noOfInventorsToAward;
                            }
                            else
                            {
                                result.LeadAmount = individualAmount;
                                result.IndividualAmount = individualAmount;
                                int maxCanAwardInventors = (int)Math.Floor((decimal)(maxAmount / individualAmount));
                                result.AwardNoOfInventors = noOfInventorsToAward >= maxCanAwardInventors ? (int)noOfInventorsToAward : maxCanAwardInventors;
                            }
                        }
                    }
                }
                //if (maxAmount == null || noOfInventorsToAward * individualAmount <= maxAmount)
                //{
                //    result.LeadAmount = individualAmount;
                //    result.IndividualAmount = individualAmount;
                //}
                //else
                //{
                //    result.LeadAmount = maxAmount / noOfInventorsToAward;
                //    result.IndividualAmount = maxAmount / noOfInventorsToAward;
                //}
            }
            else
            {
                if (isNullOrZero(maxAmount))
                {
                    if (isNullOrZero(individualMaxAmount))
                    {
                        result.LeadAmount = leadAmount;
                        result.IndividualAmount = individualAmount;
                        result.AwardNoOfInventors = (int)noOfInventorsToAward;
                    }
                    else
                    {
                        result.LeadAmount = leadAmount;
                        result.IndividualAmount = individualMaxAmount;
                        result.AwardNoOfInventors = (int)noOfInventorsToAward;
                    }
                }
                else
                {
                    if (leadAmount >= maxAmount)
                    {
                        result.LeadAmount = maxAmount;
                        result.IndividualAmount = 0;
                        result.AwardNoOfInventors = 1;
                    }
                    else
                    {
                        if (isNullOrZero(individualMaxAmount))
                        {
                            if (divide)
                            {
                                result.LeadAmount = leadAmount;
                                result.IndividualAmount = noOfInventorsToAward == 1 ? 0 : (maxAmount - leadAmount) / (noOfInventorsToAward - 1);
                                result.AwardNoOfInventors = (int)noOfInventorsToAward;
                            }
                            else
                            {
                                result.LeadAmount = leadAmount;
                                result.IndividualAmount = individualAmount;
                                int maxCanAwardInventors = 1 + (int)Math.Floor((decimal)((maxAmount - leadAmount) / individualAmount));
                                result.AwardNoOfInventors = noOfInventorsToAward >= maxCanAwardInventors ? (int)noOfInventorsToAward : maxCanAwardInventors;
                            }
                        }
                        else
                        {
                            if (maxAmount >= leadAmount + individualMaxAmount * (noOfInventorsToAward - 1))
                            {
                                result.LeadAmount = leadAmount;
                                result.IndividualAmount = individualMaxAmount;
                                result.AwardNoOfInventors = (int)noOfInventorsToAward;
                            }
                            else
                            {
                                if (divide)
                                {
                                    result.LeadAmount = leadAmount;
                                    result.IndividualAmount = noOfInventorsToAward == 1 ? 0 : (maxAmount - leadAmount) / (noOfInventorsToAward - 1);
                                    result.AwardNoOfInventors = (int)noOfInventorsToAward;
                                }
                                else
                                {
                                    result.LeadAmount = leadAmount;
                                    result.IndividualAmount = individualAmount;
                                    int maxCanAwardInventors = 1 + (int)Math.Floor((decimal)((maxAmount - leadAmount) / individualAmount));
                                    result.AwardNoOfInventors = noOfInventorsToAward >= maxCanAwardInventors ? (int)noOfInventorsToAward : maxCanAwardInventors;
                                }
                            }
                        }
                    }
                }

                //if (maxAmount == null || (leadAmount <= maxAmount && (noOfInventorsToAward - 1) * individualAmount <= maxAmount - leadAmount))
                //{
                //    result.LeadAmount = leadAmount;
                //    result.IndividualAmount = individualAmount;
                //}
                //else if (leadAmount > maxAmount)
                //{
                //    result.LeadAmount = maxAmount;
                //    result.IndividualAmount = 0;
                //}
                //else
                //{
                //    result.LeadAmount = leadAmount;
                //    result.IndividualAmount = noOfInventorsToAward == 1 ? 0 : (maxAmount - leadAmount) / (noOfInventorsToAward - 1);
                //}
            }

            return result;
        }

        private class AwardAmount
        {
            public decimal? LeadAmount;
            public decimal? IndividualAmount;
            public int AwardNoOfInventors;
        }

        private bool UpdateWhenNeed(PatInventorAppAward existRecord, PatInventorAppAward generatedRecord)
        {
            var result = false;
            if (existRecord.AwardCriteriaId != generatedRecord.AwardCriteriaId) return result;
            if (existRecord.Amount != generatedRecord.Amount)
            {
                existRecord.Amount = generatedRecord.Amount;
                result = true;
            }
            if (existRecord.AwardDate != generatedRecord.AwardDate)
            {
                existRecord.AwardDate = generatedRecord.AwardDate;
                result = true;
            }
            if (existRecord.Remarks != generatedRecord.Remarks)
            {
                existRecord.Remarks = generatedRecord.Remarks;
                result = true;
            }
            return result;
        }

        private bool isNullOrZero(int? value)
        {
            return value == null || value == 0;
        }

        private bool isNullOrZero(decimal? value)
        {
            return value == null || value == 0;
        }

        private List<PatInventorAwardCriteria> GetInventorAwardCriteria()
        {
            List<PatInventorAwardCriteria> criterias = _awardCriteriaService.QueryableList.ToList();
            criterias.Each(item => item.PatInventorAwardType = _awardTypeService.QueryableList.FirstOrDefault(c => c.AwardTypeId == item.AwardTypeId));
            return criterias;
        }

        private PatInventorAppAward AwardInventor(PatInventorApp inventorApp, PatInventorAwardCriteria criteria, decimal awardAmount, DateTime awardDate)
        {

            PatInventorAppAward award = new PatInventorAppAward()
            {
                AppId = inventorApp.AppId,
                InventorID = inventorApp.InventorID,
                Amount = awardAmount,
                AwardCriteriaId = criteria.AwardCriteriaId,
                AwardDate = awardDate,
                AwardType = criteria.PatInventorAwardType.AwardType
            };

            return award;
        }

        #endregion AppAward

        #region DMSAward
        public async Task UpdateInventorDMSAwards(int DMSId, bool fromInventorUpdate, string userName, string basedOn)
        {
            await CheckInventorDMSAwards(await _disclosureService.GetInventorAwardInfo(DMSId), fromInventorUpdate, userName, basedOn);
        }

        public async Task DeleteInventorDMSAwards(List<DMSInventor> deleted, string userName)
        {
            if (deleted.Count() < 1) return;
            IList<PatInventorDMSAward> deletedDMSAwards = new List<PatInventorDMSAward>();
            IList<PatInventorDMSAward> existAwards = (await _disclosureService.GetInventorAwardInfo(deleted.FirstOrDefault().DMSId)).Awards.ToList();
            foreach (DMSInventor inventorDMS in deleted)
            {
                IList<PatInventorDMSAward> deletedInventorAwards = existAwards.Where(a => a.InventorID == inventorDMS.InventorID).ToList();
                deletedDMSAwards.AddRange(deletedInventorAwards);
            }
            foreach (var delete in deletedDMSAwards)
            {
                delete.Disclosure = null;
                delete.PatInventor = null;
                delete.PatInventorAwardCriteria = null;
            }
            await _dmsAwardParentEntityService.ChildService.Update(deleted.FirstOrDefault().DMSId, userName, new List<PatInventorDMSAward>(), new List<PatInventorDMSAward>(), deletedDMSAwards);
        }

        public async Task UpdateInventorDMSAwardsByDMSInventorId(int DMSInventorID, string userName)
        {
            DMSInventor inventorDMS = _inventorDMSService.QueryableList.FirstOrDefault(c => c.DMSInventorID == DMSInventorID);
            if (inventorDMS != null)
            {
                await UpdateInventorDMSAwards(inventorDMS.DMSId, true, userName, "Submission");
                await UpdateInventorDMSAwards(inventorDMS.DMSId, true, userName, "Recommendation");
            }
        }

        private async Task CheckInventorDMSAwards(Disclosure disclosure, bool fromInventorUpdate, string userName, string basedOn)
        {
            var reviewStatus = _disclosureService.DisclosureStatuses.Where(s => s.DisclosureStatus == disclosure.DisclosureStatus && (s.CanReview == true || s.CanPreview == true)).Any();
            if (basedOn.Equals("Submission") && reviewStatus && disclosure.SubmittedDate != null)
            {
                //List<PatInventorAwardCriteria> awardCriteria = GetInventorAwardCriteria();
                //disclosure.Awards.Each(item => item.PatInventorAwardCriteria = awardCriteria.FirstOrDefault(c => c.AwardCriteriaId == item.AwardCriteriaId));

                //await CheckInventorDMSAwards(disclosure, "Submission", awardCriteria, fromInventorUpdate, userName);
                UpdateInventorAwards("DMS", "Submission", disclosure.DMSId, userName);
            }
            if (basedOn.Equals("Recommendation") && disclosure.Recommendation != null && disclosure.RecommendationDate != null)
            {
                //List<PatInventorAwardCriteria> awardCriteria = GetInventorAwardCriteria();
                //disclosure.Awards.Each(item => item.PatInventorAwardCriteria = awardCriteria.FirstOrDefault(c => c.AwardCriteriaId == item.AwardCriteriaId));

                //await CheckInventorDMSAwards(disclosure, "Recommendation", awardCriteria, fromInventorUpdate, userName);
                UpdateInventorAwards("DMS", "Recommendation", disclosure.DMSId, userName);
            }
        }

        private async Task CheckInventorDMSAwards(Disclosure disclosure, string basedOn, List<PatInventorAwardCriteria> criteria, bool fromInventorUpdate, string userName)
        {
            List<PatInventorAwardCriteria> awardCriteria = criteria.Where(c => c.PatInventorAwardType != null && c.PatInventorAwardType.BasedOn == basedOn && (string.IsNullOrEmpty(c.PatInventorAwardType.ClientIds) || ("," + c.PatInventorAwardType.ClientIds + ",").Contains("," + disclosure.ClientID + ","))).ToList();
            if (awardCriteria.Count() == 0) return;

            DateTime awardDate = basedOn.Equals("Submission") ? (DateTime)disclosure.SubmittedDate : (DateTime)disclosure.RecommendationDate;

            var avaliableAwardCriteria = awardCriteria.Where(a => a.PatInventorAwardType.BasedOn.Equals(basedOn) && (string.IsNullOrEmpty(a.PatInventorAwardType.ClientIds) || ("," + a.PatInventorAwardType.ClientIds + ",").Contains("," + disclosure.ClientID + ",")) && (!a.PatInventorAwardType.BasedOn.Equals("Recommendation") || a.Recommendation == disclosure.Recommendation) && (a.EffStartDate == null || a.EffStartDate <= awardDate) && (a.EffEndDate == null || a.EffEndDate >= awardDate));

            if (avaliableAwardCriteria.Count() == 0) return;

            foreach (PatInventorAwardCriteria generatedAwardCriteria in avaliableAwardCriteria)
            {
                if (isNullOrZero(generatedAwardCriteria.NoOfInventors) ||
                    ((isNullOrZero(generatedAwardCriteria.IndividualAmount)) && (isNullOrZero(generatedAwardCriteria.LeadAmount)))) continue;

                IList<DMSInventor> inventorDMSs = disclosure.Inventors.Where(c => c.PatInventor.EligibleForBasicAward).OrderBy(c => c.OrderOfEntry).ToList();
                IList<PatInventorDMSAward> added = new List<PatInventorDMSAward>();
                IList<PatInventorDMSAward> updateded = new List<PatInventorDMSAward>();
                IList<PatInventorDMSAward> deleted = disclosure.Awards.Where(c => c.AwardCriteriaId == generatedAwardCriteria.AwardCriteriaId).ToList();

                if (deleted.Count() == 0 && disclosure.Inventors.Count() == 0) continue;

                //add from disclosure Update or from DMS Inventor Update
                if (fromInventorUpdate)
                {
                    var noOfInventorsToAward = generatedAwardCriteria.NoOfInventors <= inventorDMSs.Count() ? generatedAwardCriteria.NoOfInventors : inventorDMSs.Count();
                    if (noOfInventorsToAward == 0)
                        return;

                    AwardAmount awardAmount = GetAwardAmount(generatedAwardCriteria.MaxAmount, generatedAwardCriteria.PatInventorAwardType.MaxAmount, noOfInventorsToAward, generatedAwardCriteria.LeadAmount, generatedAwardCriteria.IndividualAmount, generatedAwardCriteria.IndividualMaxAmount, generatedAwardCriteria.DivideMaxAmount);

                    for (int i = 0; i < awardAmount.AwardNoOfInventors; i++)
                    {
                        DMSInventor inventorDMS = inventorDMSs.ElementAt(i);
                        if (inventorDMS != null)
                        {
                            var existAward = deleted.Where(a => a.InventorID == inventorDMS.InventorID).FirstOrDefault();
                            if (existAward != null)
                            {
                                if (UpdateWhenNeed(existAward, AwardInventor(inventorDMS, generatedAwardCriteria, (!isNullOrZero(generatedAwardCriteria.LeadAmount) && i == 0) ? (decimal)awardAmount.LeadAmount : (decimal)awardAmount.IndividualAmount, awardDate)))
                                {
                                    updateded.Add(existAward);
                                }
                                deleted.Remove(existAward);
                            }
                            else
                            {
                                added.Add(AwardInventor(inventorDMS, generatedAwardCriteria, (!isNullOrZero(generatedAwardCriteria.LeadAmount) && i == 0) ? (decimal)awardAmount.LeadAmount : (decimal)awardAmount.IndividualAmount, awardDate));
                            }
                        }
                    }
                }
                //add from new DMS case
                else if (!fromInventorUpdate)
                {
                    var noOfInventorsToAward = generatedAwardCriteria.NoOfInventors <= inventorDMSs.Count() ? generatedAwardCriteria.NoOfInventors : inventorDMSs.Count();
                    if (noOfInventorsToAward == 0)
                        return;

                    AwardAmount awardAmount = GetAwardAmount(generatedAwardCriteria.MaxAmount, generatedAwardCriteria.PatInventorAwardType.MaxAmount, noOfInventorsToAward, generatedAwardCriteria.LeadAmount, generatedAwardCriteria.IndividualAmount, generatedAwardCriteria.IndividualMaxAmount, generatedAwardCriteria.DivideMaxAmount);

                    for (int i = 0; i < awardAmount.AwardNoOfInventors; i++)
                    {
                        DMSInventor inventorDMS = inventorDMSs.ElementAt(i);
                        if (inventorDMS != null)
                        {
                            added.Add(AwardInventor(inventorDMS, generatedAwardCriteria, (!isNullOrZero(generatedAwardCriteria.LeadAmount) && i == 0) ? (decimal)awardAmount.LeadAmount : (decimal)awardAmount.IndividualAmount, awardDate));
                        }
                    }
                }

                foreach (var update in updateded)
                {
                    update.Disclosure = null;
                    update.PatInventor = null;
                    update.PatInventorAwardCriteria = null;
                }

                IList<PatInventorDMSAward> noDeleted = new List<PatInventorDMSAward>();
                noDeleted.AddRange(deleted.Where(c => c.PaymentDate != null));

                foreach (var noDelete in noDeleted)
                {
                    deleted.Remove(noDelete);
                }

                foreach (var delete in deleted)
                {
                    delete.Disclosure = null;
                    delete.PatInventor = null;
                    delete.PatInventorAwardCriteria = null;
                }

                await _dmsAwardParentEntityService.ChildService.Update(disclosure.DMSId, userName, updateded, added, deleted);
            }
        }

        private bool UpdateWhenNeed(PatInventorDMSAward existRecord, PatInventorDMSAward generatedRecord)
        {
            if (existRecord.PaymentDate != null)
                return false;

            var result = false;

            if (existRecord.AwardCriteriaId != generatedRecord.AwardCriteriaId) return result;
            if (existRecord.Amount != generatedRecord.Amount)
            {
                existRecord.Amount = generatedRecord.Amount;
                result = true;
            }
            if (existRecord.AwardDate != generatedRecord.AwardDate)
            {
                existRecord.AwardDate = generatedRecord.AwardDate;
                result = true;
            }
            if (existRecord.Remarks != generatedRecord.Remarks)
            {
                existRecord.Remarks = generatedRecord.Remarks;
                result = true;
            }
            return result;
        }

        private PatInventorDMSAward AwardInventor(DMSInventor inventorDMS, PatInventorAwardCriteria criteria, decimal awardAmount, DateTime awardDate)
        {
            PatInventorDMSAward award = new PatInventorDMSAward()
            {
                DMSId = inventorDMS.DMSId,
                InventorID = inventorDMS.InventorID,
                Amount = awardAmount,
                AwardCriteriaId = criteria.AwardCriteriaId,
                AwardDate = awardDate,
                AwardType = criteria.PatInventorAwardType.AwardType
            };

            return award;
        }
        #endregion DMSAward

        #region Workflow
        public async Task<List<WorkflowEmailViewModel>> GenerationWorkflow(List<PatInventorAppAward> oldAwards, List<PatInventorAppAward> newAwards, CountryApplication app, string emailUrl)
        {
            var workFlows = new List<WorkflowViewModel>();
            var emailWorkflows = new List<WorkflowEmailViewModel>();

            var newGenerates = newAwards.Where(a => a.PaymentDate == null)
                .Except(oldAwards.Where(a => a.PaymentDate == null), new PatAwardComparer())
                .ToList();

            if (newGenerates.Any())
            {
                var workflowActions = await _workflowViewModelService.GetCountryApplicationWorkflowActions(app, PatWorkflowTriggerType.InventorAwardGenerated, false);
                if (workflowActions.Any())
                {
                    workflowActions = _workflowViewModelService.ClearPatBaseWorkflowActions(workflowActions);
                    foreach (var item in workflowActions)
                    {
                        foreach (var newGenerate in newGenerates)
                        {
                            workFlows.Add(new WorkflowViewModel
                            {
                                ActionTypeId = item.ActionTypeId,
                                ActionValueId = item.ActionValueId,
                                Preview = item.Preview,
                                AutoAttachImages = item.IncludeAttachments,
                                EmailUrl = emailUrl,
                                Id = newGenerate.AwardId,
                                AttachmentFilter = item.AttachmentFilter
                            });
                        }
                    }
                }
            }

            emailWorkflows = workFlows.Where(wf => wf.ActionTypeId == (int)PatWorkflowActionType.SendEmail)
                 .Select(wf => new WorkflowEmailViewModel
                 {
                     isAutoEmail = !wf.Preview,
                     qeSetupId = wf.ActionValueId,
                     autoAttachImages = wf.AutoAttachImages,
                     id = (int)wf.Id,
                     fileNames = new string[] { },
                     emailUrl = wf.EmailUrl,
                     attachmentFilter = wf.AttachmentFilter
                 }).Distinct().ToList();

            return emailWorkflows;
        }

        public async Task<List<WorkflowEmailViewModel>> GenerationWorkflow(List<PatInventorDMSAward> oldAwards, List<PatInventorDMSAward> newAwards, Disclosure disclosure, string emailUrl)
        {
            var emailWorkflows = new List<WorkflowEmailViewModel>();
            var newGenerates = newAwards.Where(a => a.PaymentDate == null)
                .Except(oldAwards.Where(a => a.PaymentDate == null), new DMSAwardComparer())
                .ToList();

            if (newGenerates.Any())
            {
                var dmsEmailWorkflows = await _disclosureViewModelService.ProcessSaveWorkflow(emailUrl, string.Empty, disclosure, checkInventorAwardGenerateWorkflow: true);
                foreach (var item in dmsEmailWorkflows)
                {
                    foreach (var newGenerate in newGenerates)
                    {
                        emailWorkflows.Add(new WorkflowEmailViewModel()
                        {
                            isAutoEmail = item.isAutoEmail,
                            qeSetupId = item.qeSetupId,
                            autoAttachImages = item.autoAttachImages,
                            id = newGenerate.AwardId,
                            fileNames = item.fileNames,
                            emailUrl = item.emailUrl,
                            attachmentFilter = item.attachmentFilter
                        });
                    }
                }
            }

            return emailWorkflows;
        }

        private class PatAwardComparer : IEqualityComparer<PatInventorAppAward>
        {
            public bool Equals(PatInventorAppAward x, PatInventorAppAward y)
            {
                return x.InventorID == y.InventorID
                    && x.Amount == y.Amount
                    && x.AwardCriteriaId == y.AwardCriteriaId
                    && x.AwardDate == y.AwardDate
                    && x.AwardType == y.AwardType
                    ;
            }

            public int GetHashCode(PatInventorAppAward obj)
            {
                return HashCode.Combine(obj.InventorID, obj.Amount, obj.AwardCriteriaId, obj.AwardDate, obj.AwardType);
            }
        }

        private class DMSAwardComparer : IEqualityComparer<PatInventorDMSAward>
        {
            public bool Equals(PatInventorDMSAward x, PatInventorDMSAward y)
            {
                return x.InventorID == y.InventorID
                    && x.Amount == y.Amount
                    && x.AwardCriteriaId == y.AwardCriteriaId
                    && x.AwardDate == y.AwardDate
                    && x.AwardType == y.AwardType
                    ;
            }

            public int GetHashCode(PatInventorDMSAward obj)
            {
                return HashCode.Combine(obj.InventorID, obj.Amount, obj.AwardCriteriaId, obj.AwardDate, obj.AwardType);
            }
        }
        #endregion
    }
}
