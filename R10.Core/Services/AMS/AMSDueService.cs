using Microsoft.EntityFrameworkCore;
using R10.Core.Entities;
using R10.Core.Entities.AMS;
using R10.Core.Exceptions;
using R10.Core.Helpers;
using R10.Core.Identity;
using R10.Core.Interfaces;
using R10.Core.Interfaces.AMS;
using R10.Core.Queries.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace R10.Core.Services.AMS
{
    public class AMSDueService : ChildEntityService<AMSMain, AMSDue>, IAMSDueService
    {
        private readonly IAMSMainService _amsMainService;
        private readonly IAMSFeeService _feeService;
        private readonly ISystemSettings<AMSSetting> _amsSettings;

        public AMSDueService(
            IAMSMainService amsMainService,
            IAMSFeeService feeService,
            ISystemSettings<AMSSetting> amsSettings,
            ICPiDbContext cpiDbContext,
            ClaimsPrincipal user) : base(cpiDbContext, user)
        {
            _amsMainService = amsMainService;
            _feeService = feeService;
            _amsSettings = amsSettings;
        }

        public override IQueryable<AMSDue> QueryableList
        {
            get
            {
                var queryableList = base.QueryableList
                                                .Where(d =>
                                                    d.CPIDeleteFlag == false &&
                                                    (EF.Functions.Like(d.PaymentType, "ANN%") || d.PaymentType.ToUpper() == "WORKING") &&
                                                    _amsMainService.QueryableList.Any(ams => ams.AnnID == d.AnnID)
                                                    );

                return queryableList;
            }
        }

        //TODO: VERIFY WHERE AMSMain.SendReminder IS COMING FROM
        //public Expression<Func<AMSDue, bool>> NeedsInstruction => IsInstructable.And(d => !d.IgnoreRecord && (!(d.AMSMain.EndClientHandles ?? false) || (d.AMSMain.SendReminder ?? false)));

        public IQueryable<AMSDue> PortfolioReviewList => QueryableList
                                                //.Where(NeedsInstruction);
                                                .Where(Instructable.And(d => !d.IgnoreRecord && (!(d.AMSMain.EndClientHandles ?? false) || (d.AMSMain.SendReminder ?? false))));

        public IQueryable<AMSDue> InstructableList => QueryableList
                                                .Where(Instructable.And(d => !d.IgnoreRecord && (!(d.AMSMain.EndClientHandles ?? false))));

        // Instructable due dates that are close to end of grace date
        // Also known as "NEEDS SPECIAL HANDLING"
        public IQueryable<AMSDue> InGracePeriodList => InstructableList.Where(InGracePeriod);        

        // Past due with CPIGraceDate
        public IQueryable<AMSDue> PastDueWithGraceDateList => QueryableList
                                                .Where((Instructable.Or(InGracePeriod)) // includes not instructable but still InGracePeriod
                                                        .And(d => !d.IgnoreRecord && (!(d.AMSMain.EndClientHandles ?? false)) &&
                                                                   d.AnnuityDueDate < DateTime.Now.Date && d.CPIGraceDate != null));

        //vwWebAMSInstrx_ToCPi
        //WHERE (ISNULL(due.ClientInstructionType, '') <> '') AND (due.ClientInstructionSentToCPIFlag = 0) AND 
        //      (ISNULL(main.EndClientHandles, 0) = 0) AND (ISNULL(due.IgnoreRecord, 0) = 0) AND 
        //      (main.CPIDeleteFlag = 0) AND (due.CPIDeleteFlag = 0)
        //
        //include non-instructable records that are still in actual grace period
        public IQueryable<AMSDue> InstructionsToCPiList
        {
            get
            {
                var queryableList = QueryableList
                                                .Where((Instructable.Or(InGracePeriod)).And(d =>
                                                        !string.IsNullOrEmpty(d.ClientInstructionType) && !d.ClientInstructionSentToCPIFlag &&
                                                        !d.IgnoreRecord && !(d.AMSMain.EndClientHandles ?? false)
                                                ));

                //only return records with full modify permission if resp office is on
                if (_user.HasRespOfficeFilter(SystemType.AMS))
                    return queryableList.Where(d => CPiUserSystemRoles.Any(r => r.UserId == UserId && r.SystemId == SystemType.AMS && 
                                                                                d.AMSMain.CPIClientCode == r.RespOffice && 
                                                                                CPiPermissions.FullModify.Contains(r.RoleId.ToLower())));

                return queryableList;
            }
        }

        public IQueryable<AMSProjection> ProjectionList
        {
            get
            {
                var queryableList = _cpiDbContext.GetRepository<AMSProjection>().QueryableList
                                                .Where(d =>
                                                    d.CPIDeleteFlag == false && (d.AMSDue == null || d.AMSDue.IgnoreRecord == false) && //PROJECTION DATA MAY NOT HAVE MATCHING AMSDUE DATA
                                                    (d.AMSMain.CPIStopDate == null || d.AMSMain.CPIStopDate >= d.AnnuityDueDate) &&
                                                    (d.AMSMain.Country != "EP" || d.AMSMain.CPIIssDate == null || d.AnnuityDueDate <= d.AMSMain.CPIIssDate) &&
                                                    _amsMainService.QueryableList.Any(ams => ams.AnnID == d.AnnID)
                                                    );

                return queryableList;
            }
        }

        public IQueryable<AMSInstrxChangeLog> InstrxChangeLogList => _cpiDbContext.GetRepository<AMSInstrxChangeLog>().QueryableList;
        
        public IQueryable<AMSInstrxDecisionMgt> InstrxDecisionMgtList => _cpiDbContext.GetRepository<AMSInstrxDecisionMgt>().QueryableList;

        public IQueryable<ClientContact> DecisionMakers => _cpiDbContext.GetRepository<ClientContact>().QueryableList;

        public IQueryable<AMSDue> PaidWithNoPostedReceiptList
        {
            get
            {
                var invalidStatuses = new List<string>() { "3RD PARTY", "ABANDONED", "NOT VALID", "EXPIRED", "UNPAYABLE", "NOTPAYABLE", "CANCELLED", "PART OF UP" };
                var queryableList = base.QueryableList
                                                .Where(d => d.CPIDeleteFlag == false && d.IgnoreRecord == false
                                                    && d.CPIPaymentDate != null && d.CPIReceiptPostDate == null
                                                    && _amsMainService.QueryableList.Any(ams => ams.AnnID == d.AnnID)
                                                    && (d.CPIPaymentStatus == null || !invalidStatuses.Contains(d.CPIPaymentStatus))
                                                ); 
                return queryableList;
            }
        }

        /// <summary>
        /// Instructable flag computation ported from AS/400.
        /// The program will set the instructable flag to Y if the following conditions are all true; otherwise, the flag is set to N:
        /// - case has not been paid
        /// - no stop date or stop date >= due date
        /// - start date <= due date
        /// - system date + 2 working days < end of grace period
        /// - if *EPO and status = I: Issue year = due year and TSD month/day < Issue month/day
        ///
        /// Exceptions: 
        /// - if (case type = EPC or EDV or SEP or SED)  and issue date = spaces or issue date > due date, set the instructable flag to N
        /// - if case type = EUC, set the instructable flag to N
        ///
        /// Notes:
        /// Stop date -> stop date is taken from the patent file or client file or end client file whichever is the earliest
        /// Start date -> start date is taken from the client file or end client file whichever is the earliest
        /// </summary>
        /// <returns></returns>
        private static Expression<Func<AMSDue, bool>> Instructable
        {
            get
            {
                var graceDateEnd = DateTime.Now.AddBusinessDays(2).Date;
                List<string> caseTypes = new List<string>() { "EPC", "EDV", "SEP", "SED" };

                return d => d.AnnuityDueDate.HasValue &&
                            d.CPIPaymentDate.HasValue == false &&
                            (d.AMSMain.CPIStopDate == null || d.AMSMain.CPIStopDate >= d.AnnuityDueDate) &&
                            (d.CPIGraceDate == null || d.CPIGraceDate >= graceDateEnd) &&
                            d.AMSMain.CPICaseType != "EUC" &&
                            (!caseTypes.Any(c => c == d.AMSMain.CPICaseType) || (d.AMSMain.CPIIssDate.HasValue && d.AMSMain.CPIIssDate <= d.AnnuityDueDate)) &&
                            (d.AMSMain.Country != "EP" || d.AMSMain.CPIStatus != "Granted" || (((DateTime)d.AMSMain.CPIIssDate).Year == ((DateTime)d.AnnuityDueDate).Year && ((((DateTime)d.AMSMain.CPITaxStartDate).Month * 100) + ((DateTime)d.AMSMain.CPITaxStartDate).Day < (((DateTime)d.AMSMain.CPIIssDate).Month * 100) + ((DateTime)d.AMSMain.CPIIssDate).Day)));
            }
        }

        /// <summary>
        /// Checks if CPIGraceDate is within 4 business days of current date
        /// to alert users that intructions to CPI are urgent.
        /// 
        /// Show non-instructable due dates that are still in grace period (-2 business days)
        /// so users may still call CPI to process their instructions.
        /// </summary>
        private static Expression<Func<AMSDue, bool>> InGracePeriod
        {
            get
            {
                var alertDays = 2;
                var graceDateStart = DateTime.Now.AddBusinessDays(-2).Date;
                var graceDateEnd = DateTime.Now.AddBusinessDays(2 + alertDays).Date;

                return d => d.CPIPaymentDate == null && d.CPIGraceDate != null && d.CPIGraceDate >= graceDateStart && d.CPIGraceDate <= graceDateEnd;
            }
        }

        /// <summary>
        /// Need to enter ClientPaymentDate before sending to CPI when:
        /// ClientInstructionType is "Y" (Pay) and
        /// Client.PayBeforeSending is true
        /// </summary>
        public Expression<Func<AMSDue, bool>> ClientPaymentNeeded
        {
            get
            {
                if (_user.IsAMSIntegrated())
                    return d => d.ClientInstructionType == "Y" && (d.AMSMain.CountryApplication == null ? (d.AMSMain.Client.PayBeforeSending ?? false) : (d.AMSMain.CountryApplication.Invention.Client.PayBeforeSending ?? false));
                else
                    return d => d.ClientInstructionType == "Y" && (d.AMSMain.Client.PayBeforeSending ?? false);
            }
        }

        //todo: NOT ClientPaymentNeeded ???
        //private Expression<Func<AMSDue, bool>> ClientPaymentNotNeeded =>
        //    Expression.Lambda<Func<AMSDue, bool>>(Expression.Not(ClientPaymentNeeded.Body), ClientPaymentNeeded.Parameters);
        private Expression<Func<AMSDue, bool>> ClientPaymentNotNeeded
        {
            get
            {
                if (_user.IsAMSIntegrated())
                    return d => d.ClientInstructionType != "Y" || (d.AMSMain.CountryApplication == null ? (d.AMSMain.Client.PayBeforeSending ?? false) : (d.AMSMain.CountryApplication.Invention.Client.PayBeforeSending ?? false)) == false;
                else
                    return d => d.ClientInstructionType != "Y" || (d.AMSMain.Client.PayBeforeSending ?? false) == false;
            }
        }

        public bool IsInstructable(DateTime? dueDate, DateTime? paymentDate, DateTime? stopDate, DateTime? graceDate,
            string caseType, DateTime? issDate, string country, string status, DateTime? taxStartDate)
        {
            var d = (new List<AMSDue>()
            {
                new AMSDue() {
                    AMSMain = new AMSMain() {
                        CPIStopDate = stopDate,
                        CPICaseType = caseType,
                        CPIIssDate = issDate,
                        Country = country,
                        CPIStatus = status,
                        CPITaxStartDate = taxStartDate
                    },
                    AnnuityDueDate = dueDate, CPIPaymentDate = paymentDate,
                    CPIGraceDate = graceDate
                }
            }).AsQueryable();

            return d.Where(Instructable).Any();
        }

        public bool IsInGracePeriod(DateTime? graceDate, DateTime? paymentDate)
        {
            var d = (new List<AMSDue>()
            {
                new AMSDue() { CPIPaymentDate = paymentDate, CPIGraceDate = graceDate }
            }).AsQueryable();

            return d.Where(InGracePeriod).Any();
        }

        public bool IsClientPaymentNeeded(string instructionType, bool? payBeforeSending)
        {
            var d = (new List<AMSDue>()
            {
                new AMSDue() {
                    AMSMain = new AMSMain() {
                        Client = new Client()
                        {
                            PayBeforeSending = payBeforeSending
                        }
                    },
                    ClientInstructionType = instructionType
                }
            }).AsQueryable();

            return d.Where(ClientPaymentNeeded).Any();
        }

        //public Expression<Func<AMSDue, bool>> ExcludeInstructed => d => string.IsNullOrEmpty(d.ClientInstructionType) || d.ClientInstrxType.Remind;
        public async Task<Expression<Func<AMSDue, bool>>> ExcludeInstructed()
        {
            if (await IsDecisionManagementUser())
            {
                var contactId = await GetUserEntityId();
                if (_user.IsAMSIntegrated())
                    return d => ((d.AMSMain.CountryApplication == null ? d.AMSMain.Client.UseDecisionMgt ?? false : d.AMSMain.CountryApplication.Invention.Client.UseDecisionMgt ?? false) && 
                        (string.IsNullOrEmpty(d.AMSInstrxDecisionMgt.Where(dm => dm.ContactID == contactId).FirstOrDefault().ClientInstructionType) ||
                        d.AMSInstrxDecisionMgt.Where(dm => dm.ContactID == contactId).FirstOrDefault().AMSInstrxType.Remind)) ||
                        (!(d.AMSMain.CountryApplication == null ? d.AMSMain.Client.UseDecisionMgt ?? false : d.AMSMain.CountryApplication.Invention.Client.UseDecisionMgt ?? false) &&
                        (string.IsNullOrEmpty(d.ClientInstructionType) || d.ClientInstrxType.Remind));
                else
                    return d => ((d.AMSMain.Client.UseDecisionMgt ?? false) && 
                        (string.IsNullOrEmpty(d.AMSInstrxDecisionMgt.Where(dm => dm.ContactID == contactId).FirstOrDefault().ClientInstructionType) ||
                        d.AMSInstrxDecisionMgt.Where(dm => dm.ContactID == contactId).FirstOrDefault().AMSInstrxType.Remind)) ||
                        (!(d.AMSMain.Client.UseDecisionMgt ?? false) && (string.IsNullOrEmpty(d.ClientInstructionType) || d.ClientInstrxType.Remind));
            }
                
            return d => string.IsNullOrEmpty(d.ClientInstructionType)
                            || !d.ClientInstrxType.InUse //show not in use instrx as not instructed
                            || d.ClientInstrxType.Remind;
        }

        public Expression<Func<AMSDue, bool>> ExcludeCPIInstructed => d => string.IsNullOrEmpty(d.CPIInstructionType) || d.CPIInstructionType == "HOLD";
        public Expression<Func<AMSDue, bool>> ExcludeNP => d => (d.PaidThru ?? "") != "NP";

        public async Task<IQueryable<AMSDue>> GetAnnuities(bool outstandingOnly)
        {
            var annuities = QueryableList;

            if (outstandingOnly)
            {
                var amsSettings = await _amsSettings.GetSetting();

                //TODO: REVIEW ISOUTSTANDING CRITERIA
                //FROM procWebAMSDue_Select
                //  cross apply(values(
                //   CASE WHEN d.CPIPaymentDate IS NULL AND asts.ActiveSwitch = 1 AND d.CPIDeleteFlag = 0 AND
                //                ((d.PaymentType) LIKE 'ANN%' OR(d.PaymentType) = 'WORKING') 
                //                    AND(m.CPIStopDate IS NULL OR m.CPIStopDate > d.[AnnuityDueDate])
                //                    AND(m.CPIExpireDate IS NULL OR m.CPIExpireDate >= d.[AnnuityDueDate])
                //                    AND(@ShowIgnore = 1 OR(d.IgnoreRecord) = 0)
                //                    AND((dbo.fnAMSAnnuity_IncludeHistorical(d.[AnnuityDueDate], getdate(), m.Country)) <> 0)
                //                    AND((dbo.fnAMSAnnuity_EPDue(m.[Country], d.[AnnuityDueDate], m.[CPIIssDate])) <> 0)
                //                    AND((dbo.fnAMSAnnuity_PastPaidThru(d.[AnnuityYear], d.[PaidThru])) <> 0)
                //                    AND((@HasCPIInstrxonRem = 0 AND d.CPIInstructionDate IS NULL) OR @HasCPIInstrxonRem<> 0) 
                //                    AND((@HasNPonReminder = 0 AND '' + d.PaidThru <> 'NP') OR @HasNPonReminder<> 0) 
                //  THEN 1 ELSE 0 END
                //  )
                //  ) as xa(IsOutstanding)

                annuities = InstructableList
                                .Where(d =>
                                    //d.AMSMain.CountryApplication.PatApplicationStatus.ActiveSwitch
                                    d.AMSMain.PatApplicationStatus.ActiveSwitch

                                    //filters that are in procWebAMSDue_Select but not used in tickler/reminder/instructions:
                                    //fnAMSAnnuity_IncludeHistorical
                                    //fnAMSAnnuity_EPDue (same in instructable filter)
                                    //fnAMSAnnuity_PastPaidThru
                                    //(d.AMSMain.CPIExpireDate == null || d.AMSMain.CPIExpireDate >= d.AnnuityDueDate)
                                );

                if (!amsSettings.PortfolioHasCPIInstructed)
                    annuities = annuities.Where(ExcludeCPIInstructed);

                if (!amsSettings.PortfolioHasNP)
                    annuities = annuities.Where(ExcludeNP);
            }

            return annuities;
        }

        public async Task<IQueryable<AMSDue>> GetInstructionsToCPi()
        {
            //exclude InGraceDate
            var instructionsToCPi = InstructionsToCPiList.Where(Instructable);

            //validate PayBeforeSending if LAWFIRM
            var amsSettings = await _amsSettings.GetSetting();
            if (!amsSettings.IsCorporation)
            {
                //USE REUSABLE EXPRESSION
                //if (_user.IsAMSIntegrated())
                //    instructionsToCPi = instructionsToCPi.Where(d =>
                //            d.ClientInstructionType != "Y" || (d.AMSMain.CountryApplication == null ? (d.AMSMain.Client.PayBeforeSending ?? false) == false : (d.AMSMain.CountryApplication.Invention.Client.PayBeforeSending ?? false) == false) ||
                //            (d.ClientInstructionType == "Y" && (d.AMSMain.CountryApplication == null ? (d.AMSMain.Client.PayBeforeSending ?? false) : (d.AMSMain.CountryApplication.Invention.Client.PayBeforeSending ?? false)) &&
                //            d.ClientPaymentDate != null));
                //else
                //    instructionsToCPi = instructionsToCPi.Where(d =>
                //            d.ClientInstructionType != "Y" || (d.AMSMain.Client.PayBeforeSending ?? false) == false ||
                //            (d.ClientInstructionType == "Y" && (d.AMSMain.Client.PayBeforeSending ?? false) &&
                //            d.ClientPaymentDate != null));

                //ClientPaymentNeeded Equals False DOES NOT WORK
                //instructionsToCPi = instructionsToCPi.Where((ClientPaymentNeeded.And(d => d.ClientPaymentDate != null)).Or(d =>
                //    ClientPaymentNeeded.Equals(ExpressionHelper.False<AMSDue>())));

                instructionsToCPi = instructionsToCPi.Where((ClientPaymentNeeded.And(d => d.ClientPaymentDate != null))
                                                            .Or(ClientPaymentNotNeeded));
            }

            return instructionsToCPi;
        }

        public decimal GetAnnuityCost(string paymentType, decimal? invoiceAmount, decimal? reviewAmount, decimal? reminderFaxAmount, decimal? settleAmount, decimal? secondReminderFaxAmount, decimal? serviceFee = 0, decimal? vatAmount = 0)
        {
            decimal annuityCost = 0;
            paymentType = paymentType.ToUpper();

            if (!(paymentType.StartsWith("ANN") || paymentType == "WORKING" || paymentType == "WRKG" || paymentType == "WKG"))
                annuityCost = annuityCost + (settleAmount ?? 0);
            else if (invoiceAmount > 0)
                annuityCost = annuityCost + (invoiceAmount ?? 0) + (settleAmount ?? 0);
            else if (settleAmount > 0)
                annuityCost = annuityCost + (settleAmount ?? 0);
            else if (reviewAmount > 0)
                annuityCost = annuityCost + (reviewAmount ?? 0);
            else if (reminderFaxAmount > 0)
                annuityCost = annuityCost + (reminderFaxAmount ?? 0);
            else
                annuityCost = (secondReminderFaxAmount ?? 0);

            return annuityCost + (serviceFee ?? 0) + (vatAmount ?? 0);
        }

        public override async Task<AMSDue> GetByIdAsync(int dueId)
        {
            return await QueryableList.SingleOrDefaultAsync(d => d.DueID == dueId);
        }

        public IQueryable<ClientContact> GetDecisionMakers(int clientId)
        {
            return DecisionMakers.Where(cc => cc.ClientID == clientId && (cc.IsDecisionMaker ?? false));
        }

        public async Task<bool> IsDecisionManagementUser()
        {
            var amsSettings = await _amsSettings.GetSetting();
            return _user.GetUserType() == CPiUserType.ContactPerson && amsSettings.HasDecisionMgt;
        }

        public async Task<bool> IsUseDecisionManagement(int dueId)
        {
            var amsSettings = await _amsSettings.GetSetting();
            if (amsSettings.HasDecisionMgt)
                return await QueryableList.AnyAsync(d => d.DueID == dueId && (
                        !_user.IsAMSIntegrated() || d.AMSMain.CountryApplication == null ?
                        d.AMSMain.Client.UseDecisionMgt ?? false :
                        d.AMSMain.CountryApplication.Invention.Client.UseDecisionMgt ?? false
                        ));

            return false;
        }

        public async Task<List<string>> GetDecisionMakerClients()
        {
            var contactId = await GetUserEntityId();
            return await DecisionMakers
                                        .Where(cc => cc.ContactID == contactId && (cc.IsDecisionMaker ?? false))
                                        .Select(cc => cc.Client.ClientCode).ToListAsync();
        }

        public async Task<List<int>> GetDecisionConflicts(List<int> dueIds)
        {
            var dueIdClients = QueryableList
                                    .Where(d => dueIds.Contains(d.DueID) &&
                                        ((!_user.IsAMSIntegrated() || d.AMSMain.CountryApplication == null ?
                                        d.AMSMain.Client.UseDecisionMgt :
                                        d.AMSMain.CountryApplication.Invention.Client.UseDecisionMgt) ?? false) ? true : false)
                                    .Select(d => new {
                                        DueID = d.DueID,
                                        ClientID = !_user.IsAMSIntegrated() || d.AMSMain.CountryApplication == null ?
                                            d.AMSMain.Client.ClientID :
                                            d.AMSMain.CountryApplication.Invention.ClientID ?? 0
                                        });
            var clientContacts = DecisionMakers.Where(cc => cc.IsDecisionMaker ?? false);
            var decisionMakers = from d in dueIdClients
                               join cc in clientContacts on d.ClientID equals cc.ClientID
                               select new {
                                   DueID = d.DueID,
                                   ClientID = d.ClientID,
                                   ContactID = cc.ContactID
                               };
            var instructions = from d in decisionMakers
                               join i in InstrxDecisionMgtList on 
                                    new { DueID = d.DueID, ContactID = d.ContactID } equals 
                                    new { DueID = i.DueID, ContactID = i.ContactID } into decisionMgt
                               from dm in decisionMgt.DefaultIfEmpty()
                               select new {
                                   DueID = d.DueID,
                                   ClientInstructionType = dm.ClientInstructionType ?? ""
                               };

            return await instructions.Distinct().GroupBy(i => i.DueID).Where(g => g.Count() > 1).Select(g => g.Key).ToListAsync();
        }

        public async Task<List<int>> GetDecisionConflicts()
        {
            var instructions = from cc in DecisionMakers
                               join d in InstructableList on cc.Client.ClientCode equals
                                    !_user.IsAMSIntegrated() || d.AMSMain.CountryApplication == null ? d.AMSMain.CPIClient : d.AMSMain.CountryApplication.Invention.Client.ClientCode
                               join dm in InstrxDecisionMgtList on
                                    new { dueId = d.DueID, contactId = cc.ContactID } equals
                                    new { dueId = dm.DueID, contactId = dm.ContactID } into gj //group join/left outer join
                               from i in gj.DefaultIfEmpty()
                               where (cc.Client.UseDecisionMgt ?? false) && (cc.IsDecisionMaker ?? false)
                               select new
                               {
                                   DueID = d.DueID,
                                   ClientInstructionType = i.ClientInstructionType ?? ""
                               };

            return await instructions.Distinct().GroupBy(i => i.DueID).Where(g => g.Count() > 1).Select(g => g.Key).ToListAsync();
        }

        //private async Task<bool> HasDecisionMgtConflict(int dueId, int clientId)
        //{
        //    var decisionMakers = GetDecisionMakers(clientId);
        //    var decisions = InstrxDecisionMgtList.Where(d => d.DueID == dueId);
        //    var instructions = await (from cc in decisionMakers
        //                        join d in decisions on cc.ContactID equals d.ContactID into decisionMgt
        //                        from dm in decisionMgt.DefaultIfEmpty()
        //                        select dm.ClientInstructionType ?? "").Distinct().CountAsync();

        //    return instructions > 1;
        //}

        private async Task<string> GetDecisionMgtFinalInstruction(int dueId, int clientId)
        {
            var decisionMakers = GetDecisionMakers(clientId);
            var decisions = InstrxDecisionMgtList.Where(d => d.DueID == dueId);
            var instructions = await (from cc in decisionMakers
                                      join d in decisions on cc.ContactID equals d.ContactID into decisionMgt
                                      from dm in decisionMgt.DefaultIfEmpty()
                                      select dm.ClientInstructionType ?? "").Distinct().ToListAsync();

            return instructions.Count == 1 ? instructions[0] : "";
        }

        private async Task<byte[]> SaveDecisionMgtInstruction(int dueId, int clientId, string instructionType, byte[] tStamp, string userName)
        {
            //validate if contact person is decision maker
            var contactId = await GetUserEntityId();
            var isDecisionMaker = await DecisionMakers.Where(cc => cc.ClientID == clientId && cc.ContactID == contactId).Select(cc => cc.IsDecisionMaker ?? false).FirstOrDefaultAsync();

            Guard.Against.UnAuthorizedAccess(isDecisionMaker);

            var dm = await InstrxDecisionMgtList.Where(d => d.DueID == dueId && d.ContactID == contactId).FirstOrDefaultAsync();

            //save instruction
            var repository = _cpiDbContext.GetRepository<AMSInstrxDecisionMgt>();
            var lastUpdate = DateTime.Now;

            if (dm == null)
            {
                dm = new AMSInstrxDecisionMgt()
                {
                    DueID = dueId,
                    ContactID = contactId,
                    ClientInstructionType = instructionType,
                    ClientInstructionDate = lastUpdate,
                    CreatedBy = userName,
                    DateCreated = lastUpdate,
                    UpdatedBy = userName,
                    LastUpdate = lastUpdate
                };

                repository.Add(dm);
            }
            else
            {
                repository.Attach(dm);

                dm.ClientInstructionType = instructionType;
                dm.ClientInstructionDate = lastUpdate;
                dm.UpdatedBy = userName;
                dm.LastUpdate = lastUpdate;
                dm.tStamp = tStamp;
            }
            await _cpiDbContext.SaveChangesAsync();

            //save final instruction
            await SaveFinalDecision(dueId, clientId, userName);

            return dm.tStamp;
        }

        public async Task SaveDecisionMgtInstructions(List<AMSInstrxDecisionMgt> instructions)
        {
            if (!instructions.Any())
                return;

            var dueId = instructions.Select(i => i.DueID).FirstOrDefault();
            var annId = await QueryableList.Where(d => d.DueID == dueId).Select(d => d.AnnID).FirstOrDefaultAsync();
            Guard.Against.NoRecordPermission(annId > 0);
            
            var clientId = await ValidateDecisionMgtClient(annId);
            Guard.Against.NoRecordPermission(clientId > 0);

            var repository = _cpiDbContext.GetRepository<AMSInstrxDecisionMgt>();
            var updatedContacts = instructions.Where(i => i.tStamp != null).Select(i => i.ContactID).ToList();
            var updated = await repository.QueryableList.Where(dm => dm.DueID == dueId && updatedContacts.Any(c => c == dm.ContactID)).ToListAsync();
            var added = instructions.Where(i => i.tStamp == null).ToList();
            var updatedBy = instructions.Select(i => i.UpdatedBy).FirstOrDefault();
            var lastUpdate = DateTime.Now;

            repository.Attach(updated);
            foreach (var item in updated)
            {
                var instruction = instructions.Find(i => i.DueID == item.DueID && i.ContactID == item.ContactID);

                item.ClientInstructionType = instruction?.ClientInstructionType;
                item.ClientInstructionDate = lastUpdate;
                item.UpdatedBy = updatedBy;
                item.LastUpdate = lastUpdate;
                item.tStamp = instruction?.tStamp;
            }

            foreach (var item in added)
            {
                item.ClientInstructionDate = lastUpdate;
                item.CreatedBy = updatedBy;
                item.DateCreated = lastUpdate;
                item.UpdatedBy = updatedBy;
                item.LastUpdate = lastUpdate;
            }            
            repository.Add(added);
            await _cpiDbContext.SaveChangesAsync();

            await SaveFinalDecision(dueId, clientId, updatedBy);
        }

        private async Task SaveFinalDecision(int dueId, int clientId, string userName)
        {
            var finalInstruction = await GetDecisionMgtFinalInstruction(dueId, clientId);
            var due = await QueryableList.Where(d => d.DueID == dueId).Select(d => new {
                AnnID = d.AnnID,
                ClientInstructionType = d.ClientInstructionType,
                ClientInstructionSource = d.ClientInstructionSource
            }).FirstOrDefaultAsync();

            if (string.IsNullOrEmpty(due.ClientInstructionType) || due.ClientInstructionSource == "D")
                //save final decision
                await SaveInstruction(due.AnnID, dueId, finalInstruction,
                    string.IsNullOrEmpty(finalInstruction) ? "Decision Conflict" : "Final Instruction",
                    "D",
                    await QueryableList.Where(d => d.DueID == dueId).Select(d => d.tStamp).FirstOrDefaultAsync(),
                    userName);
        }

        private async Task<byte[]> SaveDecisionMgtRemarks(int dueId, int clientId, string remarks, byte[] tStamp, string userName)
        {
            //validate if contact person is decision maker
            var contactId = await GetUserEntityId();
            var isDecisionMaker = await DecisionMakers.Where(cc => cc.ClientID == clientId && cc.ContactID == contactId).Select(cc => cc.IsDecisionMaker ?? false).FirstOrDefaultAsync();

            Guard.Against.UnAuthorizedAccess(isDecisionMaker);

            var dm = await InstrxDecisionMgtList.Where(d => d.DueID == dueId && d.ContactID == contactId).FirstOrDefaultAsync();

            //save remarks
            var repository = _cpiDbContext.GetRepository<AMSInstrxDecisionMgt>();
            var lastUpdate = DateTime.Now;

            if (dm == null)
            {
                dm = new AMSInstrxDecisionMgt()
                {
                    DueID = dueId,
                    ContactID = contactId,
                    ClientInstrxRemarks = remarks,
                    CreatedBy = userName,
                    DateCreated = lastUpdate,
                    UpdatedBy = userName,
                    LastUpdate = lastUpdate
                };

                repository.Add(dm);
            }
            else
            {
                repository.Attach(dm);

                dm.ClientInstrxRemarks = remarks;
                dm.UpdatedBy = userName;
                dm.LastUpdate = lastUpdate;
                dm.tStamp = tStamp;
            }
            await _cpiDbContext.SaveChangesAsync();

            return dm.tStamp;
        }

        private async Task<int> ValidateDecisionMgtClient(int annId)
        {
            var client = await _amsMainService.QueryableList.Where(m => m.AnnID == annId).Select(m =>
                                    !_user.IsAMSIntegrated() || m.CountryApplication == null ?
                                        new { ClientID = m.Client.ClientID, UseDecisionMgt = m.Client.UseDecisionMgt ?? false } :
                                        new { ClientID = m.CountryApplication.Invention.Client.ClientID, UseDecisionMgt = m.CountryApplication.Invention.Client.UseDecisionMgt ?? false }).FirstOrDefaultAsync();

            Guard.Against.UnAuthorizedAccess(client != null);

            if (client.UseDecisionMgt)
                return client.ClientID;

            return 0;
        }

        public async Task<byte[]> SaveInstruction(int dueId, string instructionType, string reason, string source, byte[] tStamp, string userName)
        {
            var amsSettings = await _amsSettings.GetSetting();
            var annId = await ValidatePermission(InstructableList, dueId, CPiPermissions.DecisionMaker);
            var result = new byte[] { };

            if (await IsDecisionManagementUser())
            {   
                //validate client
                var clientId = await ValidateDecisionMgtClient(annId);

                //save dm instruction if client is using decision management
                if (clientId > 0)
                    result = await SaveDecisionMgtInstruction(dueId, clientId, instructionType, tStamp, userName);
                else
                    result = await SaveInstruction(annId, dueId, instructionType, reason, source, tStamp, userName);
            }
            else
                result = await SaveInstruction(annId, dueId, instructionType, reason, source, tStamp, userName);

            if (amsSettings.RecalcOnClear && string.IsNullOrEmpty(instructionType))
                await _feeService.RecalculateServiceFee(dueId);

            return result;
        }

        private async Task<byte[]> SaveInstruction(int annId, int dueId, string instructionType, string reason, string source, byte[] tStamp, string userName)
        {
            var parent = await _amsMainService.QueryableList.Where(m => m.AnnID == annId).Select(m => new { m.AnnID, m.CaseNumber, m.Country, m.SubCase, m.tStamp }).FirstOrDefaultAsync();

            Guard.Against.NoRecordPermission(parent != null);

            var instructionDate = DateTime.Now;

            //create instrx change log
            var instrxChangeLog = new AMSInstrxChangeLog()
            {
                DueID = dueId,
                ClientInstruction = (await _cpiDbContext.GetReadOnlyRepositoryAsync<AMSInstrxType>().QueryableList.Where(i => i.InstructionType == instructionType).Select(i => i.ClientDescription).FirstOrDefaultAsync()) ?? "",
                ClientInstructionType = instructionType ?? "",
                ClientInstructionDate = instructionDate,
                ClientInstructionSource = source ?? "E",
                ReasonForChange = reason ?? "",
                DateChanged = instructionDate,
                CreatedBy = userName
            };
            //EF.Core 6 breaking change
            //insert instrxChangeLog through AMSDue to be able to use instrxChangeLog.LogID
            //_cpiDbContext.GetRepository<AMSInstrxChangeLog>().Add(instrxChangeLog);

            //update tblAMSDue
            var amsDue = new AMSDue()
            {
                DueID = dueId,
                tStamp = tStamp,
                ClientInstructionSentToCPI = DateTime.Now,  //set default to force IsModified 
                ClientInstructionSentToCPIFlag = true       //set default to force IsModified
            };
            _cpiDbContext.GetRepository<AMSDue>().Attach(amsDue);
            amsDue.ClientInstructionType = instructionType ?? "";
            amsDue.ClientInstructionDate = instructionDate;
            amsDue.ClientInstructionSource = source ?? "E";
            amsDue.ClientInstructionLogId = instrxChangeLog.LogID;
            amsDue.ClientInstructionSentToCPI = null;
            amsDue.ClientInstructionSentToCPIFlag = false;
            amsDue.UpdatedBy = userName;
            amsDue.LastUpdate = instructionDate;

            //EF.Core 6 breaking change
            //insert instrxChangeLog through AMSDue to be able to use instrxChangeLog.LogID
            amsDue.AMSInstrxChangeLogs = new List<AMSInstrxChangeLog>() { instrxChangeLog };

            //update tblAMSMain
            var amsMain = new AMSMain() { AnnID = parent.AnnID, CaseNumber = parent.CaseNumber, Country = parent.Country, SubCase = parent.SubCase, tStamp = parent.tStamp };
            _cpiDbContext.GetRepository<AMSMain>().Attach(amsMain);
            amsMain.LastInstructionType = instructionType ?? "";
            amsMain.LastInstructionDate = instructionDate;
            amsMain.UpdatedBy = userName;
            amsMain.LastUpdate = instructionDate;

            await _cpiDbContext.SaveChangesAsync();

            //detach for multiple calls from mark all/family
            _cpiDbContext.Detach(amsMain);
            _cpiDbContext.Detach(amsDue);

            return amsDue.tStamp;
        }

        public async Task<byte[]> SaveInstructionRemarks(int dueId, string remarks, byte[] tStamp, string userName)
        {
            var annId = await ValidatePermission(InstructableList, dueId, CPiPermissions.DecisionMaker);

            if (await IsDecisionManagementUser())
            {
                var clientId = await ValidateDecisionMgtClient(annId);
                if (clientId > 0)
                    return await SaveDecisionMgtRemarks(dueId, clientId, remarks, tStamp, userName);
            }

            var updated = new AMSDue()
            {
                DueID = dueId,
                tStamp = tStamp
            };

            _cpiDbContext.GetRepository<AMSDue>().Attach(updated);
            updated.ClientInstrxRemarks = remarks ?? "";
            updated.UpdatedBy = userName;
            updated.LastUpdate = DateTime.Now;

            await _cpiDbContext.SaveChangesAsync();

            return updated.tStamp;
        }

        public async Task<byte[]> MarkDeleted(int dueId, bool ignoreRecord, byte[] tStamp, string userName)
        {
            await ValidatePermission(QueryableList, dueId, CPiPermissions.FullModify);

            var updated = new AMSDue()
            {
                DueID = dueId,
                tStamp = tStamp,
                IgnoreRecord = !ignoreRecord
            };

            _cpiDbContext.GetRepository<AMSDue>().Attach(updated);
            updated.IgnoreRecord = ignoreRecord;
            updated.UpdatedBy = userName;
            updated.LastUpdate = DateTime.Now;

            await _cpiDbContext.SaveChangesAsync();

            return updated.tStamp;
        }

        public async Task<byte[]> SaveClientPaymentDate(int dueId, DateTime? paymentDate, byte[] tStamp, string userName)
        {
            await ValidatePermission(QueryableList, dueId, CPiPermissions.FullModify);

            var updated = new AMSDue()
            {
                DueID = dueId,
                tStamp = tStamp,
                ClientPaymentDate = DateTime.Now,
                ClientPaymentDateFlag = paymentDate == null
            };

            _cpiDbContext.GetRepository<AMSDue>().Attach(updated);
            updated.ClientPaymentDate = paymentDate;
            updated.ClientPaymentDateFlag = paymentDate != null ;
            updated.UpdatedBy = userName;
            updated.LastUpdate = DateTime.Now;

            await _cpiDbContext.SaveChangesAsync();

            return updated.tStamp;
        }

        public async Task SaveClientLastReminderDate(int remId, DateTime remDate)
        {
            var amsDues = await base.QueryableList.Where(d => d.AMSRemLogDues.Any(l => l.RemId == remId && l.DueId == d.DueID)).ToListAsync();

            _cpiDbContext.GetRepository<AMSDue>().Attach(amsDues);
            amsDues.ForEach(d =>
            {
                d.ClientLastReminderDate = remDate;
            });

            await _cpiDbContext.SaveChangesAsync();
        }

        public async Task SaveClientLastPrepayReminderDate(int remId, DateTime remDate)
        {
            var amsDues = await base.QueryableList.Where(d => d.AMSRemLogDues.Any(l => l.RemId == remId && l.DueId == d.DueID)).ToListAsync();

            _cpiDbContext.GetRepository<AMSDue>().Attach(amsDues);
            amsDues.ForEach(d => {
                d.PrePayLastRemDate = remDate;
            });

            await _cpiDbContext.SaveChangesAsync();
        }

        public async Task SaveClientReceiptLetterSentDate(int sendId, DateTime letterSentDate, List<string> clientCodes)
        {
            var amsDues = await base.QueryableList.Where(d => 
                            d.AMSInstrxCPiLogDetails.Any(l => l.SendId == sendId && l.DueId == d.DueID) &&
                            !_user.IsAMSIntegrated() || d.AMSMain.CountryApplication == null ?
                                clientCodes.Contains(d.AMSMain.Client.ClientCode) :
                                clientCodes.Contains(d.AMSMain.CountryApplication.Invention.Client.ClientCode)).ToListAsync();

            _cpiDbContext.GetRepository<AMSDue>().Attach(amsDues);
            amsDues.ForEach(d =>
            {
                d.ClientReceiptLetterSentDate = letterSentDate;
            });

            await _cpiDbContext.SaveChangesAsync();

            //detach for separate client/agent calls from InstructionsToCPiController
            _cpiDbContext.Detach(amsDues);
        }

        public async Task SaveClientInstructionSentToAgent(int sendId, DateTime letterSentDate, List<string> agentCodes)
        {
            var amsDues = await base.QueryableList.Where(d => 
                            d.AMSInstrxCPiLogDetails.Any(l => l.SendId == sendId && l.DueId == d.DueID && 
                            (l.SentInstructionType == "Y" || l.SentInstructionType == "A")) &&
                            !_user.IsAMSIntegrated() || d.AMSMain.CountryApplication == null ?
                                agentCodes.Contains(d.AMSMain.Agent.AgentCode) :
                                agentCodes.Contains(d.AMSMain.CountryApplication.Agent.AgentCode)).ToListAsync();

            _cpiDbContext.GetRepository<AMSDue>().Attach(amsDues);
            amsDues.ForEach(d =>
            {
                d.ClientInstructionSentToAgent = letterSentDate;
            });

            await _cpiDbContext.SaveChangesAsync();

            //detach for separate client/agent calls from InstructionsToCPiController
            _cpiDbContext.Detach(amsDues);
        }

        public override Task Add(AMSDue entity)
        {
            //return base.Add(entity);
            //NOT ALLOWED
            throw new UnauthorizedAccessException();
        }

        public override Task Update(AMSDue entity)
        {
            //return base.Update(entity);
            //NOT ALLOWED
            throw new UnauthorizedAccessException();
        }

        public override Task<bool> Update(object key, string userName, IEnumerable<AMSDue> updated, IEnumerable<AMSDue> added, IEnumerable<AMSDue> deleted)
        {
            //return base.Update(key, userName, updated, added, deleted);
            //NOT ALLOWED
            throw new UnauthorizedAccessException();
        }

        public override Task UpdateRemarks(AMSDue entity)
        {
            //return base.UpdateRemarks(entity);
            //NOT ALLOWED
            throw new UnauthorizedAccessException();
        }

        public override Task Delete(AMSDue entity)
        {
            //return base.Delete(entity);
            //NOT ALLOWED
            throw new UnauthorizedAccessException();
        }

        private async Task<int> ValidatePermission(IQueryable<AMSDue> queryable, int dueId, List<string> roles)
        {
            var item = await queryable.Where(i => i.DueID == dueId).Select(i => new { i.DueID, i.AnnID, i.AMSMain.CPIClientCode }).FirstOrDefaultAsync();
            Guard.Against.NoRecordPermission(item.DueID > 0);
            Guard.Against.NoRecordPermission(await ValidatePermission(SystemType.AMS, roles, item.CPIClientCode));

            return item.AnnID;
        }

        public async Task<List<string>> GetDecisionMakerCpiCodes()
        {
            return await CPiUserSystemRoles.Where(r => r.UserId == UserId && r.SystemId == SystemType.AMS && CPiPermissions.DecisionMaker.Contains(r.RoleId.ToLower()))
                                           .Select(r => r.RespOffice).ToListAsync();
        }

        public async Task<bool> IsProductsOn()
        {
            return await _amsMainService.IsProductsOn();
        }

        public async Task<bool> IsLicenseesOn()
        {
            return await _amsMainService.IsLicenseesOn();
        }

        public async Task<bool> IsPatentScoreOn()
        {
            return await _amsMainService.IsPatentScoreOn();
        }
    }
}
