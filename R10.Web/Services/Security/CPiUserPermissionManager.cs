using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.Graph;
using Newtonsoft.Json.Linq;
using R10.Core;
using R10.Core.DTOs;
using R10.Core.Entities;
using R10.Core.Entities.Identity;
using R10.Core.Entities.Shared;
using R10.Core.Exceptions;
using R10.Core.Helpers;
using R10.Core.Identity;
using R10.Core.Interfaces;
using R10.Core.Services.Shared;
using System.Data.SqlClient;
using System.Security.Claims;

namespace R10.Web.Services
{
    public class CPiUserPermissionManager : ICPiUserPermissionManager
    {
        private readonly ICPiUserEntityFilterRepository _userEntityFilterStore;
        private readonly ICPiDbContext _cpiDbContext;
        private readonly ISystemSettings<DefaultSetting> _settings;
        private readonly ICPiUserSettingManager _settingManager;
        private readonly ClaimsPrincipal _user;

        public CPiUserPermissionManager(
            ICPiUserEntityFilterRepository userEntityFilterStore,
            ICPiDbContext cpiDbContext,
            ISystemSettings<DefaultSetting> settings,
            ICPiUserSettingManager settingManager,
            ClaimsPrincipal user)
        {
            _userEntityFilterStore = userEntityFilterStore;
            _cpiDbContext = cpiDbContext;
            _settings = settings;
            _settingManager = settingManager;
            _user = user;
        }

        public IQueryable<EntityFilterDTO> AvailableEntityFilter(CPiEntityType entityType, string entity, string userId)
        {
            if (!string.IsNullOrEmpty(entity))
                entity = $"%{entity.Trim().ToLower()}%";

            IQueryable<EntityFilterDTO>? entities = null;
            var userEntityFilters = _cpiDbContext.GetRepository<CPiUserEntityFilter>().QueryableList.Where(ef => ef.UserId == userId);

            switch (entityType)
            {

                case CPiEntityType.Inventor:
                    // PatInventor entity removed during debloat
                    entities = (new List<EntityFilterDTO>()).AsQueryable();
                    break;
            }

            if (entities == null)
            {
                return (new List<EntityFilterDTO>()).AsQueryable();
            }
            return entities;
        }

        public IQueryable<EntityFilterDTO> UserEntityFilter(CPiEntityType entityType, string userId)
        {
            IQueryable<EntityFilterDTO>? entities = null;
            var userEntityFilters = _cpiDbContext.GetRepository<CPiUserEntityFilter>().QueryableList.Where(ef => ef.UserId == userId);

            switch (entityType)
            {

                case CPiEntityType.Inventor:
                    // PatInventor entity removed during debloat
                    entities = (new List<EntityFilterDTO>()).AsQueryable();
                    break;
            }

            if (entities == null)
            {
                return (new List<EntityFilterDTO>()).AsQueryable();
            }

            return entities;
        }
        

        public IQueryable<CPiSystem> CPiSystems => _cpiDbContext.GetRepository<CPiSystem>().QueryableList;
        public IQueryable<CPiRole> CPiRoles => _cpiDbContext.GetRepository<CPiRole>().QueryableList.Where(r => r.IsEnabled);
        public IQueryable<CPiUserEntityFilter> CPiUserEntityFilters => _cpiDbContext.GetRepository<CPiUserEntityFilter>().QueryableList;
        public IQueryable<CPiUserSystemRole> CPiUserSystemRoles => _cpiDbContext.GetRepository<CPiUserSystemRole>().QueryableList;
        private IQueryable<CPiSystemRole> CPiSystemRoles => _cpiDbContext.GetRepository<CPiSystemRole>().QueryableList;
        private IQueryable<CPiRespOffice> CPiRespOffices => _cpiDbContext.GetRepository<CPiRespOffice>().QueryableList;

        public async Task<IQueryable> AvailableSystems(string userId, string systemId)
        {
            var isRespOfficeOn = await CPiSystems.AnyAsync(s => s.IsRespOfficeOn);
            return CPiSystems
                                            .GroupJoin(CPiUserSystemRoles.Where(u => u.UserId == userId && CPiPermissions.RegularUser.Contains(u.RoleId.ToLower())), s => s.Id, u => u.SystemId, (s, g) => new { s, g })
                                            .SelectMany(p => p.g.DefaultIfEmpty(), (p, u) => new { system = p.s, user = u })
                                            .Where(j => j.system.IsEnabled && (j.user.UserId == null || j.system.Id == systemId || isRespOfficeOn))
                                            .Select(s => new { s.system.Id, s.system.Name })
                                            .Distinct()
                                            .OrderBy(o => o.Id); //issue: if not implicitly set, ef will include: order by (select 1) which causes issue with distinct
        }
        public async Task<IQueryable> AvailableRoles(string systemId)
        {
            var settings = await _settings.GetSetting();
            var roles = CPiSystemRoles.Join(CPiRoles, r => r.RoleId, r => r.Id, (sr, r) => new { sr, r })
                                      .Where(j => j.sr.SystemId == systemId && j.r.IsEnabled);

            //hide dedocketer role if dedocket is disabled
            if (!settings.IsDeDocketOn)
                roles = roles.Where(j => !CPiPermissions.DeDocketer.Contains(j.r.Id));

            return roles.Select(s => new { s.r.Id, s.r.Name })
                         .Distinct()
                         .OrderBy(o => o.Id); //issue: if not implicitly set, ef will include: order by (select 1) which causes issue with distinct
        }

