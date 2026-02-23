using R10.Core.DTOs;
using R10.Core.Entities;
using R10.Core.Entities.DMS;
using R10.Core.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Core.Interfaces.DMS
{
    public interface IDMSValuationMatrixService
    {
        IQueryable<DMSValuationMatrix> DMSValuationMatrices { get; }
        IQueryable<DMSValuationMatrixRate> DMSValuationMatrixRates { get; }

        Task AddValuationMatrix(DMSValuationMatrix valuationMatrix);
        Task UpdateValuationMatrix(DMSValuationMatrix valuationMatrix);
        Task DeleteValuationMatrix(DMSValuationMatrix valuationMatrix);
        Task CopyValuationMatrix(int oldValId, int newValId, string userName, bool copyRating);

        Task UpdateChild(int parentId, string userName, IEnumerable<DMSValuationMatrixRate> updated, IEnumerable<DMSValuationMatrixRate> added, IEnumerable<DMSValuationMatrixRate> deleted);
        Task DeleteValuationMatrixRate(int parentId, string userName, IEnumerable<DMSValuationMatrixRate> deleted);
        Task ReorderValuationMatrixRate(int id, string userName, int newIndex);
    }
}