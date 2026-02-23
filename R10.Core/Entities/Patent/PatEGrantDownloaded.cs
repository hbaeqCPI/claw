using System.ComponentModel.DataAnnotations;

namespace R10.Core.Entities.Patent
{
    public class PatEGrantDownloaded: PatEGrantDownloadedDetail
    {        
        public CountryApplication? CountryApplication { get; set; }
    }

    public class PatEGrantDownloadedDetail : BaseEntity
    {
        [Key]
        public int EntityId { get; set; }
        public int AppId { get; set; }
    }
}



