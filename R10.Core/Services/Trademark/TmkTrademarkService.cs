using Microsoft.EntityFrameworkCore;
using R10.Core.DTOs;
using R10.Core.Entities;
using R10.Core.Entities.Trademark;
using R10.Core.Exceptions;
using R10.Core.Helpers;
using R10.Core.Identity;
using R10.Core.Interfaces;
using R10.Core.Entities.Documents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Threading.Tasks;
using R10.Core.Entities.Patent;
using System.Xml.XPath;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.IdentityModel.Tokens;

namespace R10.Core.Services
{

    public class TmkTrademarkService : ITmkTrademarkService
    {
        private readonly ITmkTrademarkRepository _trademarkRepository;
        private readonly IApplicationDbContext _repository;
        private readonly IEntityService<TmkTrademarkStatus> _trademarkStatusService;
        private readonly ClaimsPrincipal _user;
        private readonly ISystemSettings<TmkSetting> _settings;
        private readonly ICPiSystemSettingManager _systemSettingManager;

        private bool? _isOwnerRequired = null;

        public TmkTrademarkService(
            ITmkTrademarkRepository tmkTrademarkRepository,
             IApplicationDbContext repository,
            IEntityService<TmkTrademarkStatus> trademarkStatusService,
            ClaimsPrincipal user,
            ISystemSettings<TmkSetting> settings,
                ICPiSystemSettingManager systemSettingManager
            ) 
        {
            _trademarkRepository = tmkTrademarkRepository;
            _repository = repository;
            _trademarkStatusService = trademarkStatusService;
            _user = user;
            _settings = settings;
            _systemSettingManager = systemSettingManager;
        }

        public IQueryable<TmkTrademark> TmkTrademarks
        {
            get
            {
                var trademarks = _trademarkRepository.QueryableList;

                if (_user.HasRespOfficeFilter(SystemType.Trademark))
                    trademarks = trademarks.Where(RespOfficeFilter());

                if (HasEntityFilter())
                    trademarks = trademarks.Where(EntityFilter());

                return trademarks;
            }
        }

        private bool IsMultipleOwners => _settings.GetSetting().Result.IsMultipleOwnerOn;

        public bool IsOwnerRequired
        {
            get
            {
                _isOwnerRequired = _isOwnerRequired ?? (_user.GetEntityFilterType() == CPiEntityType.Owner && IsMultipleOwners);
                return (bool)_isOwnerRequired;
            }
        }
        public IQueryable<T> QueryableChildList<T>() where T : BaseEntity
        {
            var queryableList = _repository.Set<T>() as IQueryable<T>;

            if (_user.HasRespOfficeFilter(SystemType.Trademark) || _user.HasEntityFilter())
                queryableList = queryableList.Where(t => this.TmkTrademarks.Any(tmk => tmk.TmkId == EF.Property<int>(t, "TmkId")));

            return queryableList;
        }

        protected bool HasEntityFilter()
        {
            if (_user.HasEntityFilter())
                switch (_user.GetEntityFilterType())
                {
                    case CPiEntityType.Client:
                    case CPiEntityType.Agent:
                    case CPiEntityType.Attorney:
                    case CPiEntityType.Owner:
                    case CPiEntityType.ContactPerson:
                        return true;
                }

            return false;
        }

        public Expression<Func<TmkTrademark, bool>> RespOfficeFilter()
        {
            return a => _trademarkRepository.CPiUserSystemRoles.Any(r => r.UserId == _user.GetUserIdentifier() && r.SystemId == SystemType.Trademark && a.RespOffice == r.RespOffice && !string.IsNullOrEmpty(r.RespOffice));
        }

        public Expression<Func<TmkTrademark, bool>> EntityFilter()
        {
            var userId = _user.GetUserIdentifier();

            switch (_user.GetEntityFilterType())
            {
                case CPiEntityType.Client:
                    return t => _trademarkRepository.UserEntityFilters.Any(f => f.UserId == userId && f.EntityId == t.ClientID);

                case CPiEntityType.Owner:
                    return t => _trademarkRepository.UserEntityFilters.Any(f => f.UserId == userId && t.Owners.Any(o => o.OwnerID == f.EntityId));

                case CPiEntityType.Attorney:
                    return t => _trademarkRepository.UserEntityFilters.Any(f => f.UserId == userId && (f.EntityId == t.Attorney1ID || f.EntityId == t.Attorney2ID || f.EntityId == t.Attorney3ID || f.EntityId == t.Attorney4ID || f.EntityId == t.Attorney5ID));

                case CPiEntityType.Agent:
                    return t => _trademarkRepository.UserEntityFilters.Any(f => f.UserId == userId && (f.EntityId == t.AgentID));

                //RMS DECISION MAKERS
                case CPiEntityType.ContactPerson:
                    return t => _trademarkRepository.UserEntityFilters.Any(f => f.UserId == userId && t.Client.ClientContacts.Any(c => c.ContactID == f.EntityId));
            }
            return null;
        }

        public async Task<TmkTrademark> GetByIdAsync(int tmkId)
        {
            return await TmkTrademarks.SingleOrDefaultAsync(tm => tm.TmkId == tmkId);
        }

        public async Task AddTrademark(TmkTrademark trademark, DateTime dateCreated)
        {
            trademark.SubCase = trademark.SubCase ?? "";
            await ValidateTrademark(trademark);
            await ComputeStatus(trademark);
            var enteredDateFields = GetEnteredDateFields(trademark);
            await _trademarkRepository.AddAsync(trademark, enteredDateFields, dateCreated);
        }

        public async Task<int> UpdateTrademark(TmkTrademark trademark,DateTime? dateCreated)
        {
            await ValidatePermission(trademark.TmkId);
            await ValidateTrademark(trademark);
            await ComputeStatus(trademark);
            var modifiedFields = await GetModifiedFields(trademark);

            var settings = await _settings.GetSetting();
            return await _trademarkRepository.UpdateAsync(trademark, modifiedFields, dateCreated,settings.IsMultipleOwnerOn);
        }

        public async Task UpdateDeDocket(TmkTrademark trademark)
        {
            await ValidatePermission(trademark.TmkId);
            await ValidateTrademark(trademark);

            var settings = await _settings.GetSetting();
            var deDocketFields = await _systemSettingManager.GetSystemSetting<DeDocketFields>();
            var updated = await GetByIdAsync(trademark.TmkId);

            Guard.Against.NoRecordPermission(updated != null);
            
            if (updated != null && deDocketFields.Trademark != null)
            {
                if (deDocketFields.Trademark.TrademarkName)
                    updated.TrademarkName = trademark.TrademarkName;

                if (deDocketFields.Trademark.Attorney1)
                    updated.Attorney1ID = trademark.Attorney1ID;

                if (deDocketFields.Trademark.Attorney2)
                    updated.Attorney2ID = trademark.Attorney2ID;

                if (deDocketFields.Trademark.Attorney3)
                    updated.Attorney3ID = trademark.Attorney3ID;

                if (deDocketFields.Trademark.Attorney4)
                    updated.Attorney4ID = trademark.Attorney4ID;

                if (deDocketFields.Trademark.Attorney5)
                    updated.Attorney5ID = trademark.Attorney5ID;

                if (deDocketFields.Trademark.ClientReference)
                    updated.ClientRef = trademark.ClientRef;

                if (deDocketFields.Trademark.OtherReferenceNumber)
                    updated.OtherReferenceNumber = trademark.OtherReferenceNumber;

                if (deDocketFields.Trademark.AgentReference)
                    updated.AgentRef = trademark.AgentRef;

                if (deDocketFields.Trademark.Remarks)
                    updated.Remarks = trademark.Remarks;

                updated.tStamp = trademark.tStamp;

                await _trademarkRepository.UpdateAsync(updated, new TmkTrademarkModifiedFields(), null, settings.IsMultipleOwnerOn);
            }
            else
                Guard.Against.UnAuthorizedAccess(false);
        }

