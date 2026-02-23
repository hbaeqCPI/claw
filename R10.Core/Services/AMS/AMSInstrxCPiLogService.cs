using Microsoft.EntityFrameworkCore;
using R10.Core.Entities.AMS;
using R10.Core.Helpers;
using R10.Core.Interfaces;
using R10.Core.Interfaces.AMS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace R10.Core.Services.AMS
{
    public class AMSInstrxCPiLogService : IAMSInstrxCPiLogService
    {
        protected readonly ICPiDbContext _cpiDbContext;
        protected readonly ClaimsPrincipal _user;

        public AMSInstrxCPiLogService(ICPiDbContext cpiDbContext, ClaimsPrincipal user)
        {
            _cpiDbContext = cpiDbContext;
            _user = user;
        }

        public IQueryable<AMSInstrxCPiLog> AMSInstrxCPiLogs => _cpiDbContext.GetRepository<AMSInstrxCPiLog>().QueryableList;

        public IQueryable<AMSInstrxCPiLogDetail> AMSInstrxCPiLogDetails => _cpiDbContext.GetRepository<AMSInstrxCPiLogDetail>().QueryableList;

        public IQueryable<AMSInstrxCPiLogEmail> AMSInstrxCPiLogEmails => _cpiDbContext.GetRepository<AMSInstrxCPiLogEmail>().QueryableList;

        public IQueryable<string> GetClients(int sendId)
        {
            var logDetails = AMSInstrxCPiLogDetails.Where(l => l.SendId == sendId);
            IQueryable<string> clients;
            if (_user.IsAMSIntegrated())
                clients = logDetails.Select(l => l.AMSDue.AMSMain.CountryApplication == null ? l.AMSDue.AMSMain.CPIClient : l.AMSDue.AMSMain.CountryApplication.Invention.Client.ClientCode).Distinct();
            else
                clients = logDetails.Select(l => l.AMSDue.AMSMain.CPIClient).Distinct();

            return clients;
        }

        public IQueryable<string> GetAgents(int sendId)
        {
            var logDetails = AMSInstrxCPiLogDetails.Where(l => l.SendId == sendId && (l.SentInstructionType == "Y" || l.SentInstructionType == "A"));
            IQueryable<string> agents;
            if (_user.IsAMSIntegrated())
                agents = logDetails.Select(l => l.AMSDue.AMSMain.CountryApplication == null ? l.AMSDue.AMSMain.CPIAgent : l.AMSDue.AMSMain.CountryApplication.Agent.AgentCode).Distinct();
            else
                agents = logDetails.Select(l => l.AMSDue.AMSMain.CPIAgent).Distinct();

            return agents;
        }

        public IQueryable<string> GetAttorneys(int sendId)
        {
            var logDetails = AMSInstrxCPiLogDetails.Where(l => l.SendId == sendId);
            IQueryable<string> attorneys;
            if (_user.IsAMSIntegrated())
                attorneys = logDetails.Select(l => l.AMSDue.AMSMain.CountryApplication == null ? l.AMSDue.AMSMain.CPIAttorney : l.AMSDue.AMSMain.CountryApplication.Invention.Attorney1.AttorneyCode).Distinct();
            else
                attorneys = logDetails.Select(l => l.AMSDue.AMSMain.CPIAttorney).Distinct();

            return attorneys;
        }

        public async Task<int> SaveInstrxCPiLog(IEnumerable<AMSInstrxCPiLogDetail> details, DateTime sendToCPiDate, string userId)
        {
            var lastUpdate = DateTime.Now;
            var amsDues = await _cpiDbContext.GetRepository<AMSDue>().QueryableList.Where(d => details.Select(l => l.DueId).Contains(d.DueID)).ToListAsync();            
            var instrxCPILog = new AMSInstrxCPiLog()
            {
                SendDate = sendToCPiDate,
                CPIConfirmDate = sendToCPiDate,
                SendMethod = "E",
                CreatedBy = userId
            };

            //add child data to parent entity
            instrxCPILog.AMSInstrxCPiLogDetails = details.Select(d => new AMSInstrxCPiLogDetail()
            {
                DueId = d.DueId,
                ClientInstructionLogId = d.ClientInstructionLogId,
                SentInstructionType = d.SentInstructionType,
                SentInstructionDate = d.SentInstructionDate,
                CPITaxSchedule = d.CPITaxSchedule
            }).ToList();

            _cpiDbContext.GetRepository<AMSDue>().Attach(amsDues);
            amsDues.ForEach(d =>
            {
                d.ClientInstructionSentToCPI = sendToCPiDate;
                d.ClientInstructionSentToCPIFlag = true;
                d.UpdatedBy = userId;
                d.LastUpdate = lastUpdate;
            });

            _cpiDbContext.GetRepository<AMSInstrxCPiLog>().Add(instrxCPILog);
            //does not work with EF.Core 6
            //add child data to parent entity
            //_cpiDbContext.GetRepository<AMSInstrxCPiLogDetail>().Add(details.Select(d => new AMSInstrxCPiLogDetail()
            //{
            //    SendId = instrxCPILog.SendId,
            //    DueId = d.DueId,
            //    ClientInstructionLogId = d.ClientInstructionLogId,
            //    SentInstructionType = d.SentInstructionType,
            //    SentInstructionDate = d.SentInstructionDate,
            //    CPITaxSchedule = d.CPITaxSchedule
            //}));
            await _cpiDbContext.SaveChangesAsync();

            //detach amsDues to allow other updates
            _cpiDbContext.Detach(amsDues);

            return instrxCPILog.SendId;
        }

        public async Task SaveRecipient(AMSInstrxCPiLogEmail recipient)
        {
            _cpiDbContext.GetRepository<AMSInstrxCPiLogEmail>().Add(recipient);
            await _cpiDbContext.SaveChangesAsync();
        }

        public async Task SaveError(AMSInstrxCPiLogError error)
        {
            _cpiDbContext.GetRepository<AMSInstrxCPiLogError>().Add(error);
            await _cpiDbContext.SaveChangesAsync();
        }
    }
}
