using R10.Core.Interfaces;
using System.Threading.Tasks;
using System.Collections.Generic;
using R10.Core.Entities;
using System.Linq;
using R10.Core.Interfaces.Patent;
using R10.Core.Entities.Patent;
using R10.Core.Identity;
using System.Linq.Expressions;
using System;
using System.Security.Claims;
using R10.Core.Helpers;
using Microsoft.EntityFrameworkCore;
using R10.Core.Exceptions;
using static System.Net.Mime.MediaTypeNames;
using System.Transactions;
using System.Data;
using R10.Core.DTOs;
using R10.Core.Interfaces.Shared;
using R10.Core.Entities.Shared;

namespace R10.Core.Services
{
    public class InventionService : EntityService<Invention>, IInventionService
    {
        private readonly ISystemSettings<PatSetting> _settings;
        private readonly ICPiSystemSettingManager _systemSettingManager;
        private readonly IApplicationDbContext _repository;
        private readonly IInventionRepository _inventionRepository;
        private readonly ITradeSecretService _tradeSecretService;

        private bool? _isOwnerRequired = null;
        private bool? _isInventorRequired = null;

        public InventionService(
            ICPiDbContext cpiDbContext,
            ClaimsPrincipal user,
            ISystemSettings<PatSetting> settings,
            ICPiSystemSettingManager systemSettingManager,
            IApplicationDbContext repository,
            IInventionRepository inventionRepository,
            ITradeSecretService tradeSecretService) : base(cpiDbContext, user)
        {
            _settings = settings;
            _systemSettingManager = systemSettingManager;
            _repository = repository;
            _inventionRepository = inventionRepository;
            _tradeSecretService = tradeSecretService;
        }

        public override IQueryable<Invention> QueryableList
        {
            get
            {
                var inventions = _cpiDbContext.GetRepository<Invention>().QueryableList;

                if (_user.HasRespOfficeFilter(SystemType.Patent))
                    inventions = inventions.Where(RespOfficeFilter());

                if (_user.HasEntityFilter())
                    inventions = inventions.Where(EntityFilter());

                if (!_user.CanAccessPatTradeSecret())
                    inventions = inventions.Where(i => !(i.IsTradeSecret ?? false));

                return inventions;
            }
        }

        public IQueryable<InventionCopySetting> InventionCopySettings => _cpiDbContext.GetRepository<InventionCopySetting>().QueryableList;

        private Expression<Func<Invention, bool>> RespOfficeFilter()
        {
            return a => CPiUserSystemRoles.Any(r => r.UserId == UserId && r.SystemId == SystemType.Patent && a.RespOffice == r.RespOffice && !string.IsNullOrEmpty(r.RespOffice));
        }

        private Expression<Func<Invention, bool>> EntityFilter()
        {
            switch (_user.GetEntityFilterType())
            {
                case CPiEntityType.Client:
                    return i => UserEntityFilters.Any(f => f.UserId == UserId && f.EntityId == i.ClientID);

                case CPiEntityType.Owner:
                    return i => UserEntityFilters.Any(f => f.UserId == UserId && i.Owners.Any(o => o.OwnerID == f.EntityId));

                //case CPiEntityType.Owner:
                //    if (IsMultipleOwners)
                //        return i => UserEntityFilters.Any(f => f.UserId == UserId && i.Owners.Any(o => o.OwnerID == f.EntityId));
                //    else
                //        return i => UserEntityFilters.Any(f => f.UserId == UserId && f.EntityId == i.OwnerID);

                case CPiEntityType.Attorney:
                    return i => UserEntityFilters.Any(f => f.UserId == UserId && (
                                                                                f.EntityId == i.Attorney1ID || 
                                                                                f.EntityId == i.Attorney2ID || 
                                                                                f.EntityId == i.Attorney3ID ||
                                                                                f.EntityId == i.Attorney4ID ||
                                                                                f.EntityId == i.Attorney5ID
                                                                                ));

                case CPiEntityType.Inventor:
                    return i => UserEntityFilters.Any(f => f.UserId == UserId && i.Inventors.Any(iv => iv.InventorID == f.EntityId));

                case CPiEntityType.Agent:
                    return i => !i.CountryApplications.Any() || UserEntityFilters.Any(f => f.UserId == UserId && (i.CountryApplications.Any(ca => ca.AgentID == f.EntityId)));

                case CPiEntityType.ContactPerson:
                    return i => UserEntityFilters.Any(f => f.UserId == UserId && i.Client.ClientContacts.Any(d => d.ContactID == f.EntityId));
            }
            return i => true;
        }

