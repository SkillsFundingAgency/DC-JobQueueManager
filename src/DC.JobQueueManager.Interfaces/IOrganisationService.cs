using System.Collections.Generic;
using System.Threading.Tasks;
using ESFA.DC.CollectionsManagement.Models;

namespace ESFA.DC.JobQueueManager.Interfaces
{
    public interface IOrganisationService
    {
        Organisation GetByUkprn(long ukprn);

        Task<IEnumerable<CollectionType>> GetAvailableCollectionTypesAsync(long ukprn);

        Task<IEnumerable<Collection>> GetAvailableCollectionsAsync(long ukprn, string collectionType);

        Task<Collection> GetCollectionAsync(long ukprn, string collectionName);
    }
}
