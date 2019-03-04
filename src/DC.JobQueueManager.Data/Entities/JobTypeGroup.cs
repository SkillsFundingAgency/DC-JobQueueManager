using System;
using System.Collections.Generic;

namespace ESFA.DC.JobQueueManager.Data.Entities
{
    public partial class JobTypeGroup
    {
        public JobTypeGroup()
        {
            JobType = new HashSet<JobType>();
        }

        public int JobTypeGroupId { get; set; }
        public string Description { get; set; }
        public int? ConcurrentExecutionCount { get; set; }

        public ICollection<JobType> JobType { get; set; }
    }
}