        public override async Task<Invention> GetByIdAsync(int invId)
        {
            return await QueryableList.SingleOrDefaultAsync(i => i.InvId == invId);
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

        public bool IsInventorRequired
        {
            get
            {
                _isInventorRequired = _isInventorRequired ?? (_user.GetEntityFilterType() == CPiEntityType.Inventor);
                return (bool)_isInventorRequired;
            }
        }

        public async Task Add(Invention invention, List<int> requiredEntityIds)
        {
            Guard.Against.NoRecordPermission(await ValidatePermission(SystemType.Patent, CPiPermissions.FullModify, invention.RespOffice));

            if (IsOwnerRequired || IsInventorRequired)
                Guard.Against.Null(requiredEntityIds?.Count > 0 ? requiredEntityIds : null, IsOwnerRequired ? "Owner" : "Inventor");

            await ValidateInvention(invention);

            _cpiDbContext.GetRepository<Invention>().Add(invention);
            if (IsOwnerRequired)
            {
                var inventionOwners = requiredEntityIds.Select((ownerId, orderOfEntry) => 
                    new PatOwnerInv
                        {
                            InvId = invention.InvId,
                            OwnerID = ownerId,
                            OrderOfEntry = orderOfEntry,
                            CreatedBy = invention.CreatedBy,
                            DateCreated = invention.DateCreated,
                            UpdatedBy = invention.UpdatedBy,
                            LastUpdate = invention.LastUpdate
                        }).ToList();
                _cpiDbContext.GetRepository<PatOwnerInv>().Add(inventionOwners);
            }
            else if (IsInventorRequired)
            {
                var inventionInventors = requiredEntityIds.Select((inventorId, orderOfEntry) => 
                    new PatInventorInv
                        {
                            InvId = invention.InvId,
                            InventorID = inventorId,
                            OrderOfEntry = orderOfEntry,
                            CreatedBy = invention.CreatedBy,
                            DateCreated = invention.DateCreated,
                            UpdatedBy = invention.UpdatedBy,
                            LastUpdate = invention.LastUpdate
                    }).ToList();
                _cpiDbContext.GetRepository<PatInventorInv>().Add(inventionInventors);
            }
            await AddSingleOwner(invention);
            var tsActivity = await SetTradeSecret(invention);
            await _cpiDbContext.SaveChangesAsync();

            if (!string.IsNullOrEmpty(tsActivity.ActivityCode))
            {
                await CreateTradeSecretActivityLog(invention.InvId, tsActivity.ActivityCode, tsActivity.AuditLogs);
                await _cpiDbContext.SaveChangesAsync();
            }
        }

        public override async Task Add(Invention invention)
        {
            Guard.Against.NoRecordPermission(await ValidatePermission(SystemType.Patent, CPiPermissions.FullModify, invention.RespOffice));

            if (IsOwnerRequired || IsInventorRequired)
                Guard.Against.Null(null, IsOwnerRequired ? "Owner" : "Inventor");

            await ValidateInvention(invention);
            _cpiDbContext.GetRepository<Invention>().Add(invention);
            await AddSingleOwner(invention);
            var tsActivity = await SetTradeSecret(invention);
            await _cpiDbContext.SaveChangesAsync();

            if (!string.IsNullOrEmpty(tsActivity.ActivityCode))
            {
                await CreateTradeSecretActivityLog(invention.InvId, tsActivity.ActivityCode, tsActivity.AuditLogs);
                await _cpiDbContext.SaveChangesAsync();
            }
        }

        public async Task Update(Invention invention, bool hasRelatedCasesMassCopy) {

            if (hasRelatedCasesMassCopy)
            {
                using (var scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted }, TransactionScopeAsyncFlowOption.Enabled))
                {
                    await Update(invention);
                    await RelatedCasesMassCopy(invention.InvId, invention.UpdatedBy);
                    scope.Complete();
                }
            }
            else {
                await Update(invention);
            } 
        }

