using R10.Core.DTOs;
using R10.Core.Entities;
using R10.Core.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using R10.Core.Entities.Patent;

namespace R10.Core.Interfaces
{
    public interface IPatCEAnnuitySetupService
    {
        Task AddAnnuitySetup(PatCEAnnuitySetup annuitySetup);
        Task UpdateAnnuitySetup(PatCEAnnuitySetup annuitySetup);
        Task DeleteAnnuitySetup(PatCEAnnuitySetup annuitySetup);
        Task<string?> CheckExistingAnnuitySetup(PatCEAnnuitySetup annuitySetup);

        Task CopyAnnuitySetup(int oldCEAnnuityId, int newCEAnnuityId, string userName, bool copyCosts);

        IQueryable<PatCEAnnuitySetup> PatCEAnnuitySetups { get; }
        IQueryable<PatCEAnnuityCost> PatCEAnnuityCosts { get; }
        IQueryable<PatCaseType> PatCaseTypes { get; }
    }
}
