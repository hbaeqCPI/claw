using R10.Core.Entities.FormExtract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace R10.Core.Interfaces
{
    public interface IFormExtractService
    {
        IQueryable<FormSystem> FormSystems { get;  }
        IQueryable<FormSource> FRSources { get; }


    }
}