        public override async Task Update(Invention invention)
        {
            await ValidatePermission(invention.InvId, CPiPermissions.FullModify);
            await ValidateInvention(invention);
            _cpiDbContext.GetRepository<Invention>().Update(invention);
            await AddSingleOwner(invention);

            var tsActivity = await SetTradeSecret(invention);
            if (!string.IsNullOrEmpty(tsActivity.ActivityCode))
                await CreateTradeSecretActivityLog(invention.InvId, tsActivity.ActivityCode, tsActivity.AuditLogs);

            await _cpiDbContext.SaveChangesAsync();
        }

        public override async Task UpdateRemarks(Invention entity)
        {
            await ValidatePermission(entity.InvId, CPiPermissions.RemarksOnly);
            await base.UpdateRemarks(entity);
        }

        public override async Task Delete(Invention invention)
        {
            await ValidatePermission(invention.InvId, CPiPermissions.CanDelete);

            var canDeleteTradeSecret = await CanDeleteTradeSecret(invention.InvId);
            Guard.Against.UnAuthorizedAccess(canDeleteTradeSecret.Allowed);

            //Check if record is being used in Related Invention tab on Disclosure screen     
            if (await _cpiDbContext.GetRepository<InventionRelatedDisclosure>().QueryableList.AnyAsync(d => d.InvId == invention.InvId))
            {
                //If so, only allow to delete if user has patent delete permission
                if (!_user.IsInSystem(SystemType.DMS))
                {
                    throw new NoRecordPermissionException("Record cannot be deleted. It is already in use under Related Invention tab on Disclosure");
                }
            }

            _cpiDbContext.GetRepository<Invention>().Delete(invention);

            if (canDeleteTradeSecret.InvId > 0)
                await CreateTradeSecretActivityLog(canDeleteTradeSecret.InvId, TradeSecretActivityCode.Delete, CreateAuditLogs(await GetByIdAsync(invention.InvId), null));

            await _cpiDbContext.SaveChangesAsync();
        }

        public async Task CopyInvention(int oldInvId, int newInvId, string userName, bool copyCaseInfo,
            bool CopyOwners, bool CopyInventors, bool CopyPriorities, bool CopyAbstract,
            bool CopyKeywords, bool CopyImages, bool CopyRelatedInventions, bool CopyProducts, 
            bool copyCosts)
        {
            await _cpiDbContext.ExecuteSqlRawAsync($"procPatInvCopy @OldInvId={oldInvId},@NewInvId={newInvId},@CreatedBy='{userName}',@CopyCaseInfo={copyCaseInfo},@CopyOwners={CopyOwners},@CopyInventors={CopyInventors},@CopyPriorities={CopyPriorities},@CopyAbstract={CopyAbstract},@CopyKeywords={CopyKeywords},@CopyImages={CopyImages},@CopyRelatedInventions={CopyRelatedInventions},@CopyProducts={CopyProducts},@CopyCosts={copyCosts}");
        }

        public async Task RefreshCopySetting(List<InventionCopySetting> added, List<InventionCopySetting> deleted)
        {
            if (added.Count > 0)
                _cpiDbContext.GetRepository<InventionCopySetting>().Add(added);

            if (deleted.Count > 0)
                _cpiDbContext.GetRepository<InventionCopySetting>().Delete(deleted);
            await _cpiDbContext.SaveChangesAsync();
        }

        public async Task UpdateCopySetting(InventionCopySetting setting)
        {
            var existing = await _cpiDbContext.GetRepository<InventionCopySetting>().GetByIdAsync(setting.CopySettingId);
            if (existing != null)
            {
                existing.Copy = setting.Copy;
                _cpiDbContext.GetRepository<InventionCopySetting>().Update(existing);
                await _cpiDbContext.SaveChangesAsync();
            }
        }

        public async Task AddCopySettings(List<InventionCopySetting> settings)
        {
            if (settings.Count > 0) {
                _cpiDbContext.GetRepository<InventionCopySetting>().Add(settings);
                await _cpiDbContext.SaveChangesAsync();
            }
        }

