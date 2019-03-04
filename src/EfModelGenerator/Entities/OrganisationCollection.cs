using System;
using System.Collections.Generic;

namespace ESFA.DC.JobQueueManager.Data.Entities
{
    public partial class OrganisationCollection
    {
        public int OrganisationId { get; set; }
        public int CollectionId { get; set; }

        public Collection Collection { get; set; }
        public Organisation Organisation { get; set; }
    }
}
