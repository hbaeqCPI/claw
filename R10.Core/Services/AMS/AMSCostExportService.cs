using Microsoft.EntityFrameworkCore;
using R10.Core.Entities.AMS;
using R10.Core.Entities.Patent;
using R10.Core.Exceptions;
using R10.Core.Helpers;
using R10.Core.Interfaces;
using R10.Core.Interfaces.AMS;
using System.Security.Claims;

namespace R10.Core.Services.AMS
{
    public interface IAMSCostExportService : IEntityService<AMSCostExportLog>
    {
        IQueryable<AMSDue> CostExportList { get; }
        decimal GetAnnuityCost(string paymentType, decimal? invoiceAmount, decimal? reviewAmount, decimal? reminderFaxAmount, decimal? settleAmount, decimal? secondReminderFaxAmount, decimal? serviceFee, decimal? vatAmount);

        Task<byte[]> SaveOption(int dueId, bool exclude, byte[]? tStamp);

        Task ExportCost(PatCostTrack cost, AMSCostExportLog log);
    }

    public class AMSCostExportService : EntityService<AMSCostExportLog>, IAMSCostExportService
    {
        private readonly IAMSDueService _amsDueService;

        public AMSCostExportService(ICPiDbContext cpiDbContext, ClaimsPrincipal user, IAMSDueService amsDueService) : base(cpiDbContext, user)
        {
            _amsDueService = amsDueService;
        }

        public IQueryable<AMSDue> CostExportList => _amsDueService.QueryableList
                                        .Where(d => d.CPIPaymentDate != null &&                 //PAID ANNUITIES
                                            !string.IsNullOrEmpty(d.CPIInvoiceNo) &&            //HAS INVOICE NUMBER
                                            d.CPIInvoiceDate.HasValue &&                        //HAS INVOICE DATE
                                            d.AMSMain.CountryApplication != null &&             //HAS APPLICATION RECORD
                                            (d.AMSCostExportLog == null || d.AMSCostExportLog.ProcessDate == null) &&   //NOT PROCESSED
                                            (d.AMSMain.CountryApplication.CostTrackings == null ||                      //NOT EXIST IN PAT COST TRACKING
                                                !d.AMSMain.CountryApplication.CostTrackings.Any(c => c.CostType == d.PaymentType && c.InvoiceNumber == d.CPIInvoiceNo && c.InvoiceDate.Date == ((DateTime)d.CPIInvoiceDate).Date))
                                            );

        public decimal GetAnnuityCost(string paymentType, decimal? invoiceAmount, decimal? reviewAmount, decimal? reminderFaxAmount, decimal? settleAmount, decimal? secondReminderFaxAmount, decimal? serviceFee, decimal? vatAmount)
        {
            return _amsDueService.GetAnnuityCost(paymentType, invoiceAmount, reviewAmount, reminderFaxAmount, settleAmount, secondReminderFaxAmount, serviceFee, vatAmount);
        }

        public async Task<byte[]> SaveOption(int dueId, bool exclude, byte[]? tStamp)
        {
            Guard.Against.RecordNotFound(dueId > 0);

            var log = await GetCostExportLogByDueId(dueId);

            //update timestamp before tracking to enforce concurrency control
            log.tStamp = tStamp;

            _cpiDbContext.GetRepository<AMSCostExportLog>().Attach(log);
            log.Exclude = exclude;
            log.UpdatedBy = _user.GetUserName();
            log.LastUpdate = DateTime.Now;

            await _cpiDbContext.SaveChangesAsync();
            _cpiDbContext.Detach(log);

            return log.tStamp;
        }

        public async Task ExportCost(PatCostTrack cost, AMSCostExportLog log)
        {
            Guard.Against.RecordNotFound(log.DueID > 0);

            var exportCost = !(log.Exclude ?? false);
            var costExportLog = await GetCostExportLogByDueId(log.DueID);
            costExportLog.tStamp = log.tStamp;

            _cpiDbContext.GetRepository<AMSCostExportLog>().Attach(costExportLog);
            costExportLog.Exclude = log.Exclude;
            costExportLog.UpdatedBy = _user.GetUserName();
            costExportLog.LastUpdate = DateTime.Now;

            if (exportCost)
            {
                costExportLog.ProcessDate = DateTime.Now;

                cost.CreatedBy = costExportLog.UpdatedBy;
                cost.UpdatedBy = costExportLog.UpdatedBy;
                cost.DateCreated = costExportLog.LastUpdate;
                cost.LastUpdate = costExportLog.LastUpdate;
                cost.Remarks = $"Generated from AMS Annuity Cost Export utility " +
                        $"by {costExportLog.UpdatedBy} " +
                        $"on {costExportLog.LastUpdate?.ToString("dd-MMM-yyyy hh:mm tt")}";
                _cpiDbContext.GetRepository<PatCostTrack>().Add(cost);
            }

            await _cpiDbContext.SaveChangesAsync();
            _cpiDbContext.Detach(costExportLog);
            _cpiDbContext.Detach(cost);
        }

        protected async Task<AMSCostExportLog> GetCostExportLogByDueId(int dueId) {
            var costExport =  await QueryableList.FirstOrDefaultAsync(l => l.AMSDue != null && l.AMSDue.DueID == dueId);

            if (costExport == null)
            {
                var userName = _user.GetUserName();
                var lastUpdate = DateTime.Now;
                costExport = new AMSCostExportLog()
                { 
                    DueID = dueId,
                    CreatedBy = userName,
                    DateCreated = lastUpdate,
                    UpdatedBy = userName,
                    LastUpdate = lastUpdate
                };
                await base.Add(costExport);
            }

            return costExport;
        }
    }
}