        public async Task<CPiUserSetting> GetMainCopySettings(string userId)
        {
            var setting = await _repository.CPiSettings.Where(d => d.Name == "InventionCopySetting").AsNoTracking().FirstOrDefaultAsync();
            if (setting == null)
            {
                setting = new CPiSetting { Name = "InventionCopySetting", Policy = "*" };
                _cpiDbContext.GetRepository<CPiSetting>().Add(setting);
                await _cpiDbContext.SaveChangesAsync();
            }
            return await _cpiDbContext.GetRepository<CPiUserSetting>().QueryableList.Where(u => u.UserId == userId && u.SettingId == setting.Id).AsNoTracking().FirstOrDefaultAsync();
        }

        public async Task UpdateMainCopySettings(CPiUserSetting userSetting)
        {
            if (userSetting.Id > 0)
               _cpiDbContext.GetRepository<CPiUserSetting>().Update(userSetting);
            else
                _cpiDbContext.GetRepository<CPiUserSetting>().Add(userSetting);
            await _cpiDbContext.SaveChangesAsync();
        }

        public async Task<int> GetMainCopySettingId()
        {
            var setting = await _repository.CPiSettings.Where(d => d.Name == "InventionCopySetting").AsNoTracking().FirstOrDefaultAsync();
            if (setting != null)
            {
                return setting.Id;
            }
            else {
                setting = new CPiSetting { Name = "InventionCopySetting", Policy = "*" };
                _cpiDbContext.GetRepository<CPiSetting>().Add(setting);
                await _cpiDbContext.SaveChangesAsync();
                return setting.Id;
            }
        }


        public async Task ValidatePermission(int invId, List<string> roles)
        {
            var respOfc = "";
            if (_user.HasEntityFilter() || _user.HasRespOfficeFilter(SystemType.Patent))
            {
                var item = (await QueryableList.Where(i => i.InvId == invId).Select(i => new { i.InvId, i.RespOffice }).ToDictionaryAsync(i => i.InvId, i => i.RespOffice)).FirstOrDefault();
                Guard.Against.NoRecordPermission(item.Key > 0);

                respOfc = item.Value;
            }

            Guard.Against.NoRecordPermission(await ValidatePermission(SystemType.Patent, roles, respOfc));
        }

        private async Task AddSingleOwner(Invention invention) {
            if (invention.OwnerID > 0) {
                var existing = await _cpiDbContext.GetRepository<PatOwnerInv>().QueryableList.FirstOrDefaultAsync(o => o.InvId == invention.InvId);
                if (existing != null)
                {
                    existing.OwnerID = (int)invention.OwnerID;
                    existing.UpdatedBy = invention.UpdatedBy;
                    existing.LastUpdate = invention.LastUpdate;
                    _cpiDbContext.GetRepository<PatOwnerInv>().Update(existing);
                }
                else
                    _cpiDbContext.GetRepository<PatOwnerInv>().Add(new PatOwnerInv
                    {
                        InvId = invention.InvId,
                        OwnerID = (int)invention.OwnerID,
                        CreatedBy = invention.CreatedBy,
                        UpdatedBy = invention.UpdatedBy,
                        DateCreated = invention.DateCreated,
                        LastUpdate = invention.LastUpdate
                    });
                invention.OwnerID = null;
            }
        }

