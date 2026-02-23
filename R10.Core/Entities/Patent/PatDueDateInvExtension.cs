
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace R10.Core.Entities.Patent
{
    public class PatDueDateInvExtension : DueDateExtension
    {
        public PatDueDateInv? PatDueDateInv { get; set; }
    }


}
