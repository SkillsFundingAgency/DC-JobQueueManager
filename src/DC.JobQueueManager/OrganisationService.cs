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

        public OrganisationService(DbContextOptions<JobQueueDataContext> dbContextOptions)
        {
            _collectionsManagementContext = new JobQueueDataContext(dbContextOptions);
        }

        public async Task<IEnumerable<CollectionType>> GetAvailableCollectionTypesAsync(long ukprn)
        {
            var data = await _collectionsManagementContext.OrganisationCollection
                .Where(x => x.Organisation.Ukprn == ukprn)
                .GroupBy(x => x.Collection.CollectionType)
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
            var data = await _collectionsManagementContext.OrganisationCollection
                .Include(x => x.Collection)
                .ThenInclude(x => x.CollectionType)
                .Where(x => x.Organisation.Ukprn == ukprn &&
                            x.Collection.IsOpen &&
                            x.Collection.CollectionType.Type == collectionType).
                ToListAsync();
            var items = data.Select(y => new CollectionsManagement.Models.Collection()
                {
                    CollectionTitle = y.Collection.Name,
                    IsOpen = y.Collection.IsOpen,
                    CollectionType = y.Collection.CollectionType.Type,
                    CollectionYear = y.Collection.CollectionYear
                });

            return items;
        }

        public async Task<Collection> GetCollectionAsync(long ukprn, string collectionName)
        {
            var data = await _collectionsManagementContext.OrganisationCollection
                .Include(x => x.Collection)
                .ThenInclude(x => x.CollectionType)
                .Where(x => x.Organisation.Ukprn == ukprn &&
                            x.Collection.Name.Equals(collectionName, StringComparison.CurrentCultureIgnoreCase))
                .FirstOrDefaultAsync();
            if (data != null)
            {
                return new Collection()
                {
                    CollectionTitle = data.Collection.Name,
                    IsOpen = data.Collection.IsOpen,
                    CollectionType = data.Collection.CollectionType.Type,
                    CollectionYear = data.Collection.CollectionYear
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
