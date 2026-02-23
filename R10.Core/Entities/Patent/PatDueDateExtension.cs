
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace R10.Core.Entities.Patent
{
    public class PatDueDateExtension : DueDateExtension
    {
        public PatDueDate? PatDueDate { get; set; }
    }

    
}