        public async Task DeleteTrademark(TmkTrademark tmkTrademark, bool validateRecordFilter = true)
        {
            if (validateRecordFilter)
               await ValidatePermission(tmkTrademark.TmkId);

            await _trademarkRepository.DeleteAsync(tmkTrademark);
        }

        public async Task UpdateChild<T>(int tmkId, string userName, IEnumerable<T> updated, IEnumerable<T> added, IEnumerable<T> deleted) where T : BaseEntity
        {
            var trademark = await GetByIdAsync(tmkId);
            await _trademarkRepository.UpdateChild(trademark, userName, updated, added, deleted);
        }

        public async Task SyncChildToDesignatedTrademarks(TmkTrademark trademark, TmkTrademarkModifiedFields modifiedFields, Type childType) {
            await _trademarkRepository.SyncChildToDesignatedTrademarks(trademark, modifiedFields, childType);
        }

        public async Task<Tuple<string, string, string,string>> CopyTrademark(int oldTmkId, string newCaseNumber, string newSubCase, List<int> countryIds,
                                        bool copyCaseInfo, bool copyRemarks, bool copyAssignments, bool copyGoods, bool copyImages, bool copyKeywords, 
                                        bool copyDesCountries, bool copyLicenses, bool copyRelatedCases, string createdBy, string relationship, bool copyProducts, bool copyOwners)
        {
            return await _trademarkRepository.CopyTrademark(oldTmkId, newCaseNumber, newSubCase, countryIds, copyCaseInfo, copyRemarks, copyAssignments, copyGoods,
                                                        copyImages, copyKeywords, copyDesCountries, copyLicenses, copyRelatedCases,createdBy, relationship,copyProducts,copyOwners);
        }

        public async Task<List<TmkWorkflowAction>> CheckWorkflowAction(TmkWorkflowTriggerType triggerType)
        {
            var actions = await _repository.TmkWorkflowActions.Where(w => w.Workflow.TriggerTypeId == (int)triggerType && w.Workflow.ActiveSwitch).Include(w => w.Workflow).ThenInclude(w=>w.SystemScreen).OrderBy(w => w.OrderOfEntry).ToListAsync();
            return actions;
        }

        public IQueryable<TmkStandardGood> TmkStandardGoods => _repository.TmkStandardGoods.AsNoTracking();
        public IQueryable<TmkCountryDue> TmkCountryDues => _repository.TmkCountryDues.AsNoTracking();
        public IQueryable<TmkActionType> TmkActionTypes => _repository.TmkActionTypes.AsNoTracking();
        public IQueryable<TmkTrademarkClass> TmkTrademarkClasses => _repository.TmkTrademarkClasses.AsNoTracking();

        //--------------------------




        public async Task<List<PatParentCaseDTO>> ParentTrademarks()
        {
            // used in search screen

            var list = await TmkTrademarks
                .Where(p => TmkTrademarks.Any(c => c.ParentTmkId == p.TmkId))
                .Select(p => new PatParentCaseDTO
                {
                    ParentId = p.TmkId,
                    ParentCase = p.CaseNumber + "/" + p.Country + (p.SubCase.Length == 0 ? "" : "/" + p.SubCase) + "/" + p.CaseType
                })
                .OrderBy(p => p.ParentCase)
                .ToListAsync();
            return list;
        }


        public async Task<bool> CanHaveDesignatedCountry(string country, string caseType)
        {
            return await _trademarkRepository.CanHaveDesignatedCountry(country, caseType);
        }

        public async Task<List<PatParentCaseDTO>> GetPossibleFamilyReferences(int tmkId, string trademarkName)
        {
            // used in 'Designation' tab Parent Case lookup
            // in R8, this was procWebTmkFamilyGetPossibleReferences
            var list = await TmkTrademarks
                .Where(t => t.TmkId != tmkId && (string.IsNullOrEmpty(t.TrademarkName) || t.TrademarkName == trademarkName))
                .Select(t => new PatParentCaseDTO
                {
                    ParentId = t.TmkId,
                    ParentCase = t.CaseNumber + "/" + t.Country + (t.SubCase.Length == 0 ? "" : "/" + t.SubCase) + "/" + t.CaseType
                })
                .OrderBy(p => p.ParentCase)
                .ToListAsync();
            list.Insert(0, new PatParentCaseDTO { ParentId = 0, ParentCase = "" });
            return list;
        }

        public TmkTrademarkRenewalFields GetTrademarkRenewal(TmkTrademarkRenewalParameters param)
        {
            return _trademarkRepository.GetTrademarkRenewal(param);
        }

        public DateTime? GetTrademarkRenewalDate(TmkTrademarkRenewalParameters param)
        {
            var renewal = _trademarkRepository.GetTrademarkRenewal(param);
            return renewal.CalcRenewalDate;
        }

        public bool AnyRenewalDateParametersModified(TmkTrademark trademark)
        {
            var modifiedFields = GetModifiedFields(trademark).Result;
            var anyChanges =  _trademarkRepository.AnyActionFieldsModified(modifiedFields);
            return anyChanges ;
        }

        public async Task<List<LookupDTO>> GetAllowedRespOffices(List<string> roles)
        {
            return await GetAllowedRespOffices(_user.GetUserIdentifier(),
                SystemType.Trademark,roles);
        }
        public async Task<List<SysCustomFieldSetting>> GetCustomFields()
        {
            return await _repository.SysCustomFieldSettings.Where(s => s.TableName == "tblTmkTrademark" && s.Visible == true).OrderBy(s => s.OrderOfEntry).ToListAsync();
        }

        public async Task<bool> HasParentClassNotInChild(int parentTmkId,int tmkId)
        {
            var result = await _repository.TmkTrademarkClasses.AnyAsync(pc => pc.TmkId == parentTmkId && !_repository.TmkTrademarkClasses.Any(cc=> cc.TmkId==tmkId && cc.ClassId==pc.ClassId));
            //var result = await _repository.TmkTrademarkClasses.AnyAsync(pc => pc.TmkId == parentTmkId && !_repository.TmkTrademarkClasses.Any(cc => cc.TmkId == tmkId));
            return result;
        }