        public async Task<IQueryable> AvailableRespOffices(string systemId)
        {
            if (!string.IsNullOrEmpty(systemId))
            {
                var system = await CPiSystems.FirstOrDefaultAsync(s => s.Id == systemId);

                if (system != null && system.IsRespOfficeOn)
                {
                    var systemType = (system.SystemType ?? "-").ToCharArray().FirstOrDefault();
                    return CPiRespOffices.Where(r => (r.SystemTypes ?? "") == "" || EF.Functions.Like((r.SystemTypes ?? ""), $"%{systemType}%"));
                }

                //return one empty row if not IsRespOfficeOn
                List<CPiRespOffice> result = new List<CPiRespOffice>();
                result.Add(new CPiRespOffice { RespOffice = "", Name = "" });
                return result.AsQueryable();
            }

            //return nothing if system is not selected
            return (new List<CPiRespOffice>()).AsQueryable();
        }

        public async Task<IdentityResult> AddEntityFilters(string userId, List<int> entityList)
        {
            List<CPiUserEntityFilter> entityFilters = new List<CPiUserEntityFilter>();
            foreach (int entityId in entityList)
            {
                entityFilters.Add(new CPiUserEntityFilter { UserId = userId, EntityId = entityId });
            }
            
            try
            {
                await _userEntityFilterStore.CreateAsync(entityFilters);
            }
            catch (Exception e) //catch (DbUpdateException e)
            {
                var message = e.Message;
                IdentityError err = new IdentityError();
                err.Description = "An error occurred while saving to the database.";

                return IdentityResult.Failed(err);
            }

            return IdentityResult.Success;
        }
        public async Task<IdentityResult> RemoveEntityFilters(string userId, List<int> entityList)
        {
            List<CPiUserEntityFilter> entityFilters = new List<CPiUserEntityFilter>();
            foreach (int entityId in entityList)
            {
                entityFilters.Add(new CPiUserEntityFilter { UserId = userId, EntityId = entityId });
            }

            try
            {
                await _userEntityFilterStore.DeleteAsync(entityFilters);
            }
            catch (Exception e) //catch (DbUpdateException e)
            {
                var message = e.Message;
                IdentityError err = new IdentityError();
                err.Description = "An error occurred while saving to the database.";

                return IdentityResult.Failed(err);
            }

            return IdentityResult.Success;
        }
        public async Task<IdentityResult> RemoveEntityFilters(string userId)
        {
            var entityFilters = await _userEntityFilterStore.GetUserEntityFilters(userId);

            try
            {
                await _userEntityFilterStore.DeleteAsync(entityFilters);
            }
            catch (Exception e) //catch (DbUpdateException e)
            {
                var message = e.Message;
                IdentityError err = new IdentityError();
                err.Description = "An error occurred while saving to the database.";

                return IdentityResult.Failed(err);
            }

            return IdentityResult.Success;
        }
        
        private async Task<IdentityResult> ValidateSystemRoleAsync(CPiUserSystemRole userSystemRole)
        {
            //system-role is not valid
            if (!await CPiSystemRoles.AnyAsync(sr => sr.SystemId == userSystemRole.SystemId && sr.RoleId == userSystemRole.RoleId))
            {
                return IdentityResult.Failed(new IdentityError { Code = "RoleId", Description = "Invalid Role." });
            }

            //system-role-respoffice already exists
            if (await CPiUserSystemRoles.AnyAsync(usr => usr.Id != userSystemRole.Id && usr.UserId == userSystemRole.UserId && usr.SystemId == userSystemRole.SystemId && usr.RoleId == userSystemRole.RoleId && usr.RespOffice == userSystemRole.RespOffice))
            {
                return IdentityResult.Failed(new IdentityError { Code = "RoleId", Description = "Role already exists." });
            }

            //exclude aux/letters/query roles
            var auxRoles = CPiPermissions.Auxiliary
                .Concat(CPiPermissions.Letters)
                .Concat(CPiPermissions.CustomQuery)
                .Concat(CPiPermissions.CountryLaw)
                .Concat(CPiPermissions.ActionType)
                .Concat(CPiPermissions.Products)
                .Concat(CPiPermissions.CostEstimator)
                .Concat(CPiPermissions.GermanRemuneration)
                .Concat(CPiPermissions.FrenchRemuneration)
                .Concat(CPiPermissions.PatentScore)
                .Concat(CPiPermissions.DocumentVerification)
                .Concat(CPiPermissions.Workflow)
                .Concat(CPiPermissions.SoftDocket)
                .Concat(CPiPermissions.RequestDocket)
                .Concat(CPiPermissions.Upload);


            if (await CPiUserSystemRoles.AnyAsync(usr => usr.Id != userSystemRole.Id && usr.UserId == userSystemRole.UserId && usr.SystemId == userSystemRole.SystemId && usr.RespOffice == userSystemRole.RespOffice && !auxRoles.Contains(usr.RoleId)))
            {
                return IdentityResult.Failed(new IdentityError { Code = "SystemId", Description = "System already exists." });
            }

            var system = await CPiSystems.FirstOrDefaultAsync(s => s.Id == userSystemRole.SystemId);

            //resp office required if IsRespOfficeOn
            if (system.IsRespOfficeOn && string.IsNullOrEmpty(userSystemRole.RespOffice))
            {
                return IdentityResult.Failed(new IdentityError { Code = "RespOffice", Description = "The Resp Office field is required." });
            }

            //resp office must be blank if not IsRespOfficeOn
            if (!system.IsRespOfficeOn && !string.IsNullOrEmpty(userSystemRole.RespOffice))
            {
                return IdentityResult.Failed(new IdentityError { Code = "RespOffice", Description = "Invalid Resp Office." });
            }

            return IdentityResult.Success;
        }

