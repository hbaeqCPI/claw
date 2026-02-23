using DocuSign.eSign.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.Areas.Patent.ViewModels.Report
{
    public class PatInventorAwardsReportUpdateViewModel
    {
        public DateTime PaymentDate { get; set; }
        public string Cases { get; set; }
    }
}