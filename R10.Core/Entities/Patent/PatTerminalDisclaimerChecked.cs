using System.ComponentModel.DataAnnotations;

namespace R10.Core.Entities.Patent
{
    public class PatTerminalDisclaimerChecked: PatTerminalDisclaimerCheckedDetail
    {        
        public CountryApplication? CountryApplication { get; set; }
    }

    public class PatTerminalDisclaimerCheckedDetail : BaseEntity
    {
        [Key]
        public int EntityId { get; set; }
        public int AppId { get; set; }
    }
}



