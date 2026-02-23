using R10.Core.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.Areas.Shared.ViewModels
{
    public class EFSViewModel : EFS
    {
        public string? SignatureQETemplateName { get; set; }

    }


}
