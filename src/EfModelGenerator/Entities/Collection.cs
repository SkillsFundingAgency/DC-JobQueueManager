using System;
using System.Collections.Generic;

namespace ESFA.DC.JobQueueManager.Data.Entities
{
    public partial class Collection
    {
        public Collection()
        {
            OrganisationCollection = new HashSet<OrganisationCollection>();
            ReturnPeriod = new HashSet<ReturnPeriod>();
        }

        public int CollectionId { get; set; }
        public string Name { get; set; }
        public bool IsOpen { get; set; }
        public int CollectionTypeId { get; set; }
        public int? CollectionYear { get; set; }

        public CollectionType CollectionType { get; set; }
        public ICollection<OrganisationCollection> OrganisationCollection { get; set; }
        public ICollection<ReturnPeriod> ReturnPeriod { get; set; }
    }
}