        /// <summary>
        /// Add CPiUserSystemRole and CPiUserClaim
        /// </summary>
        /// <param name="userSystemRole"></param>
        /// <returns></returns>
        public async Task<IdentityResult> AddRole(CPiUserSystemRole userSystemRole)
        {
            var result = await ValidateSystemRoleAsync(userSystemRole);

            if (!result.Succeeded)
            {
                return result;
            }

            //todo: use ExceptionFilter
            try
            {
                await AddUserSystemRole(userSystemRole);
            }
            catch (Exception e) //catch (DbUpdateException e)
            {
                var message = e.Message;
                IdentityError err = new IdentityError()
                {
                    Code = "AddRole",
                    Description = "An error occurred while saving to the database."
                };

                result = IdentityResult.Failed(err);
            }

            return result;
        }

        private async Task AddUserSystemRole(CPiUserSystemRole role)
        {
            var result = IdentityResult.Success;

            _cpiDbContext.GetRepository<CPiUserSystemRole>().Add(role);
            _cpiDbContext.GetRepository<CPiUserClaim>().Add(role.ToCPiUserClaims());

            await _cpiDbContext.SaveChangesAsync();
        }

        private async Task DeleteUserSystemRole(CPiUserSystemRole role)
        {
            List<CPiUserClaim> oldUserClaims = await GetUserClaimsFromRole(role);

            _cpiDbContext.GetRepository<CPiUserSystemRole>().Delete(role);
            _cpiDbContext.GetRepository<CPiUserClaim>().Delete(oldUserClaims);

            await _cpiDbContext.SaveChangesAsync();
        }

        public async Task<IdentityResult> CopyUserRoles(CPiUser userFrom, CPiUser userTo)
        {
            var oldRoles = _cpiDbContext.GetRepository<CPiUserSystemRole>().QueryableList.Where(e => e.UserId == userTo.Id);
            var oldClaims = _cpiDbContext.GetRepository<CPiUserClaim>().QueryableList.Where(e => e.UserId == userTo.Id);
            var oldEntityFilters = CPiUserEntityFilters.Where(e => e.UserId == userTo.Id);

            _cpiDbContext.GetRepository<CPiUserEntityFilter>().Delete(oldEntityFilters);
            _cpiDbContext.GetRepository<CPiUserSystemRole>().Delete(oldRoles);
            _cpiDbContext.GetRepository<CPiUserClaim>().Delete(oldClaims);

            var roles = await _cpiDbContext.GetRepository<CPiUserSystemRole>().QueryableList.Where(e => e.UserId == userFrom.Id)
                .Select(r => new CPiUserSystemRole()
                {
                    UserId = userTo.Id,
                    SystemId = r.SystemId,
                    RoleId = r.RoleId,
                    RespOffice = r.RespOffice
                }).ToListAsync();

            var claims = await _cpiDbContext.GetRepository<CPiUserClaim>().QueryableList.Where(e => e.UserId == userFrom.Id)
                .Select(c => new CPiUserClaim()
                {
                    UserId = userTo.Id,
                    ClaimType = c.ClaimType,
                    ClaimValue = c.ClaimValue
                }).ToListAsync();

            if (userTo.UserType.IsRegularUser() && userTo.EntityFilterType != CPiEntityType.None)
            {
                var entityFilters = await CPiUserEntityFilters.Where(e => e.UserId == userFrom.Id)
                    .Select(e => new CPiUserEntityFilter()
                    {
                        UserId = userTo.Id,
                        EntityId = e.EntityId
                    }).ToListAsync();

                _cpiDbContext.GetRepository<CPiUserEntityFilter>().Add(entityFilters);
            }

            _cpiDbContext.GetRepository<CPiUserSystemRole>().Add(roles);
            _cpiDbContext.GetRepository<CPiUserClaim>().Add(claims);
            try
            {
                await _cpiDbContext.SaveChangesAsync();
            }
            catch (Exception e) //catch (DbUpdateException e)
            {
                var message = e.Message;
                IdentityError err = new IdentityError();
                err.Code = "CopyUserRoles";
                err.Description = "An error occurred while saving to the database.";

                return IdentityResult.Failed(err);
            }

            return IdentityResult.Success;
        }

