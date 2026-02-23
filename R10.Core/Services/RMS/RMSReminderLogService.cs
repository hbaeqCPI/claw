using R10.Core.Entities;
using R10.Core.Entities.RMS;
using R10.Core.Entities.Trademark;
using R10.Core.Interfaces;
using R10.Core.Services.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;

namespace R10.Core.Services.RMS
{
    public class RMSReminderLogService : ReminderLogService<TmkDueDate, RMSRemLogDue>, IReminderLogService<TmkDueDate, RMSRemLogDue>
    {
        public RMSReminderLogService(ICPiDbContext cpiDbContext, ClaimsPrincipal user) : base(cpiDbContext, user)
        {
        }

        public override void AddRemLogDues(RemLog<TmkDueDate, RMSRemLogDue> remLog, IQueryable<TmkDueDate> dueDateList)
        {
            //does not work with EF.Core 6
            //_cpiDbContext.GetRepository<RMSRemLogDue>().Add(dueDateList.Select(d => new RMSRemLogDue()
            //{
            //    RemId = remLog.RemId,
            //    DueId = d.DDId,
            //    Client = d.TmkActionDue.TmkTrademark.Client.ClientCode
            //}));

            remLog.RemLogDues = dueDateList.Select(d => new RMSRemLogDue()
            {
                DueId = d.DDId,
                Client = d.TmkActionDue.TmkTrademark.Client.ClientCode,
                Agent = d.TmkActionDue.TmkTrademark.Agent.AgentCode
            }).ToList();
        }
    }
}
