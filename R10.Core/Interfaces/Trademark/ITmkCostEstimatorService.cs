using R10.Core.DTOs;
using R10.Core.Entities.Trademark;

namespace R10.Core.Interfaces
{
    public interface ITmkCostEstimatorService : IEntityService<TmkCostEstimator>
    {
        Task ValidatePermission(int keyId, List<string> roles);

        IQueryable<TmkCostEstimator> TmkCostEstimators { get; }
        IQueryable<TmkTrademark> TmkCostEstimatorBaseTmks { get; }

        Task CopyCostEstimator(int oldKeyId, int newKeyId, string userName, bool copyCountries, bool copyAnswers);

        Task AddCountryCosts(int keyId, string userName, List<int> addedCECountryIds);
        Task DeleteCountryCosts(int keyId, string userName, List<int> deletedCECountryIds, bool estimateTypeChanged = false, TmkCostEstimateType? estimateType = TmkCostEstimateType.Both);
        
        Task<List<CEEstimatedCostDTO>> GetEstimatedCosts(int keyId);
        Task<List<CEEstimatedCostDTO>> GetEstimatedRenewalCosts(List<int> dueIds);

        Task CascadeAnswersByGridAsync(int keyId, List<TmkCostEstimatorCountryCost> updatedList);
    }
}