        public async Task<IdentityResult> UpdateRole(CPiUserSystemRole userSystemRole)
        {
            var result = await ValidateSystemRoleAsync(userSystemRole);

            if (!result.Succeeded)
            {
                return result;
            }

            try
            {
                await UpdateUserSystemRole(userSystemRole);
            }
            catch (Exception e) //catch (DbUpdateException e)
            {
                var message = e.Message;
                IdentityError err = new IdentityError();
                err.Code = "SystemId";
                err.Description = "An error occurred while saving to the database.";

                return IdentityResult.Failed(err);
            }

            return IdentityResult.Success;
        }

        private async Task UpdateUserSystemRole(CPiUserSystemRole userSystemRole)
        {
            var oldUserSystemRole = await _cpiDbContext.GetRepository<CPiUserSystemRole>().QueryableList.Where(e => e.Id == userSystemRole.Id).SingleOrDefaultAsync();
            Guard.Against.NoRecordPermission(oldUserSystemRole != null);

            List<CPiUserClaim> oldUserClaims = await GetUserClaimsFromRole(oldUserSystemRole);

            _cpiDbContext.GetRepository<CPiUserSystemRole>().Update(userSystemRole);
            _cpiDbContext.GetRepository<CPiUserClaim>().Add(userSystemRole.ToCPiUserClaims());
            _cpiDbContext.GetRepository<CPiUserClaim>().Delete(oldUserClaims);

            await _cpiDbContext.SaveChangesAsync();
        }

        /// <summary>
        /// Delete CPiUserSystemRole and CPiUserClaims
        /// </summary>
        /// <param name="userSystemRole"></param>
        /// <returns></returns>
        public async Task<IdentityResult> RemoveRole(CPiUserSystemRole userSystemRole)
        {

            try
            {
                await DeleteUserSystemRole(userSystemRole);
            }
            catch (Exception e) //catch (DbUpdateException e)
            {
                var message = e.Message;
                IdentityError err = new IdentityError();
                err.Code = "RemoveRole";
                err.Description = "An error occurred while saving to the database.";

                return IdentityResult.Failed(err);
            }

            return IdentityResult.Success;
        }
        
        private async Task<List<CPiUserClaim>> GetUserClaimsFromRole(CPiUserSystemRole userSystemRole)
        {
            var claims = userSystemRole.ToClaims();
            var userClaims = await _cpiDbContext.GetRepository<CPiUserClaim>().QueryableList.Where(c => c.UserId == userSystemRole.UserId).ToListAsync();

            List<CPiUserClaim> claimsFromRole = new List<CPiUserClaim>();
            foreach (var claim in claims)
            {
                //GET FIRST CLAIM ID
                //THERE COULD BE MULTIPLE CLAIMS WITH SAME TYPE AND VALUE
                var claimId = userClaims.Where(c => c.ClaimType == claim.Type && c.ClaimValue == claim.Value).Select(c => c.Id).FirstOrDefault();
                if (claimId > 0)
                    claimsFromRole.Add(new CPiUserClaim() { Id = claimId, UserId = userSystemRole.UserId, ClaimType = claim.Type, ClaimValue = claim.Value });
            }

            return claimsFromRole;
        }

        public async Task<List<CPiUserSystemRole>> GetUserRoles(CPiUser user)
        {
            return await CPiUserSystemRoles.Where(u => u.UserId == user.Id).ToListAsync();
        }

        public async Task<bool> UserHasSystemPermission(string userId, string systemId, List<string> roleIds)
        {
            return await _cpiDbContext.GetReadOnlyRepositoryAsync<CPiUser>().QueryableList.AnyAsync(u =>
                                        u.Id == userId && (u.UserType == CPiUserType.Administrator ||
                                                            u.UserType == CPiUserType.SuperAdministrator ||
                                                            (u.CPiUserSystemRoles.Any(usr => usr.SystemId == systemId && roleIds.Contains(usr.RoleId)))
                                                            ));
        }

        public async Task LinkEntity(CPiUser user, int entityId, CPiEntityType entityType)
        {
            //VALIDATION
            //ONLY ALLOW ONE USER ACCOUNT PER ENTITY
            var found = await CPiUserEntityFilters.AnyAsync(e => e.UserId != user.Id && e.EntityId == entityId && e.CPiUser.EntityFilterType == entityType && e.CPiUser.UserType == user.UserType);
            if (found)
                throw new ValueNotAllowedException($"{entityType} is already linked to another user account.");

            //SET ENTITY FILTER TYPE
            if (user.EntityFilterType != entityType)
            {
                _cpiDbContext.GetRepository<CPiUser>().Attach(user);

                user.EntityFilterType = entityType;
            }

            //REMOVE OLD ENTITY FILTERS
            var entityFilters = CPiUserEntityFilters.Where(e => e.UserId == user.Id);
            _cpiDbContext.GetRepository<CPiUserEntityFilter>().Delete(entityFilters);

            //ADD NEW ENTITY FILTER
            if (entityId > 0)
                _cpiDbContext.GetRepository<CPiUserEntityFilter>().Add(new CPiUserEntityFilter() { UserId = user.Id, EntityId = entityId });

            await _cpiDbContext.SaveChangesAsync();
        }

