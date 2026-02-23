using R10.Core.Entities.RMS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R10.Core.Interfaces.RMS
{
    public interface IRMSInstrxTypeService : IBaseService<RMSInstrxType>
    {
        IQueryable<RMSInstrxType> TicklerInstructionTypes { get; }
        IQueryable<RMSInstrxType> InstructionTypes { get; }
        Task<byte[]> SaveSetting(int instructionId, InstructionTypeSetting setting, bool value, byte[] tStamp, string userName);
    }
}
