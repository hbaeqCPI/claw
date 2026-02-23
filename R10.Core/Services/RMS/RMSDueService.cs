using Microsoft.EntityFrameworkCore;
using R10.Core.Entities;
using R10.Core.Entities.RMS;
using R10.Core.Entities.Trademark;
using R10.Core.Exceptions;
using R10.Core.Helpers;
using R10.Core.Identity;
using R10.Core.Interfaces;
using R10.Core.Interfaces.RMS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace R10.Core.Services.RMS
{
    public class RMSDueService : EntityService<RMSDue>, IRMSDueService
    {
        private readonly IDueDateService<TmkActionDue, TmkDueDate> _tmkDueDateService;
        private readonly ITmkTrademarkService _trademarkService;

        public RMSDueService(
            IDueDateService<TmkActionDue, TmkDueDate> tmkDueDateService,
            ITmkTrademarkService trademarkService,
            ICPiDbContext cpiDbContext, 
            ClaimsPrincipal user) : base(cpiDbContext, user)
        {
            _tmkDueDateService = tmkDueDateService;
            _trademarkService = trademarkService;
        }

        public IQueryable<TmkTrademark> Trademarks => _cpiDbContext.GetRepository<TmkTrademark>().QueryableList;

        public IQueryable<TmkTrademark> TrademarkList => _trademarkService.TmkTrademarks.Where(t => t.ActionDues.Any(a => InstructableList.Any(d => d.ActId == a.ActId)));

        public IQueryable<TmkDueDate> TmkDueDateList => _tmkDueDateService.QueryableList;

        public IQueryable<TmkDueDate> InstructableList => _tmkDueDateService.QueryableList.Where(Instructable.And(d => !(d.RMSDue.IgnoreRecord ?? false)));

        public IQueryable<TmkDueDate> ActionClosingList => InstructableList.Where(d => !string.IsNullOrEmpty(d.RMSDue.ClientInstructionType));

        public IQueryable<RMSDueCountry> RMSDueCountries => _cpiDbContext.GetRepository<RMSDueCountry>().QueryableList;

        public IQueryable<RMSInstrxChangeLog> InstrxChangeLogList => _cpiDbContext.GetRepository<RMSInstrxChangeLog>().QueryableList;

        public IQueryable<RMSInstrxTypeActionDetail> ActionInstrxTypes => _cpiDbContext.GetRepository<RMSInstrxTypeActionDetail>().QueryableList
                                                                                        .Where(i => i.RMSInstrxType.InUse).OrderBy(i => i.RMSInstrxType.OrderOfDisplay);
        public IQueryable<RMSInstrxTypeActionDetail> TicklerActionInstrxTypes => ActionInstrxTypes.Where(i => !i.RMSInstrxType.HideToClient);

        public IQueryable<TmkOwner> OwnerList => _cpiDbContext.GetRepository<TmkOwner>().QueryableList.Where(o => TrademarkList.Any(t => t.TmkId == o.TmkID));

        public IQueryable<TmkTrademarkClass> ClassList => _cpiDbContext.GetRepository<TmkTrademarkClass>().QueryableList.Where(c => TrademarkList.Any(t => t.TmkId == c.TmkId));

        private Expression<Func<TmkDueDate, bool>> Instructable =>
            d => !(d.RMSDue.IsActionClosed ?? false) &&                                 //OPEN ACTIONS
                d.TmkActionDue.TmkTrademark.TmkTrademarkStatus.ActiveSwitch &&          //ACTIVE STATUS
                d.DueDate >= DateTime.Now.AddYears(-1) &&                               //GO BACK 1 YEAR TO SHOW ANYTHING IN GRACE PERIOD
                d.DueDate <= DateTime.Now.AddYears(1) &&                                //1 YEAR LIMIT FOR FUTURE DATES
                d.DateTaken == null &&                                                  //OUTSTANDING ACTIONS
                d.TmkIndicator.RMSIndicator &&                                          //RMS Indicator flag in tblTmkIndicator
                _cpiDbContext.GetRepository<RMSReminderSetup>().QueryableList.Any(s =>
                                                                    (string.IsNullOrEmpty(s.Country) || s.Country == d.TmkActionDue.TmkTrademark.Country) &&
                                                                    (string.IsNullOrEmpty(s.CaseType) || s.CaseType == d.TmkActionDue.TmkTrademark.CaseType) &&
                                                                    s.ActionType == d.TmkActionDue.ActionType &&
                                                                    (string.IsNullOrEmpty(s.ActionDue) || s.ActionDue == d.ActionDue)
                                                                );


        public async Task<(int DueId, byte[] tStamp)> SaveInstruction(int ddId, string instructionType, string reason, string source, List<string> countries, byte[] tStamp, string userName)
        {
            await ValidatePermission(InstructableList, ddId, CPiPermissions.DecisionMaker);

            var instructionDate = DateTime.Now;

            //insert tblRMSDue
            var dueId = await QueryableList.Where(d => d.DDId == ddId).Select(d => d.DueId).FirstOrDefaultAsync();
            if (dueId == 0)
            {
                var newRMSDue = new RMSDue()
                {
                    DDId = ddId,
                    CreatedBy = userName,
                    DateCreated = instructionDate
                };
                _cpiDbContext.GetRepository<RMSDue>().Add(newRMSDue);
                await _cpiDbContext.SaveChangesAsync();

                _cpiDbContext.Detach(newRMSDue);

                dueId = newRMSDue.DueId;
                tStamp = newRMSDue.tStamp;
            }

            //create instrx change log
            var instrxChangeLog = new RMSInstrxChangeLog()
            {
                DueId = dueId,
                ClientInstruction = (await _cpiDbContext.GetReadOnlyRepositoryAsync<RMSInstrxType>().QueryableList.Where(i => i.InstructionType == instructionType).Select(i => i.ClientDescription).FirstOrDefaultAsync()) ?? "",
                ClientInstructionType = instructionType ?? "",
                ClientInstructionDate = instructionDate,
                ClientInstructionSource = source ?? "E",
                ReasonForChange = reason ?? "",
                DateChanged = instructionDate,
                CreatedBy = userName
            };
            //EF.Core 6 breaking change
            //insert instrxChangeLog through RMSDue to be able to use instrxChangeLog.LogId
            //_cpiDbContext.GetRepository<RMSInstrxChangeLog>().Add(instrxChangeLog);

            //update tblRMSDue
            var rmsDue = new RMSDue()
            {
                DueId = dueId,
                tStamp = tStamp
            };
            _cpiDbContext.GetRepository<RMSDue>().Attach(rmsDue);
            rmsDue.ClientInstructionType = instructionType ?? "";
            rmsDue.ClientInstructionDate = instructionDate;
            rmsDue.ClientInstructionSource = source ?? "E";
            rmsDue.ClientInstructionLogId = instrxChangeLog.LogId;
            rmsDue.CloseDate = null;
            rmsDue.IsActionClosed = false;

            rmsDue.UpdatedBy = userName;
            rmsDue.LastUpdate = instructionDate;

            //EF.Core 6 breaking change
            //insert instrxChangeLog through RMSDue to be able to use instrxChangeLog.LogId
            rmsDue.RMSInstrxChangeLog = instrxChangeLog;

            //update RMSDueCountry
            var deletedCountries = await _cpiDbContext.GetRepository<RMSDueCountry>().QueryableList.Where(c => c.DueId == dueId).ToListAsync();
            var newCountries = new List<RMSDueCountry>();

            foreach (var ctry in countries)
            {
                var country = deletedCountries.FirstOrDefault(c => c.Country == ctry);
                if (country != null)
                    deletedCountries.Remove(country);   //keep existing
                else
                    newCountries.Add(new RMSDueCountry() //add new
                    {
                        DueId = dueId,
                        Country = ctry,
                        CreatedBy = userName,
                        DateCreated = instructionDate,
                        UpdatedBy = userName,
                        LastUpdate = instructionDate
                    });
            }
            //add countries
            _cpiDbContext.GetRepository<RMSDueCountry>().Add(newCountries);

            //remove countries
            _cpiDbContext.GetRepository<RMSDueCountry>().Delete(deletedCountries);

            await _cpiDbContext.SaveChangesAsync();

            return (DueId: dueId, tStamp: rmsDue.tStamp);
        }

        public async Task<byte[]> SaveInstructionRemarks(int ddId, string remarks, byte[] tStamp, string userName)
        {
            await ValidatePermission(InstructableList, ddId, CPiPermissions.DecisionMaker);

            var lastUpdate = DateTime.Now;

            //insert tblRMSDue
            var dueId = await QueryableList.Where(d => d.DDId == ddId).Select(d => d.DueId).FirstOrDefaultAsync();
            if (dueId == 0)
            {
                var newRMSDue = new RMSDue()
                {
                    DDId = ddId,
                    CreatedBy = userName,
                    DateCreated = lastUpdate
                };
                _cpiDbContext.GetRepository<RMSDue>().Add(newRMSDue);
                await _cpiDbContext.SaveChangesAsync();

                _cpiDbContext.Detach(newRMSDue);

                dueId = newRMSDue.DueId;
                tStamp = newRMSDue.tStamp;
            }

            //update tblRMSDue
            var rmsDue = new RMSDue()
            {
                DueId = dueId,
                tStamp = tStamp
            };
            _cpiDbContext.GetRepository<RMSDue>().Attach(rmsDue);
            rmsDue.ClientInstrxRemarks = remarks ?? "";
            rmsDue.UpdatedBy = userName;
            rmsDue.LastUpdate = lastUpdate;

            await _cpiDbContext.SaveChangesAsync();

            return rmsDue.tStamp;
        }

        public async Task<byte[]> MarkDeleted(int dueId, bool ignoreRecord, byte[] tStamp, string userName)
        {
            await ValidatePermission(_tmkDueDateService.QueryableList.Where(d => d.RMSDue != null), dueId, CPiPermissions.FullModify);

            var updated = new RMSDue()
            {
                DueId = dueId,
                tStamp = tStamp,
                IgnoreRecord = !ignoreRecord
            };

            _cpiDbContext.GetRepository<RMSDue>().Attach(updated);
            updated.IgnoreRecord = ignoreRecord;
            updated.UpdatedBy = userName;
            updated.LastUpdate = DateTime.Now;

            await _cpiDbContext.SaveChangesAsync();

            return updated.tStamp;
        }

        public async Task SaveClientLastReminderDate(int remId, DateTime remDate)
        {
            var rmsDues = await base.QueryableList.Where(d => d.TmkDueDate.RMSRemLogDues.Any(l => l.RemId == remId && l.DueId == d.DDId)).ToListAsync();

            _cpiDbContext.GetRepository<RMSDue>().Attach(rmsDues);
            rmsDues.ForEach(d => {
                d.ClientLastReminderDate = remDate;
            });

            await _cpiDbContext.SaveChangesAsync();
        }

        public async Task SaveClientReceiptLetterSentDate(int logId, DateTime letterSentDate, List<string> clientCodes)
        {
            var rmsDues = await base.QueryableList.Where(d =>
                            d.TmkDueDate.RMSActionCloseLogDues.Any(l => l.LogId == logId && l.DueId == d.DDId) &&
                            clientCodes.Contains(d.TmkDueDate.TmkActionDue.TmkTrademark.Client.ClientCode)).ToListAsync();

            _cpiDbContext.GetRepository<RMSDue>().Attach(rmsDues);
            rmsDues.ForEach(d => {
                d.ClientReceiptLetterSentDate = letterSentDate;
            });

            await _cpiDbContext.SaveChangesAsync();

            //detach for separate client/agent calls from InstructionsToCPiController
            _cpiDbContext.Detach(rmsDues);
        }

        public async Task SaveClientInstructionSentToAgent(int logId, DateTime letterSentDate, List<string> agentCodes)
        {
            //check instrx type send to agent flag
            var rmsDues = await base.QueryableList.Where(d =>
                            d.TmkDueDate.RMSActionCloseLogDues.Any(l => l.LogId == logId && l.DueId == d.DDId &&
                            l.SentInstrxType.SendToAgent) &&
                            agentCodes.Contains(d.TmkDueDate.TmkActionDue.TmkTrademark.Agent.AgentCode)).ToListAsync();

            await SaveClientInstructionSentToAgent(rmsDues, letterSentDate);
        }

        public async Task SaveClientInstructionSentToAgent(List<RMSDue> rmsDues, DateTime letterSentDate)
        {
            _cpiDbContext.GetRepository<RMSDue>().Attach(rmsDues);
            rmsDues.ForEach(d => {
                d.ClientInstructionSentToAssoc = letterSentDate;
            });

            await _cpiDbContext.SaveChangesAsync();

            //detach for separate client/agent calls from InstructionsToCPiController
            _cpiDbContext.Detach(rmsDues);
        }

        public async Task<byte[]> SaveNextRenewalDate(int ddId, DateTime? nextRenewalDate, byte[] tStamp, string userName)
        {
            await ValidatePermission(ActionClosingList, ddId, CPiPermissions.FullModify);

            var lastUpdate = DateTime.Now;

            //insert tblRMSDue
            var dueId = await QueryableList.Where(d => d.DDId == ddId).Select(d => d.DueId).FirstOrDefaultAsync();
            if (dueId == 0)
            {
                var newRMSDue = new RMSDue()
                {
                    DDId = ddId,
                    CreatedBy = userName,
                    DateCreated = lastUpdate
                };
                _cpiDbContext.GetRepository<RMSDue>().Add(newRMSDue);
                await _cpiDbContext.SaveChangesAsync();

                _cpiDbContext.Detach(newRMSDue);

                dueId = newRMSDue.DueId;
                tStamp = newRMSDue.tStamp;
            }

            //update tblRMSDue
            var rmsDue = new RMSDue()
            {
                DueId = dueId,
                NextRenewalDate = lastUpdate,
                tStamp = tStamp
            };
            _cpiDbContext.GetRepository<RMSDue>().Attach(rmsDue);
            rmsDue.NextRenewalDate = nextRenewalDate;
            rmsDue.UpdatedBy = userName;
            rmsDue.LastUpdate = lastUpdate;

            await _cpiDbContext.SaveChangesAsync();

            return rmsDue.tStamp;
        }

        public async Task<byte[]> SaveAgentPaymentDate(int ddId, DateTime? agentPaymentDate, byte[] tStamp, string userName)
        {
            await ValidatePermission(ActionClosingList, ddId, CPiPermissions.FullModify);

            var lastUpdate = DateTime.Now;

            //insert tblRMSDue
            var dueId = await QueryableList.Where(d => d.DDId == ddId).Select(d => d.DueId).FirstOrDefaultAsync();
            if (dueId == 0)
            {
                var newRMSDue = new RMSDue()
                {
                    DDId = ddId,
                    CreatedBy = userName,
                    DateCreated = lastUpdate
                };
                _cpiDbContext.GetRepository<RMSDue>().Add(newRMSDue);
                await _cpiDbContext.SaveChangesAsync();

                _cpiDbContext.Detach(newRMSDue);

                dueId = newRMSDue.DueId;
                tStamp = newRMSDue.tStamp;
            }

            //update tblRMSDue
            var rmsDue = new RMSDue()
            {
                DueId = dueId,
                AgentPaymentDate = lastUpdate,
                tStamp = tStamp
            };
            _cpiDbContext.GetRepository<RMSDue>().Attach(rmsDue);
            rmsDue.AgentPaymentDate = agentPaymentDate;
            rmsDue.UpdatedBy = userName;
            rmsDue.LastUpdate = lastUpdate;

            await _cpiDbContext.SaveChangesAsync();

            return rmsDue.tStamp;
        }

        public async Task<byte[]> SaveExclude(int ddId, bool value, byte[] tStamp, string userName)
        {
            await ValidatePermission(ActionClosingList, ddId, CPiPermissions.FullModify);

            var lastUpdate = DateTime.Now;

            //insert tblRMSDue
            var dueId = await QueryableList.Where(d => d.DDId == ddId).Select(d => d.DueId).FirstOrDefaultAsync();
            if (dueId == 0)
            {
                var newRMSDue = new RMSDue()
                {
                    DDId = ddId,
                    CreatedBy = userName,
                    DateCreated = lastUpdate
                };
                _cpiDbContext.GetRepository<RMSDue>().Add(newRMSDue);
                await _cpiDbContext.SaveChangesAsync();

                _cpiDbContext.Detach(newRMSDue);

                dueId = newRMSDue.DueId;
                tStamp = newRMSDue.tStamp;
            }

            //update tblRMSDue
            var rmsDue = new RMSDue()
            {
                DueId = dueId,
                Exclude = !value,
                tStamp = tStamp
            };
            _cpiDbContext.GetRepository<RMSDue>().Attach(rmsDue);
            rmsDue.Exclude = value;
            rmsDue.UpdatedBy = userName;
            rmsDue.LastUpdate = lastUpdate;

            await _cpiDbContext.SaveChangesAsync();

            return rmsDue.tStamp;
        }

        private async Task<int> ValidatePermission(IQueryable<TmkDueDate> queryable, int ddId, List<string> roles)
        {
            var item = await queryable.Where(i => i.DDId == ddId).Select(i => new { i.DDId, i.TmkActionDue.TmkTrademark.TmkId, i.TmkActionDue.TmkTrademark.RespOffice }).FirstOrDefaultAsync();
            Guard.Against.NoRecordPermission(item.DDId > 0);
            Guard.Against.NoRecordPermission(await ValidatePermission(SystemType.RMS, roles, item.RespOffice));

            return item.TmkId;
        }

        public override Task Add(RMSDue entity)
        {
            //return base.Add(entity);
            //NOT ALLOWED
            throw new UnauthorizedAccessException();
        }

        public override Task Delete(RMSDue entity)
        {
            //return base.Delete(entity);
            //NOT ALLOWED
            throw new UnauthorizedAccessException();
        }

        public override Task Update(RMSDue entity)
        {
            //return base.Update(entity);
            //NOT ALLOWED
            throw new UnauthorizedAccessException();
        }

        public override Task UpdateRemarks(RMSDue entity)
        {
            //return base.UpdateRemarks(entity);
            //NOT ALLOWED
            throw new UnauthorizedAccessException();
        }
    }
}
