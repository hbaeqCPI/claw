using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using R10.Core.Entities;
using R10.Core.Entities.Patent;

namespace R10.Core.DTOs
{
    public class PatScoreDTO
    {
        //[Key]
        public int ScoreId { get; set; }

        [Key]
        public int CategoryId { get; set; }
        public string? Category { get; set; }
        public int AppId { get; set; }
        public double Score { get; set; }
        public string? Remarks { get; set; }
    }

    public class PatAverageScoreDTO
    {
        [Key]
        public int AppId { get; set; }
        public double AverageScore { get; set; }

        public CountryApplication CountryApplication { get; set; }
    }
}