        public async Task<List<TmkTrademarkClass>> GetParentClassNotInChild(int parentTmkId, int tmkId)
        {
            var list = await _repository.TmkTrademarkClasses.Where(pc => pc.TmkId == parentTmkId && !_repository.TmkTrademarkClasses.Any(cc => cc.TmkId == tmkId && cc.ClassId == pc.ClassId)).Include(pc=> pc.TmkStandardGood).ToListAsync();
            return list;
        }

        public async Task<List<TmkTrademarkClass>> GetTrademarkClass(string? caseNumber, string? country, string? subCase, string? appNo, string? regNo, string? trademark, int classId, string? goods) {
            var list = _repository.TmkTrademarkClasses.AsQueryable();

            if (!string.IsNullOrEmpty(caseNumber)) {
                caseNumber = caseNumber.Replace("*", "%");
                list = list.Where(tc => EF.Functions.Like(tc.TmkTrademark.CaseNumber, caseNumber));
            }
            if (!string.IsNullOrEmpty(country))
            {
                country = country.Replace("*", "%");
                list = list.Where(tc => EF.Functions.Like(tc.TmkTrademark.Country, country));
            }
            if (!string.IsNullOrEmpty(subCase))
            {
                subCase = subCase.Replace("*", "%");
                list = list.Where(tc => EF.Functions.Like(tc.TmkTrademark.SubCase, subCase));
            }
            if (!string.IsNullOrEmpty(appNo))
            {
                appNo = appNo.Replace("*", "%");
                list = list.Where(tc => EF.Functions.Like(tc.TmkTrademark.AppNumber, appNo));
            }
            if (!string.IsNullOrEmpty(regNo))
            {
                regNo = regNo.Replace("*", "%");
                list = list.Where(tc => EF.Functions.Like(tc.TmkTrademark.RegNumber, regNo));
            }
            if (!string.IsNullOrEmpty(trademark))
            {
                trademark = trademark.Replace("*", "%");
                list = list.Where(tc => EF.Functions.Like(tc.TmkTrademark.TrademarkName, trademark));
            }
            if (classId > 0)
            {
                list = list.Where(tc => tc.ClassId==classId);
            }

            if (!string.IsNullOrEmpty(goods))
            {
                goods = goods.Replace("*", "%");
                list = list.Where(tc => EF.Functions.Like(tc.Goods, goods));
            }

            var result = await list.Include(pc => pc.TmkStandardGood).ToListAsync();
            return result;
        }

        public async Task<List<TmkTrademarkClass>> GetTrademarkClass() {
            var list = await _repository.TmkTrademarkClasses.ToListAsync();
            return list;
        }

        #region State Field Computation
        protected async Task ComputeStatus(TmkTrademark tmkTrademark)
        {
            var newStatus = tmkTrademark.TrademarkStatus;

            if (string.IsNullOrEmpty(tmkTrademark.TrademarkStatus) ||
                await _trademarkStatusService.QueryableList.AnyAsync(s => s.TrademarkStatus == tmkTrademark.TrademarkStatus && s.CPITmkStatus))
            {
                if (!(tmkTrademark.RegDate == null && string.IsNullOrEmpty(tmkTrademark.RegNumber)))
                {
                    newStatus = "Registered";
                }
                else if (tmkTrademark.AllowanceDate != null && tmkTrademark.Country == "US")
                {
                    newStatus = "Allowed";
                }
                else if (!(tmkTrademark.PubDate == null && string.IsNullOrEmpty(tmkTrademark.PubNumber)))
                {
                    newStatus = "Published";
                }
                else if (!(tmkTrademark.FilDate == null && string.IsNullOrEmpty(tmkTrademark.AppNumber)))
                {
                    newStatus = "Pending";
                }
                else
                {
                    newStatus = "Unfiled";
                }
            }
            else {
                if (tmkTrademark.TmkId > 0) {
                    var existing = await TmkTrademarks.FirstOrDefaultAsync(t => t.TmkId == tmkTrademark.TmkId);
                    if (existing != null && existing.TrademarkStatus != tmkTrademark.TrademarkStatus)
                        if (tmkTrademark.TrademarkStatusDate == null ||
                            (tmkTrademark.TrademarkStatusDate == existing.TrademarkStatusDate)) {
                            tmkTrademark.TrademarkStatusDate = DateTime.Now.Date;
                        }
                }
            }

            if (newStatus != tmkTrademark.TrademarkStatus)
            {
                if (newStatus.ToLower() == "registered" && (tmkTrademark.RegDate != null))
                {
                    tmkTrademark.TrademarkStatusDate = tmkTrademark.RegDate.Value.Date;
                }
                else if (newStatus.ToLower() == "allowed" && (tmkTrademark.AllowanceDate != null))
                {
                    tmkTrademark.TrademarkStatusDate = tmkTrademark.AllowanceDate.Value.Date;
                }
                else if (newStatus.ToLower() == "published" && (tmkTrademark.PubDate != null))
                {
                    tmkTrademark.TrademarkStatusDate = tmkTrademark.PubDate.Value.Date;
                }
                else if (newStatus.ToLower() == "pending" && (tmkTrademark.FilDate != null))
                {
                    tmkTrademark.TrademarkStatusDate = tmkTrademark.FilDate.Value.Date;
                }
                else
                {
                    if (tmkTrademark.TrademarkStatusDate == null)
                        tmkTrademark.TrademarkStatusDate = DateTime.Today;
                }
            }
            tmkTrademark.TrademarkStatus = newStatus;
            tmkTrademark.SubCase = tmkTrademark.SubCase ?? "";
        }

        private async Task<TmkTrademarkModifiedFields> GetModifiedFields(TmkTrademark modified)
        {
            var existing = await _trademarkRepository.QueryableList.Where(t => t.TmkId == modified.TmkId).FirstOrDefaultAsync();
            var modifiedFields = new TmkTrademarkModifiedFields
            {
                //KeyModified = (existing.CaseNumber != modified.CaseNumber || existing.Country != modified.Country || existing.SubCase != modified.SubCase),
                IsChgCtryCaseType = existing.Country != modified.Country || existing.CaseType != modified.CaseType,
                IsChgFilDate = existing.FilDate != modified.FilDate,
                IsChgPubDate = existing.PubDate != modified.PubDate,
                IsChgRegDate = existing.RegDate != modified.RegDate,
                IsChgPriDate = existing.PriDate != modified.PriDate,
                IsChgAllowDate = existing.AllowanceDate != modified.AllowanceDate,
                IsChgLastRenDate = existing.LastRenewalDate != modified.LastRenewalDate,
                IsChgNextRenDate = existing.NextRenewalDate != modified.NextRenewalDate,
                IsChgParentFilDate = existing.ParentFilDate != modified.ParentFilDate
            };
            return modifiedFields;
        }