        public async Task SetReviewerRole(CPiUser user, bool isReviewer)
        {
            var systemId = SystemType.DMS;
            var roleId = "Reviewer";
            var userRoles = await GetUserRoles(user);
            var reviewerRole = userRoles.FirstOrDefault(r => r.SystemId == systemId && r.RoleId == roleId);
            await SetUserSystemRole(user.Id, systemId, isReviewer ? roleId : "", reviewerRole);
        }

        public async Task SetPreviewerRole(CPiUser user, bool isPreviewer)
        {
            var systemId = SystemType.DMS;
            var roleId = "Previewer";
            var userRoles = await GetUserRoles(user);
            var previewerRole = userRoles.FirstOrDefault(r => r.SystemId == systemId && r.RoleId == roleId);
            await SetUserSystemRole(user.Id, systemId, isPreviewer ? roleId : "", previewerRole);
        }

        public async Task SetClearanceReviewerRole(CPiUser user, bool isReviewer)
        {
            var systemId = SystemType.SearchRequest;
            var roleId = "Reviewer";
            var userRoles = await GetUserRoles(user);
            var reviewerRole = userRoles.FirstOrDefault(r => r.SystemId == systemId && r.RoleId == roleId);
            await SetUserSystemRole(user.Id, systemId, isReviewer ? roleId : "", reviewerRole);
        }

        public async Task SetPatClearanceReviewerRole(CPiUser user, bool isReviewer)
        {
            var systemId = SystemType.PatClearance;
            var roleId = "Reviewer";
            var userRoles = await GetUserRoles(user);
            var reviewerRole = userRoles.FirstOrDefault(r => r.SystemId == systemId && r.RoleId == roleId);
            await SetUserSystemRole(user.Id, systemId, isReviewer ? roleId : "", reviewerRole);
        }

        public async Task SetDecisionMakerRole(CPiUser user, bool isDecisionMaker, string systemId)
        {
            var roleId = "DecisionMaker";
            var userRoles = await GetUserRoles(user);
            var decisionMakerRole = userRoles.FirstOrDefault(r => r.SystemId == systemId && r.RoleId == roleId);
            await SetUserSystemRole(user.Id, systemId, isDecisionMaker ? roleId : "", decisionMakerRole);
        }

        public async Task SetAuxiliaryRole(CPiUser user, string systemId, string roleId)
        {
            var userRoles = await GetUserRoles(user);
            var auxiliaryRole = userRoles.FirstOrDefault(r => r.SystemId == systemId && CPiPermissions.Auxiliary.Contains(r.RoleId.ToLower()));
            await SetUserSystemRole(user.Id, systemId, roleId, auxiliaryRole);
        }

        public async Task SetCountryLawRole(CPiUser user, string systemId, string roleId)
        {
            var userRoles = await GetUserRoles(user);
            var countryLawRole = userRoles.FirstOrDefault(r => r.SystemId == systemId && CPiPermissions.CountryLaw.Contains(r.RoleId.ToLower()));
            await SetUserSystemRole(user.Id, systemId, roleId, countryLawRole);
        }

        public async Task SetActionTypeRole(CPiUser user, string systemId, string roleId)
        {
            var userRoles = await GetUserRoles(user);
            var actionTypeRole = userRoles.FirstOrDefault(r => r.SystemId == systemId && CPiPermissions.ActionType.Contains(r.RoleId.ToLower()));
            await SetUserSystemRole(user.Id, systemId, roleId, actionTypeRole);
        }

        public async Task SetLetterRole(CPiUser user, string systemId, string roleId)
        {
            var userRoles = await GetUserRoles(user);
            var letterRole = userRoles.FirstOrDefault(r => r.SystemId == systemId && CPiPermissions.Letters.Contains(r.RoleId.ToLower()));
            await SetUserSystemRole(user.Id, systemId, roleId, letterRole);
        }

        public async Task SetCustomQueryRole(CPiUser user, string systemId, string roleId)
        {
            var userRoles = await GetUserRoles(user);
            var customQueryRole = userRoles.FirstOrDefault(r => r.SystemId == systemId && CPiPermissions.CustomQuery.Contains(r.RoleId.ToLower()));
            await SetUserSystemRole(user.Id, systemId, roleId, customQueryRole);
        }

        public async Task SetProductsRole(CPiUser user, string systemId, string roleId)
        {
            var userRoles = await GetUserRoles(user);
            var productsRole = userRoles.FirstOrDefault(r => r.SystemId == systemId && CPiPermissions.Products.Contains(r.RoleId.ToLower()));
            await SetUserSystemRole(user.Id, systemId, roleId, productsRole);
        }

        public async Task<CPiRole> GetAttorneyRole()
        {
            var roleId = (await _settings.GetSetting()).AttorneyRole;
            var role = await CPiRoles.FirstOrDefaultAsync(r => r.Id == roleId);

            Guard.Against.ValueNotAllowed(role != null, "Role");
            return role;
        } 

