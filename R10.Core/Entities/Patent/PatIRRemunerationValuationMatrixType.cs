using System.ComponentModel.DataAnnotations;

namespace R10.Core.Entities.Patent
{
    public class PatIRRemunerationValuationMatrixType : BaseEntity
    {
        [Key]
        public int MatrixTypeId { get; set; }

        [StringLength(20)]
        [Display(Name = "Matrix Type")]
        public string? MatrixType { get; set; }

        [StringLength(20)]
        [Display(Name = "MatrixType Component")]
        public string? MatrixTypeComponent { get; set; }

        [Display(Name = "Avaliable Matrix Options")]
        public string? AvaliableMatrixOptions { get; set; }

        public List<PatIRRemunerationValuationMatrix>? Matrixes { get; set; }
    }
}
