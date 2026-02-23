using System;
using Microsoft.EntityFrameworkCore;
using R10.Core.Entities;
using R10.Core.Entities.Shared;
using R10.Core.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Infrastructure.Data
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
