using System;
using System.ComponentModel.DataAnnotations;

namespace ESFA.DC.JobQueueManager.Data.Entities
{
    public class JobEntity
    {
        [Key]
        public long JobId { get; set; }

        public short JobType { get; set; }

        public string FileName { get; set; }

        public long? Ukprn { get; set; }

        public short Priority { get; set; }

        public DateTime DateTimeSubmittedUtc { get; set; }

        public DateTime? DateTimeUpdatedUtc { get; set; }

        public string StorageReference { get; set; }

        public short Status { get; set; }

        [Timestamp]
        public byte[] RowVersion { get; set; }
    }
}