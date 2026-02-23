using Microsoft.EntityFrameworkCore;
using R10.Core.Entities.ForeignFiling;
using R10.Core.Entities.Patent;
using R10.Core.Interfaces;
using R10.Core.Interfaces.ForeignFiling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace R10.Core.Services.ForeignFiling
{
    public class FFActionCloseService : IFFActionCloseService
    {
        protected readonly ICPiDbContext _cpiDbContext;
        protected readonly ClaimsPrincipal _user;
        protected readonly IDueDateService<PatActionDue, PatDueDate> _patDueDateService;

        public FFActionCloseService(ICPiDbContext cpiDbContext, ClaimsPrincipal user, IDueDateService<PatActionDue, PatDueDate> patDueDateService)
        {
            _cpiDbContext = cpiDbContext;
            _user = user;
            _patDueDateService = patDueDateService;
        }

        public IQueryable<FFActionCloseLog> FFActionCloseLogs => _cpiDbContext.GetRepository<FFActionCloseLog>().QueryableList;

        public IQueryable<FFActionCloseLogDue> FFActionCloseLogDues => _cpiDbContext.GetRepository<FFActionCloseLogDue>().QueryableList;

        public IQueryable<FFActionCloseLogEmail> FFActionCloseLogEmails => _cpiDbContext.GetRepository<FFActionCloseLogEmail>().QueryableList;

        public IQueryable<string> GetClients(int logId)
        {
            var logDetails = FFActionCloseLogDues.Where(l => l.LogId == logId);
            var clients = logDetails.Select(l => l.PatDueDate.PatActionDue.CountryApplication.Invention.Client.ClientCode).Distinct();

            return clients;
        }

        public IQueryable<string> GetAgents(int logId)
        {
            var logDetails = FFActionCloseLogDues.Where(l => l.LogId == logId && l.SentInstrxType.SendToAgent);
            var agents = logDetails.Select(l => l.PatDueDate.PatActionDue.CountryApplication.Agent.AgentCode).Distinct();

            return agents;
        }

        public async Task<int> SaveActionCloseLog(IEnumerable<FFActionCloseLogDue> details, DateTime closeDate, string filter, string userId)
        {
            var actionCloseLog = new FFActionCloseLog()
            {
                CloseDate = closeDate,
                Filter = filter,
                CreatedBy = userId
            };

            //add child data to parent entity
            actionCloseLog.FFActionCloseLogDues = details.Select(d => new FFActionCloseLogDue()
            {
                DueId = d.DueId,
                ClientInstructionLogId = d.ClientInstructionLogId,
                SentInstructionType = d.SentInstructionType,
                SentInstructionDate = d.SentInstructionDate
            }).ToList();

            _cpiDbContext.GetRepository<FFActionCloseLog>().Add(actionCloseLog);
            //does not work with EF.Core 6
            //add child data to parent entity
            //_cpiDbContext.GetRepository<FFActionCloseLogDue>().Add(details.Select(d => new FFActionCloseLogDue()
            //{
            //    LogId = actionCloseLog.LogId,
            //    DueId = d.DueId,
            //    ClientInstructionLogId = d.ClientInstructionLogId,
            //    SentInstructionType = d.SentInstructionType,
            //    SentInstructionDate = d.SentInstructionDate
            //}));

            await _cpiDbContext.SaveChangesAsync();

            return actionCloseLog.LogId;
        }

        public async Task SaveRecipient(FFActionCloseLogEmail recipient)
        {
            _cpiDbContext.GetRepository<FFActionCloseLogEmail>().Add(recipient);
            await _cpiDbContext.SaveChangesAsync();
        }

        public async Task SaveError(FFActionCloseLogError error)
        {
            _cpiDbContext.GetRepository<FFActionCloseLogError>().Add(error);
            await _cpiDbContext.SaveChangesAsync();
        }

        public async Task CloseActions(IEnumerable<FFActionCloseLogDue> details, DateTime closeDate, string userId)
        {
            //update process flag and date
            var ffDues = await _cpiDbContext.GetRepository<FFDue>().QueryableList.Where(d => 
                                        details.Select(l => l.DueId).Contains(d.PatDueDate.DDId) &&
                                        !(d.Exclude ?? false)   //do not process excluded actions
                                        ).ToListAsync();

            _cpiDbContext.GetRepository<FFDue>().Attach(ffDues);
            ffDues.ForEach(d => {
                d.CloseDate = closeDate;
                d.IsActionClosed = true;
                d.UpdatedBy = userId;
                d.LastUpdate = closeDate;
            });
            
            await _cpiDbContext.SaveChangesAsync();

            //detach ffDues to allow other updates
            _cpiDbContext.Detach(ffDues);

            //close due dates by updating due date date taken
            var dueIds = details.Select(d => d.DueId).ToList();
            var patDues = await _cpiDbContext.GetRepository<PatDueDate>().QueryableList.Where(d =>
                                    dueIds.Contains(d.DDId)
                                    //update remarks regardless of CloseAction flag
                                    //&& d.FFDue.ClientInstrxType.CloseAction //only close flagged instructions
                                    && !(d.FFDue.Exclude ?? false) //do not close excluded actions
                                    ).Select(d => new PatDueDate()
                                    {
                                        DDId = d.DDId,
                                        ActId = d.ActId,
                                        ActionDue = d.ActionDue,
                                        DueDate = d.DueDate,
                                        Indicator = d.Indicator,
                                        DateTaken = !d.FFDue.ClientInstrxType.CloseAction ? d.DateTaken : closeDate,
                                        Remarks = (d.Remarks ?? "") + ((d.Remarks ?? "") == "" ? "" : "\r\n") +
                                                    $"{closeDate.ToString("dd-MMM-yyyy")} {userId}: " +
                                                    $"Client instructed through Foreign Filing System.\r\n" +
                                                        $"Instruction: {d.FFDue.ClientInstrxType.ClientDescription}\r\n" +
                                                        (d.FFDue.ClientInstrxType.GetCountries ?
                                                            //Only show countries from All tab for Foreign Filing (do not show designations)
                                                            //$"Countries: {string.Join(", ", d.FFDue.FFDueCountries.Where(c => d.ActionDue != "Foreign Filing Due" || c.Source == "All").Select(c => c.Country))}\r\n" :
                                                            //Show all countries
                                                            $"Countries: {string.Join(", ", d.FFDue.FFDueCountries.OrderBy(c => c.Country).Select(c => c.Country))}\r\n" :
                                                            //Show all countries but group EP and WO designation (performance hit)
                                                            //$"Countries: {string.Join(", ", d.FFDue.FFDueCountries.Where(c => d.ActionDue != "Foreign Filing Due" || c.Source == "All").OrderBy(c => c.Country).Select(c => d.FFDue.FFDueCountries.Any(dc => dc.Source == c.Country) ? c.Country + "(" + string.Join(", ", d.FFDue.FFDueCountries.Where(des => des.Source == c.Country).OrderBy(des => des.Country).Select(des => des.Country)) + ")" : c.Country))}\r\n" :
                                                            //Show all countries with EP/WO label for designations
                                                            //$"Countries: {string.Join(", ", d.FFDue.FFDueCountries.OrderBy(c => c.Source).ThenBy(c => c.Country).Select(c => d.ActionDue != "Foreign Filing Due" || c.Source == "All" ? c.Country : c.Source + ":" + c.Country))}\r\n" :
                                                            "") +
                                                        $"Instruction Date: {((DateTime)d.FFDue.ClientInstructionDate).ToString("dd-MMM-yyyy")}\r\n" +
                                                        $"Instructed By: {d.FFDue.InstrxChangeLog.CreatedBy}",
                                        IsForVerify = d.IsForVerify,
                                        IsVerifyDate = d.IsVerifyDate,
                                        JobId_EPDS = d.JobId_EPDS,
                                        CreatedBy = d.CreatedBy,
                                        DateCreated = d.DateCreated,
                                        UpdatedBy = userId,
                                        LastUpdate = closeDate,
                                        tStamp = d.tStamp
                                    }).ToListAsync();

            //pct nat filing due 30 mos
            var woChapterIDDIds = patDues.Where(d => d.ActionDue == "Chapter I Due (Extended)").Select(d => d.DDId).ToList();
            if (woChapterIDDIds?.Any() ?? false)
            {
                //close Chapter II Due (Nat. Filing) when closing Chapter I Due (Extended)
                var woChapterIAppIds = await _cpiDbContext.GetRepository<PatDueDate>().QueryableList.Where(d =>
                                        woChapterIDDIds.Contains(d.DDId)).Select(d => d.PatActionDue.AppId).ToListAsync();
                var woChapterIIDues = await _cpiDbContext.GetRepository<PatDueDate>().QueryableList.Where(d =>
                                        woChapterIAppIds.Contains(d.PatActionDue.AppId) &&
                                        d.ActionDue == "Chapter II Due (Nat. Filing)"
                                        ).Select(d => new PatDueDate()
                                        {
                                            DDId = d.DDId,
                                            ActId = d.ActId,
                                            ActionDue = d.ActionDue,
                                            DueDate = d.DueDate,
                                            Indicator = d.Indicator,
                                            DateTaken = closeDate,
                                            Remarks = (d.Remarks ?? "") + ((d.Remarks ?? "") == "" ? "" : "\r\n") +
                                                        $"{closeDate.ToString("dd-MMM-yyyy")} {userId}: " +
                                                        $"Action closed through instruction from Foreign Filing System.",
                                            IsForVerify = d.IsForVerify,
                                            IsVerifyDate = d.IsVerifyDate,
                                            JobId_EPDS = d.JobId_EPDS,
                                            CreatedBy = d.CreatedBy,
                                            DateCreated = d.DateCreated,
                                            UpdatedBy = userId,
                                            LastUpdate = closeDate,
                                            tStamp = d.tStamp
                                        }).ToListAsync();
                patDues.AddRange(woChapterIIDues);

                //close Nat. Filing Due - 31 Months if all countries are already selected
                var woCount = await _cpiDbContext.GetRepository<PatDesCaseType>().QueryableList.Where(c => c.IntlCode == "WO" && c.CaseType == "ORD").CountAsync();
                var woNatlDueAppIds = await _cpiDbContext.GetRepository<PatDueDate>().QueryableList.Where(d =>
                                            woChapterIAppIds.Contains(d.PatActionDue.AppId) &&
                                            d.ActionDue == "Nat. Filing Due - 31 Months"
                                        ).Select(d => d.PatActionDue.AppId).ToListAsync();

                foreach(var appId in woNatlDueAppIds)
                {
                    var woDesCount = await _cpiDbContext.GetRepository<FFDueCountry>().QueryableList.Where(d =>
                                            d.FFDue.PatDueDate.PatActionDue.AppId == appId &&
                                            d.Source == "WO" &&   
                                            (dueIds.Contains(d.FFDue.DDId) ||  //count countries from this batch
                                            (d.FFDue.IsActionClosed ?? false)) //or from closed actions
                                        ).CountAsync();
                    if (woDesCount == woCount)
                    {
                        patDues.AddRange(await _cpiDbContext.GetRepository<PatDueDate>().QueryableList.Where(d =>
                                            d.PatActionDue.AppId == appId &&
                                            d.ActionDue == "Nat. Filing Due - 31 Months"
                                        ).Select(d => new PatDueDate()
                                        {
                                            DDId = d.DDId,
                                            ActId = d.ActId,
                                            ActionDue = d.ActionDue,
                                            DueDate = d.DueDate,
                                            Indicator = d.Indicator,
                                            DateTaken = closeDate,
                                            Remarks = (d.Remarks ?? "") + ((d.Remarks ?? "") == "" ? "" : "\r\n") +
                                                        $"{closeDate.ToString("dd-MMM-yyyy")} {userId}: " +
                                                        $"Action closed through instruction from Foreign Filing System.",
                                            IsForVerify = d.IsForVerify,
                                            IsVerifyDate = d.IsVerifyDate,
                                            JobId_EPDS = d.JobId_EPDS,
                                            CreatedBy = d.CreatedBy,
                                            DateCreated = d.DateCreated,
                                            UpdatedBy = userId,
                                            LastUpdate = closeDate,
                                            tStamp = d.tStamp
                                        }).ToListAsync());
                    }
                }
            }

            foreach (var actId in patDues.Select(d => d.ActId).Distinct().ToList())
            {
                await _patDueDateService.Update(actId, userId, patDues.Where(d => d.ActId == actId).ToList(), new List<PatDueDate>(), new List<PatDueDate>());
            }
        }
    }
}
