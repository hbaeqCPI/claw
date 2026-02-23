using R10.Core.Interfaces;
using System.Threading.Tasks;
using System.Collections.Generic;
using R10.Core.Entities;
using System.Linq;
using R10.Core.Identity;
using System.Linq.Expressions;
using System;
using System.Data;
using R10.Core.DTOs;
using Microsoft.EntityFrameworkCore;
using System.Collections;
using R10.Core.Entities.GeneralMatter;
using R10.Core.Entities.Patent;
using R10.Core.Entities.Trademark;
using System.Text.RegularExpressions;
using R10.Core.Interfaces.Patent;
using System.Security.Claims;
using R10.Core.Helpers;

namespace R10.Core.Services
{
    public class DocketRequestService : IDocketRequestService
    {
        private readonly IApplicationDbContext _repository;
        private readonly ICountryApplicationService _countryAppService;
        private readonly IInventionService _inventionService;
        private readonly ITmkTrademarkService _trademarkService;
        private readonly IGMMatterService _matterService;
        private readonly ClaimsPrincipal _user;

        public DocketRequestService(IApplicationDbContext repository, ClaimsPrincipal user, 
            ICountryApplicationService countryAppService,
            IInventionService inventionService,
            ITmkTrademarkService trademarkService,
            IGMMatterService matterService)
        {
            _repository = repository;
            _user = user;
            _countryAppService = countryAppService;
            _inventionService = inventionService;
            _trademarkService = trademarkService;
            _matterService = matterService;
        }

        public IQueryable<PatDocketRequest> PatDocketRequests
        {
            get
            {
                var patDocketRequests = _repository.PatDocketRequests.AsQueryable();
                if (_user.HasEntityFilter() || _user.HasRespOfficeFilter(SystemType.Patent) || _user.RestrictExportControl() || !_user.CanAccessPatTradeSecret())
                    patDocketRequests = patDocketRequests.Where(a => _countryAppService.CountryApplications.Any(ca => ca.AppId == a.AppId));

                return patDocketRequests;
            }
        }

        public IQueryable<PatDocketInvRequest> PatDocketInvRequests
        {
            get
            {
                var patDocketInvRequests = _repository.PatDocketInvRequests.AsQueryable();

                if (_user.HasEntityFilter() || _user.HasRespOfficeFilter(SystemType.Patent) || _user.RestrictExportControl())
                    patDocketInvRequests = patDocketInvRequests.Where(a => _inventionService.Inventions.Any(inv => inv.InvId == a.InvId));

                return patDocketInvRequests;
            }
        }

        public IQueryable<TmkDocketRequest> TmkDocketRequests
        {
            get
            {
                var tmkDocketRequests = _repository.TmkDocketRequests.AsQueryable();
                if (_user.HasEntityFilter() || _user.HasRespOfficeFilter(SystemType.Trademark))
                    tmkDocketRequests = tmkDocketRequests.Where(a => _trademarkService.TmkTrademarks.Any(tmk => tmk.TmkId == a.TmkId));

                return tmkDocketRequests;
            }
        }

        public IQueryable<GMDocketRequest> GMDocketRequests
        {
            get
            {
                var gmDocketRequests = _repository.GMDocketRequests.AsQueryable();
                if (_user.HasEntityFilter() || _user.HasRespOfficeFilter(SystemType.GeneralMatter))
                    gmDocketRequests = gmDocketRequests.Where(a => _matterService.QueryableList.Any(gm => gm.MatId == a.MatId));

                return gmDocketRequests;
            }
        }