        private TmkTrademarkModifiedFields GetEnteredDateFields(TmkTrademark trademark)
        {
            var enteredDateFields = new TmkTrademarkModifiedFields
            {
                IsChgCtryCaseType = false,
                IsChgFilDate = trademark.FilDate.HasValue,
                IsChgPubDate = trademark.PubDate.HasValue,
                IsChgRegDate = trademark.RegDate.HasValue,
                IsChgPriDate = trademark.PriDate.HasValue,
                IsChgAllowDate = trademark.AllowanceDate.HasValue,
                IsChgLastRenDate = trademark.LastRenewalDate.HasValue,
                IsChgNextRenDate = trademark.NextRenewalDate.HasValue,
                IsChgParentFilDate = trademark.ParentFilDate.HasValue
            };
            return enteredDateFields;
        }

        //protected TmkTrademarkModifiedFields SetActionFieldsAsModified()
        //{

        //    var modifiedFields = new TmkTrademarkModifiedFields
        //    {
        //        IsChgCtryCaseType = true,
        //        IsChgFilDate = true,
        //        IsChgPubDate = true,
        //        IsChgRegDate = true,
        //        IsChgPriDate = true,
        //        IsChgAllowDate = true,
        //        IsChgLastRenDate = true
        //    };
        //    return modifiedFields;
        //}

        #endregion

        //---------------- review below

        #region Trademark Validation

        public async Task ValidatePermission(int tmkId)
        {
            if (HasEntityFilter() || _user.HasRespOfficeFilter(SystemType.Trademark))
                Guard.Against.NoRecordPermission(await TmkTrademarks.AnyAsync(t => t.TmkId == tmkId));
        }

        
        protected async Task ValidateTrademark(TmkTrademark tmkTrademark)
        {
            var entityFilterType = _user.GetEntityFilterType();
            var settings = await _settings.GetSetting();

            if (entityFilterType == CPiEntityType.Attorney)
            {
                var current = new TmkTrademark();

                bool canModifyAttorney1 = true;
                bool canModifyAttorney2 = true;
                bool canModifyAttorney3 = true;
                bool canModifyAttorney4 = true;
                bool canModifyAttorney5 = true;

                if (tmkTrademark.TmkId > 0)
                {
                    current = await GetByIdAsync(tmkTrademark.TmkId);

                    canModifyAttorney1 = await CanModifyAttorney(current.Attorney1ID ?? 0);
                    canModifyAttorney2 = await CanModifyAttorney(current.Attorney2ID ?? 0);
                    canModifyAttorney3 = await CanModifyAttorney(current.Attorney3ID ?? 0);
                    canModifyAttorney4 = await CanModifyAttorney(current.Attorney4ID ?? 0);
                    canModifyAttorney5 = await CanModifyAttorney(current.Attorney5ID ?? 0);

                    Guard.Against.NoFieldPermission(current.Attorney1ID == null || current.Attorney1ID == tmkTrademark.Attorney1ID || (current.Attorney1ID != tmkTrademark.Attorney1ID && canModifyAttorney1), settings.LabelAttorney1 ?? "Attorney 1");
                    Guard.Against.NoFieldPermission(current.Attorney2ID == null || current.Attorney2ID == tmkTrademark.Attorney2ID || (current.Attorney2ID != tmkTrademark.Attorney2ID && canModifyAttorney2), settings.LabelAttorney2 ?? "Attorney 2");
                    Guard.Against.NoFieldPermission(current.Attorney3ID == null || current.Attorney3ID == tmkTrademark.Attorney3ID || (current.Attorney3ID != tmkTrademark.Attorney3ID && canModifyAttorney3), settings.LabelAttorney3 ?? "Attorney 3");
                    Guard.Against.NoFieldPermission(current.Attorney4ID == null || current.Attorney4ID == tmkTrademark.Attorney4ID || (current.Attorney4ID != tmkTrademark.Attorney4ID && canModifyAttorney4), settings.LabelAttorney4 ?? "Attorney 4");
                    Guard.Against.NoFieldPermission(current.Attorney5ID == null || current.Attorney5ID == tmkTrademark.Attorney5ID || (current.Attorney5ID != tmkTrademark.Attorney5ID && canModifyAttorney5), settings.LabelAttorney5 ?? "Attorney 5");
                }

                if (tmkTrademark.Attorney1ID != null && tmkTrademark.Attorney1ID != current.Attorney1ID && canModifyAttorney1)
                    Guard.Against.ValueNotAllowed(await EntityFilterAllowed(tmkTrademark.Attorney1ID ?? 0), settings.LabelAttorney1 ?? "Attorney 1");

                if (tmkTrademark.Attorney2ID != null && tmkTrademark.Attorney2ID != current.Attorney2ID && canModifyAttorney2)
                    Guard.Against.ValueNotAllowed(await EntityFilterAllowed(tmkTrademark.Attorney2ID ?? 0), settings.LabelAttorney2 ?? "Attorney 2");

                if (tmkTrademark.Attorney3ID != null && tmkTrademark.Attorney3ID != current.Attorney3ID && canModifyAttorney3)
                    Guard.Against.ValueNotAllowed(await EntityFilterAllowed(tmkTrademark.Attorney3ID ?? 0), settings.LabelAttorney3 ?? "Attorney 3");

                if (tmkTrademark.Attorney4ID != null && tmkTrademark.Attorney4ID != current.Attorney4ID && canModifyAttorney4)
                    Guard.Against.ValueNotAllowed(await EntityFilterAllowed(tmkTrademark.Attorney4ID ?? 0), settings.LabelAttorney4 ?? "Attorney 4");

                if (tmkTrademark.Attorney5ID != null && tmkTrademark.Attorney5ID != current.Attorney5ID && canModifyAttorney5)
                    Guard.Against.ValueNotAllowed(await EntityFilterAllowed(tmkTrademark.Attorney5ID ?? 0), settings.LabelAttorney5 ?? "Attorney 5");

                if ((tmkTrademark.Attorney1ID == null || !canModifyAttorney1) &&
                    (tmkTrademark.Attorney2ID == null || !canModifyAttorney2) &&
                    (tmkTrademark.Attorney3ID == null || !canModifyAttorney3) &&
                    (tmkTrademark.Attorney4ID == null || !canModifyAttorney4) &&
                    (tmkTrademark.Attorney5ID == null || !canModifyAttorney5))
                    Guard.Against.Null(null, "Attorney");
            }

            if (entityFilterType == CPiEntityType.Client)
            {
                var clientLabel = settings.LabelClient;
                Guard.Against.Null(tmkTrademark.ClientID, clientLabel);
                Guard.Against.ValueNotAllowed(await EntityFilterAllowed(tmkTrademark.ClientID ?? 0), clientLabel);
            }

            if (entityFilterType == CPiEntityType.Owner)
            {
                var ownerLabel = settings.LabelOwner;
                Guard.Against.Null(tmkTrademark.OwnerID, ownerLabel);
                Guard.Against.ValueNotAllowed(await EntityFilterAllowed(tmkTrademark.OwnerID ?? 0), ownerLabel);
            }

            await ValidateRespOffice(tmkTrademark.RespOffice, SystemType.Trademark);
        }

