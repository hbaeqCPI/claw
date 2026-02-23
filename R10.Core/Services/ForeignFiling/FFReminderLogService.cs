using R10.Core.Entities;
using R10.Core.Entities.ForeignFiling;
using R10.Core.Entities.Patent;
using R10.Core.Interfaces;
using R10.Core.Services.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;

namespace R10.Core.Services.ForeignFiling
{
    public class FFReminderLogService : ReminderLogService<PatDueDate, FFRemLogDue>, IReminderLogService<PatDueDate, FFRemLogDue>
    {
        public FFReminderLogService(ICPiDbContext cpiDbContext, ClaimsPrincipal user) : base(cpiDbContext, user)
        {
        }

        public override void AddRemLogDues(RemLog<PatDueDate, FFRemLogDue> remLog, IQueryable<PatDueDate> dueDateList)
        {
            //does not work with EF.Core 6
            //_cpiDbContext.GetRepository<FFRemLogDue>().Add(dueDateList.Select(d => new FFRemLogDue()
            //{
            //    RemId = remLog.RemId,
            //    DueId = d.DDId,
            //    Client = d.PatActionDue.CountryApplication.Invention.Client.ClientCode
            //}));

            remLog.RemLogDues = dueDateList.Select(d => new FFRemLogDue()
            {
                DueId = d.DDId,
                Client = d.PatActionDue.CountryApplication.Invention.Client.ClientCode
            }).ToList();
        }
    }
}