        private async Task ValidateInvention(Invention invention, List<string>? roles = null)
        {
            var entityFilterType = _user.GetEntityFilterType();
            var settings = await _settings.GetSetting();

            if (entityFilterType == CPiEntityType.Attorney)
            {
                var current = new Invention();

                bool canModifyAttorney1 = true;
                bool canModifyAttorney2 = true;
                bool canModifyAttorney3 = true;
                bool canModifyAttorney4 = true;
                bool canModifyAttorney5 = true;

                if (invention.InvId > 0)
                {
                    current = await GetByIdAsync(invention.InvId);

                    canModifyAttorney1 = await CanModifyAttorney(current.Attorney1ID ?? 0);
                    canModifyAttorney2 = await CanModifyAttorney(current.Attorney2ID ?? 0);
                    canModifyAttorney3 = await CanModifyAttorney(current.Attorney3ID ?? 0);
                    canModifyAttorney4 = await CanModifyAttorney(current.Attorney4ID ?? 0);
                    canModifyAttorney5 = await CanModifyAttorney(current.Attorney5ID ?? 0);

                    Guard.Against.NoFieldPermission(current.Attorney1ID == null || current.Attorney1ID == invention.Attorney1ID || (current.Attorney1ID != invention.Attorney1ID && canModifyAttorney1), settings.LabelAttorney1 ?? "Attorney 1");
                    Guard.Against.NoFieldPermission(current.Attorney2ID == null || current.Attorney2ID == invention.Attorney2ID || (current.Attorney2ID != invention.Attorney2ID && canModifyAttorney2), settings.LabelAttorney2 ?? "Attorney 2");
                    Guard.Against.NoFieldPermission(current.Attorney3ID == null || current.Attorney3ID == invention.Attorney3ID || (current.Attorney3ID != invention.Attorney3ID && canModifyAttorney3), settings.LabelAttorney3 ?? "Attorney 3");
                    Guard.Against.NoFieldPermission(current.Attorney4ID == null || current.Attorney4ID == invention.Attorney4ID || (current.Attorney4ID != invention.Attorney4ID && canModifyAttorney4), settings.LabelAttorney4 ?? "Attorney 4");
                    Guard.Against.NoFieldPermission(current.Attorney5ID == null || current.Attorney5ID == invention.Attorney5ID || (current.Attorney5ID != invention.Attorney5ID && canModifyAttorney5), settings.LabelAttorney5 ?? "Attorney 5");
                }

                if (invention.Attorney1ID != null && invention.Attorney1ID != current.Attorney1ID && canModifyAttorney1)
                    Guard.Against.ValueNotAllowed(await EntityFilterAllowed(invention.Attorney1ID ?? 0), settings.LabelAttorney1 ?? "Attorney 1");

                if (invention.Attorney2ID != null && invention.Attorney2ID != current.Attorney2ID && canModifyAttorney2)
                    Guard.Against.ValueNotAllowed(await EntityFilterAllowed(invention.Attorney2ID ?? 0), settings.LabelAttorney2 ?? "Attorney 2");

                if (invention.Attorney3ID != null && invention.Attorney3ID != current.Attorney3ID && canModifyAttorney3)
                    Guard.Against.ValueNotAllowed(await EntityFilterAllowed(invention.Attorney3ID ?? 0), settings.LabelAttorney3 ?? "Attorney 3");

                if (invention.Attorney4ID != null && invention.Attorney4ID != current.Attorney4ID && canModifyAttorney4)
                    Guard.Against.ValueNotAllowed(await EntityFilterAllowed(invention.Attorney4ID ?? 0), settings.LabelAttorney4 ?? "Attorney 4");

                if (invention.Attorney5ID != null && invention.Attorney5ID != current.Attorney5ID && canModifyAttorney5)
                    Guard.Against.ValueNotAllowed(await EntityFilterAllowed(invention.Attorney5ID ?? 0), settings.LabelAttorney5 ?? "Attorney 5");

                if ((invention.Attorney1ID == null || !canModifyAttorney1) && 
                    (invention.Attorney2ID == null || !canModifyAttorney2) && 
                    (invention.Attorney3ID == null || !canModifyAttorney3) &&
                    (invention.Attorney4ID == null || !canModifyAttorney4) &&
                    (invention.Attorney5ID == null || !canModifyAttorney5))
                    Guard.Against.Null(null, "Attorney");
            }

            if (entityFilterType == CPiEntityType.Client)
            {
                var clientLabel = settings.LabelClient;
                Guard.Against.Null(invention.ClientID, clientLabel);
                Guard.Against.ValueNotAllowed(await EntityFilterAllowed(invention.ClientID ?? 0), clientLabel);
            }

            if (entityFilterType == CPiEntityType.Owner && !IsMultipleOwners)
            {
                var ownerLabel = settings.LabelOwner;
                Guard.Against.Null(invention.OwnerID, ownerLabel);
                Guard.Against.ValueNotAllowed(await EntityFilterAllowed(invention.OwnerID ?? 0), ownerLabel);
            }

            if (_user.IsRespOfficeOn(SystemType.Patent))
            {
                Guard.Against.Null(invention.RespOffice, "Responsible Office");
                Guard.Against.ValueNotAllowed(await ValidateRespOffice(SystemType.Patent, invention.RespOffice, roles), "Responsible Office");
            }
        }

