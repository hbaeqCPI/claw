using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace LawPortal.Core.Entities
{
    // The Deploy detail form posts every field as a JSON string (form
    // serializer in pageHelper.js builds the payload from .serializeArray()).
    // The default System.Text.Json options on this project are strict, so
    // posting "119" for a nullable int silently fails to bind and the field
    // stays null. AllowReadingFromString tells the deserializer to coerce
    // numeric strings into the typed property — applies to every int / int?
    // on this entity (DeployPasswordId, Year, and the 12 doc-id fields).
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public class DeployPassword : BaseEntity
    {
        [Key]
        public int DeployPasswordId { get; set; }

        [Required]
        [Display(Name = "Year")]
        public int Year { get; set; }

        [Required]
        [StringLength(2)]
        [Display(Name = "Quarter")]
        public string Quarter { get; set; } = "";

        [StringLength(30)]
        [Display(Name = "Patent Password")]
        public string PatentPassword { get; set; } = "";

        [StringLength(30)]
        [Display(Name = "Trademark Password")]
        public string TrademarkPassword { get; set; } = "";

        // Per-path document selections. Each nullable DocId points at a row in
        // DocDocument and corresponds to a hardcoded deploy path on the Deploy
        // detail screen. Tag column shows the matching Release.Systems entry
        // used to populate the dropdown.
        //
        //   Path                          Tag
        //   LawDocs/Pat/Ver9and10/        R4 (Pat-side)
        //   Mdbs/Pat/Ver9and10/           R4 (Pat-side)
        //   LawDocs/Pat/R5/               PatR5-7
        //   Mdbs/Pat/R5/                  PatR5-7
        //   LawDocs/Pat/R8/               PatR8-R10v2.1
        //   Mdbs/Pat/R8/                  PatR8-R10v2.1
        //   LawDocs/Tmk/Ver9and10/        R4 (Tmk-side)
        //   Mdbs/Tmk/Ver9and10/           R4 (Tmk-side)
        //   LawDocs/Tmk/R5/               TmkR5-8
        //   Mdbs/Tmk/R5/                  TmkR5-8
        //   LawDocs/Tmk/R9/               TmkR9-10v2.2
        //   Mdbs/Tmk/R9/                  TmkR9-10v2.2

        [Display(Name = "LawDocs/Pat/Ver9and10/")]
        public int? PatVer9And10LawDocId { get; set; }

        [Display(Name = "Mdbs/Pat/Ver9and10/")]
        public int? PatVer9And10MdbId { get; set; }

        [Display(Name = "LawDocs/Pat/R5/")]
        public int? PatR5LawDocId { get; set; }

        [Display(Name = "Mdbs/Pat/R5/")]
        public int? PatR5MdbId { get; set; }

        [Display(Name = "LawDocs/Pat/R8/")]
        public int? PatR8LawDocId { get; set; }

        [Display(Name = "Mdbs/Pat/R8/")]
        public int? PatR8MdbId { get; set; }

        [Display(Name = "LawDocs/Tmk/Ver9and10/")]
        public int? TmkVer9And10LawDocId { get; set; }

        [Display(Name = "Mdbs/Tmk/Ver9and10/")]
        public int? TmkVer9And10MdbId { get; set; }

        [Display(Name = "LawDocs/Tmk/R5/")]
        public int? TmkR5LawDocId { get; set; }

        [Display(Name = "Mdbs/Tmk/R5/")]
        public int? TmkR5MdbId { get; set; }

        [Display(Name = "LawDocs/Tmk/R9/")]
        public int? TmkR9LawDocId { get; set; }

        [Display(Name = "Mdbs/Tmk/R9/")]
        public int? TmkR9MdbId { get; set; }
    }
}
