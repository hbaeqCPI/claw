using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace R10.Core.Entities.Patent
{
    public class PubNumberConverted 
    {
        [Key]
        public int AppId { get; set; }
        public bool? UpdateAppNo { get; set; }
        public bool? UpdatePubNo { get; set; }
        public bool? UpdatePatNo { get; set; }
        public bool? UpdateFilDate { get; set; }
        public bool? UpdatePubDate { get; set; }
        public bool? UpdateIssDate { get; set; }
        public bool? UpdateParentPCTDate { get; set; }
        public bool? UpdateCaseType { get; set; }
        public bool? ExcludeUpdate { get; set; }

        [Column(TypeName = "timestamp")]
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        [MaxLength(8)]
        [ConcurrencyCheck]
        public byte[]? tStamp { get; set; }

    }

}
