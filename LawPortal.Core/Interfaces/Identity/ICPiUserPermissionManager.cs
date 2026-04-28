using Microsoft.AspNetCore.Identity;
using LawPortal.Core.DTOs;
using LawPortal.Core.Entities;
using LawPortal.Core.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LawPortal.Core.Interfaces
{
    public interface ICPiUserPermissionManager : IDisposable
    {
        IQueryable<CPiRole> CPiRoles { get; }
        IQueryable<CPiUserEntityFilter> CPiUserEntityFilters { get; }
        IQueryable<CPiUserSystemRole> CPiUserSystemRoles { get; }

        Task<IQueryable> AvailableSystems(string userId, string systemId);
        Task<IQueryable> AvailableRoles(string systemId);
        Task<IQueryable> AvailableRespOffices(string systemId);

        IQueryable<EntityFilterDTO> AvailableEntityFilter(CPiEntityType entityType, string entity, string userId);
        IQueryable<EntityFilterDTO> UserEntityFilter(CPiEntityType entityType, string userId);

        Task<IdentityResult> AddEntityFilters(string userId, List<int> entityList);
        Task<IdentityResult> RemoveEntityFilters(string userId, List<int> entityList);
        Task<IdentityResult> RemoveEntityFilters(string userId);
        
        Task<List<CPiUserSystemRole>> GetUserRoles(CPiUser user);

        Task<IdentityResult> AddRole(CPiUserSystemRole userSystemRole);

        Task<IdentityResult> UpdateRole(CPiUserSystemRole userSystemRole);
        Task<IdentityResult> RemoveRole(CPiUserSystemRole userSystemRole);
        Task<IdentityResult> CopyUserRoles(CPiUser userFrom, CPiUser userTo);

        Task<bool> UserHasSystemPermission(string userId, string systemId, List<string> roleIds);

        Task LinkEntity(CPiUser user, int entityId, CPiEntityType entityType);
        Task SetReviewerRole(CPiUser user, bool isReviewer);
        Task SetPreviewerRole(CPiUser user, bool isPreviewer);
        Task SetClearanceReviewerRole(CPiUser user, bool isReviewer);
        Task SetPatClearanceReviewerRole(CPiUser user, bool isReviewer);
        Task SetDecisionMakerRole(CPiUser user, bool isDecisionMaker, string systemId);
        Task SetAuxiliaryRole(CPiUser user, string systemId, string roleId);
        Task SetCountryLawRole(CPiUser user, string systemId, string roleId);
        Task SetActionTypeRole(CPiUser user, string systemId, string roleId);
        Task SetLetterRole(CPiUser user, string systemId, string roleId);
        Task SetCustomQueryRole(CPiUser user, string systemId, string roleId);
        Task SetProductsRole(CPiUser user, string systemId, string roleId);
        Task<CPiRole> GetAttorneyRole();
        Task SetAttorneyRole(CPiUser user, bool canAccess, string systemId);
        Task SetSoftDocketRole(CPiUser user, bool canModify, string systemId);
        Task SetRequestDocketRole(CPiUser user, bool canModify, string systemId);
        Task SetUploadRole(CPiUser user, bool canUpload, string systemId);
        Task<CPiRole> GetInventorRole();
        Task SetInventorRole(CPiUser user, bool canAccess, string systemId);
        Task SetCostEstimatorRole(CPiUser user, string systemId, string roleId);
        Task SetGermanRemunerationRole(CPiUser user, string systemId, string roleId);
        Task SetFrenchRemunerationRole(CPiUser user, string systemId, string roleId);
        Task SetPatentScoreRole(CPiUser user, bool canModify, string systemId);
        Task SetDocumentVerificationRole(CPiUser user, string systemId, string roleId);
        Task SetWorkflowRole(CPiUser user, string systemId, string roleId);

        Task ResetSettings(CPiUser user);
        Task<bool> CanReceiveAMSNotifications(string userId);
        Task<bool> CanReceiveRMSNotifications(string userId);
        Task<bool> CanReceiveFFNotifications(string userId);
        Task<bool> CanReceiveDeDocketNotifications(string userId);
    }
}
