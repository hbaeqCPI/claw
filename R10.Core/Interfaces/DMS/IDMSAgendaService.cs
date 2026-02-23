using R10.Core.DTOs;
using R10.Core.Entities.DMS;
using R10.Core.Entities.Patent;
using R10.Core.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Core.Interfaces.DMS
{
    public interface IDMSAgendaService : IEntityService<DMSAgenda>
    {
        Task AddAgenda(DMSAgenda agenda, bool hasReviewersCopy = false, bool hasDisclosuresCopy = false);        
        Task CopyAgenda(int oldAgendaId, int newAgendaId, string userName, bool copyReviewers, bool copyDisclosures);
    }
}
