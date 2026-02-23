using R10.Core.Entities.ForeignFiling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R10.Core.Interfaces.ForeignFiling
{
    public interface IFFInstrxTypeService : IBaseService<FFInstrxType>
    {
        IQueryable<FFInstrxType> TicklerInstructionTypes { get; }
        IQueryable<FFInstrxType> InstructionTypes { get; }
        Task<byte[]> SaveSetting(int instructionId, InstructionTypeSetting setting, bool value, byte[] tStamp, string userName);
    }
}