        public async Task<bool> CanModifyAttorney(int attorneyId)
        {
            if (_user.GetEntityFilterType() == CPiEntityType.Attorney && attorneyId > 0)
                return await EntityFilterAllowed(attorneyId);
            else
                return true;
        }
        public async Task<List<SysCustomFieldSetting>> GetCustomFields()
        {
            //var customFieldSettings =  _cpiDbContext.GetRepository<SysCustomFieldSetting>().QueryableList; //dbcontext thread issue?
            var customFieldSettings = _repository.SysCustomFieldSettings;
            return await customFieldSettings.Where(s => s.TableName == "tblPatInvention" && s.Visible == true).OrderBy(s => s.OrderOfEntry).ToListAsync();
        }

        public async Task UpdateDeDocket(Invention invention)
        {
            await ValidatePermission(invention.InvId, CPiPermissions.DeDocketer);
            await ValidateInvention(invention, CPiPermissions.DeDocketer);

            var deDocketFields = await _systemSettingManager.GetSystemSetting<DeDocketFields>();
            var updated = await GetByIdAsync(invention.InvId);

            Guard.Against.NoRecordPermission(updated != null);

            if (updated != null && deDocketFields.Invention != null)
            {
                if (deDocketFields.Invention.Title)
                    updated.InvTitle = invention.InvTitle;

                if (deDocketFields.Invention.ClientReference)
                    updated.ClientRef = invention.ClientRef;

                if (deDocketFields.Invention.Attorney1)
                    updated.Attorney1ID = invention.Attorney1ID;

                if (deDocketFields.Invention.Attorney2)
                    updated.Attorney2ID = invention.Attorney2ID;

                if (deDocketFields.Invention.Attorney3)
                    updated.Attorney3ID = invention.Attorney3ID;

                if (deDocketFields.Invention.Attorney4)
                    updated.Attorney4ID = invention.Attorney4ID;

                if (deDocketFields.Invention.Attorney5)
                    updated.Attorney5ID = invention.Attorney5ID;

                if (deDocketFields.Invention.Remarks)
                    updated.Remarks = invention.Remarks;

                updated.LastUpdate = invention.LastUpdate;
                updated.UpdatedBy = invention.UpdatedBy;
                updated.tStamp = invention.tStamp;

                _cpiDbContext.GetRepository<Invention>().Update(updated);

                var tsActivity = await SetTradeSecret(updated);
                if (!string.IsNullOrEmpty(tsActivity.ActivityCode))
                    await CreateTradeSecretActivityLog(invention.InvId, tsActivity.ActivityCode, tsActivity.AuditLogs);

                await _cpiDbContext.SaveChangesAsync();
            }
            else
                Guard.Against.UnAuthorizedAccess(false);
        }

        #region Products
        public async Task<bool> HasProducts(int invId)
        {
            return await _repository.PatProductInvs.AnyAsync(p => p.InvId == invId);
        }

        public IQueryable<Product> Products => _repository.Products.AsNoTracking();
        #endregion

        public async Task RelatedCasesMassCopy(int invId, string? createdBy)
        {
            await _repository.Database.ExecuteSqlInterpolatedAsync($"procPatRelatedCasesMassCopyFamily @InvId={invId},@CreatedBy={createdBy}");
        }

        public IQueryable<Invention> Inventions
        {
            get
            {
                var inventions = _repository.Inventions.AsNoTracking();

                if (_user.HasRespOfficeFilter(SystemType.Patent))
                    inventions = inventions.Where(RespOfficeFilter());

                if (_user.HasEntityFilter())
                    inventions = inventions.Where(EntityFilter());

                //if (_user.RestrictExportControl())
                //    inventions = inventions.Where(i => !(i.ExportControl ?? false));

                return inventions;
            }
        }

        public async Task AddCustomFieldsAsCopyFields()
        {
            await _inventionRepository.AddCustomFieldsAsCopyFields();
        }

