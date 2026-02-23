using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using R10.Core.DTOs;

namespace R10.Web.Areas.Patent.ViewModels
{
    public class RTSContinuityViewModel
    {
        public List<RTSSearchContinuityParentDTO> ParentContinuities { get; set; }
        public List<RTSSearchContinuityChildDTO> ChildContinuities { get; set; }
    }
}
