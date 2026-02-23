using R10.Core.Entities;
using R10.Core.Entities.AMS;
using R10.Core.Helpers;
using R10.Core.Interfaces;
using R10.Core.Interfaces.AMS;
using R10.Core.Services.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace R10.Core.Services.AMS
{
    public class AMSReminderLogService : ReminderLogService<AMSDue, AMSRemLogDue>, IReminderLogService<AMSDue, AMSRemLogDue>
    {
        public AMSReminderLogService(ICPiDbContext cpiDbContext, ClaimsPrincipal user) : base(cpiDbContext, user)
        {
        }

        public override void AddRemLogDues(RemLog<AMSDue, AMSRemLogDue> remLog, IQueryable<AMSDue> dueDateList)
        {
            //does not work with EF.Core 6
            //_cpiDbContext.GetRepository<AMSRemLogDue>().Add(dueDateList.Select(d => new AMSRemLogDue()
            //{
            //    RemId = remLog.RemId,
            //    DueId = d.DueID,
            //    CPIClient = !_user.IsAMSIntegrated() || d.AMSMain.CountryApplication == null ? d.AMSMain.CPIClient : d.AMSMain.CountryApplication.Invention.Client.ClientCode,
            //    CPIAttorney = !_user.IsAMSIntegrated() || d.AMSMain.CountryApplication == null ? d.AMSMain.CPIAttorney : d.AMSMain.CountryApplication.Invention.Attorney1.AttorneyCode,
            //}));

            remLog.RemLogDues = dueDateList.Select(d => new AMSRemLogDue()
            {
                DueId = d.DueID,
                CPIClient = !_user.IsAMSIntegrated() || d.AMSMain.CountryApplication == null ? d.AMSMain.CPIClient : d.AMSMain.CountryApplication.Invention.Client.ClientCode,
                CPIAttorney = !_user.IsAMSIntegrated() || d.AMSMain.CountryApplication == null ? d.AMSMain.CPIAttorney : d.AMSMain.CountryApplication.Invention.Attorney1.AttorneyCode,
            }).ToList();
        }
    }
}
