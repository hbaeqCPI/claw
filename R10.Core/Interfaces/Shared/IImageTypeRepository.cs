using R10.Core.Entities;
using R10.Core.Entities.Patent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R10.Core.Interfaces
{
    public interface IImageTypeRepository : IAsyncRepository<ImageType>
    {
        IQueryable<ImageType> ImageTypes { get; }

        Task<ImageType> GetImageTypeByExtension(string extension);

        Task<int> GetImageTypeIdByImageTypeName(string imageTypeName);

    }
}