        private async Task ValidateRespOffice(string respOffice, string systemId)
        {
            if (_user.HasRespOfficeFilter(SystemType.Trademark))
            {
                Guard.Against.Null(respOffice, "Responsible Office");

                var allowed = await _trademarkRepository.CPiUserSystemRoles.AnyAsync(r => r.UserId == _user.GetUserIdentifier() && r.SystemId == systemId && r.RespOffice == respOffice);
                Guard.Against.ValueNotAllowed(allowed, "Responsible Office");
            }
        }

        private async Task<List<LookupDTO>> GetAllowedRespOffices(string userIdentifier, string systemType, List<string> roles)
        {
            if (_user.HasRespOfficeFilter(systemType))
                return await _trademarkRepository.CPiRespOffices.Where(ro => ro.UserSystemRoles.Any(r => r.UserId == userIdentifier && r.SystemId == systemType && (!roles.Any() || roles.Contains(r.RoleId)) && r.RespOffice == ro.RespOffice))
                        .Select(r => new LookupDTO() { Value = r.RespOffice, Text = r.RespOffice }).ToListAsync();

            return await _trademarkRepository.CPiRespOffices.AsNoTracking().Select(r => new LookupDTO() { Value = r.RespOffice, Text = r.RespOffice }).ToListAsync();
        }

        public async Task<bool> CanModifyAttorney(int attorneyId)
        {
            if (_user.GetEntityFilterType() == CPiEntityType.Attorney && attorneyId > 0)
                return await EntityFilterAllowed(attorneyId);
            else
                return true;
        }

        private async Task<bool> EntityFilterAllowed(int entityId)
        {
            return await _trademarkRepository.UserEntityFilters.AnyAsync(f => f.UserId == _user.GetUserIdentifier() && f.EntityId == entityId);
        }

        #endregion

        #region Action
        public async Task<List<ActionTabDTO>> GetActions(int tmkId, ActionDisplayOption actionDisplayOption)
        {
            return await _trademarkRepository.GetActions(tmkId, actionDisplayOption);
        }
        public async Task ActionsUpdate(int tmkId, string userName, IEnumerable<TmkDueDate> updatedActions, IEnumerable<TmkDueDate> deletedActions)
        {
            await _trademarkRepository.ActionsUpdate(tmkId, userName, updatedActions, deletedActions);

            // check if actions with deleted due dates are now childless
            var deletedActIds = deletedActions.Select(d => d.ActId).Distinct();
            if (deletedActIds.Any()) 
                await _trademarkRepository.CheckChildlessActionDue(deletedActIds);
        }
        public async Task ActionDelete(TmkDueDate deletedAction)
        {
            await _trademarkRepository.ActionDelete(deletedAction);

            // check if actions with deleted due date is now childless
            IEnumerable<int> deletedActId = new List<int>() { deletedAction.ActId };
            await _trademarkRepository.CheckChildlessActionDue(deletedActId);
        }
        public async Task<List<DelegationEmailDTO>> GetDelegationEmails(int delegationId)
        {
            return await _trademarkRepository.GetDelegationEmails(delegationId);
        }
        public async Task MarkDelegationasEmailed(int delegationId) {
            await _trademarkRepository.MarkDelegationasEmailed(delegationId);
        }
        public async Task<List<LookupIntDTO>> GetDelegatedDdIds(int action, int[] recIds) {
            return await _trademarkRepository.GetDelegatedDdIds(action,recIds);
        }

        public async Task<List<DelegationEmailDTO>> GetDeletedDelegationEmails(int delegationId) {
            return await _trademarkRepository.GetDeletedDelegationEmails(delegationId);
        }
        public async Task<List<LookupIntDTO>> GetDuedateChangedDelegationIds(int action, List<TmkDueDate> updated) {
            return await _trademarkRepository.GetDuedateChangedDelegationIds(action,updated);
        }
        public async Task<DelegationDetailDTO> GetDeletedDelegation(int delegationId) {
            return await _trademarkRepository.GetDeletedDelegation(delegationId);
        }
        public IQueryable<DeDocketInstruction> DeDocketInstructions
        {
            get
            {
                return _repository.DeDocketInstructions.AsNoTracking();
            }
        }
        public async Task<bool> HasOutstandingDedocket(int tmkId)
        {
            return await _repository.TmkActionDues.AnyAsync(ad => ad.TmkId == tmkId && ad.DueDates.Any(dd => dd.DeDocketOutstanding != null));
        }
        #endregion

        #region Cost
        public async Task<List<TmkCostTrack>> GetCosts(int tmkId)
        {
            return await _trademarkRepository.GetCosts(tmkId);
        }
        public Task CostsUpdate(int tmkId, string userName, IEnumerable<TmkCostTrack> updatedCostTracks, IEnumerable<TmkCostTrack> deletedCostTracks)
        {
            throw new NotImplementedException();

        }
        public Task CostDelete(TmkCostTrack deletedCostTrack)
        {
            throw new NotImplementedException();
        }

        #endregion
        #region Licensee
        public async Task<List<TmkLicensee>> GetLicensees(int tmkId)
        {
            return await _trademarkRepository.GetLicensees(tmkId);
        }

        public async Task LicenseesUpdate(int tmkId, string userName, IEnumerable<TmkLicensee> updatedLicensees, IEnumerable<TmkLicensee> newLicensees, IEnumerable<TmkLicensee> deletedLicensees)
        {
            await _trademarkRepository.LicenseesUpdate(tmkId, userName, updatedLicensees, newLicensees, deletedLicensees);
        }

        public async Task LicenseeDelete(TmkLicensee deletedLicensee)
        {
            await _trademarkRepository.LicenseeDelete(deletedLicensee);
        }

        public async Task<bool> HasLicensees(int tmkId)
        {
            return await _trademarkRepository.TmkLicensees.AnyAsync(l => l.TmkId == tmkId);
        }
        #endregion

        #region Family Tree View
        public async Task<IEnumerable<FamilyTreeDTO>> GetFamilyTree(string paramType, string paramValue, string paramParent)
        {
            return await _trademarkRepository.GetFamilyTree(paramType, paramValue, paramParent);
        }

        public async Task<FamilyTreeTmkDTO> GetNodeDetails(string paramType, string paramValue)
        {
            return await _trademarkRepository.GetNodeDetails(paramType, paramValue);
        }

        public void UpdateParent(int childTmkId, int newParentId, string parentInfo, string userName)
        {
            _trademarkRepository.UpdateParent(childTmkId, newParentId, parentInfo, userName);
        }



        #endregion
        #region Product
        public async Task<bool> HasProducts(int tmkId)
        {
            return await _trademarkRepository.TmkProducts.AnyAsync(l => l.TmkId == tmkId);
        }
        public IQueryable<Product> Products => _repository.Products.AsNoTracking();
        #endregion
        #region Assignment
        public IQueryable<TmkAssignmentHistory> TmkAssignmentsHistory => _repository.TmkAssignmentsHistory.AsNoTracking();
        #endregion

