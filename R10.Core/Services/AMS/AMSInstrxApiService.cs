using Microsoft.EntityFrameworkCore;
using R10.Core.Entities;
using R10.Core.Entities.AMS;
using R10.Core.Exceptions;
using R10.Core.Helpers;
using R10.Core.Interfaces;
using R10.Core.Interfaces.AMS;
using System.Security.Claims;

namespace R10.Core.Services.AMS
{
    public class AMSInstrxApiService : WebApiBaseService<AMSInstrxWebSvc>, IAMSInstrxApiService
    {
        protected readonly IAMSDueService _amsDueService;

        public AMSInstrxApiService(IAMSDueService amsDueService, ICPiDbContext cpiDbContext, ClaimsPrincipal user) : base(cpiDbContext, user)
        {
            _amsDueService = amsDueService;
        }

        public IQueryable<AMSDue> InstructableList => _amsDueService.InstructableList;

        public IQueryable<AMSInstrxType> InstructionTypes => _cpiDbContext.GetReadOnlyRepositoryAsync<AMSInstrxType>().QueryableList
            .Where(i => i.InUse);

        public IQueryable<AMSInstrxCPiLogDetail> InstructionCPiLogDetails => _cpiDbContext.GetReadOnlyRepositoryAsync<AMSInstrxCPiLogDetail>().QueryableList;

        public async Task<IQueryable<AMSDue>> GetInstructionsToCPi()
        {
            return await _amsDueService.GetInstructionsToCPi();
        }

        public async Task<List<int>> SaveInstructions(List<AMSInstrxWebSvc> webApiInstrx, DateTime runDate)
        {
            var instructions = await ValidateInstructions(webApiInstrx);

            foreach(var instruction in instructions)
            {
                await _amsDueService.SaveInstruction(instruction.DueId, instruction.InstructionType, "Web API instruction", "W", instruction.tStamp, _user.GetUserName());
            }

            return instructions.Select(i => i.DueId).ToList();
        }

        private async Task<List<(int DueId, string InstructionType, byte[] tStamp)>> ValidateInstructions(List<AMSInstrxWebSvc> webApiInstrx)
        {
            var instructions = new List<(int DueId, string InstructionType, byte[] tStamp)>();
            var errors = new List<string>();
            var duplicates = webApiInstrx.GroupBy(i => new { i.CaseNumber, i.Country, i.SubCase, i.PaymentType, i.AnnuityYear }).Where(g => g.Count() > 1).ToList();

            if (duplicates.Count > 0)
                throw new WebApiValidationException("Duplicate records found.", duplicates.Select(d => $"{d.Key}").ToList());

            var instructionTypes = await InstructionTypes.Select(i => new { i.InstructionType, i.ClientDescription }).ToListAsync();

            for (int i = 0; i < webApiInstrx.Count; i++)
            {
                var instruction = webApiInstrx[i];

                try
                {
                    var due = await ValidateData(instruction);
                    var instructionType = instructionTypes.Where(i => i.ClientDescription.ToLower() == instruction.Instruction?.ToLower()).Select(i => i.InstructionType).FirstOrDefault();

                    Guard.Against.ValueNotAllowed(!string.IsNullOrEmpty(instructionType), "Instruction");

                    instructions.Add(new (due.DueId, instructionType, due.tStamp));
                }
                catch (Exception ex)
                {
                    errors.Add(FormatErrorMessage(i, ex.Message, instruction.CaseNumber, instruction.Country, instruction.SubCase, instruction.PaymentType, instruction.AnnuityYear));
                }
            }

            if (errors.Count > 0)
                throw new WebApiValidationException(errors);

            _cpiDbContext.GetRepository<AMSInstrxWebSvc>().Add(webApiInstrx);
            await _cpiDbContext.SaveChangesAsync();

            return instructions;
        }

        private async Task<(int DueId, byte[] tStamp)> ValidateData(AMSInstrxWebSvc webApiInstrx)
        {
            webApiInstrx.SubCase = webApiInstrx.SubCase ?? "";

            Guard.Against.NullOrEmpty(webApiInstrx.CaseNumber, "CaseNumber");
            Guard.Against.NullOrEmpty(webApiInstrx.Country, "Country");
            Guard.Against.NullOrEmpty(webApiInstrx.PaymentType, "PaymentType");
            Guard.Against.NullOrEmpty(webApiInstrx.AnnuityYear, "AnnuityYear");
            Guard.Against.NullOrEmpty(webApiInstrx.Instruction, "Instruction");

            var due = await InstructableList.Where(d =>
                            d.AMSMain.CaseNumber == webApiInstrx.CaseNumber &&
                            d.AMSMain.Country == webApiInstrx.Country &&
                            d.AMSMain.SubCase == (webApiInstrx.SubCase ?? "") &&
                            d.PaymentType == (webApiInstrx.PaymentType ?? "ANNUITY") &&
                            d.AnnuityYear == webApiInstrx.AnnuityYear
                            ).Select(d => new { d.DueID, d.tStamp, d.AMSMain.CPIClientCode }).FirstOrDefaultAsync();

            Guard.Against.RecordNotFound(due != null && due.DueID > 0);
            Guard.Against.NoRecordPermission(await ValidatePermission(SystemType.AMS, CPiPermissions.FullModify, due?.CPIClientCode ?? ""));

            return (due.DueID, due.tStamp);
        }
    }

    public interface IAMSInstrxApiService
    {
        IQueryable<AMSDue> InstructableList { get; }
        IQueryable<AMSInstrxType> InstructionTypes { get; }
        IQueryable<AMSInstrxCPiLogDetail> InstructionCPiLogDetails { get; }
        Task<IQueryable<AMSDue>> GetInstructionsToCPi();
        Task<List<int>> SaveInstructions(List<AMSInstrxWebSvc> webApiInstrx, DateTime runDate);
    }
}
