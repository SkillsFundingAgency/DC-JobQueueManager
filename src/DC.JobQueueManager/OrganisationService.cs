using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ESFA.DC.CollectionsManagement.Models;
using ESFA.DC.JobQueueManager.Data;
using ESFA.DC.JobQueueManager.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ESFA.DC.JobQueueManager
{
    public class OrganisationService : IOrganisationService, IDisposable
    {
        private readonly JobQueueDataContext _collectionsManagementContext;

        public OrganisationService(DbContextOptions dbContextOptions)
        {
            _collectionsManagementContext = new JobQueueDataContext(dbContextOptions);
        }

        public async Task<IEnumerable<CollectionType>> GetAvailableCollectionTypesAsync(long ukprn)
        {
            var data = await _collectionsManagementContext.OrganisationCollections
                .Where(x => x.OrganisationEntity.Ukprn == ukprn)
                .GroupBy(x => x.CollectionEntity.CollectionTypeEntity)
                .ToListAsync();
            var items = data.Select(y => new CollectionType()
            {
                Description = y.Key.Description,
                Type = y.Key.Type
            });

            return items;
        }

        public async Task<IEnumerable<Collection>> GetAvailableCollectionsAsync(long ukprn, string collectionType)
        {
            var data = await _collectionsManagementContext.OrganisationCollections
                .Include(x => x.CollectionEntity)
                .ThenInclude(x => x.CollectionTypeEntity)
                .Where(x => x.OrganisationEntity.Ukprn == ukprn &&
                            x.CollectionEntity.IsOpen &&
                            x.CollectionEntity.CollectionTypeEntity.Type == collectionType).
                ToListAsync();
            var items = data.Select(y => new CollectionsManagement.Models.Collection()
                {
                    CollectionTitle = y.CollectionEntity.Name,
                    IsOpen = y.CollectionEntity.IsOpen,
                    CollectionType = y.CollectionEntity.CollectionTypeEntity.Type
                });

            return items;
        }

        public async Task<Collection> GetCollectionAsync(long ukprn, string collectionName)
        {
            var data = await _collectionsManagementContext.OrganisationCollections
                .Include(x => x.CollectionEntity)
                .ThenInclude(x => x.CollectionTypeEntity)
                .Where(x => x.OrganisationEntity.Ukprn == ukprn &&
                            x.CollectionEntity.Name.Equals(collectionName, StringComparison.CurrentCultureIgnoreCase))
                .FirstOrDefaultAsync();
            if (data != null)
            {
                return new Collection()
                {
                    CollectionTitle = data.CollectionEntity.Name,
                    IsOpen = data.CollectionEntity.IsOpen,
                    CollectionType = data.CollectionEntity.CollectionTypeEntity.Type
                };
            }

            return null;
        }

        public Organisation GetByUkprn(long ukprn)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            _collectionsManagementContext.Dispose();
        }
    }
}
