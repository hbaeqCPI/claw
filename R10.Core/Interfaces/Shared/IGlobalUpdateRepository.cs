using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using R10.Core.DTOs;

namespace R10.Core.Interfaces
{
    public interface IGlobalUpdateRepository
    {
        Task<List<LookupDTO>> GetUpdateFields(string systemType);

    }
}
