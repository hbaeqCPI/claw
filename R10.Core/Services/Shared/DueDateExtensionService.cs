using R10.Core.Interfaces;
using System.Threading.Tasks;
using System.Collections.Generic;
using R10.Core.Entities;
using System.Linq;
using R10.Core.Interfaces.Patent;
using R10.Core.Entities.Patent;
using R10.Core.Identity;
using System.Linq.Expressions;
using System;
using System.Data;
using R10.Core.DTOs;
using Microsoft.EntityFrameworkCore;
using System.Collections;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using System.Transactions;

namespace R10.Core.Services
{
    public class DueDateExtensionService : IDueDateExtensionService
    {
        private readonly IApplicationDbContext _repository;

        public DueDateExtensionService(IApplicationDbContext repository)
        {
            _repository = repository;
        }

        public DateTime? ComputeRunDate(DueDateExtension setting)
        {
            var startDate = setting.StartDate ?? DateTime.Now.Date.AddDays(1);
            var nextRunDate = startDate;

            if (startDate > DateTime.Now.Date)
            {
                nextRunDate = startDate;
            }

            // for start date <= current date (rundate should be future, assuming that we can no longer run on current date)
            else
            {
                //if repeat interval is 0 (one time), then don't run
                if (setting.RepeatInterval <= 0 && setting.StartDate <= DateTime.Now.Date)
                {
                    return null;
                }

                switch (setting.RepeatRecurrence)
                {
                    case DueDateExtensionRecurrence.Day:
                        nextRunDate = startDate.AddDays((int)setting.RepeatInterval);
                        while (nextRunDate <= DateTime.Now.Date)
                        {
                            nextRunDate = nextRunDate.AddDays((int)setting.RepeatInterval);
                        }
                        break;

                    case DueDateExtensionRecurrence.Week:
                        nextRunDate = startDate.AddDays((int)setting.RepeatInterval * 7);
                        while (nextRunDate <= DateTime.Now.Date)
                        {
                            nextRunDate = nextRunDate.AddDays((int)setting.RepeatInterval * 7);
                        }

                        if (setting.RepeatOnDay >= 0 && setting.RepeatOnDay <= 6)
                        {
                            while (nextRunDate.DayOfWeek != (DayOfWeek)setting.RepeatOnDay)
                            {
                                nextRunDate = nextRunDate.AddDays(-1); //deduct 1 day until you meet the RepeatOnDay setting
                            }
                        }
                        break;

                    case DueDateExtensionRecurrence.Month:
                        nextRunDate = startDate.AddMonths((int)setting.RepeatInterval);
                        while (nextRunDate <= DateTime.Now.Date)
                        {
                            nextRunDate = nextRunDate.AddMonths((int)setting.RepeatInterval);
                        }
                        break;

                    default:
                        return null;
                }
            }

            switch (setting.StopIndicator)
            {
                case DueDateExtensionStopIndicator.On:
                    if (setting.StopDate.HasValue && setting.StopDate <= nextRunDate)
                        return null;

                    break;
                case DueDateExtensionStopIndicator.After:
                    if (setting.StopAfterCount <= setting.OccurenceCount)
                        return null;
                    break;
                default:
                    return nextRunDate;
            }
            return nextRunDate;
        }

        public DateTime ComputeNextDueDate(DateTime currentDueDate, int? months, int? weeks, int? days)
        {
            var newDueDate = currentDueDate;
            if (months > 0)
                newDueDate = newDueDate.AddMonths((int)months);

            if (weeks > 0)
                newDueDate = newDueDate.AddDays((int)weeks * 7);

            if (days > 0)
                newDueDate = newDueDate.AddDays((int)days);

            return newDueDate;
        }


        public async Task ExtendDueDate(string updatedBy)
        {
            await ExtendPatDueDate(updatedBy);
            await ExtendPatDueDateInv(updatedBy);
            await ExtendTmkDueDate(updatedBy);
            // await ExtendGMDueDate(updatedBy); // Removed during deep clean - GeneralMatter module removed
            // await ExtendDMSDueDate(updatedBy); // Removed during deep clean - DMS module removed
        }