        #region Action
        public async Task<List<DelegationEmailDTO>> GetDelegationEmails(int delegationId)
        {
            return await _inventionRepository.GetDelegationEmails(delegationId);
        }
        public async Task MarkDelegationasEmailed(int delegationId)
        {
            await _inventionRepository.MarkDelegationasEmailed(delegationId);
        }
        public async Task<List<LookupIntDTO>> GetDelegatedDdIds(int action, int[] recIds)
        {
            return await _inventionRepository.GetDelegatedDdIds(action, recIds);
        }
        public async Task<List<DelegationEmailDTO>> GetDeletedDelegationEmails(int delegationId)
        {
            return await _inventionRepository.GetDeletedDelegationEmails(delegationId);
        }
        public async Task<List<LookupIntDTO>> GetDuedateChangedDelegationIds(int action, List<PatDueDateInv> updated)
        {
            return await _inventionRepository.GetDuedateChangedDelegationIds(action, updated);
        }

        public async Task<DelegationDetailDTO> GetDeletedDelegation(int delegationId)
        {
            return await _inventionRepository.GetDeletedDelegation(delegationId);
        }

        public IQueryable<DeDocketInstruction> DeDocketInstructions
        {
            get
            {
                return _repository.DeDocketInstructions.AsNoTracking();
            }
        }
        #endregion

        private async Task<(string? ActivityCode, Dictionary<string, string?[]>? AuditLogs)> SetTradeSecret(Invention invention)
        {
            var activityCode = string.Empty;

            if (!_user.CanAccessPatTradeSecret())
                return (null, null);

            var current = await GetByIdAsync(invention.InvId);
            var currentTradeSecret = current?.TradeSecret ?? new InventionTradeSecret();
            var currentIsTradeSecret = current?.IsTradeSecret ?? false;

            if (current != null)
                invention.TradeSecretDate = current.TradeSecretDate;

            if (currentIsTradeSecret)
            {
                var isTSCleared = await _tradeSecretService.IsUserCleared(TradeSecretScreen.Invention, invention.InvId);

                //only cleared admins can turn off IsTradeSecret flag
                if (!(invention.IsTradeSecret ?? false) && !(_user.IsPatTradeSecretAdmin() && isTSCleared))
                    invention.IsTradeSecret = true;

                //only cleared full modify users can edit trade secret fields
                if (!(_user.CanEditPatTradeSecretFields() && isTSCleared))
                    invention.RestoreTradeSecret(currentTradeSecret, true);
            }

            if (invention.IsTradeSecret ?? false)
            {
                invention.TradeSecret = invention.CreateTradeSecret(new InventionTradeSecret());

                if (invention.InvId == 0 || !currentIsTradeSecret)
                {
                    invention.TradeSecretDate = DateTime.Now;

                    activityCode = TradeSecretActivityCode.Create;
                }
                else if (invention.TradeSecret.InvTitle != current?.TradeSecret?.InvTitle)
                {
                    activityCode = TradeSecretActivityCode.Update;
                }
            }
            else if (currentIsTradeSecret)
            {
                invention.RestoreTradeSecret(currentTradeSecret);
                invention.TradeSecret = new InventionTradeSecret();

                activityCode = TradeSecretActivityCode.Update;
            }

            //process abstract
            await SetAbstractTradeSecret(invention);

            //process country apps
            await SetCountryApplicationTradeSecret(invention);

            //create activity log
            if (!string.IsNullOrEmpty(activityCode))
                return (activityCode, CreateAuditLogs(current, invention));

            return (null, null);
        }

        /// <summary>
        /// Encrypts trade secret records and refreshes redacted string length
        /// </summary>
        /// <param name="invId"></param>
        /// <returns></returns>
        public async Task RefreshTradeSecret(int invId)
        {
            if (!_user.CanAccessPatTradeSecret())
                return;

            var invention = await GetByIdAsync(invId);
            if (invention == null)
                return;

            var tradeSecret = invention.TradeSecret ?? new InventionTradeSecret();

            _cpiDbContext.GetRepository<Invention>().Update(invention);

            if (!invention.IsTradeSecret ?? false)
                invention.RestoreTradeSecret(tradeSecret, true);
            else
                invention.TradeSecret = invention.CreateTradeSecret(tradeSecret);

            //process abstract
            await SetAbstractTradeSecret(invention);

            //process country apps
            await SetCountryApplicationTradeSecret(invention);

            //create activity log
            await CreateTradeSecretActivityLog(invention.InvId, TradeSecretActivityCode.Refresh, CreateAuditLogs(invention, null));

            await _cpiDbContext.SaveChangesAsync();

            return;
        }