        #region Copy
        public IQueryable<TmkTrademarkCopySetting> TmkTrademarkCopySettings => _repository.TrademarkCopySettings.AsNoTracking();
        public async Task UpdateCopySetting(TmkTrademarkCopySetting setting)
        {
            var existing = await _repository.TrademarkCopySettings.FirstOrDefaultAsync(s => s.CopySettingId == setting.CopySettingId);
            if (existing != null)
            {
                existing.Copy = setting.Copy;
                _repository.TrademarkCopySettings.Update(existing);
                await _repository.SaveChangesAsync();
            }
        }
        public async Task AddCopySettings(List<TmkTrademarkCopySetting> settings)
        {
            if (settings.Count > 0)
            {
                _repository.TrademarkCopySettings.AddRange(settings);
                await _repository.SaveChangesAsync();
            }
        }

        public async Task<CPiUserSetting> GetMainCopySettings(string userId)
        {
            var setting = await _repository.CPiSettings.Where(d => d.Name == "TrademarkCopySetting").AsNoTracking().FirstOrDefaultAsync();
            if (setting == null)
            {
                setting = new CPiSetting { Name = "TrademarkCopySetting", Policy = "*" };
                _repository.CPiSettings.Add(setting);
                await _repository.SaveChangesAsync();
            }
            return await _repository.CPiUserSettings.Where(u => u.UserId == userId && u.SettingId == setting.Id).AsNoTracking().FirstOrDefaultAsync();
        }

        public async Task UpdateMainCopySettings(CPiUserSetting userSetting)
        {
            if (userSetting.Id > 0)
                _repository.CPiUserSettings.Update(userSetting);
            else
                _repository.CPiUserSettings.Add(userSetting);
            await _repository.SaveChangesAsync();
        }

        public async Task<int> GetMainCopySettingId()
        {
            var setting = await _repository.CPiSettings.Where(d => d.Name == "TrademarkCopySetting").AsNoTracking().FirstOrDefaultAsync();
            if (setting != null)
            {
                return setting.Id;
            }
            else
            {
                setting = new CPiSetting { Name = "TrademarkCopySetting", Policy = "*" };
                _repository.CPiSettings.Add(setting);
                await _repository.SaveChangesAsync();
                return setting.Id;
            }
        }
        public async Task AddCustomFieldsAsCopyFields() { 
            await _trademarkRepository.AddCustomFieldsAsCopyFields();
        }
        #endregion

        #region Documents
        public IQueryable<DocDocument> Documents
        {
            get
            {
                var documents = _repository.DocDocuments.AsNoTracking().Where(d => d.DocFolder.SystemType == "T" && d.DocFolder.DataKey == "TmkId");
                return documents;
            }
        }
        #endregion

        #region Workflow
        public async Task GenerateWorkflowFromEmailSent(int tmkId, int qeSetupId)
        {
            var workflowActions = (await CheckWorkflowAction(TmkWorkflowTriggerType.EmailSent)).Where(wf => (wf.Workflow.SystemScreen == null || wf.Workflow.SystemScreen.ScreenCode.ToLower() == "tmk-workflow") && (wf.Workflow.TriggerValueId == 0 || wf.Workflow.TriggerValueId == qeSetupId)).ToList();
            if (workflowActions.Any())
            {
                var trademark = await TmkTrademarks.Where(c => c.TmkId == tmkId).FirstOrDefaultAsync();
                workflowActions = workflowActions.Where(w => string.IsNullOrEmpty(w.Workflow.ClientFilter) || w.Workflow.ClientFilter.Contains("|" + trademark.ClientID.ToString() + "|")).ToList();

                if (workflowActions.Any())
                {
                    //client specific will override the base
                    foreach (var item in workflowActions.Where(wf => !string.IsNullOrEmpty(wf.Workflow.ClientFilter)).ToList())
                    {
                        workflowActions.RemoveAll(bf => string.IsNullOrEmpty(bf.Workflow.ClientFilter) && bf.ActionTypeId == item.ActionTypeId);
                    }

                    var createActionWorkflows = workflowActions.Where(wf => wf.ActionTypeId == (int)TmkWorkflowActionType.CreateAction).Distinct().ToList();
                    foreach (var item in createActionWorkflows)
                    {
                        await GenerateWorkflowAction(trademark.TmkId, item.ActionValueId);
                    }
                }
            }
        }

        public async Task GenerateWorkflowFromActionEmailSent(int actId, int qeSetupId)
        {
            var workflowActions = (await CheckWorkflowAction(TmkWorkflowTriggerType.EmailSent)).Where(wf => (wf.Workflow.SystemScreen == null || wf.Workflow.SystemScreen.ScreenCode.ToLower() == "act-workflow") && (wf.Workflow.TriggerValueId == 0 || wf.Workflow.TriggerValueId == qeSetupId)).ToList();
            if (workflowActions.Any())
            {
                var actionDue = await _repository.TmkActionDues.Where(a => a.ActId == actId).Include(a => a.TmkTrademark).FirstOrDefaultAsync();
                workflowActions = workflowActions.Where(w => string.IsNullOrEmpty(w.Workflow.ClientFilter) || w.Workflow.ClientFilter.Contains("|" + actionDue.TmkTrademark.ClientID.ToString() + "|")).ToList();
                if (workflowActions.Any())
                {
                    //client specific will override the base
                    foreach (var item in workflowActions.Where(wf => !string.IsNullOrEmpty(wf.Workflow.ClientFilter)).ToList())
                    {
                        workflowActions.RemoveAll(bf => string.IsNullOrEmpty(bf.Workflow.ClientFilter) && bf.ActionTypeId == item.ActionTypeId);
                    }

                    var createActionWorkflows = workflowActions.Where(wf => wf.ActionTypeId == (int)TmkWorkflowActionType.CreateAction).Distinct().ToList();
                    foreach (var item in createActionWorkflows)
                    {
                        await GenerateWorkflowAction(actionDue.TmkId, item.ActionValueId);
                    }
                }
            }
        }

