using System;
using System.Collections.Generic;

namespace ESFA.DC.JobQueueManager.Data.Entities
{
    public partial class JobSubscriptionTask
    {
        public int JobTopicTaskId { get; set; }
        public int JobTopicId { get; set; }
        public string TaskName { get; set; }
        public short TaskOrder { get; set; }
        public bool? Enabled { get; set; }

        public JobTopicSubscription JobTopic { get; set; }
    }
}
