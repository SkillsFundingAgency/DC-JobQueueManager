using System.Collections.Generic;

namespace ESFA.DC.JobQueueManager.Data.Entities
{
    public partial class JobTopic
    {
        public JobTopic()
        {
            JobTopicTask = new HashSet<JobTopicTask>();
        }

        public int Id { get; set; }
        public short JobTypeId { get; set; }
        public string TopicName { get; set; }
        public short TopicOrder { get; set; }
        public bool IsFirstStage { get; set; }
        public bool? Enabled { get; set; }

        public virtual ICollection<JobTopicTask> JobTopicTask { get; set; }
    }
}
