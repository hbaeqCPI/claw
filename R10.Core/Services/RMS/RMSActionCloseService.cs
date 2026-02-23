using Microsoft.EntityFrameworkCore;
using R10.Core.Entities.RMS;
using R10.Core.Entities.Trademark;
using R10.Core.Interfaces;
using R10.Core.Interfaces.RMS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace R10.Core.Services.RMS
{
    public class RMSActionCloseService : IRMSActionCloseService
    {
        protected readonly ICPiDbContext _cpiDbContext;
        protected readonly ClaimsPrincipal _user;
        protected readonly IDueDateService<TmkActionDue, TmkDueDate> _tmkDueDateService;
        protected readonly ITmkTrademarkService _tmkTrademarkService;

        public RMSActionCloseService(ICPiDbContext cpiDbContext, ClaimsPrincipal user, IDueDateService<TmkActionDue, TmkDueDate> tmkDueDateService, ITmkTrademarkService tmkTrademarkService)
        {
            _cpiDbContext = cpiDbContext;
            _user = user;
            _tmkDueDateService = tmkDueDateService;
            _tmkTrademarkService = tmkTrademarkService;
        }

        public string AbandonInstructionType => "A";
        public string AbandonTrademarkStatus => "Abandoned";

        public IQueryable<RMSActionCloseLog> RMSActionCloseLogs => _cpiDbContext.GetRepository<RMSActionCloseLog>().QueryableList;

        public IQueryable<RMSActionCloseLogDue> RMSActionCloseLogDues => _cpiDbContext.GetRepository<RMSActionCloseLogDue>().QueryableList;

        public IQueryable<RMSActionCloseLogEmail> RMSActionCloseLogEmails => _cpiDbContext.GetRepository<RMSActionCloseLogEmail>().QueryableList;

        public IQueryable<string> GetClients(int logId)
        {
            var logDetails = RMSActionCloseLogDues.Where(l => l.LogId == logId);
            var clients = logDetails.Select(l => l.TmkDueDate.TmkActionDue.TmkTrademark.Client.ClientCode).Distinct();

            return clients;
        }

        public IQueryable<string> GetAgents(int logId)
        {
            var logDetails = RMSActionCloseLogDues.Where(l => l.LogId == logId && l.SentInstrxType.SendToAgent);
            var agents = logDetails.Select(l => l.TmkDueDate.TmkActionDue.TmkTrademark.Agent.AgentCode).Distinct();

            return agents;
        }

        public async Task<int> SaveActionCloseLog(IEnumerable<RMSActionCloseLogDue> details, DateTime closeDate, string filter, string userId)
        {
            var actionCloseLog = new RMSActionCloseLog()
            {
                CloseDate = closeDate,
                Filter = filter,
                CreatedBy = userId
            };

            //add child data to parent entity
            actionCloseLog.RMSActionCloseLogDues = details.Select(d => new RMSActionCloseLogDue()
            {
                DueId = d.DueId,
                ClientInstructionLogId = d.ClientInstructionLogId,
                SentInstructionType = d.SentInstructionType,
                SentInstructionDate = d.SentInstructionDate,
                NextRenewalDate = d.NextRenewalDate,
                CloseAction = d.CloseAction
            }).ToList();

            _cpiDbContext.GetRepository<RMSActionCloseLog>().Add(actionCloseLog);
            //does not work with EF.Core 6
            //add child data to parent entity
            //_cpiDbContext.GetRepository<RMSActionCloseLogDue>().Add(details.Select(d => new RMSActionCloseLogDue()
            //{
            //    LogId = actionCloseLog.LogId,
            //    DueId = d.DueId,
            //    ClientInstructionLogId = d.ClientInstructionLogId,
            //    SentInstructionType = d.SentInstructionType,
            //    SentInstructionDate = d.SentInstructionDate,
            //    NextRenewalDate = d.NextRenewalDate,
            //    CloseAction = d.CloseAction
            //}));

            await _cpiDbContext.SaveChangesAsync();

            return actionCloseLog.LogId;
        }

        public async Task SaveRecipient(RMSActionCloseLogEmail recipient)
        {
            _cpiDbContext.GetRepository<RMSActionCloseLogEmail>().Add(recipient);
            await _cpiDbContext.SaveChangesAsync();
        }

        public async Task SaveError(RMSActionCloseLogError error)
        {
            //clear all tracked entries that may have caused the error
            //to avoid getting executed on next SaveChangesAsync call
            _cpiDbContext.DetachAll();

            _cpiDbContext.GetRepository<RMSActionCloseLogError>().Add(error);
            await _cpiDbContext.SaveChangesAsync();
        }

        private async Task<string> SaveError(int logId, Exception e)
        {
            var message = string.IsNullOrEmpty(e.InnerException.Message) ? e.Message : e.InnerException.Message;
            await SaveError(new RMSActionCloseLogError() { LogId = logId, Message = message });

            //todo: return friendly error message
            return message;
        }

        public async Task<List<string>>  CloseActions(IEnumerable<RMSActionCloseLogDue> details, DateTime closeDate, int logId, string userId)
        {
            var errors = new List<string>();
            var dueIds = details.Select(d => d.DueId).ToList();

            //update process flag and date
            var rmsDues = await _cpiDbContext.GetRepository<RMSDue>().QueryableList.Where(d =>
                                        dueIds.Contains(d.TmkDueDate.DDId) &&
                                        !(d.Exclude ?? false)   //do not process excluded actions
                                        ).ToListAsync();

            _cpiDbContext.GetRepository<RMSDue>().Attach(rmsDues);
            rmsDues.ForEach(d => {
                d.CloseDate = closeDate;
                d.IsActionClosed = true;
                d.UpdatedBy = userId;
                d.LastUpdate = closeDate;
            });

            await _cpiDbContext.SaveChangesAsync();

            //detach rmsDues to allow other updates
            _cpiDbContext.Detach(rmsDues);

            //close due dates by updating due date date taken
            var tmkDues = await _cpiDbContext.GetRepository<TmkDueDate>().QueryableList.Where(d =>
                                    dueIds.Contains(d.DDId)
                                    //update remarks regardless of indicator or CloseAction flag
                                    //&& d.Indicator != "Ren/Due" //not renewals
                                    //&& d.RMSDue.ClientInstrxType.CloseAction //only close flagged instructions
                                    && !(d.RMSDue.Exclude ?? false) //do not close excluded actions
                                    ) //.ToListAsync();
                                    .Select(d => new TmkDueDate()
                                    {
                                        DDId = d.DDId,
                                        ActId = d.ActId,
                                        ActionDue = d.ActionDue,
                                        DueDate = d.DueDate,
                                        Indicator = d.Indicator,
                                        DateTaken = d.Indicator == "Ren/Due" || !d.RMSDue.ClientInstrxType.CloseAction ? d.DateTaken : closeDate,
                                        Remarks = (d.Remarks ?? "") + ((d.Remarks ?? "") == "" ? "" : "\r\n") +
                                                    $"{closeDate.ToString("dd-MMM-yyyy")} {userId}: " +
                                                    $"Client instructed through Renewal Management System.\r\n" +
                                                        $"Instruction: {d.RMSDue.ClientInstrxType.ClientDescription}\r\n" +
                                                        $"Instruction Date: {((DateTime)d.RMSDue.ClientInstructionDate).ToString("dd-MMM-yyyy")}\r\n" +
                                                        $"Instructed By: {d.RMSDue.RMSInstrxChangeLog.CreatedBy}",
                                        CreatedBy = d.CreatedBy,
                                        UpdatedBy = userId,
                                        DateCreated = d.DateCreated,
                                        LastUpdate = closeDate,
                                        tStamp = d.tStamp
                                    }).ToListAsync();

            //tmkDues.ForEach(d => {
            //    d.DateTaken = closeDate;
            //    d.UpdatedBy = userId;
            //    d.LastUpdate = closeDate;
            //});

            foreach (var actId in tmkDues.Select(d => d.ActId).Distinct().ToList())
            {
                try
                {
                    await _tmkDueDateService.Update(actId, userId, tmkDues.Where(d => d.ActId == actId).ToList(), new List<TmkDueDate>(), new List<TmkDueDate>());
                }
                catch (Exception e)
                {
                    errors.Add(await SaveError(logId, e));
                }
            }

            //close renewals and generate next renewal by updating trademark last renewal date
            var tmkTrademarks = await _cpiDbContext.GetRepository<TmkTrademark>().QueryableList.Where(t => t.ActionDues.Any(a => a.DueDates.Any(d =>
                                    dueIds.Contains(d.DDId)
                                    && d.Indicator == "Ren/Due" //renewals
                                    && d.RMSDue.ClientInstrxType.CloseAction //only close flagged instructions
                                    && d.RMSDue.ClientInstrxType.Active //only close active instructions
                                    && !(d.RMSDue.Exclude ?? false) //do not close excluded actions
                                    ))).ToListAsync();

            foreach (var t in tmkTrademarks)
            {
                var nextRenewalDate = details.Where(d => d.TmkDueDate.TmkActionDue.TmkId == t.TmkId).Select(d => d.NextRenewalDate).FirstOrDefault();
                if (nextRenewalDate != null)
                {
                    t.LastRenewalDate = t.NextRenewalDate;
                    t.NextRenewalDate = nextRenewalDate;
                    t.UpdatedBy = userId;
                    t.LastUpdate = closeDate;

                    //Trademark is not using TmkTrademark.OwnerID to store owner data but
                    //_tmkTrademarkService.UpdateTrademark expects TmkTrademark.OwnerID when editing from screen then moves the value to TmkOwner table.
                    //_tmkTrademarkService.UpdateTrademark will delete TmkOwner entry if TmkTrademark.OwnerID is null.
                    //Set TmkTrademark.OwnerID = 0 so TmkOwner table checking is ignored.
                    t.OwnerID = 0;

                    try
                    {
                        await _tmkTrademarkService.UpdateTrademark(t, closeDate);
                    }
                    catch (Exception e)
                    {
                        errors.Add(await SaveError(logId, e));
                    }
                }
            }

            //update trademark status when instruction is abandon
            var abandonDueIds = details.Where(d => d.SentInstructionType == AbandonInstructionType).Select(d => d.DueId).ToList();
            if (abandonDueIds?.Count > 0)
            {
                var abandonTmks = await _cpiDbContext.GetRepository<TmkTrademark>().QueryableList.Where(t => t.ActionDues.Any(a => a.DueDates.Any(d =>
                                        abandonDueIds.Contains(d.DDId)
                                        && d.RMSDue.ClientInstructionType == AbandonInstructionType //abandon instruction
                                        && !(d.RMSDue.Exclude ?? false)                             //do not close excluded actions
                                        ))).ToListAsync();

                foreach (var t in abandonTmks)
                {
                    t.TrademarkStatus = AbandonTrademarkStatus;
                    t.UpdatedBy = userId;
                    t.LastUpdate = closeDate;
                    t.Remarks = (t.Remarks ?? "") + ((t.Remarks ?? "") == "" ? "" : "\r\n") +
                                                    $"{closeDate.ToString("dd-MMM-yyyy")} {userId}: " +
                                                    $"Client instructed Abandon through Renewal Management System.";
                    t.OwnerID = 0;

                    try
                    {
                        await _tmkTrademarkService.UpdateTrademark(t, closeDate);
                    }
                    catch (Exception e)
                    {
                        errors.Add(await SaveError(logId, e));
                    }
                }
            }

            return errors;
        }
    }
}
