namespace ESFA.DC.JobQueueManager.Data.Entities
{
    public partial class JobTopicTask
    {
        public int Id { get; set; }
        public int JobTopicId { get; set; }
        public string TaskName { get; set; }
        public short TaskOrder { get; set; }
        public bool? Enabled { get; set; }

        public virtual JobTopic JobTopic { get; set; }
    }
}
