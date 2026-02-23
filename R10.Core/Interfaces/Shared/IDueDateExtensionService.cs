using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using R10.Core.DTOs;
using R10.Core.Entities;
using R10.Core.Entities.Patent;


namespace R10.Core.Interfaces
{
    public interface IDueDateExtensionService
    {
        DateTime? ComputeRunDate(DueDateExtension setting);
        DateTime ComputeNextDueDate(DateTime currentDueDate, int? months, int? weeks, int? days);
        Task ExtendDueDate(string updatedBy);
    }
        
}
