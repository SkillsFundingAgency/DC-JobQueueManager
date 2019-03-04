using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ESFA.DC.JobQueueManager.Data;
using ESFA.DC.JobQueueManager.Data.Entities;
using ESFA.DC.JobQueueManager.Interfaces;
using Microsoft.EntityFrameworkCore;
using Collection = ESFA.DC.CollectionsManagement.Models.Collection;

namespace ESFA.DC.JobQueueManager
{
    public class CollectionService : ICollectionService
    {
        private readonly JobQueueDataContext _context;

        public CollectionService(DbContextOptions<JobQueueDataContext> dbContextOptions)
        {
            _context = new JobQueueDataContext(dbContextOptions);
        }

        public async Task<Collection> GetCollectionAsync(string collectionType)
        {
            var data = await _context.Collection.Where(x =>
                    x.CollectionType.Type == collectionType)
                .FirstOrDefaultAsync();
            return new Collection()
            {
                CollectionYear = data.CollectionYear,
                CollectionType = collectionType,
                CollectionTitle = data.Name,
                IsOpen = data.IsOpen
            };
        }
    }
}
