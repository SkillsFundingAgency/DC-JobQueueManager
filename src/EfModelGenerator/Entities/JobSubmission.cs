using System;
using System.Collections.Generic;

namespace ESFA.DC.JobQueueManager.Data.Entities
{
    public partial class JobSubmission
    {
        public long Id { get; set; }
        public long JobId { get; set; }
        public DateTime DateTimeUtc { get; set; }

        public Job Job { get; set; }
    }
}
