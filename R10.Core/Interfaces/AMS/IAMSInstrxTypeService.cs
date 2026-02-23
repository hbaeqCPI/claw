using R10.Core.Entities.AMS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R10.Core.Interfaces.AMS
{
    public interface IAMSInstrxTypeService : IBaseService<AMSInstrxType>
    {
        IQueryable<AMSInstrxType> TicklerInstructionTypes { get; }
        IQueryable<AMSInstrxType> InstructionTypes { get; }
        Task<byte[]> SaveSetting(int instructionId, InstructionTypeSetting setting, bool value, byte[] tStamp, string userName);
    }
}
