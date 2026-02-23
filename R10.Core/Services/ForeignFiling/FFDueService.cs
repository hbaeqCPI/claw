using Microsoft.EntityFrameworkCore;
using R10.Core.Entities;
using R10.Core.Entities.ForeignFiling;
using R10.Core.Entities.Patent;
using R10.Core.Exceptions;
using R10.Core.Helpers;
using R10.Core.Identity;
using R10.Core.Interfaces;
using R10.Core.Interfaces.ForeignFiling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace R10.Core.Services.ForeignFiling
{
    public class FFDueService : EntityService<FFDue>, IFFDueService
    {
        private readonly IDueDateService<PatActionDue, PatDueDate> _patDueDateService;

        public FFDueService(
            IDueDateService<PatActionDue, PatDueDate> patDueDateService,
            ICPiDbContext cpiDbContext, 
            ClaimsPrincipal user) : base(cpiDbContext, user)
        {
            _patDueDateService = patDueDateService;
        }

        public IQueryable<CountryApplication> CountryApplicationList => _cpiDbContext.GetRepository<CountryApplication>().QueryableList.Where(t => t.ActionDues.Any(a => InstructableList.Any(d => d.ActId == a.ActId)));
        public IQueryable<PatDueDate> PatDueDateList => _patDueDateService.QueryableList;
        public IQueryable<PatDueDate> InstructableList => PatDueDateList.Where(Instructable);

        private Expression<Func<PatDueDate, bool>> Instructable =>
            d => !(d.FFDue.IsActionClosed ?? false) &&                                  //OPEN ACTIONS
                d.PatActionDue.CountryApplication.PatApplicationStatus.ActiveSwitch &&  //ACTIVE STATUS ONLY
                d.DueDate >= DateTime.Now.Date &&                                       //CURRENT DUE DATES
                d.DueDate <= DateTime.Now.AddYears(1) &&                                //1 YEAR LIMIT FOR FUTURE DATES
                d.DateTaken == null &&                                                  //OUTSTANDING ACTIONS
                d.PatIndicator.FFIndicator &&
                _cpiDbContext.GetRepository<FFReminderSetup>().QueryableList.Any(s =>
                                                                    (string.IsNullOrEmpty(s.Country) || s.Country == d.PatActionDue.CountryApplication.Country) &&
                                                                    (string.IsNullOrEmpty(s.CaseType) || s.CaseType == d.PatActionDue.CountryApplication.CaseType) &&
                                                                    s.ActionType == d.PatActionDue.ActionType &&
                                                                    (string.IsNullOrEmpty(s.ActionDue) || s.ActionDue == d.ActionDue)
                                                                );

        public IQueryable<PatDueDate> ActionClosingList => InstructableList.Where(d => !string.IsNullOrEmpty(d.FFDue.ClientInstructionType)
                                                            //Only close EP Validation if all countries are selected
                                                            && (d.ActionDue != "Validate Designated Countries" || d.FFDue.FFDueCountries.Where(c => c.Source == "EP").Count() == _cpiDbContext.GetRepository<PatDesCaseType>().QueryableList.Where(c => c.IntlCode == "EP" && c.CaseType == "PCT").Count())
                                                            );

        public IQueryable<FFInstrxChangeLog> InstrxChangeLogList => _cpiDbContext.GetRepository<FFInstrxChangeLog>().QueryableList;

        public IQueryable<FFInstrxTypeActionDetail> ActionInstrxTypes => _cpiDbContext.GetRepository<FFInstrxTypeActionDetail>().QueryableList
                                                                                        .Where(i => i.FFInstrxType.InUse).OrderBy(i => i.FFInstrxType.OrderOfDisplay);
        public IQueryable<FFInstrxTypeActionDetail> TicklerActionInstrxTypes => ActionInstrxTypes.Where(i => !i.FFInstrxType.HideToClient);

        public async Task<bool> IsForeignFiling(int ddId)
        {
            return await InstructableList.AnyAsync(d => d.DDId == ddId && d.ActionDue == "Foreign Filing Due");
        }

        public async Task<(int DueId, byte[] tStamp)> SaveInstruction(int ddId, string instructionType, string reason, string source,
            List<string> ctryEP, List<string> ctryWO, List<string> ctryAll, byte[] tStamp, string userName)
        {
            //todo: validation
            //await ValidatePermission(InstructableList, ddId, CPiPermissions.DecisionMaker);

            var instructionDate = DateTime.Now;

            //insert tblFFDue
            var dueId = await QueryableList.Where(d => d.DDId == ddId).Select(d => d.DueId).FirstOrDefaultAsync();
            if (dueId == 0)
            {
                var newFFDue = new FFDue()
                {
                    DDId = ddId,
                    CreatedBy = userName,
                    DateCreated = instructionDate
                };
                _cpiDbContext.GetRepository<FFDue>().Add(newFFDue);
                await _cpiDbContext.SaveChangesAsync();

                _cpiDbContext.Detach(newFFDue);

                dueId = newFFDue.DueId;
                tStamp = newFFDue.tStamp;
            }

            //create instrx change log
            var instrxChangeLog = new FFInstrxChangeLog()
            {
                DueId = dueId,
                ClientInstruction = (await _cpiDbContext.GetReadOnlyRepositoryAsync<FFInstrxType>().QueryableList.Where(i => i.InstructionType == instructionType).Select(i => i.ClientDescription).FirstOrDefaultAsync()) ?? "",
                ClientInstructionType = instructionType ?? "",
                ClientInstructionDate = instructionDate,
                ClientInstructionSource = source ?? "E",
                ReasonForChange = reason ?? "",
                DateChanged = instructionDate,
                CreatedBy = userName
            };
            //EF.Core 6 breaking change
            //insert instrxChangeLog through FFDue to be able to use instrxChangeLog.LogId
            //_cpiDbContext.GetRepository<FFInstrxChangeLog>().Add(instrxChangeLog);

            //update tblFFDue
            var ffDue = new FFDue() 
            { 
                DueId = dueId,
                tStamp = tStamp
            };
            _cpiDbContext.GetRepository<FFDue>().Attach(ffDue);
            ffDue.ClientInstructionType = instructionType ?? "";
            ffDue.ClientInstructionDate = instructionDate;
            ffDue.ClientInstructionSource = source ?? "E";
            ffDue.ClientInstructionLogId = instrxChangeLog.LogId;
            ffDue.UpdatedBy = userName;
            ffDue.LastUpdate = instructionDate;

            //EF.Core 6 breaking change
            //insert instrxChangeLog through FFDue to be able to use instrxChangeLog.LogId
            ffDue.InstrxChangeLog = instrxChangeLog;

            //update FFDueCountry
            var deletedCountries = await _cpiDbContext.GetRepository<FFDueCountry>().QueryableList.Where(c => c.DueId == dueId).ToListAsync();
            var newCountries = new List<FFDueCountry>();
            foreach (var ctry in ctryEP)
            {
                var country = deletedCountries.Where(c => c.Country == ctry && c.Source == "EP").FirstOrDefault();
                if (country != null)
                    deletedCountries.Remove(country);   //keep existing
                else
                    newCountries.Add(new FFDueCountry() //add new
                    {
                        DueId = dueId,
                        Source = "EP",
                        Country = ctry,
                        CreatedBy = userName,
                        DateCreated = instructionDate,
                        UpdatedBy = userName,
                        LastUpdate = instructionDate
                    });
            }
            foreach (var ctry in ctryWO)
            {
                var country = deletedCountries.Where(c => c.Country == ctry && c.Source == "WO").FirstOrDefault();
                if (country != null)
                    deletedCountries.Remove(country);   //keep existing
                else
                    newCountries.Add(new FFDueCountry() //add new
                    {
                        DueId = dueId,
                        Source = "WO",
                        Country = ctry,
                        CreatedBy = userName,
                        DateCreated = instructionDate,
                        UpdatedBy = userName,
                        LastUpdate = instructionDate
                    });
            }
            foreach (var ctry in ctryAll)
            {
                var country = deletedCountries.Where(c => c.Country == ctry && c.Source == "All").FirstOrDefault();
                if (country != null)
                    deletedCountries.Remove(country);   //keep existing
                else
                    newCountries.Add(new FFDueCountry() //add new
                    {
                        DueId = dueId,
                        Source = "All",
                        Country = ctry,
                        CreatedBy = userName,
                        DateCreated = instructionDate,
                        UpdatedBy = userName,
                        LastUpdate = instructionDate
            });
            }
            //add countries
            _cpiDbContext.GetRepository<FFDueCountry>().Add(newCountries);

            //remove countries
            _cpiDbContext.GetRepository<FFDueCountry>().Delete(deletedCountries);

            //delete unselected generated country application record
            var appIds = deletedCountries.Where(c => c.Source == "All" && (c.GenId ?? 0) > 0).Select(c => c.GenId).ToList();
            if (appIds.Any())
            {
                var countryApps = await _cpiDbContext.GetRepository<CountryApplication>().QueryableList.Where(ca => appIds.Contains(ca.AppId)).ToListAsync();
                _cpiDbContext.GetRepository<CountryApplication>().Delete(countryApps);
            }

            //delete unselected generated designated country record
            var desIds = deletedCountries.Where(c => c.Source != "All" && (c.GenId ?? 0) > 0).Select(c => c.GenId).ToList();
            if (desIds.Any())
            {
                var desCountries = await _cpiDbContext.GetRepository<PatDesignatedCountry>().QueryableList.Where(dc => desIds.Contains(dc.DesId)).ToListAsync();
                _cpiDbContext.GetRepository<PatDesignatedCountry>().Delete(desCountries);
            }

            await _cpiDbContext.SaveChangesAsync();

            return (DueId: dueId, tStamp: ffDue.tStamp);
        }

        public async Task<byte[]> SaveInstructionRemarks(int ddId, string remarks, byte[] tStamp, string userName)
        {
            //todo: validation
            //await ValidatePermission(InstructableList, ddId, CPiPermissions.DecisionMaker);

            var lastUpdate = DateTime.Now;

            //insert tblRMSDue
            var dueId = await QueryableList.Where(d => d.DDId == ddId).Select(d => d.DueId).FirstOrDefaultAsync();
            if (dueId == 0)
            {
                var newFFDue = new FFDue()
                {
                    DDId = ddId,
                    CreatedBy = userName,
                    DateCreated = lastUpdate
                };
                _cpiDbContext.GetRepository<FFDue>().Add(newFFDue);
                await _cpiDbContext.SaveChangesAsync();

                _cpiDbContext.Detach(newFFDue);

                dueId = newFFDue.DueId;
                tStamp = newFFDue.tStamp;
            }

            //update tblFFDue
            var ffDue = new FFDue()
            {
                DueId = dueId,
                tStamp = tStamp
            };
            _cpiDbContext.GetRepository<FFDue>().Attach(ffDue);
            ffDue.ClientInstrxRemarks = remarks ?? "";
            ffDue.UpdatedBy = userName;
            ffDue.LastUpdate = lastUpdate;

            await _cpiDbContext.SaveChangesAsync();

            return ffDue.tStamp;
        }

        public async Task SaveClientLastReminderDate(int remId, DateTime remDate)
        {
            var ffDues = await base.QueryableList.Where(d => d.PatDueDate.FFRemLogDues.Any(l => l.RemId == remId && l.DueId == d.DDId)).ToListAsync();

            _cpiDbContext.GetRepository<FFDue>().Attach(ffDues);
            ffDues.ForEach(d => {
                d.ClientLastReminderDate = remDate;
            });

            await _cpiDbContext.SaveChangesAsync();
        }

        public async Task SaveClientReceiptLetterSentDate(int logId, DateTime letterSentDate, List<string> clientCodes)
        {
            var ffDues = await base.QueryableList.Where(d =>
                            d.PatDueDate.FFActionCloseLogDues.Any(l => l.LogId == logId && l.DueId == d.DDId) &&
                            clientCodes.Contains(d.PatDueDate.PatActionDue.CountryApplication.Invention.Client.ClientCode)).ToListAsync();

            _cpiDbContext.GetRepository<FFDue>().Attach(ffDues);
            ffDues.ForEach(d => {
                d.ClientReceiptLetterSentDate = letterSentDate;
            });

            await _cpiDbContext.SaveChangesAsync();

            //detach for separate client/agent calls from InstructionsToCPiController
            _cpiDbContext.Detach(ffDues);
        }

        public async Task SaveClientInstructionSentToAgent(int logId, DateTime letterSentDate, List<string> agentCodes)
        {
            //check instrx type send to agent flag
            var ffDues = await base.QueryableList.Where(d =>
                            d.PatDueDate.FFActionCloseLogDues.Any(l => l.LogId == logId && l.DueId == d.DDId &&
                            l.SentInstrxType.SendToAgent) &&
                            agentCodes.Contains(d.PatDueDate.PatActionDue.CountryApplication.Agent.AgentCode)).ToListAsync();

            _cpiDbContext.GetRepository<FFDue>().Attach(ffDues);
            ffDues.ForEach(d => {
                d.ClientInstructionSentToAssoc = letterSentDate;
            });

            await _cpiDbContext.SaveChangesAsync();

            //detach for separate client/agent calls from InstructionsToCPiController
            _cpiDbContext.Detach(ffDues);
        }

        public async Task<byte[]> SaveExclude(int ddId, bool value, byte[] tStamp, string userName)
        {
            //todo: validation
            //await ValidatePermission(ActionClosingList, ddId, CPiPermissions.FullModify);

            var lastUpdate = DateTime.Now;

            //insert tblFFDue
            var dueId = await QueryableList.Where(d => d.DDId == ddId).Select(d => d.DueId).FirstOrDefaultAsync();
            if (dueId == 0)
            {
                var newFFDue = new FFDue()
                {
                    DDId = ddId,
                    CreatedBy = userName,
                    DateCreated = lastUpdate
                };
                _cpiDbContext.GetRepository<FFDue>().Add(newFFDue);
                await _cpiDbContext.SaveChangesAsync();

                _cpiDbContext.Detach(newFFDue);

                dueId = newFFDue.DueId;
                tStamp = newFFDue.tStamp;
            }

            //update tblFFDue
            var ffDue = new FFDue()
            {
                DueId = dueId,
                Exclude = !value,
                tStamp = tStamp
            };
            _cpiDbContext.GetRepository<FFDue>().Attach(ffDue);
            ffDue.Exclude = value;
            ffDue.UpdatedBy = userName;
            ffDue.LastUpdate = lastUpdate;

            await _cpiDbContext.SaveChangesAsync();

            return ffDue.tStamp;
        }
    }
}
