using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace R10.Core.Entities.DMS
{
    public class DMSValuationMatrixRate : BaseEntity, IValidatableObject
    {
        [Key]
        public int RateId { get; set; }

        public int ValId { get; set; }

        [Display(Name = "In Use?")]
        public bool InUse { get; set; }

        [Required, StringLength(255)]
        [Display(Name = "Rating")]
        public string Rating { get; set; }

        [Display(Name = "Min Value")]
        [StringLength(2)]
        public string? WeightMin { get; set; }

        [Display(Name = "Max Value")]
        [StringLength(2)]
        public string? WeightMax { get; set; }

        public int OrderOfEntry { get; set; }

        [NotMapped]
        public byte[]? ParentTStamp { get; set; }

        [NotMapped]
        public string? RatingSystem { get; set; }

        [NotMapped]
        public bool CanEditWeight { get; set; }

        [NotMapped]
        public bool CanDelete { get; set; }

        public DMSValuationMatrix? DMSValuationMatrix { get; set; }

        public List<DMSValuation>? Valuations { get; set; }

        IEnumerable<ValidationResult> IValidatableObject.Validate(ValidationContext validationContext)
        {
            List<ValidationResult> res = new List<ValidationResult>();
            string ratingSystem = RatingSystem?.ToUpper() ?? "NULL";

            if (ratingSystem == "NUMERIC RANGE")
            {
                // Validation for numeric range
                if (string.IsNullOrEmpty(WeightMin) || string.IsNullOrEmpty(WeightMax))
                {
                    res.Add(new ValidationResult("Min and Max Values cannot be null for NUMERIC RANGE."));
                    return res;
                }

                // Check if both are numeric
                if (int.TryParse(WeightMin, out int minInt) && int.TryParse(WeightMax, out int maxInt))
                {
                    // Numeric: allow up to 2 digits
                    if (WeightMin.Length > 2 || WeightMax.Length > 2)
                    {
                        res.Add(new ValidationResult("Min and Max Values must be at most 2 digits for NUMERIC RANGE."));
                    }
                    if (minInt >= maxInt)
                    {
                        res.Add(new ValidationResult("Min Value must be less than Max Value."));
                    }
                }
                // Check if both are alphabetic
                else if (char.TryParse(WeightMin.ToUpper(), out char minChar) && char.TryParse(WeightMax.ToUpper(), out char maxChar) &&
                         char.IsLetter(minChar) && char.IsLetter(maxChar))
                {
                    // Alphabetic: allow only 1 character
                    if (WeightMin.Length > 1 || WeightMax.Length > 1)
                    {
                        res.Add(new ValidationResult("Min and Max Values must be a single letter for NUMERIC RANGE with letters."));
                    }
                    if (minChar >= maxChar)
                    {
                        res.Add(new ValidationResult("Min Value must be less than Max Value."));
                    }
                }
                else
                {
                    res.Add(new ValidationResult("Min and Max Values must be of the same type (numeric or letter)."));
                }                
            }
            else if (ratingSystem == "NUMERIC")
            {
                // Validation for numeric only
                if (string.IsNullOrEmpty(WeightMin))
                {
                    res.Add(new ValidationResult("Value cannot be null for NUMERIC rating system."));
                    return res;
                }

                if (int.TryParse(WeightMin, out int minInt))
                {
                    // Numeric: allow up to 2 digits
                    if (WeightMin.Length > 2)
                    {
                        res.Add(new ValidationResult("Value must be at most 2 digits for NUMERIC rating system."));
                    }
                }
                else
                {
                    res.Add(new ValidationResult("Value must be an integer for NUMERIC rating system."));
                }
            }
            else if (ratingSystem == "LETTERS")
            {
                // Validation for letters only
                if (string.IsNullOrEmpty(WeightMin))
                {
                    res.Add(new ValidationResult("Value cannot be null for LETTERS rating system."));
                    return res;
                }

                if (char.TryParse(WeightMin.ToUpper(), out char minChar) && char.IsLetter(minChar))
                {
                    // Alphabetic: allow only 1 character
                    if (WeightMin.Length > 1)
                    {
                        res.Add(new ValidationResult("Value must be a single letter for LETTERS rating system."));
                    }
                }
                else
                {
                    res.Add(new ValidationResult("Value must be a letter for LETTERS rating system."));
                }
            }

            return res;
        }
    }
}
