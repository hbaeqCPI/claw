using R10.Core.DTOs;
using R10.Core.Entities.Patent;

namespace R10.Core.Interfaces
{
    public interface IPatCostEstimatorService : IEntityService<PatCostEstimator>
    {
        Task ValidatePermission(int keyId, List<string> roles);

        IQueryable<PatCostEstimator> PatCostEstimators { get; }
        IQueryable<PatCostEstimatorBaseAppDTO> PatCostEstimatorBaseApps { get; }

        Task CopyCostEstimator(int oldKeyId, int newKeyId, string userName, bool copyCountries, bool copyAnswers);

        Task AddCountryCosts(int keyId, string userName, List<int> addedCECountryIds);
        Task DeleteCountryCosts(int keyId, string userName, List<int> deletedEntityIds);  
        
        //Task AddGeneralQuestions(int keyId, string userName);        

        Task<List<CEEstimatedCostDTO>> GetEstimatedCosts(int keyId);
        Task CascadeAnswersByGridAsync(int keyId, List<PatCostEstimatorCountryCost> updatedList);
    }
}