        public IQueryable<PatDocketRequestResp> PatDocketRequestResps
        {
            get
            {
                var patDocketRequestResps = _repository.PatDocketRequestResps.AsQueryable();
                if (_user.HasEntityFilter() || _user.HasRespOfficeFilter(SystemType.Patent) || _user.RestrictExportControl() || !_user.CanAccessPatTradeSecret())
                    patDocketRequestResps = patDocketRequestResps.Where(a => a.PatDocketRequest != null && _countryAppService.CountryApplications.Any(ca => ca.AppId == a.PatDocketRequest.AppId));

                return patDocketRequestResps;
            }
        }
        public IQueryable<TmkDocketRequestResp> TmkDocketRequestResps
        {
            get
            {
                var tmkDocketRequestResps = _repository.TmkDocketRequestResps.AsQueryable();
                if (_user.HasEntityFilter() || _user.HasRespOfficeFilter(SystemType.Trademark))
                    tmkDocketRequestResps = tmkDocketRequestResps.Where(a => a.TmkDocketRequest != null && _trademarkService.TmkTrademarks.Any(tmk => tmk.TmkId == a.TmkDocketRequest.TmkId));

                return tmkDocketRequestResps;
            }
        }
        public IQueryable<GMDocketRequestResp> GMDocketRequestResps
        {
            get
            {
                var gmDocketRequestResps = _repository.GMDocketRequestResps.AsQueryable();
                if (_user.HasEntityFilter() || _user.HasRespOfficeFilter(SystemType.GeneralMatter))
                    gmDocketRequestResps = gmDocketRequestResps.Where(a => a.GMDocketRequest != null && _matterService.QueryableList.Any(gm => gm.MatId == a.GMDocketRequest.MatId));

                return gmDocketRequestResps;
            }
        }


        public async Task<CountryApplication?> GetApplication(int appId)
        {
            return await _repository.CountryApplications
                .Include(ca => ca.Invention)
                .ThenInclude(i => i.Attorney1)
                .Include(ca => ca.Invention)
                .ThenInclude(i => i.Attorney2)
                .Include(ca => ca.Invention)
                .ThenInclude(i => i.Attorney3)
                .Include(ca => ca.Invention)
                .ThenInclude(i => i.Attorney4)
                .Include(ca => ca.Invention)
                .ThenInclude(i => i.Attorney5)
                .FirstOrDefaultAsync(r => r.AppId == appId);
        }

        public async Task<TmkTrademark?> GetTrademark(int tmkId)
        {
            return await _repository.TmkTrademarks
              .Include(tmk => tmk.Attorney1)
              .Include(tmk => tmk.Attorney2)
              .Include(tmk => tmk.Attorney3)
              .Include(tmk => tmk.Attorney4)
              .Include(tmk => tmk.Attorney5)
              .FirstOrDefaultAsync(r => r.TmkId == tmkId);
        }
        public async Task<GMMatter?> GetMatter(int matId)
        {
            return await _repository.GMMatters.FirstOrDefaultAsync(r => r.MatId == matId);
        }

        public async Task<Invention?> GetInvention(int invId)
        {
            return await _repository.Inventions
                .Include(i => i.Attorney1)
                .Include(i => i.Attorney2)
                .Include(i => i.Attorney3)
                .Include(i => i.Attorney4)
                .Include(i => i.Attorney5)
                .FirstOrDefaultAsync(r => r.InvId == invId);
        }


        public async Task SavePatDocketRequest(PatDocketRequest docketRequest)
        {
            _repository.PatDocketRequests.Add(docketRequest);
            await _repository.SaveChangesAsync();
        }

        public async Task SavePatDocketInvRequest(PatDocketInvRequest docketRequest)
        {
            _repository.PatDocketInvRequests.Add(docketRequest);
            await _repository.SaveChangesAsync();
        }

        public async Task SaveTmkDocketRequest(TmkDocketRequest docketRequest)
        {
            _repository.TmkDocketRequests.Add(docketRequest);
            await _repository.SaveChangesAsync();
        }

        public async Task SaveGMDocketRequest(GMDocketRequest docketRequest)
        {
            _repository.GMDocketRequests.Add(docketRequest);
            await _repository.SaveChangesAsync();
        }


        public async Task MarkPatDocketRequestsAsCompleted(List<int> reqIds, DateTime? completedDate)
        {
            var requestDockets = await _repository.PatDocketRequests.Where(d => reqIds.Contains(d.ReqId)).ToListAsync();
                        
            if (requestDockets.Any())
            {
                var userName = _user.GetUserName();
                requestDockets.ForEach(d => { d.CompletedBy = userName; d.CompletedDate = completedDate; });
                await _repository.SaveChangesAsync();
            }            
        }

