using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using ESFA.DC.CollectionsManagement.Models;

namespace ESFA.DC.JobQueueManager.Interfaces
{
    public interface ICollectionService
    {
       Task<Collection> GetCollectionAsync(string collectionType);
    }
}
