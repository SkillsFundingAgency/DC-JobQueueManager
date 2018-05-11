using System;
using System.ComponentModel.DataAnnotations;

namespace ESFA.DC.JobQueueManager.Data.Entities
{
    public class JobEntity
    {
        [Key]
        public long JobId { get; set; }

        [Required]
        [Range(1, 3)]
        public short JobType { get; set; }

        public string FileName { get; set; }

        public long? Ukprn { get; set; }

        [Required]
        [Range(1, 5)]
        public short Priority { get; set; }

        [Required]
        public DateTime DateTimeSubmittedUtc { get; set; }

        public DateTime? DateTimeUpdatedUtc { get; set; }

        public string StorageReference { get; set; }

        [Range(1, 7)]
        public short Status { get; set; }

        [Timestamp]
        public byte[] RowVersion { get; set; }
    }
}