        public async Task MarkTmkDocketRequestsAsCompleted(List<int> reqIds, DateTime? completedDate)
        {
            var requestDockets = await _repository.TmkDocketRequests.Where(d => reqIds.Contains(d.ReqId)).ToListAsync();
                        
            if (requestDockets.Any())
            {
                var userName = _user.GetUserName();
                requestDockets.ForEach(d => { d.CompletedBy = userName; d.CompletedDate = completedDate; });
                await _repository.SaveChangesAsync();
            }            
        }

        public async Task MarkGMDocketRequestsAsCompleted(List<int> reqIds, DateTime? completedDate)
        {
            var requestDockets = await _repository.GMDocketRequests.Where(d => reqIds.Contains(d.ReqId)).ToListAsync();
                        
            if (requestDockets.Any())
            {
                var userName = _user.GetUserName();
                requestDockets.ForEach(d => { d.CompletedBy = userName; d.CompletedDate = completedDate; });
                await _repository.SaveChangesAsync();
            }            
        }


        public async Task DeletePatDocketRequests(List<int> reqIds)
        {
            await _repository.PatDocketRequests.Where(d => reqIds.Contains(d.ReqId)).ExecuteDeleteAsync();
        }

        public async Task DeleteTmkDocketRequests(List<int> reqIds)
        {
            await _repository.TmkDocketRequests.Where(d => reqIds.Contains(d.ReqId)).ExecuteDeleteAsync();
        }

        public async Task DeleteGMDocketRequests(List<int> reqIds)
        {
            await _repository.GMDocketRequests.Where(d => reqIds.Contains(d.ReqId)).ExecuteDeleteAsync();
        }

