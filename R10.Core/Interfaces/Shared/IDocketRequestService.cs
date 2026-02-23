using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using R10.Core.DTOs;
using R10.Core.Entities;
using R10.Core.Entities.Patent;
using R10.Core.Entities.Trademark;
using R10.Core.Entities.GeneralMatter;

namespace R10.Core.Interfaces
{
    public interface IDocketRequestService
    {
        IQueryable<PatDocketRequest> PatDocketRequests { get; }
        IQueryable<PatDocketInvRequest> PatDocketInvRequests { get; }
        IQueryable<TmkDocketRequest> TmkDocketRequests { get; }
        IQueryable<GMDocketRequest> GMDocketRequests { get; }

        IQueryable<PatDocketRequestResp> PatDocketRequestResps { get; }        
        IQueryable<TmkDocketRequestResp> TmkDocketRequestResps { get; }
        IQueryable<GMDocketRequestResp> GMDocketRequestResps { get; }


        Task<CountryApplication?> GetApplication(int appId);
        Task<TmkTrademark?> GetTrademark(int tmkId);
        Task<GMMatter?> GetMatter(int matId);
        Task<Invention?> GetInvention(int invId);

        Task SavePatDocketRequest(PatDocketRequest docketRequest);
        Task SavePatDocketInvRequest(PatDocketInvRequest docketRequest);
        Task SaveTmkDocketRequest(TmkDocketRequest docketRequest);
        Task SaveGMDocketRequest(GMDocketRequest docketRequest);

        Task MarkPatDocketRequestsAsCompleted(List<int> reqIds, DateTime? completedDate);
        Task MarkTmkDocketRequestsAsCompleted(List<int> reqIds, DateTime? completedDate);
        Task MarkGMDocketRequestsAsCompleted(List<int> reqIds, DateTime? completedDate);


        Task DeletePatDocketRequests(List<int> reqIds);
        Task DeleteTmkDocketRequests(List<int> reqIds);
        Task DeleteGMDocketRequests(List<int> reqIds);

        Task UpdatePatDocketRequestResp(List<string> responsibleList, string userName, int reqId);
        Task UpdateTmkDocketRequestResp(List<string> responsibleList, string userName, int reqId);
        Task UpdateGMDocketRequestResp(List<string> responsibleList, string userName, int reqId);
    }
        
}
