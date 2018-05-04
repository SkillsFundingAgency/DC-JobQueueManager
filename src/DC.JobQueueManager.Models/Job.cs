using System;
using DC.JobQueueManager.Models.Enums;

namespace DC.JobQueueManager.Models
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

        public JobStatus Status { get; set; }

        public byte[] RowVersion { get; set; }
    }
}