        public async Task GenerateWorkflowAction(int tmkId, int actionTypeId, DateTime? baseDate = null)
        {
            if (baseDate == null) 
                baseDate = DateTime.Now.Date;

            var trademark = await GetByIdAsync(tmkId);

            var actionType = await _repository.TmkActionTypes.Where(at => at.ActionTypeID == actionTypeId).AsNoTracking().FirstOrDefaultAsync();
            if (actionType == null) return;

            var dupActionDue = await _repository.TmkActionDues.Where(a => a.TmkId == tmkId && a.BaseDate.Date == baseDate && a.ActionType == actionType.ActionType).AsNoTracking().FirstOrDefaultAsync();
            if (dupActionDue == null)
            {
                TmkActionDue actionDue = new TmkActionDue() { TmkId = trademark.TmkId, CaseNumber = trademark.CaseNumber, Country = trademark.Country, SubCase = trademark.SubCase, ActionType = actionType.ActionType, BaseDate = (DateTime)baseDate, ResponsibleID = null, CreatedBy = _user.GetUserName(), UpdatedBy = _user.GetUserName(), DateCreated = DateTime.Now, LastUpdate = DateTime.Now };

                var dueDates = new List<TmkDueDate>();
                var actionParams = await _repository.TmkActionParameters.Where(ap => ap.ActionTypeID == actionType.ActionTypeID).AsNoTracking().ToListAsync();

                if (actionParams.Any())
                    dueDates = actionParams.Select(ap => new TmkDueDate()
                    {
                        ActId = actionDue.ActId,
                        ActionDue = ap.ActionDue,
                        DueDate = actionDue.BaseDate.AddDays((double)ap.Dy).AddMonths(ap.Mo).AddYears(ap.Yr),
                        DateTaken = actionDue.ResponseDate,
                        Indicator = ap.Indicator,
                        CreatedBy = actionDue.UpdatedBy,
                        DateCreated = actionDue.LastUpdate,
                        UpdatedBy = actionDue.UpdatedBy,
                        LastUpdate = actionDue.LastUpdate
                    }).ToList();
                else
                    dueDates.Add(new TmkDueDate()
                    {
                        ActId = actionDue.ActId,
                        ActionDue = actionDue.ActionType,
                        DueDate = actionDue.BaseDate,
                        DateTaken = actionDue.ResponseDate,
                        Indicator = "Due Date",
                        CreatedBy = actionDue.UpdatedBy,
                        DateCreated = actionDue.LastUpdate,
                        UpdatedBy = actionDue.UpdatedBy,
                        LastUpdate = actionDue.LastUpdate
                    });
                actionDue.DueDates = dueDates;

                var dueDatesFromIndicatorWorkflow = await GenerateDueDateFromActionParameterWorkflow(actionDue, actionDue.DueDates, TmkWorkflowTriggerType.Indicator);
                if (dueDatesFromIndicatorWorkflow != null && dueDatesFromIndicatorWorkflow.Any())
                {
                    actionDue.DueDates.AddRange(dueDatesFromIndicatorWorkflow);
                }
                _repository.TmkActionDues.Add(actionDue);
                await _repository.SaveChangesAsync();
            }
        }

        public async Task<List<TmkActionDue>> CloseWorkflowAction(int tmkId, int actionTypeId)
        {
            var trademark = await GetByIdAsync(tmkId);
            var actionDues = new List<TmkActionDue>();

            if (actionTypeId != 0)
            {
                var actionType = await _repository.TmkActionTypes.Where(at => at.ActionTypeID == actionTypeId).AsNoTracking().FirstOrDefaultAsync();
                if (actionType != null) {
                    //var actionDue = await _repository.TmkActionDues.Where(a => a.TmkId == tmkId && a.ActionType == actionType.ActionType && a.ResponseDate == null).FirstOrDefaultAsync();
                    //if (actionDue != null)
                    //{
                    //    actionDue.ResponseDate = DateTime.Now.Date;
                    //    actionDue.UpdatedBy = _user.GetUserName();
                    //    actionDue.LastUpdate = DateTime.Now;

                    //    var dueDates = await _repository.TmkDueDates.Where(d => d.ActId == actionDue.ActId && d.DateTaken == null)
                    //                        .ToListAsync();
                    //    foreach (var dueDate in dueDates)
                    //    {
                    //        dueDate.DateTaken = DateTime.Now.Date;
                    //        dueDate.LastUpdate = actionDue.LastUpdate;
                    //        dueDate.UpdatedBy = actionDue.UpdatedBy;
                    //    }
                    //    await _repository.SaveChangesAsync();
                    //}
                    actionDues = await _repository.TmkActionDues.Where(a => a.TmkId == tmkId && a.ActionType == actionType.ActionType && (a.ResponseDate == null || a.DueDates.Any(dd => dd.DateTaken == null))).Include(a => a.DueDates).AsNoTracking().ToListAsync();
                }
            }
            //all outstanding actions
            else if (actionTypeId == 0)
            {
                actionDues = await _repository.TmkActionDues.Where(a => a.TmkId == tmkId && (a.ResponseDate == null || a.DueDates.Any(dd => dd.DateTaken == null))).Include(a => a.DueDates).AsNoTracking().ToListAsync();
            }
            if (actionDues.Any())
            {
                foreach (var actionDue in actionDues)
                {
                    //for all outstanding actions, we want to close everything and avoid followup
                    if (actionTypeId != 0)
                    {
                        if (actionDue.ResponseDate == null)
                        {
                            actionDue.ResponseDate = DateTime.Now.Date;
                            actionDue.UpdatedBy = _user.GetUserName();
                            actionDue.LastUpdate = DateTime.Now;
                        }
                    }
                    actionDue.CloseDueDates = true;

                }
            }
            return actionDues;

        }

        public async Task<List<TmkWorkflowActionParameter>> CheckWorkflowActionParameters(TmkWorkflowTriggerType triggerType)
        {
            var actionParameters = await _repository.TmkWorkflowActionParameters.Where(w => w.Workflow.TriggerTypeId == (int)triggerType && w.Workflow.ActiveSwitch).Include(w => w.Workflow).ToListAsync();
            return actionParameters;
        }

        public async Task<bool> HasWorkflowEnabled(TmkWorkflowTriggerType triggerType)
        {
            return await _repository.TmkWorkflows.AnyAsync(wf => wf.TriggerTypeId == (int)triggerType && wf.ActiveSwitch);
        }


