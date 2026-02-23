using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using R10.Core.DTOs;

namespace R10.Web.Areas.Patent.ViewModels
{
    public class RTSReferencesViewModel 
    {
        public List<RTSSearchDocCitedDTO> DocsCited { get; set; }
        public List<RTSSearchDocRefByDTO> DocsRefBy { get; set; }
    }
}
