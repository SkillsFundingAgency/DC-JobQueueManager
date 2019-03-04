using System;
using System.Collections.Generic;

namespace ESFA.DC.JobQueueManager.Data.Entities
{
    public partial class JobTopic
    {
        public int JobTopicId { get; set; }
        public short JobTypeId { get; set; }
        public string TopicName { get; set; }
        public short TopicOrder { get; set; }
        public bool? IsFirstStage { get; set; }
        public bool? Enabled { get; set; }

        public JobTopic JobTopicNavigation { get; set; }
        public JobTopic InverseJobTopicNavigation { get; set; }
    }
}