        public async Task<List<TmkDueDate>> GenerateDueDateFromActionParameterWorkflow(TmkActionDue? newActionDue, List<TmkDueDate> dueDates, TmkWorkflowTriggerType triggerType, bool clearBase = true)
        {
            var workflowActionParameters = await CheckWorkflowActionParameters(triggerType);
            if (!workflowActionParameters.Any())
                return null;

            var dueDateindicators = dueDates.Select(d => d.Indicator).ToList();
            var indicators = await _repository.TmkIndicators.ToListAsync();
            workflowActionParameters = workflowActionParameters.Where(w => indicators.Any(i => i.IndicatorId == w.Workflow.TriggerValueId && dueDateindicators.Any(s => s.ToLower() == i.Indicator.ToLower()))).ToList();

            if (!workflowActionParameters.Any())
                return null;

            var trademark = new TmkTrademark();

            //from duedate grid insert
            if (newActionDue.ActId > 0 && string.IsNullOrEmpty(newActionDue.CaseNumber))
            {
                newActionDue = await _repository.TmkActionDues.Where(ad => ad.ActId == newActionDue.ActId).Include(ad => ad.TmkTrademark).AsNoTracking().FirstOrDefaultAsync();
                if (newActionDue != null)
                    trademark = newActionDue.TmkTrademark;
                else
                    trademark = null;
            }
            else if (!string.IsNullOrEmpty(newActionDue.CaseNumber))
            {
                trademark = await _repository.TmkTrademarks.Where(ca => ca.CaseNumber == newActionDue.CaseNumber && ca.Country == newActionDue.Country && ca.SubCase == newActionDue.SubCase).AsNoTracking().FirstOrDefaultAsync();
            }

            if (trademark != null)
            {
                workflowActionParameters = workflowActionParameters.Where(w => string.IsNullOrEmpty(w.Workflow.ClientFilter) || (w.Workflow.ClientFilter != null && w.Workflow.ClientFilter.Contains("|" + trademark.ClientID.ToString() + "|"))).ToList();
                workflowActionParameters = workflowActionParameters.Where(w => string.IsNullOrEmpty(w.Workflow.CountryFilter) || (w.Workflow.CountryFilter != null && w.Workflow.CountryFilter.Contains("|" + trademark.Country + "|"))).ToList();
                workflowActionParameters = workflowActionParameters.Where(w => string.IsNullOrEmpty(w.Workflow.CaseTypeFilter) || (w.Workflow.CaseTypeFilter != null && w.Workflow.CaseTypeFilter.Contains("|" + trademark.CaseType + "|"))).ToList();
                workflowActionParameters = workflowActionParameters.Where(w => string.IsNullOrEmpty(w.Workflow.RespOfficeFilter) || (w.Workflow.RespOfficeFilter != null && w.Workflow.RespOfficeFilter.Contains("|" + trademark.RespOffice + "|"))).ToList();

                workflowActionParameters = workflowActionParameters.Where(w => string.IsNullOrEmpty(w.Workflow.AttorneyFilter) || (w.Workflow.AttorneyFilter != null &&
                                   (w.Workflow.AttorneyFilter.Contains("|" + trademark.Attorney1ID.ToString() + "|") ||
                                    w.Workflow.AttorneyFilter.Contains("|" + trademark.Attorney2ID.ToString() + "|") ||
                                    w.Workflow.AttorneyFilter.Contains("|" + trademark.Attorney3ID.ToString() + "|") ||
                                    w.Workflow.AttorneyFilter.Contains("|" + trademark.Attorney4ID.ToString() + "|") ||
                                    w.Workflow.AttorneyFilter.Contains("|" + trademark.Attorney5ID.ToString() + "|")
                                   ))).ToList();
                if (clearBase)
                {
                    workflowActionParameters = ClearTmkBaseWorkflowActionParameters(workflowActionParameters);
                }

                if (!workflowActionParameters.Any())
                    return null;

                var newDueDates = new List<TmkDueDate>();
                var basedOns = dueDates.Where(dd => indicators.Any(i => i.Indicator.ToLower() == dd.Indicator.ToLower() && workflowActionParameters.Any(wf => wf.Workflow.TriggerValueId == i.IndicatorId))).ToList();

                foreach (var dd in basedOns)
                {
                    foreach (var item in workflowActionParameters.Where(w => indicators.Any(i => i.IndicatorId == w.Workflow.TriggerValueId && i.Indicator.ToLower() == dd.Indicator.ToLower())).ToList())
                    {

                        //based on DueDate 
                        var computedDueDate = dd.DueDate.AddYears(item.Yr).AddMonths(item.Mo).AddDays((double)item.Dy);

                        //proper leap year handling
                        //var computedDueDate = actionDue.BaseDate.AddYears(ap.Yr).AddMonths(ap.Mo).AddDays((double)ap.Dy),

                        //make sure it is non existing
                        if (!dueDates.Any(edd => edd.ActionDue.ToLower() == item.ActionDue.ToLower() && edd.DueDate == computedDueDate) &&
                            !newDueDates.Any(ndd => ndd.ActionDue.ToLower() == item.ActionDue.ToLower() && ndd.DueDate == computedDueDate))
                        {
                            var dueDate = new TmkDueDate()
                            {
                                ActId = 0,
                                ActionDue = item.ActionDue,
                                DueDate = computedDueDate,
                                DateTaken = newActionDue.ResponseDate,
                                Indicator = item.Indicator,
                                AttorneyID = newActionDue.ResponsibleID,
                                CreatedBy = newActionDue.UpdatedBy,
                                DateCreated = newActionDue.LastUpdate,
                                UpdatedBy = newActionDue.UpdatedBy,
                                LastUpdate = newActionDue.LastUpdate
                            };
                            newDueDates.Add(dueDate);
                        }
                    }
                }
                return newDueDates;
            }
            return null;
        }

        public async Task<List<TmkDueDate>> GetUpdatedDueDateIndicator(int actId, List<TmkDueDate> dueDates)
        {
            var existingDueDates = await _repository.TmkDueDates.Where(dd => dd.ActId == actId).AsNoTracking().ToListAsync();
            if (existingDueDates.Any())
            {

                var updatedDueDates = dueDates.Where(udd => existingDueDates.Any(dd => udd.DDId == dd.DDId && udd.Indicator.ToLower() != dd.Indicator.ToLower())).ToList();
                return updatedDueDates;
            }
            return null;
        }


        private List<TmkWorkflowActionParameter> ClearTmkBaseWorkflowActionParameters(List<TmkWorkflowActionParameter> workflowActions)
        {

            //with filter will override the record with no filter at all
            foreach (var item in workflowActions.Where(wf => !(string.IsNullOrEmpty(wf.Workflow.ClientFilter) && string.IsNullOrEmpty(wf.Workflow.CountryFilter) && string.IsNullOrEmpty(wf.Workflow.CaseTypeFilter)
                                                              && string.IsNullOrEmpty(wf.Workflow.RespOfficeFilter) && string.IsNullOrEmpty(wf.Workflow.AttorneyFilter))).ToList())
            {
                workflowActions.RemoveAll(bf => string.IsNullOrEmpty(bf.Workflow.ClientFilter) && string.IsNullOrEmpty(bf.Workflow.CountryFilter) && string.IsNullOrEmpty(bf.Workflow.CaseTypeFilter)
                                                              && string.IsNullOrEmpty(bf.Workflow.RespOfficeFilter) && string.IsNullOrEmpty(bf.Workflow.AttorneyFilter) && bf.ActionDue == item.ActionDue && bf.Workflow.TriggerValueId == item.Workflow.TriggerValueId && (bf.Workflow.TriggerValueName ?? "") == (item.Workflow.TriggerValueName ?? ""));
            }
            return workflowActions;
        }

        #endregion

        public async Task<int> GetRequestDocketPendingCount(int tmkId)
        {
            return await _repository.TmkDocketRequests.Where(r => r.TmkId == tmkId && r.CompletedDate == null).CountAsync();
        }

        public async Task<List<TmkDocketRequest>> GetRequestDockets(int tmkId, bool outstandingOnly)
        {
            return await _repository.TmkDocketRequests.Where(r => r.TmkId == tmkId && (!outstandingOnly || (outstandingOnly && r.CompletedDate == null))).ToListAsync();
        }

        public void DetachAllEntities()
        {
            _repository.DetachAllEntities();
        }
        public List<EntityEntry> GetAllTrackedEntities()
        {
            return _repository.GetAllTrackedEntities();
        }
    }
}

