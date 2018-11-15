using System;
using System.Collections.Generic;

namespace ESFA.DC.JobQueueManager.Data.Entities
{
    public partial class JobType
    {
        public int JobTypeId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public bool IsCrossLoadingEnabled { get; set; }
        public bool? ProcessingOverrideFlag { get; set; }
        public int? JobTypeGroupId { get; set; }

        public JobTypeGroup JobTypeGroup { get; set; }
    }
}
