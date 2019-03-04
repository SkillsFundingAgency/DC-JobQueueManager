using System;
using System.Collections.Generic;

namespace ESFA.DC.JobQueueManager.Data.Entities
{
    public partial class JobStatusType
    {
        public int StatusId { get; set; }
        public string StatusTitle { get; set; }
        public string StatusDescription { get; set; }
    }
}