        public async Task SetAttorneyRole(CPiUser user, bool canAccess, string systemId)
        {
            var role = await GetAttorneyRole();
            var userRoles = await GetUserRoles(user);
            var attorneyRole = userRoles.FirstOrDefault(r => r.SystemId == systemId && r.RoleId == role.Id);
            await SetUserSystemRole(user.Id, systemId, canAccess ? role.Id : "", attorneyRole);

            if (!canAccess)
            {
                await SetUploadRole(user, false, systemId);
                await SetPatentScoreRole(user, false, systemId);
            }
        }

        public async Task SetSoftDocketRole(CPiUser user, bool canModify, string systemId)
        {
            var roleId = "SoftDocket";
            var userRoles = await GetUserRoles(user);
            var softDocketRole = userRoles.FirstOrDefault(r => r.SystemId == systemId && r.RoleId == roleId);
            await SetUserSystemRole(user.Id, systemId, canModify ? roleId : "", softDocketRole);
        }

        public async Task SetRequestDocketRole(CPiUser user, bool canModify, string systemId)
        {
            var roleId = "RequestDocket";
            var userRoles = await GetUserRoles(user);
            var requestDocketRole = userRoles.FirstOrDefault(r => r.SystemId == systemId && r.RoleId == roleId);
            await SetUserSystemRole(user.Id, systemId, canModify ? roleId : "", requestDocketRole);
        }

        public async Task SetUploadRole(CPiUser user, bool canUpload, string systemId)
        {
            var roleId = "Upload";
            var userRoles = await GetUserRoles(user);
            var uploadRole = userRoles.FirstOrDefault(r => r.SystemId == systemId && r.RoleId == roleId);
            await SetUserSystemRole(user.Id, systemId, canUpload ? roleId : "", uploadRole);
        }

        public async Task<CPiRole> GetInventorRole()
        {
            var roleId = (await _settings.GetSetting()).InventorRole;
            var role = await CPiRoles.FirstOrDefaultAsync(r => r.Id == roleId);

            Guard.Against.ValueNotAllowed(role != null, "Role");
            return role;
        }

        public async Task SetInventorRole(CPiUser user, bool canAccess, string systemId)
        {
            var role = await GetInventorRole();
            var userRoles = await GetUserRoles(user);
            var inventorRole = userRoles.FirstOrDefault(r => r.SystemId == systemId && r.RoleId == role.Id);
            await SetUserSystemRole(user.Id, systemId, canAccess ? role.Id : "", inventorRole);
        }

        private async Task SetUserSystemRole(string userId, string systemId, string roleId, CPiUserSystemRole? currentRole)
        {
            if (!string.IsNullOrEmpty(roleId) && currentRole == null)
            {
                await AddUserSystemRole(new CPiUserSystemRole()
                {
                    UserId = userId,
                    SystemId = systemId,
                    RoleId = roleId
                });
            }
            else if (currentRole != null)
            {
                if (string.IsNullOrEmpty(roleId))
                    await DeleteUserSystemRole(currentRole);
                else
                {
                    currentRole.RoleId = roleId;
                    await UpdateUserSystemRole(currentRole);
                }
            }
        }

        public async Task SetCostEstimatorRole(CPiUser user, string systemId, string roleId)
        {
            var userRoles = await GetUserRoles(user);
            var costEstimatorRole = userRoles.FirstOrDefault(r => r.SystemId == systemId && CPiPermissions.CostEstimator.Contains(r.RoleId.ToLower()));
            await SetUserSystemRole(user.Id, systemId, roleId, costEstimatorRole);
        }

        public async Task SetGermanRemunerationRole(CPiUser user, string systemId, string roleId)
        {
            var userRoles = await GetUserRoles(user);
            var germanRemunerationRole = userRoles.FirstOrDefault(r => r.SystemId == systemId && CPiPermissions.GermanRemuneration.Contains(r.RoleId.ToLower()));
            await SetUserSystemRole(user.Id, systemId, roleId, germanRemunerationRole);
        }

        public async Task SetFrenchRemunerationRole(CPiUser user, string systemId, string roleId)
        {
            var userRoles = await GetUserRoles(user);
            var frenchRemunerationRole = userRoles.FirstOrDefault(r => r.SystemId == systemId && CPiPermissions.FrenchRemuneration.Contains(r.RoleId.ToLower()));
            await SetUserSystemRole(user.Id, systemId, roleId, frenchRemunerationRole);
        }

        public async Task SetPatentScoreRole(CPiUser user, bool canModify, string systemId)
        {
            var roleId = "PatentScoreModify";
            var userRoles = await GetUserRoles(user);
            var patentScoreRole = userRoles.FirstOrDefault(r => r.SystemId == systemId && r.RoleId == roleId);
            await SetUserSystemRole(user.Id, systemId, canModify ? roleId : "", patentScoreRole);
        }

        public async Task SetDocumentVerificationRole(CPiUser user, string systemId, string roleId)
        {
            var userRoles = await GetUserRoles(user);
            var documentVerificationRole = userRoles.FirstOrDefault(r => r.SystemId == systemId && CPiPermissions.DocumentVerification.Contains(r.RoleId.ToLower()));
            await SetUserSystemRole(user.Id, systemId, roleId, documentVerificationRole);
        }

