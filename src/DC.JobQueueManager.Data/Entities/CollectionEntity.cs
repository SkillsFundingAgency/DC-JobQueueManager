using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ESFA.DC.JobQueueManager.Data.Entities
{
    public class CollectionEntity
    {
        public CollectionEntity()
        {
            OrganisationCollection = new HashSet<OrganisationCollectionEntity>();
            ReturnPeriod = new HashSet<ReturnPeriodEntity>();
        }

        [Key]
        public int CollectionId { get; set; }

        public string Name { get; set; }

        public bool IsOpen { get; set; }

        public int CollectionTypeId { get; set; }

        public CollectionTypeEntity CollectionTypeEntity { get; set; }

        public ICollection<OrganisationCollectionEntity> OrganisationCollection { get; set; }

        public ICollection<ReturnPeriodEntity> ReturnPeriod { get; set; }

        public int CollectionYear { get; set; }
    }
}
