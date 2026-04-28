using System;
using Microsoft.EntityFrameworkCore;
using LawPortal.Core.Entities;
using LawPortal.Core.Entities.Shared;
using LawPortal.Core.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LawPortal.Infrastructure.Data
{

    public class ImageTypeRepository : EFRepository<ImageType>, IImageTypeRepository
    {

        public ImageTypeRepository(ApplicationDbContext dbContext) : base(dbContext) { }

        public IQueryable<ImageType> ImageTypes => _dbContext.ImageTypes.AsNoTracking();

        public async Task<ImageType> GetImageTypeByExtension(string extension)
        {
            extension = "|" + extension + "|";
            return await _dbContext.ImageTypes.FirstOrDefaultAsync(e => e.Extensions.Contains(extension));            
        }

        public async Task<int> GetImageTypeIdByImageTypeName(string imageTypeName)
        {
            var imageType = await _dbContext.ImageTypes.FirstOrDefaultAsync(e => e.ImageTypeName == imageTypeName);
            return imageType.ImageTypeId;
        }

    }
}
