using System;
using System.Collections.Generic;

namespace ESFA.DC.JobQueueManager.Data.Entities
{
    public partial class CollectionType
    {
        public CollectionType()
        {
            Collection = new HashSet<Collection>();
        }

        public int CollectionTypeId { get; set; }
        public string Type { get; set; }
        public string Description { get; set; }

        public ICollection<Collection> Collection { get; set; }
    }
}
