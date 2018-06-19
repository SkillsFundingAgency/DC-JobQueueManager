using System;
using ESFA.DC.JobQueueManager.Models.Enums;
using ESFA.DC.JobStatus.Interface;

namespace ESFA.DC.JobQueueManager.Models
{
    public class Job
    {
        public long JobId { get; set; }

        public JobType JobType { get; set; }

        public string FileName { get; set; }

        public long? Ukprn { get; set; }

        public short Priority { get; set; }

        public DateTime DateTimeSubmittedUtc { get; set; }

        public DateTime? DateTimeUpdatedUtc { get; set; }

        public string StorageReference { get; set; }

        public JobStatusType Status { get; set; }

        public string RowVersion { get; set; }
    }
}