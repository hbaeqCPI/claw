using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.Areas.Admin.ViewModels
{
    public class ActivityLogSearchResultViewModel
    {
        public int Id { get; set; }
        public DateTime ActivityDate { get; set; }
        public string? UserId { get; set; }
        public string? HostName { get; set; }
        public string? HostIP { get; set; }
        public string? RequestUrl { get; set; }
        //public string RequestForm { get; set; }
        public string? RequestMethod { get; set; }
        public int? StatusCode { get; set; }
        public string? RemoteAddress { get; set; }
        public string? UserAgent { get; set; }
        public string? AcceptLanguage { get; set; }
    }
}
