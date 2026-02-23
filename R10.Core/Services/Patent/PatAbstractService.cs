using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using R10.Core.Entities.Patent;
using R10.Core.Entities.Shared;
using R10.Core.Exceptions;
using R10.Core.Helpers;
using R10.Core.Interfaces;
using R10.Core.Interfaces.Patent;
using R10.Core.Interfaces.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace R10.Core.Services.Patent
{
    public class PatAbstractService : PatInventionChildService<PatAbstract>, IPatAbstractService
    {
        private readonly ITradeSecretService _tradeSecretService;

        public PatAbstractService(ICPiDbContext cpiDbContext, 
            ClaimsPrincipal user, 
            IInventionService inventionService,
            ITradeSecretService tradeSecretService) : base(cpiDbContext, user, inventionService)
        {
            _tradeSecretService = tradeSecretService;
        }

        public async Task<List<PatAbstract>> GetPatAbstracts(int invId)
        {
            if (_user.CanAccessPatTradeSecret())
            {
                var tsData = await QueryableList.Include(pa => pa.Invention == null ? null : pa.Invention.TradeSecretRequests).Where(pa => pa.InvId == invId).ToListAsync();

                // show trade secret if last request is cleared
                foreach (var item in tsData.Where(pa => pa.Invention != null && (pa.Invention.IsTradeSecret ?? false) && pa.TradeSecret != null &&
                        pa.Invention.TradeSecretRequests != null))
                {
                    var tsRequest = item.Invention?.TradeSecretRequests?.Where(ts => ts.UserId == _user.GetUserIdentifier()).OrderByDescending(ts => ts.RequestDate).FirstOrDefault();

                    if (tsRequest != null && tsRequest.IsCleared && item.TradeSecret != null)
                    {
                        item.RestoreTradeSecret(item.TradeSecret, true);
                        await _tradeSecretService.LogActivity(TradeSecretScreen.Abstract, TradeSecretScreen.Abstract, item.AbstractId, TradeSecretActivityCode.View, tsRequest.RequestId);
                    }
                    else
                    {
                        // log redacted view
                        await _tradeSecretService.LogActivity(TradeSecretScreen.Abstract, TradeSecretScreen.Abstract, item.AbstractId, TradeSecretActivityCode.RedactedView, 0);
                    }
                }

                return tsData;
            }
            
            return await QueryableList.Where(pa => pa.InvId == invId).ToListAsync();
        }

        public override async Task<bool> Update(object key, string userName, IEnumerable<PatAbstract> updated, IEnumerable<PatAbstract> added, IEnumerable<PatAbstract> deleted)
        {
            var tsAdded = new List<string>(); //use language since abstractId == 0
            var tsUpdated = new List<int>();
            var tsDeleted = new List<int>();
            var tsRequestId = 0;
            var current = new List<PatAbstract>();

            if (_user.CanAccessPatTradeSecret())
            {
                var invId = (int)key;
                var isTradeSecret = await _inventionService.QueryableList.Where(i => i.InvId == invId).Select(i => i.IsTradeSecret).SingleOrDefaultAsync();
                if (isTradeSecret ?? false)
                {
                    var tsRequest = await _tradeSecretService.GetUserRequest(_tradeSecretService.CreateLocator(TradeSecretScreen.Invention, invId));
                    var isTSCleared = tsRequest?.IsCleared ?? false;

                    tsRequestId = tsRequest?.RequestId ?? 0;

                    //only cleared fullmodify users can edit trade secret fields
                    if (added.Any() || updated.Any())
                        Guard.Against.UnAuthorizedAccess(_user.CanEditPatTradeSecretFields() && isTSCleared);

                    //only cleared candelete users can delete trade secret fields
                    if (deleted.Any())
                        Guard.Against.UnAuthorizedAccess(_user.CanDeletePatTradeSecretFields() && isTSCleared);

                    current = await QueryableList.Where(a => a.InvId == invId).ToListAsync();

                    foreach (var patAbstract in added)
                    {
                        patAbstract.TradeSecret = patAbstract.CreateTradeSecret(new AbstractTradeSecret());
                        tsAdded.Add(patAbstract.LanguageName);
                    }

                    foreach (var patAbstract in updated)
                    {
                        patAbstract.TradeSecret = patAbstract.CreateTradeSecret(patAbstract.TradeSecret ?? new AbstractTradeSecret());
                        tsUpdated.Add(patAbstract.AbstractId);
                    }

                    foreach (var patAbstract in deleted)
                    {
                        tsDeleted.Add(patAbstract.AbstractId);
                    }
                }
            }

            var retVal = await base.Update(key, userName, updated, added, deleted);

            foreach (var languageName in tsAdded)
            {
                var newAbstract = added.SingleOrDefault(a => a.LanguageName == languageName);
                if (newAbstract != null)
                {
                    var auditLogs = CreateAuditLogs(null, newAbstract);
                    _tradeSecretService.CreateActivity(TradeSecretScreen.Abstract, TradeSecretScreen.Abstract, newAbstract.AbstractId, TradeSecretActivityCode.Create, tsRequestId, auditLogs);
                    await _cpiDbContext.SaveChangesAsync();
                }
            }

            foreach (var abstractId in tsUpdated)
            {
                var auditLogs = CreateAuditLogs(current.SingleOrDefault(a => a.AbstractId == abstractId), updated.SingleOrDefault(a => a.AbstractId == abstractId));
                _tradeSecretService.CreateActivity(TradeSecretScreen.Abstract, TradeSecretScreen.Abstract, abstractId, TradeSecretActivityCode.Update, tsRequestId, auditLogs);
                await _cpiDbContext.SaveChangesAsync();
            }

            foreach (var abstractId in tsDeleted)
            {
                var auditLogs = CreateAuditLogs(current.SingleOrDefault(a => a.AbstractId == abstractId), null);
                _tradeSecretService.CreateActivity(TradeSecretScreen.Abstract, TradeSecretScreen.Abstract, abstractId, TradeSecretActivityCode.Delete, tsRequestId, auditLogs);
                await _cpiDbContext.SaveChangesAsync();
            }

            return retVal;
        }

        private Dictionary<string, string?[]>? CreateAuditLogs(PatAbstract? oldValues, PatAbstract? newValues)
        {
            var auditLogs = new Dictionary<string, string?[]>();

            if (newValues?.TradeSecret?.Abstract != oldValues?.TradeSecret?.Abstract)
                auditLogs?.Add("Abstract", [oldValues?.TradeSecret?.Abstract, newValues?.TradeSecret?.Abstract]);

            return auditLogs;
        }
    }
}
