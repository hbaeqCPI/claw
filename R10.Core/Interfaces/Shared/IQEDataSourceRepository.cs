using R10.Core.Entities;
using R10.Core.Entities.Patent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using R10.Core.DTOs;

namespace R10.Core.Interfaces
{
    public interface IQEDataSourceRepository 
    {
        Task<List<QEColumnDTO>> GetDataFields(string viewName);

    }
}
