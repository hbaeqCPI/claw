using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using R10.Core.Identity;

namespace R10.Core.Entities.Patent
{
    public class PatDueDateInvDelegation : PatDueDateDelegationDetail
    {
        public PatActionDueInv? PatActionDueInv { get; set; }

        public PatDueDateInv? PatDueDateInv { get; set; }
    }
}