        public async Task UpdatePatDocketRequestResp(List<string> responsibleList, string userName, int reqId)
        {
            var idList = responsibleList.Select(d => 
                                        { 
                                            int intVal; string strVal = d;
                                            bool isInt = int.TryParse(d, out intVal);                                             
                                            return new { intVal, strVal, isInt }; 
                                        })                                  
                                        .ToList();
            
            var existingResps = await PatDocketRequestResps.Where(d => d.ReqId == reqId).ToListAsync();

            DateTime today = DateTime.Now;  
            if (idList.Any())
            {
                var selectedUsers = idList.Where(d => !d.isInt)
                    .Select(d => new PatDocketRequestResp
                    {
                        ReqId = reqId,                        
                        UserId = d.strVal,
                        GroupId = null,
                    }).ToList();

                var selectedGroups = idList.Where(d => d.isInt)
                    .Select(d => new PatDocketRequestResp
                    {
                        ReqId = reqId,                        
                        UserId = null,
                        GroupId = d.intVal,
                    }).ToList();

                //Get deleted users/groups - existing users/groups not in selected users/groups
                var deleted = existingResps.Where(d => (!string.IsNullOrEmpty(d.UserId) && !selectedUsers.Any(s => s.UserId == d.UserId)) 
                                                    || (d.GroupId > 0 && !selectedGroups.Any(s => s.GroupId == d.GroupId))
                                            ).ToList();                
                
                //Get added users/groups - selected users/groups not in existing users/groups
                var added = selectedUsers.Where(d => !string.IsNullOrEmpty(d.UserId) && !existingResps.Any(s => s.UserId == d.UserId)).ToList();
                added.AddRange(selectedGroups.Where(d => d.GroupId > 0 && !existingResps.Any(s => s.GroupId == d.GroupId)).ToList());

                if (added.Any())
                {
                    added.ForEach(d => { d.CreatedBy = userName; d.UpdatedBy = userName; d.DateCreated = today; d.LastUpdate = today; });
                    _repository.PatDocketRequestResps.AddRange(added);

                    ////Log new added                                   
                    //_repository.PatDocketRequestRespLogs.Add(new PatDocketRequestRespLog()
                    //{
                    //    DocId = docId,                        
                    //    UserIds = string.Join("|", added.Where(d => !string.IsNullOrEmpty(d.UserId)).Select(d => d.UserId).ToList()),
                    //    GroupIds = string.Join("|", added.Where(d => d.GroupId > 0).Select(d => d.GroupId.ToString()).ToList()),
                    //    RespType = DocRespLogType.Docketing,
                    //    TransxType = DocRespLogTransxType.Update,
                    //    CreatedBy = userName,
                    //    UpdatedBy = userName,
                    //    DateCreated = today,
                    //    LastUpdate = today
                    //});
                }

                if (deleted.Any())
                {
                    _repository.PatDocketRequestResps.RemoveRange(deleted);

                    ////Log deleted 
                    //_repository.PatDocketRequestRespLogs.Add(new PatDocketRequestRespLog()
                    //{
                    //    DocId = docId,                        
                    //    UserIds = string.Join("|", deleted.Where(d => !string.IsNullOrEmpty(d.UserId)).Select(d => d.UserId).ToList()),
                    //    GroupIds = string.Join("|", deleted.Where(d => d.GroupId > 0).Select(d => d.GroupId.ToString()).ToList()),
                    //    RespType = DocRespLogType.Docketing,
                    //    TransxType = DocRespLogTransxType.Delete,
                    //    CreatedBy = userName,
                    //    UpdatedBy = userName,
                    //    DateCreated = today,
                    //    LastUpdate = today
                    //});
                }                    

                await _repository.SaveChangesAsync();                                
            }
        }
        public async Task UpdateTmkDocketRequestResp(List<string> responsibleList, string userName, int reqId)
        {
            var idList = responsibleList.Select(d => 
                                        { 
                                            int intVal; string strVal = d;
                                            bool isInt = int.TryParse(d, out intVal);                                             
                                            return new { intVal, strVal, isInt }; 
                                        })                                  
                                        .ToList();
            
            var existingResps = await TmkDocketRequestResps.Where(d => d.ReqId == reqId).ToListAsync();

            DateTime today = DateTime.Now;  
            if (idList.Any())
            {
                var selectedUsers = idList.Where(d => !d.isInt)
                    .Select(d => new TmkDocketRequestResp
                    {
                        ReqId = reqId,                        
                        UserId = d.strVal,
                        GroupId = null,
                    }).ToList();

                var selectedGroups = idList.Where(d => d.isInt)
                    .Select(d => new TmkDocketRequestResp
                    {
                        ReqId = reqId,                        
                        UserId = null,
                        GroupId = d.intVal,
                    }).ToList();

                //Get deleted users/groups - existing users/groups not in selected users/groups
                var deleted = existingResps.Where(d => (!string.IsNullOrEmpty(d.UserId) && !selectedUsers.Any(s => s.UserId == d.UserId)) 
                                                    || (d.GroupId > 0 && !selectedGroups.Any(s => s.GroupId == d.GroupId))
                                            ).ToList();                
                
                //Get added users/groups - selected users/groups not in existing users/groups
                var added = selectedUsers.Where(d => !string.IsNullOrEmpty(d.UserId) && !existingResps.Any(s => s.UserId == d.UserId)).ToList();
                added.AddRange(selectedGroups.Where(d => d.GroupId > 0 && !existingResps.Any(s => s.GroupId == d.GroupId)).ToList());

                if (added.Any())
                {
                    added.ForEach(d => { d.CreatedBy = userName; d.UpdatedBy = userName; d.DateCreated = today; d.LastUpdate = today; });
                    _repository.TmkDocketRequestResps.AddRange(added);

                    ////Log new added                                   
                    //_repository.TmkDocketRequestRespLogs.Add(new TmkDocketRequestRespLog()
                    //{
                    //    DocId = docId,                        
                    //    UserIds = string.Join("|", added.Where(d => !string.IsNullOrEmpty(d.UserId)).Select(d => d.UserId).ToList()),
                    //    GroupIds = string.Join("|", added.Where(d => d.GroupId > 0).Select(d => d.GroupId.ToString()).ToList()),
                    //    RespType = DocRespLogType.Docketing,
                    //    TransxType = DocRespLogTransxType.Update,
                    //    CreatedBy = userName,
                    //    UpdatedBy = userName,
                    //    DateCreated = today,
                    //    LastUpdate = today
                    //});
                }

                if (deleted.Any())
                {
                    _repository.TmkDocketRequestResps.RemoveRange(deleted);

                    ////Log deleted 
                    //_repository.TmkDocketRequestRespLogs.Add(new TmkDocketRequestRespLog()
                    //{
                    //    DocId = docId,                        
                    //    UserIds = string.Join("|", deleted.Where(d => !string.IsNullOrEmpty(d.UserId)).Select(d => d.UserId).ToList()),
                    //    GroupIds = string.Join("|", deleted.Where(d => d.GroupId > 0).Select(d => d.GroupId.ToString()).ToList()),
                    //    RespType = DocRespLogType.Docketing,
                    //    TransxType = DocRespLogTransxType.Delete,
                    //    CreatedBy = userName,
                    //    UpdatedBy = userName,
                    //    DateCreated = today,
                    //    LastUpdate = today
                    //});
                }                    

                await _repository.SaveChangesAsync();                                
            }
        }
        public async Task UpdateGMDocketRequestResp(List<string> responsibleList, string userName, int reqId)
        {
            var idList = responsibleList.Select(d => 
                                        { 
                                            int intVal; string strVal = d;
                                            bool isInt = int.TryParse(d, out intVal);                                             
                                            return new { intVal, strVal, isInt }; 
                                        })                                  
                                        .ToList();
            
            var existingResps = await GMDocketRequestResps.Where(d => d.ReqId == reqId).ToListAsync();

            DateTime today = DateTime.Now;  
            if (idList.Any())
            {
                var selectedUsers = idList.Where(d => !d.isInt)
                    .Select(d => new GMDocketRequestResp
                    {
                        ReqId = reqId,                        
                        UserId = d.strVal,
                        GroupId = null,
                    }).ToList();

                var selectedGroups = idList.Where(d => d.isInt)
                    .Select(d => new GMDocketRequestResp
                    {
                        ReqId = reqId,                        
                        UserId = null,
                        GroupId = d.intVal,
                    }).ToList();

                //Get deleted users/groups - existing users/groups not in selected users/groups
                var deleted = existingResps.Where(d => (!string.IsNullOrEmpty(d.UserId) && !selectedUsers.Any(s => s.UserId == d.UserId)) 
                                                    || (d.GroupId > 0 && !selectedGroups.Any(s => s.GroupId == d.GroupId))
                                            ).ToList();                
                
                //Get added users/groups - selected users/groups not in existing users/groups
                var added = selectedUsers.Where(d => !string.IsNullOrEmpty(d.UserId) && !existingResps.Any(s => s.UserId == d.UserId)).ToList();
                added.AddRange(selectedGroups.Where(d => d.GroupId > 0 && !existingResps.Any(s => s.GroupId == d.GroupId)).ToList());

                if (added.Any())
                {
                    added.ForEach(d => { d.CreatedBy = userName; d.UpdatedBy = userName; d.DateCreated = today; d.LastUpdate = today; });
                    _repository.GMDocketRequestResps.AddRange(added);

                    ////Log new added                                   
                    //_repository.GMDocketRequestRespLogs.Add(new GMDocketRequestRespLog()
                    //{
                    //    DocId = docId,                        
                    //    UserIds = string.Join("|", added.Where(d => !string.IsNullOrEmpty(d.UserId)).Select(d => d.UserId).ToList()),
                    //    GroupIds = string.Join("|", added.Where(d => d.GroupId > 0).Select(d => d.GroupId.ToString()).ToList()),
                    //    RespType = DocRespLogType.Docketing,
                    //    TransxType = DocRespLogTransxType.Update,
                    //    CreatedBy = userName,
                    //    UpdatedBy = userName,
                    //    DateCreated = today,
                    //    LastUpdate = today
                    //});
                }

                if (deleted.Any())
                {
                    _repository.GMDocketRequestResps.RemoveRange(deleted);

                    ////Log deleted 
                    //_repository.GMDocketRequestRespLogs.Add(new GMDocketRequestRespLog()
                    //{
                    //    DocId = docId,                        
                    //    UserIds = string.Join("|", deleted.Where(d => !string.IsNullOrEmpty(d.UserId)).Select(d => d.UserId).ToList()),
                    //    GroupIds = string.Join("|", deleted.Where(d => d.GroupId > 0).Select(d => d.GroupId.ToString()).ToList()),
                    //    RespType = DocRespLogType.Docketing,
                    //    TransxType = DocRespLogTransxType.Delete,
                    //    CreatedBy = userName,
                    //    UpdatedBy = userName,
                    //    DateCreated = today,
                    //    LastUpdate = today
                    //});
                }                    

                await _repository.SaveChangesAsync();                                
            }
        }
    }
}