        public async Task SetWorkflowRole(CPiUser user, string systemId, string roleId)
        {
            var userRoles = await GetUserRoles(user);
            var workflowRole = userRoles.FirstOrDefault(r => r.SystemId == systemId && CPiPermissions.Workflow.Contains(r.RoleId.ToLower()));
            await SetUserSystemRole(user.Id, systemId, roleId, workflowRole);
        }

        public async Task ResetSettings(CPiUser user)
        {
            var userId = user.Id;

            if (!user.IsAdmin)
            {
                var notificationSettings = await _settingManager.GetUserSetting<UserNotificationSettings>(userId);
                notificationSettings.ReceivePendingRegistrationNotification = false;
                notificationSettings.ReceiveTaskSchedulerNotification = false;
                await _settingManager.SaveUserSetting(userId, CPiSettings.UserNotificationSettings, JObject.FromObject(notificationSettings));

                var accountSettings = await _settingManager.GetUserSetting<UserAccountSettings>(userId);
                accountSettings.AllowHandleMyEPOCommunications = false;
                await _settingManager.SaveUserSetting(userId, CPiSettings.UserAccountSettings, JObject.FromObject(accountSettings));
            }

            // trade secret settings
            if (!CPiPermissions.CanHaveTSClearance.Contains(user.UserType) || !CPiPermissions.CanAccessTSReports.Contains(user.UserType))
            {
                var settings = await _settingManager.GetUserSetting<UserAccountSettings>(userId);

                if (!CPiPermissions.CanHaveTSClearance.Contains(user.UserType))
                {
                    settings.RestrictPatTradeSecret = true;
                    settings.RestrictDMSTradeSecret = true;
                }

                if (!CPiPermissions.CanAccessTSReports.Contains(user.UserType))
                {
                    settings.RestrictPatTradeSecretReports = true;
                    settings.RestrictDMSTradeSecretReports = true;
                }

                await _settingManager.SaveUserSetting(userId, CPiSettings.UserAccountSettings, JObject.FromObject(settings));
            }

            if (!await CanReceiveAMSNotifications(userId))
            {
                var settings = await _settingManager.GetUserSetting<UserNotificationSettings>(userId);
                settings.ReceiveAMSInstructionNotification = false;

                await _settingManager.SaveUserSetting(userId, CPiSettings.UserNotificationSettings, JObject.FromObject(settings));
            }

            if (!await CanReceiveRMSNotifications(userId))
            {
                var settings = await _settingManager.GetUserSetting<UserNotificationSettings>(userId);
                settings.ReceiveRMSInstructionNotification = false;

                await _settingManager.SaveUserSetting(userId, CPiSettings.UserNotificationSettings, JObject.FromObject(settings));
            }

            if (!await CanReceiveFFNotifications(userId))
            {
                var settings = await _settingManager.GetUserSetting<UserNotificationSettings>(userId);
                settings.ReceiveFFInstructionNotification = false;

                await _settingManager.SaveUserSetting(userId, CPiSettings.UserNotificationSettings, JObject.FromObject(settings));
            }

            if (!await CanReceiveDeDocketNotifications(userId))
            {
                var settings = await _settingManager.GetUserSetting<UserNotificationSettings>(userId);
                settings.ReceiveDeDocketInstructionNotification = false;

                await _settingManager.SaveUserSetting(userId, CPiSettings.UserNotificationSettings, JObject.FromObject(settings));
            }

            if (!(await UserHasSystemPermission(userId, SystemType.AMS, CPiPermissions.RegularUser)))
            {
                await SetAuxiliaryRole(user, SystemType.AMS, "");
                await SetProductsRole(user, SystemType.AMS, "");
                await SetCustomQueryRole(user, SystemType.AMS, "");

            }
            else if (_user.IsAMSIntegrated())
            {
                //use patent products role if integrated
                await SetProductsRole(user, SystemType.AMS, "");
                await SetCustomQueryRole(user, SystemType.AMS, "");
            }

            if (!(await UserHasSystemPermission(userId, SystemType.RMS, CPiPermissions.RegularUser)))
            {
                await SetAuxiliaryRole(user, SystemType.RMS, "");
            }

            if (!(await UserHasSystemPermission(userId, SystemType.ForeignFiling, CPiPermissions.RegularUser)))
            {
                await SetAuxiliaryRole(user, SystemType.ForeignFiling, "");
            }

            if (!(await UserHasSystemPermission(userId, SystemType.SearchRequest, CPiPermissions.RegularUser)))
            {
                await SetAuxiliaryRole(user, SystemType.SearchRequest, "");
                await SetWorkflowRole(user, SystemType.SearchRequest, "");
            }

            if (!(await UserHasSystemPermission(userId, SystemType.PatClearance, CPiPermissions.RegularUser)))
            {
                await SetAuxiliaryRole(user, SystemType.PatClearance, "");
                await SetWorkflowRole(user, SystemType.PatClearance, "");
            }

            if (!(await UserHasSystemPermission(userId, SystemType.DMS, CPiPermissions.RegularUser)))
            {
                await SetAuxiliaryRole(user, SystemType.DMS, "");
                await SetWorkflowRole(user, SystemType.DMS, "");
            }

            if (!(await UserHasSystemPermission(userId, SystemType.Patent, CPiPermissions.RegularUser)))
            {
                await SetAuxiliaryRole(user, SystemType.Patent, "");
                await SetLetterRole(user, SystemType.Patent, "");
                await SetCustomQueryRole(user, SystemType.Patent, "");
                await SetCountryLawRole(user, SystemType.Patent, "");
                await SetActionTypeRole(user, SystemType.Patent, "");
                await SetProductsRole(user, SystemType.Patent, "");
                await SetCostEstimatorRole(user, SystemType.Patent, "");
                await SetGermanRemunerationRole(user, SystemType.Patent, "");
                await SetFrenchRemunerationRole(user, SystemType.Patent, "");
                await SetDocumentVerificationRole(user, SystemType.Patent, "");
                await SetWorkflowRole(user, SystemType.Patent, "");
                await SetSoftDocketRole(user, false, SystemType.Patent);
                await SetRequestDocketRole(user, false, SystemType.Patent);
            }

            if (!(await UserHasSystemPermission(userId, SystemType.Trademark, CPiPermissions.RegularUser)))
            {
                await SetAuxiliaryRole(user, SystemType.Trademark, "");
                await SetLetterRole(user, SystemType.Trademark, "");
                await SetCustomQueryRole(user, SystemType.Trademark, "");
                await SetCountryLawRole(user, SystemType.Trademark, "");
                await SetActionTypeRole(user, SystemType.Trademark, "");
                await SetProductsRole(user, SystemType.Trademark, "");
                await SetCostEstimatorRole(user, SystemType.Trademark, "");
                await SetDocumentVerificationRole(user, SystemType.Trademark, "");
                await SetWorkflowRole(user, SystemType.Trademark, "");
                await SetSoftDocketRole(user, false, SystemType.Trademark);
                await SetRequestDocketRole(user, false, SystemType.Trademark);
            }

            if (!(await UserHasSystemPermission(userId, SystemType.GeneralMatter, CPiPermissions.RegularUser)))
            {
                await SetAuxiliaryRole(user, SystemType.GeneralMatter, "");
                await SetLetterRole(user, SystemType.GeneralMatter, "");
                await SetCustomQueryRole(user, SystemType.GeneralMatter, "");
                await SetActionTypeRole(user, SystemType.GeneralMatter, "");
                await SetProductsRole(user, SystemType.GeneralMatter, "");
                await SetCostEstimatorRole(user, SystemType.GeneralMatter, "");
                await SetDocumentVerificationRole(user, SystemType.GeneralMatter, "");
                await SetWorkflowRole(user, SystemType.GeneralMatter, "");
                await SetSoftDocketRole(user, false, SystemType.GeneralMatter);
                await SetRequestDocketRole(user, false, SystemType.GeneralMatter);
            }

            //only attorney usertype and remarks only users can have upload role
            if (user.UserType != CPiUserType.Attorney && !(await UserHasSystemPermission(userId, SystemType.Patent, CPiPermissions.CanHaveUploadRole)))
                await SetUploadRole(user, false, SystemType.Patent);
            if (user.UserType != CPiUserType.Attorney && !(await UserHasSystemPermission(userId, SystemType.Trademark, CPiPermissions.CanHaveUploadRole)))
                await SetUploadRole(user, false, SystemType.Trademark);
            if (user.UserType != CPiUserType.Attorney && !(await UserHasSystemPermission(userId, SystemType.GeneralMatter, CPiPermissions.CanHaveUploadRole)))
                await SetUploadRole(user, false, SystemType.GeneralMatter);

            if (!CPiPermissions.CanHavePatentScoreRole.Contains(user.UserType))
            {
                await SetPatentScoreRole(user, false, SystemType.Patent);
            }
        }

        public async Task<bool> CanReceiveAMSNotifications(string userId)
        {
            return await UserHasSystemPermission(userId, SystemType.AMS, CPiPermissions.CanReceiveInstructionsNotifications);
        }

        public async Task<bool> CanReceiveRMSNotifications(string userId)
        {
            return await UserHasSystemPermission(userId, SystemType.RMS, CPiPermissions.CanReceiveInstructionsNotifications);
        }

        public async Task<bool> CanReceiveFFNotifications(string userId)
        {
            return await UserHasSystemPermission(userId, SystemType.ForeignFiling, CPiPermissions.CanReceiveInstructionsNotifications);
        }

        public async Task<bool> CanReceiveDeDocketNotifications(string userId)
        {
            return await UserHasSystemPermission(userId, SystemType.Patent, CPiPermissions.CanReceiveInstructionsNotifications) ||
                   await UserHasSystemPermission(userId, SystemType.Trademark, CPiPermissions.CanReceiveInstructionsNotifications) ||
                   await UserHasSystemPermission(userId, SystemType.GeneralMatter, CPiPermissions.CanReceiveInstructionsNotifications);
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~CPiUserPermissionManager() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
