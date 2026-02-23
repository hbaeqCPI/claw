using Microsoft.AspNetCore.Mvc;
using R10.Core.Entities;
using R10.Core.Entities.Patent;
using R10.Core.Identity;
using R10.Web.Areas.Shared.ViewModels;
using R10.Core.DTOs;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.Interfaces
{
    public interface IDelegationService
    {
        IQueryable<CPiGroup> GetGroups(string? system = null, int? caseId = null);
        IQueryable<CPiUser> GetUsers(string? system = null, int? caseId = null);
        List<DueDateDelegationViewModelDetail> GetAvaliableGroupAndUser(string? system = null, int? caseId = null);
        Task DelegationSetUp();
        List<DelegateUser> GetDelegateUsers(DelegationParameter parameter);
        void UpdateNotificationSent(DelegationParameter parameter);
    }
}