        private async Task SetAbstractTradeSecret(Invention invention)
        {
            if (!_user.CanAccessPatTradeSecret() || invention.InvId == 0)
                return;

            var currentAbstracts = await _cpiDbContext.GetRepository<PatAbstract>().QueryableList.Where(a => a.InvId == invention.InvId).ToListAsync();
            foreach(var patAbstract in currentAbstracts)
            {
                _cpiDbContext.GetRepository<PatAbstract>().Update(patAbstract);

                if (invention.IsTradeSecret ?? false)
                {
                    patAbstract.TradeSecret = patAbstract.CreateTradeSecret(patAbstract.TradeSecret ?? new AbstractTradeSecret());
                }
                else if (patAbstract.TradeSecret != null)
                {
                    patAbstract.RestoreTradeSecret(patAbstract.TradeSecret);
                }
            }
        }

        private async Task SetCountryApplicationTradeSecret(Invention invention)
        {
            if (!_user.CanAccessPatTradeSecret() || invention.InvId == 0)
                return;

            var currentApps = await _cpiDbContext.GetRepository<CountryApplication>().QueryableList.Where(a => a.InvId == invention.InvId).ToListAsync();
            foreach (var app in currentApps)
            {
                _cpiDbContext.GetRepository<CountryApplication>().Update(app);

                if (invention.IsTradeSecret ?? false)
                {
                    app.TradeSecret = app.CreateTradeSecret(app.TradeSecret ?? new CountryApplicationTradeSecret());
                }
                else if (app.TradeSecret != null)
                {
                    app.RestoreTradeSecret(app.TradeSecret);
                }
            }
        }

        /// <summary>
        /// Returns Allowed = true and InvId > 0 if invention is a trade secret and user has delete clearance
        /// </summary>
        /// <param name="invId"></param>
        /// <returns></returns>
        private async Task<(bool Allowed, int InvId)> CanDeleteTradeSecret(int invId)
        {
            var isTradeSecret = (await QueryableList.Where(i => i.InvId == invId).Select(i => i.IsTradeSecret).SingleOrDefaultAsync()) ?? false;

            if (isTradeSecret && _user.CanAccessPatTradeSecret())
            {
                //only cleared admins can delete trade secret
                var isTSCleared = await _tradeSecretService.IsUserCleared(TradeSecretScreen.Invention, invId);
                return (_user.IsPatTradeSecretAdmin() && isTSCleared, invId);
            }

            return (!isTradeSecret, 0);
        }

        private async Task<TradeSecretActivity> CreateTradeSecretActivityLog(int invId, string activityCode, Dictionary<string, string?[]>? auditLogs)
        {
            var tsRequest = await _tradeSecretService.GetUserRequest(_tradeSecretService.CreateLocator(TradeSecretScreen.Invention, invId));
            var tsActivity = _tradeSecretService.CreateActivity(TradeSecretScreen.Invention, TradeSecretScreen.Invention, invId, activityCode, tsRequest?.RequestId ?? 0, auditLogs);

            return tsActivity;
        }

        private Dictionary<string, string?[]> CreateAuditLogs(Invention? oldValues, Invention? newValues)
        {
            var auditLogs = new Dictionary<string, string?[]>();

            if (newValues?.IsTradeSecret != oldValues?.IsTradeSecret)
                auditLogs.Add("IsTradeSecret", [oldValues?.IsTradeSecret.ToString(), newValues?.IsTradeSecret.ToString()]);

            if (newValues?.TradeSecret?.InvTitle != oldValues?.TradeSecret?.InvTitle)
                auditLogs.Add("InvTitle", [oldValues?.TradeSecret?.InvTitle, newValues?.TradeSecret?.InvTitle]);

            return auditLogs;
        }
    }
}
