using R10.Core.DTOs;
using R10.Core.Entities.DMS;
using R10.Core.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R10.Core.Interfaces.DMS
{
    public interface IDMSPreviewService : IChildEntityService<Disclosure, DMSPreview>
    {
        Task<(int DMSPreviewId, CPiEntityType PreviewerType, int PreviewerId, byte[] tStamp)> Update(DMSPreview preview);
    }
}
