using R10.Core.Entities.Patent;
using R10.Core.Entities.Trademark;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace R10.Core.Entities
{
    public class ImageType : ImageTypeDetail
    {
        //public List<PatImageInv>? PatImagesInv { get; set; }
        //public List<PatImageApp>? PatImagesApp { get; set; }
        //public List<PatImageAct>? PatImagesAct { get; set; }
        //public List<PatImageCost>? PatImagesCost { get; set; }

        //public List<DMSImage>? DMSImages { get; set; }

        //public List<TmkImage>? TmkImages { get; set; }
        //public List<TmkImageAct>? TmkImagesAct { get; set; }
        //public List<TmkImageCost>? TmkImagesCost { get; set; }

        //public List<GMMatterImage>? GMMatterImages { get; set; }
        //public List<GMMatterImageAct>? GMMatterImagesAct { get; set; }
        //public List<GMMatterImageCost>? GMMatterImagesCost { get; set; }

        //public List<TmcImage>? TmcImages { get; set; }

        //public List<PacImage>? PacImages { get; set; }
    }

    public class ImageTypeDetail : BaseEntity
    {
        [Key]
        public int ImageTypeId { get; set; }

        [StringLength(50)]
        public string?  ImageTypeName { get; set; }

        [StringLength(255)]
        public string?  DefaultImage { get; set; }

        [StringLength(255)]
        public string?  Extensions { get; set; }
    }
}