        private async Task ExtendPatDueDate(string updatedBy)
        {
            var patDueDateExtensions = await _repository.PatDueDateExtensions.Where(e => e.PatDueDate.DateTaken == null && e.IsEnabled && e.NextRunDate <= DateTime.Now.Date).Include(e => e.PatDueDate).AsNoTracking().ToListAsync();
            foreach (var item in patDueDateExtensions)
            {
                var currentRunDate = item.NextRunDate;
                var newDueDate = ComputeNextDueDate(item.PatDueDate.DueDate, item.ExtendMonth, item.ExtendWeek, item.ExtendDay);
                ComputeNextRunDate(item);
                while (item.NextRunDate != null && item.NextRunDate > currentRunDate && item.NextRunDate < DateTime.Now.Date)
                {
                    newDueDate = ComputeNextDueDate(newDueDate, item.ExtendMonth, item.ExtendWeek, item.ExtendDay);
                    ComputeNextRunDate(item);
                }

                using (var scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted }, TransactionScopeAsyncFlowOption.Enabled))
                {
                    var result = await _repository.PatDueDateExtensions.Where(e => e.ExtensionId == item.ExtensionId && e.NextRunDate == currentRunDate)
                                           .ExecuteUpdateAsync(p => p.SetProperty(x => x.LastDueDate, x => item.PatDueDate.DueDate)
                                           .SetProperty(x => x.NewDueDate, x => newDueDate).SetProperty(x => x.NextRunDate, x => item.NextRunDate)
                                           .SetProperty(x => x.OccurenceCount, x => x.OccurenceCount + 1).SetProperty(x => x.LastRunDate, x => DateTime.Now.Date));

                    if (result > 0)
                    {
                        await _repository.PatDueDates.Where(dd => dd.DDId == item.DDId)
                           .ExecuteUpdateAsync(p => p.SetProperty(x => x.DueDate, x => newDueDate)
                           .SetProperty(x => x.UpdatedBy, x => updatedBy).SetProperty(x => x.LastUpdate, x => DateTime.Now));

                        await _repository.PatActionDues.Where(ad => ad.DueDates.Any(dd => dd.DDId == item.DDId))
                           .ExecuteUpdateAsync(p => p.SetProperty(x => x.UpdatedBy, x => updatedBy).SetProperty(x => x.LastUpdate, x => DateTime.Now));

                        var log = BuildDueDateExtensionLog(item, (DateTime)currentRunDate, newDueDate, item.PatDueDate.DueDate, "P");
                        _repository.DueDateExtensionsLog.Add(log);
                        await _repository.SaveChangesAsync();
                    }
                    scope.Complete();
                }
            }
        }

        private async Task ExtendPatDueDateInv(string updatedBy)
        {
            var patDueDateExtensions = await _repository.PatDueDateInvExtensions.Where(e => e.PatDueDateInv.DateTaken == null && e.IsEnabled && e.NextRunDate <= DateTime.Now.Date).Include(e => e.PatDueDateInv).AsNoTracking().ToListAsync();
            foreach (var item in patDueDateExtensions)
            {
                var currentRunDate = item.NextRunDate;
                var newDueDate = ComputeNextDueDate(item.PatDueDateInv.DueDate, item.ExtendMonth, item.ExtendWeek, item.ExtendDay);
                ComputeNextRunDate(item);
                while (item.NextRunDate != null && item.NextRunDate > currentRunDate && item.NextRunDate < DateTime.Now.Date)
                {
                    newDueDate = ComputeNextDueDate(newDueDate, item.ExtendMonth, item.ExtendWeek, item.ExtendDay);
                    ComputeNextRunDate(item);
                }

                using (var scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted }, TransactionScopeAsyncFlowOption.Enabled))
                {
                    var result = await _repository.PatDueDateInvExtensions.Where(e => e.ExtensionId == item.ExtensionId && e.NextRunDate == currentRunDate)
                                           .ExecuteUpdateAsync(p => p.SetProperty(x => x.LastDueDate, x => item.PatDueDateInv.DueDate)
                                           .SetProperty(x => x.NewDueDate, x => newDueDate).SetProperty(x => x.NextRunDate, x => item.NextRunDate)
                                           .SetProperty(x => x.OccurenceCount, x => x.OccurenceCount + 1).SetProperty(x => x.LastRunDate, x => DateTime.Now.Date));

                    if (result > 0)
                    {
                        await _repository.PatDueDateInvs.Where(dd => dd.DDId == item.DDId)
                           .ExecuteUpdateAsync(p => p.SetProperty(x => x.DueDate, x => newDueDate)
                           .SetProperty(x => x.UpdatedBy, x => updatedBy).SetProperty(x => x.LastUpdate, x => DateTime.Now));

                        await _repository.PatActionDueInvs.Where(ad => ad.DueDateInvs.Any(dd => dd.DDId == item.DDId))
                           .ExecuteUpdateAsync(p => p.SetProperty(x => x.UpdatedBy, x => updatedBy).SetProperty(x => x.LastUpdate, x => DateTime.Now));

                        var log = BuildDueDateExtensionLog(item, (DateTime)currentRunDate, newDueDate, item.PatDueDateInv.DueDate, "I");
                        _repository.DueDateExtensionsLog.Add(log);
                        await _repository.SaveChangesAsync();
                    }
                    scope.Complete();
                }
            }
        }

        private async Task ExtendTmkDueDate(string updatedBy)
        {
            var tmkDueDateExtensions = await _repository.TmkDueDateExtensions.Where(e => e.TmkDueDate.DateTaken == null && e.IsEnabled && e.NextRunDate <= DateTime.Now.Date).Include(e => e.TmkDueDate).AsNoTracking().ToListAsync();
            foreach (var item in tmkDueDateExtensions)
            {
                var currentRunDate = item.NextRunDate;
                var newDueDate = ComputeNextDueDate(item.TmkDueDate.DueDate, item.ExtendMonth, item.ExtendWeek, item.ExtendDay);
                ComputeNextRunDate(item);
                while (item.NextRunDate != null && item.NextRunDate > currentRunDate && item.NextRunDate < DateTime.Now.Date)
                {
                    newDueDate = ComputeNextDueDate(newDueDate, item.ExtendMonth, item.ExtendWeek, item.ExtendDay);
                    ComputeNextRunDate(item);
                }

                using (var scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted }, TransactionScopeAsyncFlowOption.Enabled))
                {
                    var result = await _repository.TmkDueDateExtensions.Where(e => e.ExtensionId == item.ExtensionId && e.NextRunDate == currentRunDate)
                                           .ExecuteUpdateAsync(p => p.SetProperty(x => x.LastDueDate, x => item.TmkDueDate.DueDate)
                                           .SetProperty(x => x.NewDueDate, x => newDueDate).SetProperty(x => x.NextRunDate, x => item.NextRunDate)
                                           .SetProperty(x => x.OccurenceCount, x => x.OccurenceCount + 1).SetProperty(x => x.LastRunDate, x => DateTime.Now.Date));

                    if (result > 0)
                    {
                        await _repository.TmkDueDates.Where(dd => dd.DDId == item.DDId)
                           .ExecuteUpdateAsync(p => p.SetProperty(x => x.DueDate, x => newDueDate)
                           .SetProperty(x => x.UpdatedBy, x => updatedBy).SetProperty(x => x.LastUpdate, x => DateTime.Now));

                        await _repository.TmkActionDues.Where(ad => ad.DueDates.Any(dd => dd.DDId == item.DDId))
                           .ExecuteUpdateAsync(p => p.SetProperty(x => x.UpdatedBy, x => updatedBy).SetProperty(x => x.LastUpdate, x => DateTime.Now));

                        var log = BuildDueDateExtensionLog(item, (DateTime)currentRunDate, newDueDate, item.TmkDueDate.DueDate, "T");
                        _repository.DueDateExtensionsLog.Add(log);
                        await _repository.SaveChangesAsync();
                    }
                    scope.Complete();
                }
            }
        }

        // Removed during deep clean - GeneralMatter module removed
        // private async Task ExtendGMDueDate(string updatedBy)
        // {
        //     var gmDueDateExtensions = await _repository.GMDueDateExtensions.Where(e => e.GMDueDate.DateTaken == null && e.IsEnabled && e.NextRunDate <= DateTime.Now.Date).Include(e => e.GMDueDate).AsNoTracking().ToListAsync();
        //     foreach (var item in gmDueDateExtensions)
        //     {
        //         ...
        //     }
        // }

        // Removed during deep clean - DMS/Disclosure module removed
        // private async Task ExtendDMSDueDate(string updatedBy)
        // {
        //     var dmsDueDateExtensions = await _repository.DMSDueDateExtensions.Where(e => e.DMSDueDate.DateTaken == null && e.IsEnabled && e.NextRunDate <= DateTime.Now.Date).Include(e => e.DMSDueDate).AsNoTracking().ToListAsync();
        //     foreach (var item in dmsDueDateExtensions)
        //     {
        //         ...
        //     }
        // }

        private DueDateExtensionLog BuildDueDateExtensionLog(DueDateExtension setting, DateTime currentRunDate, DateTime newDueDate, DateTime lastDueDate, string systemType)
        {
            var log = new DueDateExtensionLog
            {
                ExtensionId = setting.ExtensionId,
                DDId = setting.DDId,
                SystemType = systemType,
                ExtendDay = setting.ExtendDay,
                ExtendWeek = setting.ExtendWeek,
                ExtendMonth = setting.ExtendMonth,
                RepeatInterval = setting.RepeatInterval,
                RepeatRecurrence = setting.RepeatRecurrence,
                RepeatOnDay = setting.RepeatOnDay,
                StopIndicator = setting.StopIndicator,
                StopDate = setting.StopDate,
                StopAfterCount = setting.StopAfterCount,
                OccurenceCount = setting.OccurenceCount+1,
                NewDueDate = newDueDate,
                LastDueDate = lastDueDate,
                NextRunDate = currentRunDate,
                ExecutedOn = DateTime.Now
            };
            return log;
        }

        private void ComputeNextRunDate(DueDateExtension setting)
        {
            var runDate = (DateTime)setting.NextRunDate;

            // repeat interval is 0 (one time)
            if (setting.RepeatInterval <= 0)
            {
                setting.NextRunDate = null;
                return;
            }

            switch (setting.RepeatRecurrence)
            {
                case DueDateExtensionRecurrence.Day:
                    setting.NextRunDate = runDate.AddDays((int)setting.RepeatInterval);
                    break;

                case DueDateExtensionRecurrence.Week:
                    setting.NextRunDate = runDate.AddDays((int)setting.RepeatInterval * 7);

                    if (setting.RepeatOnDay >= 0 && setting.RepeatOnDay <= 6)
                    {
                        while (((DateTime)setting.NextRunDate).DayOfWeek != (DayOfWeek)setting.RepeatOnDay)
                        {
                            setting.NextRunDate = ((DateTime)setting.NextRunDate).AddDays(-1); //deduct 1 day until you meet the RepeatOnDay setting
                        }
                    }
                    break;

                case DueDateExtensionRecurrence.Month:
                    setting.NextRunDate = runDate.AddMonths((int)setting.RepeatInterval);
                    break;

                default:
                    return;
            }

            switch (setting.StopIndicator)
            {
                case DueDateExtensionStopIndicator.On:
                    if (setting.StopDate.HasValue && setting.StopDate <= setting.NextRunDate)
                    {
                        setting.NextRunDate = null;
                        return;
                    }
                    break;

                case DueDateExtensionStopIndicator.After:
                    if (setting.StopAfterCount <= setting.OccurenceCount)
                    {
                        setting.NextRunDate = null;
                        return;
                    }
                    break;
            }
            return;
        }


    }
}
