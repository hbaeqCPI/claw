using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Kendo.Mvc.UI;
using Microsoft.AspNetCore.Mvc;
using R10.Web.Areas.Shared.ViewModels;

namespace R10.Web.Interfaces
{
    public interface IEFSHelper
    {
        DataSet DataSource { get; set; }
        string SourceDocumentPath { get; set; }
        string MapFilePath { get; set; }
        byte[] FillPdfWithData(bool isManualMerge, string docType, string subType, ref bool compressed);
    }
}
